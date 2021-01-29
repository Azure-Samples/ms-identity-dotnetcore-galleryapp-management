/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

namespace daemon_core
{
    public class NewGalleryAppDetails
    {
        private PreferredSso _preferredSso;

        public NewGalleryAppDetails(PreferredSso preferredSso)
        {
            _preferredSso = preferredSso;
        }

        public string TemplateId { get; set; }
        public string DisplayName { get; set; }
        public string PreferredSsoMode => _preferredSso == PreferredSso.SAML ? "saml" : "saml";

        public string LoginUrl { get; set; }
        public string RedirectUri { get; set; }
        public string IdentifierUri { get; set; }
        public string ClaimsMappingPolicyPath { get; set; }

        public class Builder
        {
            private readonly string _appTemplateId;
            private string _displayName;
            private PreferredSso _preferredSso;
            private string _loginUrl;
            private string _replyUrl;
            private string _identifierUri;
            private string _claimsMappingPolicyPath;

            public Builder(string appTemplateId)
            {
                _appTemplateId = appTemplateId;
            }

            public void DisplayName(string displayName)
            {
                _displayName = displayName;
            }

            public void PreferredSsoMode(PreferredSso preferredSso)
            {
                _preferredSso = preferredSso;
            }

            public void LoginUrl(string loginUrl)
            {
                _loginUrl = loginUrl;
            }

            public void ReplyUrl(string replyUrl)
            {
                _replyUrl = replyUrl;
            }

            public void IdentifierUri(string identifierUri)
            {
                _identifierUri = identifierUri;
            }

            public void ClaimsMappingPolicyPath(string claimsMappingPolicyPath)
            {
                _claimsMappingPolicyPath = claimsMappingPolicyPath;
            }

            public NewGalleryAppDetails Build() => new NewGalleryAppDetails(_preferredSso)
            {
                TemplateId = _appTemplateId,
                DisplayName = _displayName,
                LoginUrl = _loginUrl,
                RedirectUri = _replyUrl,
                IdentifierUri = _identifierUri,
                ClaimsMappingPolicyPath = _claimsMappingPolicyPath
            };
        }
    }
}
