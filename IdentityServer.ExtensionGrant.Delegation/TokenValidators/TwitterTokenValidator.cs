// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using IdentityServer.ExtensionGrant.Delegation.Models;
using IdentityServer.ExtensionGrant.Delegation.Services;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityServer.ExtensionGrant.Delegation.TokenValidators
{
    public interface ITwitterTokenValidator : ITokenValidator
    {
    }


    public class TwitterTokenValidator : ITwitterTokenValidator
    {
        private const string userInfoEndpoint =
            "https://api.twitter.com/1.1/account/verify_credentials.json?include_email=true&include_entities=false&skip_status=true";

        private readonly HttpClient _client;
        private readonly OAuth1Helper _oauthHelper;
        private readonly SocialLoginOptions _options;


        public TwitterTokenValidator(HttpClient client, OAuth1Helper oauthHelper, IOptions<SocialLoginOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _oauthHelper = oauthHelper ?? throw new ArgumentNullException(nameof(oauthHelper));
            _options = options.Value;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            Uri endpoint = new Uri(userInfoEndpoint);
            var tokenValues = OAuth1Helper.GetResponseValues(token);

            tokenValues.TryGetValue("oauth_token", out string oauthToken);
            tokenValues.TryGetValue("oauth_token_secret", out string oauthTokenSecret);

            string authorizationHeader = _oauthHelper.GetAuthorizationHeader(
                endpoint, "GET", _options.TwitterConsumerAPIKey, _options.TwitterConsumerSecret, oauthToken, oauthTokenSecret, null);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(OAuth1Helper.GetMediaTypeHeader());
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            var response = await _client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var userInfo = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                var claims = new List<Claim>();
                claims.AddRange(userInfo.RootElement.EnumerateObject().Select(item => new Claim(item.Name, item.Value.ToString())));

                return new TokenValidationResult
                {
                    IsError = false,
                    Claims = claims
                };
            }

            return new TokenValidationResult
            {
                IsError = true,
                ErrorDescription = response.ReasonPhrase
            };
        }


        #region Not Used
        /// <summary>
        /// Not implemented. Use <see cref="ValidateAccessTokenAsync" />. 
        /// </summary>
        public Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Use <see cref="ValidateAccessTokenAsync" />. 
        /// </summary>
        public Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client = null)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
