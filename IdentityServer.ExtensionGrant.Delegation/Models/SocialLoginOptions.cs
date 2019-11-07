// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using IdentityServer.ExtensionGrant.Delegation.TokenValidators;
using System;
using System.Collections.Generic;

namespace IdentityServer.ExtensionGrant.Delegation.Models
{
    /// <summary>
    /// Provides programmatic configuration used by <see cref="TwitterTokenValidator"/>.
    /// </summary>
    public class SocialLoginOptions
    {
        /// <summary>
        /// Gets or sets the consumer key used to communicate with Twitter.
        /// </summary>
        public string TwitterConsumerAPIKey { get; set; }

        /// <summary>
        /// Gets or sets the consumer secret used to sign requests to Twitter.
        /// </summary>
        public string TwitterConsumerSecret { get; set; }
    }
}
