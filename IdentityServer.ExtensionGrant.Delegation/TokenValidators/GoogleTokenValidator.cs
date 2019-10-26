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
    public interface IGoogleTokenValidator : ITokenValidator
    {
    }


    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly HttpClient _client;
        private const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo?access_token={access_token}";
        private const string accessTokenReplacement = "{access_token}";

        public GoogleTokenValidator(HttpClient client)
        {
            _client = client;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            var endpoint = userInfoEndpoint.Replace(accessTokenReplacement, token);

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
