// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.ExtensionGrant.Delegation
{
    public interface IGrantValidationService
    {
        public Task<GrantValidationResult> GetValidationResultAsync(string providerUserId, string providerEmail, string email, string provider);
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

        public async Task<GrantValidationResult> GetValidationResultAsync(string providerUserId, string providerEmail, string email, string provider)
        {
            if (provider == null)
                throw new ArgumentNullException(provider);

            TUser user = null;

            if (!string.IsNullOrWhiteSpace(providerUserId))
                user = await _userManager.FindByLoginAsync(provider, providerUserId);

            if (user == null && !string.IsNullOrWhiteSpace(providerEmail))
                user = await _userManager.FindByEmailAsync(providerEmail);

            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                return new GrantValidationResult(user.Id.ToString(), provider, claims, provider, null);
            }
            else
            {
                var newUserEmail = email;

                if (string.IsNullOrWhiteSpace(newUserEmail))
                    newUserEmail = providerEmail;

                if (!string.IsNullOrWhiteSpace(newUserEmail))
                {
                    user = await CreateUserAsync(email, email);

                    var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));

                    if (result.Succeeded)
                    {
                        var claims = await _userManager.GetClaimsAsync(user);
                        return new GrantValidationResult(user.Id.ToString(), provider, claims, provider, null);
                    }
                    else
                    {
                        throw new Exception(string.Join(", ", result.Errors));
                    }
                }
            }

            return new GrantValidationResult(TokenRequestErrors.InvalidRequest);
        }

        /// <summary>
        /// Override this in your application and call Base.<see cref="CreateUserAsync" />(). Then set the necessary properties on the returned user object.
        /// call base.CreateUserAsync then set user role, set IsEnabled to true, then send verification email
        /// </summary>
        /// <param name="username">The username of the new user</param>
        /// <param name="email">The email address of the new user</param>
        /// <returns>The newly created user</returns>
        public virtual async Task<TUser> CreateUserAsync(string username, string email)
        {
            TUser user = new TUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user);

            return result.Succeeded ? user : throw new Exception(string.Join(", ", result.Errors));
        }
    }
}
