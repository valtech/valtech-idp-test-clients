# Valtech IDP Test Clients

This public open source repo contains test/example OAuth 2.0 clients for integrating with Valtech IDP in different programming languages.
The Valtech IDP client secret is not commited and will need to be fetched manually from [admin interface](https://stage-id-admin.valtech.com/#/clients/valtech.idp.testclient.local/edit) before starting any of
the examples. If you do not have access to admin interface, send an email to intranet.support@valtech.se.

**The most complete test client is [python-flask](python-flask) as it uses `id_token` and has proper CSRF protection. Look at that client first!**

All available clients are:

 * [bash-netcat](bash-netcat)
 * [csharp-mvc](csharp-mvc)
 * [csharp-nancy](csharp-nancy)
 * [nodejs-express](nodejs-express)
 * [python-flask](python-flask)
 * [ruby-sinatra](ruby-sinatra)
 * [scala-play](scala-play)
 * [java-spring-boot](java-spring-boot)

For an example on how to integrate with IDP in a mobile/native app, check out the open source project [Valtech Contact Sync Android](https://github.com/valtech/valtech-contactsync-android).
