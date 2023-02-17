// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

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
    public interface IMicrosoftTokenValidator : ITokenValidator
    {
    }


    public class MicrosoftTokenValidator : IMicrosoftTokenValidator
    {
        private const string userInfoEndpoint = "https://graph.microsoft.com/oidc/userinfo";

        private readonly HttpClient _client;

        public MicrosoftTokenValidator(HttpClient client)
        {
            _client = client;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            var tokenValues = OAuth1Helper.GetResponseValues(token);
            tokenValues.TryGetValue("access_token", out string accessToken);

            string authorizationHeader = $"Bearer {accessToken ?? token}";

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            var response = await _client.GetAsync(userInfoEndpoint);
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
