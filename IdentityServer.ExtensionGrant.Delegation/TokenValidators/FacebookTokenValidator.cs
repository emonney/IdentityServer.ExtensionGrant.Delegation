// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using IdentityServer.ExtensionGrant.Delegation.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityServer.ExtensionGrant.Delegation.TokenValidators
{
    public interface IFacebookTokenValidator : ITokenValidator
    {
    }


    public class FacebookTokenValidator : IFacebookTokenValidator
    {
        private const string userInfoEndpoint = "https://graph.facebook.com/v5.0/me?fields=id,email&access_token={token}";
        private const string tokenReplacement = "{token}";

        private readonly HttpClient _client;

        public FacebookTokenValidator(HttpClient client)
        {
            _client = client;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            var tokenValues = OAuth1Helper.GetResponseValues(token);
            tokenValues.TryGetValue("access_token", out string accessToken);
            token = accessToken ?? token;

            var endpoint = userInfoEndpoint.Replace(tokenReplacement, token);

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
