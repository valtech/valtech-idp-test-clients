package state

import config.Config
import play.api.libs.json.Json
import play.api.libs.ws.WS
import play.api.mvc._
import play.api.Play.current
import scala.concurrent.ExecutionContext.Implicits.global

trait SignIn {

  val config: Config

  import config._

  def UserAction(action: User => Result) =
    Action { request => action(User(request.session))}

  def CallbackAction(code: String)(action: Session ⇒ Result) =
    Action.async { request ⇒ {

      def session(email: String) = request.session + ("email" → email)

      for {
        token ← accessToken(code)
        user ← userInfo(token)
        email = (user \ "email").as[String]
      } yield action(session(email))

    }}


  def accessToken(code: String) = {
    val data = Json.obj(
      "grant_type" → "authorization_code",
      "code" → code,
      "client_id" → clientId,
      "client_secret" → clientSecret
    )

    WS.url(tokenUrl).post(data).map(_.json \ "access_token").map(token ⇒ token.as[String])
  }

  def userInfo(accessToken: String) =
    WS.url(userInfoUrl).withHeaders("Authorization" → s"Bearer $accessToken").get().map(user ⇒ user.json)

}

case class User(session: Session) {
  def email = session.get("email")
  def signedIn = email.nonEmpty
}
