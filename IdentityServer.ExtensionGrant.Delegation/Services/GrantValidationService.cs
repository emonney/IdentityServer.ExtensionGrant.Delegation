// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.ExtensionGrant.Delegation.Services
{
    public interface IGrantValidationService
    {
        public Task<GrantValidationResult> GetValidationResultAsync(string provider, string providerUserId, string email, string password = null, IEnumerable<Claim> claims = null);
    }


    public class GrantValidationService<TUser, TKey> : IGrantValidationService
        where TUser : IdentityUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly UserManager<TUser> _userManager;

        public GrantValidationService(UserManager<TUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<GrantValidationResult> GetValidationResultAsync(string provider, string providerUserId, string email, string password = null, IEnumerable<Claim> claims = null)
        {
            if (provider == null)
                throw new ArgumentNullException(provider);

            if (providerUserId == null)
                throw new ArgumentNullException(providerUserId);

            TUser user = await _userManager.FindByLoginAsync(provider, providerUserId);

            if (user != null)
            {
                return new GrantValidationResult(user.Id.ToString(), provider, claims, provider, null);
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        return new GrantValidationResult(TokenRequestErrors.InvalidRequest, $"User with email '{email}' already exists",
                            new Dictionary<string, object> { { "email", email } });
                    }
                    else if (!await _userManager.CheckPasswordAsync(user, password))
                    {
                        if (_userManager.SupportsUserLockout)
                            await _userManager.AccessFailedAsync(user);

                        return new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid_username_or_password");
                    }
                }
                else
                {
                    user = await CreateUserAsync(email, email);
                }

                var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
                if (result.Succeeded)
                {
                    return new GrantValidationResult(user.Id.ToString(), provider, claims, provider, null);
                }
                else
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            return new GrantValidationResult(TokenRequestErrors.InvalidGrant);
        }

        /// <summary>
        /// Override this in your application and call Base.<see cref="CreateUserAsync" />. Then set the necessary properties on the returned user object.
        /// E.g. Role, IsEnabled, send verification email, etc.
        /// </summary>
        /// <param name="username">The username of the new user</param>
        /// <param name="email">The email address of the new user</param>
        /// <returns>The newly created user</returns>
        public virtual async Task<TUser> CreateUserAsync(string username, string email)
        {
            TUser user = new TUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user);

            return result.Succeeded ? user : throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
