var path = require('path');
var http = require('http');
var qs = require('querystring');
var express = require('express');
var session = require('express-session');
var nunjucks = require('nunjucks');
var uuid = require('uuid');
var request = require('request');

var CLIENT_ID = 'valtech.idp.testclient.local';
var CLIENT_SECRET = process.env.CLIENT_SECRET;

var app = express();

nunjucks.configure(path.join(__dirname, 'templates'), {
  autoescape: true,
  express: app
});

app.use(session({ secret: 'agent, 007', resave: false, saveUninitialized: false }));
app.use('/static', express.static(path.join(__dirname, 'static')));

app.get('/', function(req, res) {
  var locals = {
    header: 'Not signed in',
    text: 'Click the button below to sign in.'
  };

  if (req.session.signedIn) {
    locals.header = 'Welcome!';
    locals.text = 'Signed in as ' + req.session.email + '.';
    locals.session = req.session;
  }

  res.render('index.html', locals);
});

app.get('/sign-in', function(req, res) {
  if (req.session.signedIn) return res.redirect('/');

  req.session.oauthState = uuid.v4();
  var authorizeParams = {
    response_type: 'code',
    client_id: CLIENT_ID,
    scope: 'email',
    state: req.session.oauthState,
  };

  res.redirect('https://stage-id.valtech.com/oauth2/authorize?' + qs.stringify(authorizeParams));
});

app.get('/sign-in/callback', function(req, res, next) {
  if (req.query.error) return next(new Error('OAuth error: ' + req.query.error + ', description: ' + req.query.error_description));
  if (!req.query.code || !req.query.state) return res.status(400).json({ error: 'Missing code or state.' });

  var code = req.query.code;
  var state = req.query.state;

  if (state !== req.session.oauthState) return res.status(400).json({ error: 'Invalid state, possible cross-site request forgery detected.' });
  delete req.session.oauthState;

  exchangeCodeForToken(code, function(err, accessToken) {
    if (err) return next(err);
    fetchUserInfo(accessToken, function(err, userInfo) {
      if (err) return next(err);

      req.session.signedIn = true;
      req.session.email = userInfo.email;
      res.redirect('/');
    });
  });
});

app.get('/sign-out', function(req, res) {
  req.session.destroy();
  var endSessionParams = {
    client_id: CLIENT_ID,
  };
  res.redirect('https://stage-id.valtech.com/oidc/end-session?' + qs.stringify(endSessionParams));
});

function exchangeCodeForToken(code, callback) {
  var tokenOptions = {
    url: 'https://stage-id.valtech.com/oauth2/token',
    json: true,
    body: {
      grant_type: 'authorization_code',
      code: code,
      client_id: CLIENT_ID,
      client_secret: CLIENT_SECRET
    }
  };
  request.post(tokenOptions, function(err, res, body) {
    if (err) return callback(err);
    if (res.statusCode !== 200) return callback(new Error(res.statusCode + ' from idp token endpoint, body: ' + JSON.stringify(body)));
    console.log('token response', body);

    callback(null, body.access_token);
  });
}

function fetchUserInfo(accessToken, callback) {
  var userInfoOptions = {
    url: 'https://stage-id.valtech.com/api/users/me',
    json: true,
    headers: {
      'Authorization': 'Bearer ' + accessToken
    }
  };

  request.get(userInfoOptions, function(err, res, body) {
    if (err) return callback(err);
    if (res.statusCode !== 200) return callback(new Error(res.statusCode + ' from idp UserInfo endpoint, body: ' + JSON.stringify(body)));

    console.log('user info response', body);
    callback(null, body);
  });
}

var port = 55066;
http.createServer(app).listen(55066, function() {
  console.log("Listening on port", port);
});
