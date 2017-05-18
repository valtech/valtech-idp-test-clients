
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;


namespace CSharp.Core.MVC.Auth
{
    public class TokenProvider
    {
        private readonly TokenProviderOptions _options;

        public TokenProvider(IOptions<TokenProviderOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken(string accesstoken)
        {

            var claims = new Claim[]
            {
                new Claim("access_token", accesstoken)
            };

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials
            );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public string ReadTokenAttribute(string attribute, string token)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken res;
            try
            {
                res = handler.ReadJwtToken(token);
            }
            catch (System.ArgumentException e)
            {
                throw e;
            }

            return res.Payload[attribute].ToString();
        }
    }
}