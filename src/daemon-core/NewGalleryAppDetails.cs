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
