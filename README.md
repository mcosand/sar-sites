# Search and Rescue Database

### Prerequisites
* Current version of Node.js and NPM
* Visual Studio 2015 Community Edition or better
* SQL Server (localdb, SQL Express, etc)

### Getting Started
1. Grab a copy of the code:
```
git clone https://github.com/mcosand/sar-sites`
cd sar-sites
git submodule update --init
```
2. Make sure you can build the project

The build script is designed to allow for automatic deployment into an Azure app service. If you don't create an `artifacts` folder in the root of the project it will drop the output to `..\artifacts` 
```
mkdir artifacts
deploy.cmd
```

3. Create a `web.local.config` at `full-database\src\website` using the template below and adding/setting keys as desired using the configuration table below.
```xml
<appSettings>
  <add key="email:from" value="Test User &lt;test@example.com>" />
</appSettings>
```
### Configuration
Configuration of the project is primarily managed through web.config AppSettings. The site will load the AppSettings from `web.config` and then merge the keys from `web.local.config`. `web.local.config` is ignored by Git to make it easy to maintain your development configuration vs. the site defaults.

Values must be properly escaped XML.

The `web.config` file in `full-database\src\website` should have reasonable defaults for a development environment.

##### Authentication
| Key | Description |
| --- | ----------- |
| auth:authority | Root URL of the OpenID Connect endpoint. Usually *https://your-hostname/auth* |
| auth:clientId | The client id for the MVC portion of the web app. Must match the value in the auth.Clients SQL table |
| auth:redirect | The URL to redirect to after logging into the MVC app. Must be listed in the auth.ClientUris table for the auth:clientId |
| auth:secret | The secret for the MVC app, must match the value in the auth.Clients table. |
| auth:spaClientId | The client id for the Angular single-page portion of the database web site. Must exist in the auth.Clients table |
| google:clientId | The Google issued client id for using Google as an external login provider |
| google:clientSecret | The key associated with the google:clientId |
| facebook:appId | The Facebook app id for using Facebook as an external login provider |
| facebook:appSecret | The secret associated with the facebook:appId |
| openid:providers | A comma delimited list of additional OpenID Connect providers to use for external logins. ex: *office365,other*. Requires openid:*key*:authority, etc. key/values below for each key |
| openid:*key*:authority | The OpenID Connect endpoint for the provider. ex: *https://login.windows.net/contoso.onmicrosoft.com* |
| openid:*key*:clientId | The client id issued by the *key* OpenId Connect provider |
| openid:*key*:clientSecret | THe secret for the client id |
| openid:*key*:caption | The text to use for the button on the login page |
| openid:*key*:fa-icon | The Font Awesome icon to use on the button on the login page. ex: *windows* |
| openid:*key*:icon-color | CSS compatible color for the icon. ex: *#008800* |
| cert:key | Passphrase for the JWT signing certificate. The certificate is stored in `cert.pfx` in the root of the website |

##### Storage
| Key | Description |
| --- | ----------- |
| dataStore | ADO.NET connection string to the database |
| authStore | ADO.NET connection string to the authentication database. May be the same as dataStore |
| encryptKey | Key used to encrypt sensitive information stored in the database |

##### Branding
| Key | Description |
| --- | ----------- |
| site:groupName | Full group name. ex: *King County Search and Rescue* |
| site:shortName | Shorter group name. ex: *King County SAR* |
| site:groupAcronym | Shortest name of the group. ex: KCSAR |



##### Sending Email
| Key | Description |
| --- | ----------- |
| email:server | The server to use to send email |
| email:port | The email server port |
| email:username | If not specified, database will try to send mail to the specified server unauthenticated without using SSL. If set (with email:password), will attempt using SSL |
| email:password | Use with email:username |
| email:dropPath | If email:server or email:port are not set, emails will not be sent via SMTP - they will be written this folder. Value is relative to the root of the web site |
| email:from | The From: header of outgoing emails. ex: Test User &amp;lt;test@example.com> |

##### Diagnostics / Analytics
| Key | Description |
| --- | ----------- |
| GoogleAnalytics | The API key for connecting to Google Analytics |
| applicationInsightsKey | The API key for connecting to Azure Application Insights |
