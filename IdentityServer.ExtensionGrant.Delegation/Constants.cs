using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer.ExtensionGrant.Delegation
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
    }
}
