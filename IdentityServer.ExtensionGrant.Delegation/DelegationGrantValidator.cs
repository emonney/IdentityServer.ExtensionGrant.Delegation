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
using System.Linq;
using System.Threading.Tasks;
using ExtensionGrantModels = IdentityServer.ExtensionGrant.Delegation.Models;

namespace IdentityServer.ExtensionGrant.Delegation
{
    public class DelegationGrantValidator : IExtensionGrantValidator
    {
        private readonly IServiceProvider _services;
        private readonly IGrantValidationService _validationService;
        private readonly TokenValidatorOptions _options;

        public string GrantType => ExtensionGrantModels.GrantType.Delegation;

        public DelegationGrantValidator(IServiceProvider services, IGrantValidationService validationService, IOptions<TokenValidatorOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _services = services ?? throw new ArgumentNullException(nameof(services));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _options = options.Value;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userToken = context.Request.Raw.Get("token");
            var provider = context.Request.Raw.Get("provider");
            var email = context.Request.Raw.Get("email");
            var password = context.Request.Raw.Get("password");

            if (string.IsNullOrWhiteSpace(userToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, errorDescription: "Missing token in request");
                return;
            }

            if (string.IsNullOrWhiteSpace(provider))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, errorDescription: "Missing provider in request");
                return;
            }

            var validatorType = _options.GetValidator(provider);
            if (validatorType == null || !(_services.GetService(validatorType) is ITokenValidator validator))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, errorDescription: "Unsupported provider");
                return;
            }

            try
            {
                var result = await validator.ValidateAccessTokenAsync(userToken);
                if (result.IsError)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, errorDescription: result.ErrorDescription ?? "Token not authorized");
                    return;
                }

                var providerUserId = result.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? result.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                email = result.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? result.Claims.FirstOrDefault(c => c.Type == "emailAddress")?.Value ?? email;

                context.Result = await _validationService.GetValidationResultAsync(provider, providerUserId, email, password, result.Claims);
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, ex.Message);
            }
        }
    }
}
