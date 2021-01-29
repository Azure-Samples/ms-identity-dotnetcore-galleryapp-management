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
    extern alias BetaLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Beta = BetaLib.Microsoft.Graph;

    public class GalleryAppsProcessor
    {
        private readonly GalleryAppsRepository _galleryAppsRepository;

        public GalleryAppsProcessor(GalleryAppsRepository galleryAppsRepository)
        {
            _galleryAppsRepository = galleryAppsRepository;
        }

        public async Task CreateGalleryAppAsync(NewGalleryAppDetails newGalleryAppDetails)
        {
            // Step 1. Create the Gallery application
            var appServicePrincipal = await _galleryAppsRepository.CreateApplicationTemplate(newGalleryAppDetails.TemplateId, newGalleryAppDetails.DisplayName);

            // Step 2. Configure single sign-on
            Thread.Sleep(10000);
            string spoId = await ConfigureSingleSignOn(appServicePrincipal, newGalleryAppDetails);

            // Step 3. Configure claims mapping
            await ConfigureClaimsMapping(spoId, newGalleryAppDetails);

            // Step 4. Configure signing certificate
            await ConfigureSigningCertificate(spoId);
        }

        private async Task<string> ConfigureSingleSignOn(Beta.ApplicationServicePrincipal galleryApp, NewGalleryAppDetails newGalleryAppDetails)
        {
            // Create a service principal resource type with the desired configuration
            var servicePrincipal = new ServicePrincipal
            {
                PreferredSingleSignOnMode = newGalleryAppDetails.PreferredSsoMode,
                LoginUrl = newGalleryAppDetails.LoginUrl
            };

            // Create the webApplication resource type with the desired configuration. Be sure to replace the redirectUris
            var web = new WebApplication
            {
                RedirectUris = new string[] { newGalleryAppDetails.RedirectUri }
            };

            // Create an application resource type with the desired configuration. Be sure to replace the IdentifierUris
            var application = new Application
            {
                Web = web,
                IdentifierUris = new string[] { newGalleryAppDetails.IdentifierUri }
            };

            string spoId = galleryApp.ServicePrincipal.AdditionalData.First(x => x.Key == "objectId").Value.ToString();
            string appoId = galleryApp.Application.AdditionalData.First(x => x.Key == "objectId").Value.ToString();

            // Send servicePrincipal and Application to configure the applicationTemplate
            await _galleryAppsRepository.ConfigureApplicationTemplate(servicePrincipal, application, spoId, appoId);
            return spoId;
        }

        private async Task ConfigureClaimsMapping(string spoId, NewGalleryAppDetails newGalleryAppDetails)
        {
            // Read and assign the claims mapping policy definition
            string policyDefinition = System.IO.File.ReadAllText(newGalleryAppDetails.ClaimsMappingPolicyPath);

            var claimsMappingPolicy = new ClaimsMappingPolicy
            {
                Definition = new List<string>()
                {
                    policyDefinition
                },
                DisplayName = "automated-mappings-policy"
            };

            // Create and assign claims mapping policy
            await _galleryAppsRepository.ConfigureClaimsMappingPolicy(claimsMappingPolicy, spoId);
        }

        private async Task ConfigureSigningCertificate(string spoId)
        {
            // Set custom signing key
            string password = Guid.NewGuid().ToString();
            string certName = "SelfSigned federation metadata";
            SelfSignedCertificate selfSignedCert = new SelfSignedCertificate(password, certName);
            Guid keyIDPrivateCert = Guid.NewGuid();

            var privateKey = new Beta.KeyCredential()
            {
                CustomKeyIdentifier = selfSignedCert.CustomKeyIdentifier,
                EndDateTime = selfSignedCert.EndDateTime,
                KeyId = keyIDPrivateCert,
                StartDateTime = selfSignedCert.StartDateTime,
                Type = "AsymmetricX509Cert",
                Usage = "Sign",
                Key = selfSignedCert.PrivateKey
            };

            var publicKey = new Beta.KeyCredential()
            {
                CustomKeyIdentifier = selfSignedCert.CustomKeyIdentifier,
                EndDateTime = selfSignedCert.EndDateTime,
                KeyId = Guid.NewGuid(),
                StartDateTime = selfSignedCert.StartDateTime,
                Type = "AsymmetricX509Cert",
                Usage = "Verify",
                Key = selfSignedCert.PublicKey
            };

            var keyCredentials = new List<Beta.KeyCredential>()
            {
                privateKey,
                publicKey
            };

            var passwordCredentials = new List<Beta.PasswordCredential>()
            {
                new Beta.PasswordCredential()
                {
                    CustomKeyIdentifier = selfSignedCert.CustomKeyIdentifier,
                    KeyId = keyIDPrivateCert,
                    EndDateTime = selfSignedCert.EndDateTime,
                    StartDateTime = selfSignedCert.StartDateTime,
                    SecretText = password
                }
            };

            var spKeyCredentials = new Beta.ServicePrincipal
            {
                KeyCredentials = keyCredentials,
                PasswordCredentials = passwordCredentials,
                PreferredTokenSigningKeyThumbprint = selfSignedCert.Thumbprint

            };

            await _galleryAppsRepository.ConfigureSelfSignedCertificate(spKeyCredentials, spoId);
        }
    }
}
