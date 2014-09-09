package config

import com.typesafe.config.{ConfigFactory, Config ⇒ Settings}

object Default extends Config(ConfigFactory.load)

sealed class Config(settings: Settings) {

  private def setting(s: String) = settings.getString(s"signin.$s")
  private def idpUrl(path: String) = s"${setting("idp.url")}/$path"

  val clientId = setting("client.id")
  val clientSecret = setting("client.secret")

  val authorizeUrl = idpUrl(s"oauth2/authorize?response_type=code&client_id=$clientId&scope=email")
  val tokenUrl = idpUrl("oauth2/token")
  val userInfoUrl = idpUrl("api/users/me")
  val logoutUrl = idpUrl(s"oidc/end-session?client_id=$clientId")
}