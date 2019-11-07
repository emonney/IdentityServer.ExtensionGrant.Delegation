// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/libraries
// =============================

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace IdentityServer.ExtensionGrant.Delegation.Services
{
    /// <summary>
    /// A helper class to generate tokens for OAuth1 requests.
    /// </summary>
    public class OAuth1Helper
    {
        private string _nonce;
        private string _timeStamp;

        /// <summary>
        /// OAuth version 1
        /// </summary>
        public const string OAuthVersion1 = "1.0";

        /// <summary>
        /// The signature method used to sign the request. HMAC-SHA1.
        /// </summary>
        public const string OAuthSignatureMethod = "HMAC-SHA1";

        /// <summary>
        /// Generates the nounce value unique to the request.
        /// </summary>
        /// <returns></returns>
        public string GetNonce()
        {
            if (_nonce == null)
                _nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.UtcNow.Ticks.ToString()));

            return _nonce;
        }

        /// <summary>
        /// Generates the timestamp for when the request was created.
        /// </summary>
        /// <returns></returns>
        public string GetTimeStamp()
        {
            if (_timeStamp == null)
            {
                TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                _timeStamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            }

            return _timeStamp;
        }

        /// <summary>
        /// Gets a new media type header for OAuth1 requests.
        /// </summary>
        /// <returns>An 'application/x-www-form-urlencoded' request header.</returns>
        public static MediaTypeWithQualityHeaderValue GetMediaTypeHeader()
        {
            return new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
        }

        /// <summary>
        /// Splits a response string into a Dictionary of name/value pairs.
        /// </summary>
        /// <param name="responseString">The response string containing tokens such as accessTokens, oauthTokens, oauthTokenSecrets, etc.</param>
        /// <returns>A dictionary of name/value pairs of the data in the response string.</returns>
        public static Dictionary<string, string> GetResponseValues(string responseString)
        {
            if (string.IsNullOrWhiteSpace(responseString))
                throw new ArgumentException("Content cannot be null or empty", nameof(responseString));

            if (!responseString.Contains('='))
                return new Dictionary<string, string> { { "", responseString } };

            return responseString.Split('&').Select(x => x.Split('=')).ToDictionary(y => y[0], y => y[1]);
        }

        /// <summary>
        /// Generates an OAuth 1.0a HMAC-SHA1 signature for an HTTP request.
        /// </summary>
        /// <param name="requestUrl">The full URL to which the requst is directed. Required.</param>
        /// <param name="httpMethod">The request method. E.g. GET or POST. Required.</param>
        /// <param name="consumerKey">The client key/id that identifies the application making the request. Required.</param>
        /// <param name="consumerSecret">The client secret/password that identifies the application making the request.</param>
        /// <param name="oauthToken">The token value/id used to identify the account/resource owner the request is acting on.</param>
        /// <param name="oauthTokenSecret">The token secret/password used to identify the account/resource owner the request is acting on.</param>
        /// <param name="oauthCallback">The url the user is directed to after a successful authorisation.</param>
        /// <returns></returns>
        public string GetSignature(
            Uri requestUrl,
            string httpMethod,
            string consumerKey,
            string consumerSecret,
            string oauthToken,
            string oauthTokenSecret,
            string oauthCallback)
        {
            if (requestUrl is null)
                throw new ArgumentNullException(nameof(requestUrl));

            if (httpMethod is null)
                throw new ArgumentNullException(nameof(httpMethod));

            if (consumerKey is null)
                throw new ArgumentNullException(nameof(consumerKey));

            SortedDictionary<string, string> oauthParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_signature_method", OAuthSignatureMethod },
                { "oauth_version", OAuthVersion1 },
                { "oauth_nonce", GetNonce() },
                { "oauth_timestamp", GetTimeStamp() }
            };

            NameValueCollection queryParams = HttpUtility.ParseQueryString(requestUrl.Query);
            foreach (string param in queryParams)
            {
                oauthParams.Add(param, queryParams.Get(param));
            }

            if (oauthToken != null)
                oauthParams.Add("oauth_token", oauthToken);

            if (oauthCallback != null)
                oauthParams.Add("oauth_callback", oauthCallback);

            string baseString = $"{httpMethod.ToUpperInvariant()}&{Uri.EscapeDataString(requestUrl.GetLeftPart(UriPartial.Path))}&";

            foreach (KeyValuePair<string, string> param in oauthParams)
            {
                baseString += Uri.EscapeDataString($"{param.Key}={param.Value}&");
            }

            baseString = baseString.Substring(0, baseString.Length - 3);

            string signingKey = Uri.EscapeDataString(consumerSecret) + "&";

            if (oauthTokenSecret != null)
                signingKey += Uri.EscapeDataString(oauthTokenSecret);

            string signature;
            using (HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey)))
            {
                signature = Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(baseString)));
            }

            return signature;
        }

        /// <summary>
        /// Generates an authorization header for the given request parameters.
        /// </summary>
        /// <param name="requestUrl">The full URL to which the requst is directed. Required.</param>
        /// <param name="httpMethod">The request method. E.g. GET or POST. Required.</param>
        /// <param name="consumerKey">The client key/id that identifies the application making the request. Required.</param>
        /// <param name="consumerSecret">The client secret/password that identifies the application making the request.</param>
        /// <param name="oauthToken">The token value/id used to identify the account/resource owner the request is acting on.</param>
        /// <param name="oauthTokenSecret">The token secret/password used to identify the account/resource owner the request is acting on.</param>
        /// <param name="oauthCallback">The url the user is directed to after a successful authorisation.</param>
        /// <returns></returns>
        public string GetAuthorizationHeader(
            Uri requestUrl,
            string httpMethod,
            string consumerKey,
            string consumerSecret,
            string oauthToken,
            string oauthTokenSecret,
            string oauthCallback)
        {
            string signature = GetSignature(requestUrl, httpMethod, consumerKey, consumerSecret, oauthToken, oauthTokenSecret, oauthCallback);

            return GetAuthorizationHeader(signature, consumerKey, oauthToken, oauthCallback);
        }

        /// <summary>
        /// Generates an authorization header for the given request parameters.
        /// </summary>
        /// <param name="signature">The OAuth signature for the request. Required.</param>
        /// <param name="consumerKey">The client key/id that identifies the application making the request. Required.</param>
        /// <param name="oauthToken">The token value/id used to identify the account/resource owner the request is acting on.</param>
        /// <param name="oauthCallback">The url the user is directed to after a successful authorisation.</param>
        /// <returns></returns>
        public string GetAuthorizationHeader(
            string signature,
            string consumerKey,
            string oauthToken,
            string oauthCallback
            )
        {
            if (consumerKey is null)
                throw new ArgumentNullException(nameof(consumerKey));

            if (signature is null)
                throw new ArgumentNullException(nameof(signature));

            string authorizationHeader = "OAuth ";
            authorizationHeader += $"oauth_consumer_key=\"{Uri.EscapeDataString(consumerKey)}\",";
            authorizationHeader += $"oauth_nonce=\"{Uri.EscapeDataString(GetNonce())}\",";
            authorizationHeader += $"oauth_signature=\"{Uri.EscapeDataString(signature)}\",";
            authorizationHeader += $"oauth_signature_method=\"{Uri.EscapeDataString(OAuthSignatureMethod)}\",";
            authorizationHeader += $"oauth_timestamp=\"{Uri.EscapeDataString(GetTimeStamp())}\",";
            authorizationHeader += $"oauth_version=\"{Uri.EscapeDataString(OAuthVersion1)}\"";

            if (oauthToken != null)
                authorizationHeader += $",oauth_token=\"{Uri.EscapeDataString(oauthToken)}\"";

            if (oauthCallback != null)
                authorizationHeader += $",oauth_callback=\"{oauthCallback}\""; //Don't UriEscape else twitter request fails

            return authorizationHeader;
        }
    }
}
