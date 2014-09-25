
require 'sinatra'
require 'json'
require 'httpclient'
require 'haml'

set :port, 55066
enable :sessions

IDP_URL = 'https://stage-id.valtech.com'
IDP_CLIENT_ID = 'valtech.idp.testclient.local'
IDP_CLIENT_SECRET = ENV['CLIENT_SECRET']

get '/' do
  locals = {
    signed_in: session[:signed_in],
    header: 'Not signed in',
    text: 'Click the button below to sign in.'
  }
  if session[:signed_in] then
    locals[:header] = 'Welcome!'
    locals[:text] = 'Signed in as ' + session[:email] + '.'
  end
  haml :index, locals: locals
end

get '/sign-in' do
  if session[:signed_in] then
    redirect to('/')
  else
    session[:oauth_state] = SecureRandom.uuid
    url = "#{IDP_URL}/oauth2/authorize?response_type=code&client_id=#{IDP_CLIENT_ID}&scope=email&state=#{session[:oauth_state]}"
    redirect to(url)
  end
end

get '/sign-in/callback' do
  code = params[:code]
  state = params[:state]
  if code.to_s.empty? or state.to_s.empty? then
    status 400
    return 'Invalid or missing code or state.'
  end

  if state != session[:oauth_state] then
    status 400
    return 'Possible CSRF detected.'
  end

  session.delete(:oauth_state)

  # read more: http://bibwild.wordpress.com/2012/04/30/httpclient-is-a-nice-http-client-forin-ruby/
  http = HTTPClient.new

  access_token = exchange_code_for_token(http, code)
  user_info = fetch_user_info(http, access_token)

  session[:signed_in] = true
  session[:email] = user_info['email']

  redirect to('/')
end

get '/sign-out' do
  if session[:signed_in] then
    session.clear
    redirect to("#{IDP_URL}/oidc/end-session?client_id=#{IDP_CLIENT_ID}")
  else
    redirect to('/')
  end
end

def exchange_code_for_token(http, code)
  token_data = {
    grant_type: 'authorization_code',
    code: code,
    client_id: IDP_CLIENT_ID,
    client_secret: IDP_CLIENT_SECRET
  }
  res = http.post "#{IDP_URL}/oauth2/token", token_data

  if res.status_code != 200 then raise Exception, res.body end

  data = JSON.parse(res.body)

  data['access_token']
end

def fetch_user_info(http, access_token)
  res = http.get("#{IDP_URL}/api/users/me", nil, 'Authorization' => "Bearer #{access_token}")

  if res.status_code != 200 then raise Exception, res.body end

  JSON.parse(res.body)
end
