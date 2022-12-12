// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using IdentityServer.ExtensionGrant.Delegation.Models;
using IdentityServer.ExtensionGrant.Delegation.Services;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityServer.ExtensionGrant.Delegation.TokenValidators
{
    public interface IGoogleTokenValidator : ITokenValidator
    {
    }


    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo?access_token={token}";
        private const string tokenReplacement = "{token}";

        private readonly HttpClient _client;

        public GoogleTokenValidator(HttpClient client)
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
