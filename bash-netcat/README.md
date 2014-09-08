# Bash and netcat example app

This is an example client written in bash and netcat.


## Prerequisites

This requires a modern bash and netcat. The preinstalled netcat on Mac OS X is not good enough, so:

```
brew install netcat
```

## Run

 1. Fetch client secret from https://stage-id-admin.valtech.com/#/clients/valtech.idp.testclient.local/edit.
 2. `CLIENT_SECRET=<fetched_client_secret> ./start.sh`
 3. Go to http://localhost:55066
