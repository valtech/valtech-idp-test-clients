# Scala Play Example App

This is an example client written in Scala and Play Framework.

## Prerequisites

This requires the Scala Build Tool (SBT). Use Homebrew on Mac OS X to install:

```
brew install sbt
```

## Run

 1. Fetch client secret from https://stage-id-admin.valtech.com/#/clients/valtech.idp.testclient.local/edit.
 2. `sbt "start -Dhttp.port=55066 -Dclient_secret=<fetched_client_secret>"`
 3. Go to http://localhost:55066
