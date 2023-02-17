# IdentityServer.ExtensionGrant.Delegation
[![NuGet Version](https://img.shields.io/nuget/v/IdentityServer.ExtensionGrant.Delegation)](https://www.nuget.org/packages/IdentityServer.ExtensionGrant.Delegation)
[![NuGet Downloads](https://img.shields.io/nuget/dt/IdentityServer.ExtensionGrant.Delegation)](https://www.nuget.org/packages/IdentityServer.ExtensionGrant.Delegation)
[![Twitter Follow](https://img.shields.io/twitter/follow/kommand?style=social)](https://twitter.com/kommand)

An IdentityServer NuGet library that adds support for validating and exchanging tokens from 3rd party OAuth providers for IdentityServer access tokens using delegation.

This package simplifies the process of adding login support from third party providers to your applications. By simply exchanging tokens from these external providers for IdentityServer access token, the entire login flow can be handled by your application's UI (e.g. SPAs) which eliminates the need to redirect to an IdentityServer page for login.

See the [documentation](https://docs.duendesoftware.com/identityserver/v6/tokens/extension_grants).

**If you find this library useful, please take a minute to STAR it. Appreciated!**

## Installation

Package Manager
```
PM> Install-Package IdentityServer.ExtensionGrant.Delegation
```

## Setup

This package includes implementation for popular social logins such as **Google**, **Facebook**, **Twitter** and **Microsoft** and is designed to be easily extensible to support other providers (e.g. GitHub, LinkedIn, etc.) or an in-house identity provider.  
See section on [Customizing](#customizing) on how to add additional providers.

Configure in Startup:
```csharp
   services.AddIdentityServer()
           /** Other IdentityServer Configurations here **/
           .AddDelegationGrant<IdentityUser, String>() // Register the extension grant 
           .AddDefaultSocialLoginValidators(); // Add google, facebook, twitter, microsoft login support
```

Google, Facebook and Microsoft uses OAuth2 or some close variation of it. In the simplest form you can implement an implicit flow, which involves redirecting the user agent to the Provider's page and obtaining an access token from the provider. This access token is then exchanged for an IdentityServer access token using this library.  

Twitter however uses OAuth1 for login which involves a [3-step process](https://developer.twitter.com/en/docs/twitter-for-websites/log-in-with-twitter/guides/implementing-sign-in-with-twitter) to obtain an access token.  
This library provides you with the `OAuth1Helper` utility class which you can use to independently generate the necessary tokens and signature for this process.  
It also provides you with a convenience method `GetAuthorizationHeader(...)` to generate the Authorization header for your requests. Simply pass in the needed tokens depending on which stage you're on in the login process.
Once the final access token is obtained use this library to exchange it for an IdentityServer access token.  
The configuration above should be modified to include the twitter `ConsumerAPIKey` and `ConsumerSecret` if you wish to use twitter login.
```csharp
   services.AddIdentityServer()
           /** Other IdentityServer Configurations here **/
           .AddDelegationGrant<IdentityUser, String>() // Register the extension grant 
           .AddDefaultSocialLoginValidators(options => // Add google, facebook, twitter, microsoft login support
           {
               options.TwitterConsumerAPIKey = Configuration["Twitter:ConsumerAPIKey"];
               options.TwitterConsumerSecret = Configuration["Twitter:ConsumerSecret"];
           });
```

## Customizing

1. To support additional providers you provide an implementation for `ITokenValidator` and add to DI. The example below adds 3 additional custom providers to the registration

    ```csharp
               /** Other IdentityServer Configurations here **/
               .AddTokenValidators(options =>
               {
                   options.AddValidator<IMyCustomTokenValidator>("mycustom"); // Adds a custom provider
                   options.AddValidator<IGitHubValidator>("github");          // Adds a github provider
                   options.AddValidator<IAWSTokenValidator>("aws");           // Adds Amazon Web Services
               });
    ```

    See the file [GoogleTokenValidator.cs](https://github.com/emonney/IdentityServer.ExtensionGrant.Delegation/blob/master/IdentityServer.ExtensionGrant.Delegation/TokenValidators/GoogleTokenValidator.cs) for an example of an `ITokenValidator` implementation.

2. Optionally to customize how a new user is created, inherit from `GrantValidationService` and override the `CreateUserAsync` method to configure or set properties on the newly created user.  
Don't forget to register your new implementation with DI:
    ```csharp
    services.AddScoped<IGrantValidationService, CustomGrantValidationService>();
    ```

## Usage
1. The client signs in with the external OAuth provider (e.g. facebook, google, etc. using the provider's recommended approach) and obtains an id/access token.
2. With the token from the OAuth provider make a call to IdentityServer to validate and exchange the external token with an IdentityServer AccessToken with which you can make API calls.  
The call to exchange tokens could look like this:
    ```
    POST /connect/token

    grant_type = delegation 
    provider = provider_name (e.g. google) 
    token = ...
    scope = [your_scopes]
    client_id = your_client_id
    client_secret = secret_if_needed
    ```

    You can also include an email field and/or a password field used to create new users and/or to find and link existing users.  
    If an existing account with the same email is found a password (or authentication) will be required to link with that account. In this case the token exchange fails with the found email included in the response. Another request will be required with the password field included to link with the existing account.
    ```
    email = your@email.com
    password = existing_account_password
    ```

    **Note**: If an email field is not included, it is automatically retrieved using the external token. In this case the external token must have the email scope.
3. If the user exist IdentityServer returns the access token, if not a new user is automatically registered and an access token is returned for that user.

## Help and Support
*	For help post your questions in the [support forum](https://www.ebenmonney.com/forum/forum/programming-support)
*	For bug reports open a [github issue](https://github.com/emonney/IdentityServer.ExtensionGrant.Delegation/issues)

## License
 [MIT](https://github.com/emonney/IdentityServer.ExtensionGrant.Delegation/blob/master/LICENSE)