package com.valtech.idp;

import com.nimbusds.jwt.JWTClaimsSet;
import com.nimbusds.jwt.PlainJWT;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.client.RestTemplate;
import org.springframework.web.servlet.View;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;
import org.springframework.web.servlet.view.RedirectView;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpSession;
import java.util.UUID;

@SpringBootApplication
@Controller
public class IdpTestClient {

    Logger log = LoggerFactory.getLogger(getClass());

    @Value("${idp.url}")
    String idpUrl;

    @Value("${client.id}")
    String clientId;

    @Value("${client.secret}")
    String clientSecret;

    RestTemplate restTemplate = new RestTemplate();

    public static void main(String[] args) {
        SpringApplication.run(IdpTestClient.class, args);
    }

    @RequestMapping("/")
    String home(Model model, HttpSession session) {

        String header = "Not signed in";
        String text = "Click the button below to sign in.";

        String email = (String) session.getAttribute("email");

        if (email != null) {
            header = "Welcome!";
            text = String.format("Signed in as %s.", email);
        }

        model.addAttribute("signedIn", email != null);
        model.addAttribute("header", header);
        model.addAttribute("text", text);

        return "index";
    }

    @RequestMapping("/sign-in")
    View signIn(RedirectAttributes params, HttpSession session) {

        if (session.getAttribute("email") != null) {
            return new RedirectView("/");
        }

        session.setAttribute("state", UUID.randomUUID().toString());

        params.addAttribute("response_type", "code");
        params.addAttribute("client_id", clientId);
        params.addAttribute("scope", "email openid");
        params.addAttribute("state", session.getAttribute("state"));

        return new RedirectView(idpUrl + "/oauth2/authorize");
    }

    @RequestMapping("/sign-in/callback")
    View signInCallback(HttpServletRequest req, HttpSession session) {

        if (req.getParameter("error") != null) {
            throw new RuntimeException(String.format("OAuth error: %s, description: %s", req.getParameter("error"),
                    req.getParameter("error_description")));
        }

        String state = req.getParameter("state");
        String code = req.getParameter("code");

        if (!state.equals(session.getAttribute("state"))) {
            throw new RuntimeException("Possible CSRF detected (state does not match stored state)");
        }
        session.removeAttribute("state");

        TokenResponse tokenResponse = exchangeCodeForTokens(code);

        // Not used anymore
        //UserInfo userInfo = fetchUserInfo(tokenResponse.access_token);

        JWTClaimsSet claims = claimsFromToken(tokenResponse.id_token);
        session.setAttribute("email", claims.toJSONObject().get("email"));

        return new RedirectView("/");
    }

    @RequestMapping("/sign-out")
    View signOut(HttpSession session) {
        session.invalidate();
        return new RedirectView(idpUrl + "/oidc/end-session?client_id=" + clientId);
    }

    private TokenResponse exchangeCodeForTokens(String code) {
        TokenRequest req = new TokenRequest(code, clientId, clientSecret);
        ResponseEntity<TokenResponse> resp = restTemplate.postForEntity(idpUrl + "/oauth2/token", req, TokenResponse.class);

        if (!resp.getStatusCode().is2xxSuccessful()) {
            throw new RuntimeException(String.format("Error exchanging code for token: %s", resp.getStatusCode()));
        }
        return resp.getBody();
    }

    private static class TokenRequest extends LinkedMultiValueMap<String, String> {
        public TokenRequest(String code, String clientId, String clientSecret) {
            add("grant_type", "authorization_code");
            add("code", code);
            add("client_id", clientId);
            add("client_secret", clientSecret);
        }
    }

    private static class TokenResponse {
        public String access_token;
        public String id_token;
    }

    // Not used but left as an example of how to call the UserInfo endpoint
    private UserInfo fetchUserInfo(String accessToken) {
        HttpHeaders headers = new HttpHeaders();
        headers.add("Authorization", String.format("Bearer %s", accessToken));
        HttpEntity<byte[]> req = new HttpEntity<>(headers);
        ResponseEntity<UserInfo> resp = restTemplate.exchange(idpUrl + "/api/users/me", HttpMethod.GET, req, UserInfo.class);
        if (!resp.getStatusCode().is2xxSuccessful()) {
            throw new RuntimeException(String.format("Error fetching user info: %s", resp.getStatusCode()));
        }
        return resp.getBody();
    }

    private static class UserInfo {
        public String email;
    }

    private JWTClaimsSet claimsFromToken(String idToken) {
        try {
            PlainJWT jwt = PlainJWT.parse(idToken);
            JWTClaimsSet claims = jwt.getJWTClaimsSet();
            return claims;
        } catch (java.text.ParseException e) {
            throw new RuntimeException("Unable to parse id token", e);
        }

    }
}
