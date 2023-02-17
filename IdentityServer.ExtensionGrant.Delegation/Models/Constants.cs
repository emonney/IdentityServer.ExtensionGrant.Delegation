// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer.ExtensionGrant.Delegation.Models
{
    public static class GrantType
    {
        public const string Delegation = "delegation";
    }

    public static class DefaultTokenProviders
    {
        public const string Google = "google";
        public const string Facebook = "facebook";
        public const string Twitter = "twitter";
        public const string Microsoft = "microsoft";
    }
}
