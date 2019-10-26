using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.ExtensionGrant.Delegation
{
    public class DelegationGrantValidator : IExtensionGrantValidator
    {
        private readonly IServiceProvider _services;
        private readonly IGrantValidationService _validationService;
        private readonly TokenValidatorOptions _options;

        public string GrantType => Delegation.GrantType.Delegation;

        public DelegationGrantValidator(IServiceProvider services, IGrantValidationService validationService, IOptions<TokenValidatorOptions> options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (validationService == null)
                throw new ArgumentNullException(nameof(validationService));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _services = services;
            _validationService = validationService;
            _options = options.Value;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userToken = context.Request.Raw.Get("token");
            var provider = context.Request.Raw.Get("provider");
            var email = context.Request.Raw.Get("email");

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

            ITokenValidator validator = _services.GetService(_options.GetValidator(provider)) as ITokenValidator;
            if (validator == null)
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
                var providerEmail = result.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? result.Claims.FirstOrDefault(c => c.Type == "emailAddress")?.Value;

                context.Result = await _validationService.GetValidationResultAsync(providerUserId, providerEmail, email, provider);
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, ex.Message);
            }
        }
    }
}
