// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer.ExtensionGrant.Delegation;
using IdentityServer.ExtensionGrant.Delegation.Models;
using IdentityServer.ExtensionGrant.Delegation.Services;
using IdentityServer.ExtensionGrant.Delegation.TokenValidators;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up extension grant in an <see cref="IIdentityServerBuilder" />.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds the extension grant <see cref="DelegationGrantValidator" />.
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TKey">The type used for the primary key for the user.</typeparam>
        /// <param name="services">The <see cref="IIdentityServerBuilder" /> to add services to.</param>
        /// <returns>The <see cref="IIdentityServerBuilder"/> so that additional calls can be chained.</returns>
        public static IIdentityServerBuilder AddDelegationGrant<TUser, TKey>(this IIdentityServerBuilder services)
            where TUser : IdentityUser<TKey>, new()
            where TKey : IEquatable<TKey>
        {
            services.AddExtensionGrantValidator<DelegationGrantValidator>();
            services.Services.AddScoped<IGrantValidationService, GrantValidationService<TUser, TKey>>();

            return services;
        }

        /// <summary>
        /// Configures Google, Facebook and Twitter token validators with the <see cref="DelegationGrantValidator" />. 
        /// </summary>
        /// <param name="services">The <see cref="IIdentityServerBuilder" /> to add services to.</param>
        /// <returns>The <see cref="IIdentityServerBuilder"/> so that additional calls can be chained.</returns>
        public static IIdentityServerBuilder AddDefaultSocialLoginValidators(this IIdentityServerBuilder services, Action<SocialLoginOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Services.Configure(configure);

            services.Services.AddHttpClient<IGoogleTokenValidator, GoogleTokenValidator>();
            services.Services.AddHttpClient<IFacebookTokenValidator, FacebookTokenValidator>();
            services.Services.AddHttpClient<ITwitterTokenValidator, TwitterTokenValidator>();
            services.Services.AddTransient<OAuth1Helper>();

            services.AddTokenValidators(options =>
            {
                options.AddValidator<IGoogleTokenValidator>(DefaultTokenProviders.Google);
                options.AddValidator<IFacebookTokenValidator>(DefaultTokenProviders.Facebook);
                options.AddValidator<ITwitterTokenValidator>(DefaultTokenProviders.Twitter);
            });

            return services;
        }

        /// <summary>
        /// Adds custom token validators to the <see cref="DelegationGrantValidator" />. 
        /// </summary>
        /// <param name="services">The <see cref="IIdentityServerBuilder" /> to add services to.</param>
        /// <param name="configure">An action delegate to configure the provided <see cref="TokenValidatorOptions"/>.</param>
        /// <returns>The <see cref="IIdentityServerBuilder"/> so that additional calls can be chained.</returns>
        public static IIdentityServerBuilder AddTokenValidators(this IIdentityServerBuilder services, Action<TokenValidatorOptions> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Services.Configure(configure);

            return services;
        }
    }
}