using IdentityServer4.Validation;
using System;
using System.Collections.Generic;

namespace IdentityServer.ExtensionGrant.Delegation
{
    /// <summary>
    /// Provides programmatic configuration used by <see cref="IExtensionGrantValidator"/>.
    /// </summary>
    public class TokenValidatorOptions
    {

        private IDictionary<string, Type> validatorMap { get; } = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Add a token validator with the given provider.
        /// </summary>
        /// <typeparam name="T">The type for the token validator.</typeparam>
        /// <param name="provider">The token provider.</param>
        public void AddValidator<T>(string provider) where T : ITokenValidator
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            validatorMap[provider] = typeof(T);
        }

        /// <summary>
        /// Returns the token validator for the specified provider, or null if a validator with the provider does not exist.
        /// </summary>
        /// <param name="provider">The provider name of the validator to return.</param>
        /// <returns>The validator type for the specified provider, or null if a validator with the provider name does not exist.</returns>
        public Type GetValidator(string provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            return validatorMap.ContainsKey(provider) ? validatorMap[provider] : null;
        }
    }
}
