package controllers

import config.{Default ⇒ DefaultConfig}
import play.api.mvc._
import state.SignIn

object Main extends Controller with SignIn {

  val config = DefaultConfig

  import controllers.Main.config._

  def index = UserAction { user ⇒
    val (header, text) = user.email match {
      case Some(email) ⇒ ("Welcome!", s"Signed in as $email.")
      case None ⇒ ("Not signed in", "Click the button below to sign in.")
    }

    Ok(views.html.index(user.signedIn)(header, text))
  }

  def signIn = UserAction { user ⇒
    if (user.signedIn) Redirect(routes.Main.index)
    else Redirect(authorizeUrl)
  }

  def signInCallback(code: String) = CallbackAction(code) { session ⇒
    Redirect(routes.Main.index).withSession(session)
  }

  def signOut = Action {
    Redirect(logoutUrl).withNewSession
  }

}
