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

namespace daemon_console
{
    extern alias BetaLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using daemon_core;
    using daemon_core.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Graph;
    using Beta = BetaLib.Microsoft.Graph;

    /// <summary>
    /// This sample shows how to create and configure an Azure AD Gallery app
    /// using the Microsoft Graph SDK from a daemon application
    /// which uses application permissions.
    /// For more information see https://aka.ms/msal-net-client-credentials
    /// </summary>
    public class Program
    {
        private static ILogger Logger = null;
        private static IInputProvider InputProvider = null;

        static void Main(string[] args)
        {
            try
            {
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<ILogger, ConsoleLogger>()
                    .AddSingleton<IInputProvider, ConsoleInputProvider>()
                    .AddSingleton(AuthenticationConfig.ReadFromJsonFile("appsettings.json"))
                    .AddSingleton<IAuthenticationProvider, ClientCredentialProvider>()
                    .AddSingleton<GalleryAppsRepository>()
                    .BuildServiceProvider();

                RunAsync(serviceProvider.GetService<ILogger>(), serviceProvider.GetService<IInputProvider>(), serviceProvider.GetService<GalleryAppsRepository>())
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public static async Task RunAsync(ILogger logger, IInputProvider inputProvider, GalleryAppsRepository galleryAppsRepository)
        {
            Logger = logger;
            InputProvider = inputProvider;

            // Step 1. Create the Gallery application
            Beta.ApplicationServicePrincipal galleryAppCreated = await CreateApplicationTemplate(galleryAppsRepository);
            // Step 2. Configure single sign-on
            string spoId = await ConfigureSingleSignOn(galleryAppsRepository, galleryAppCreated);
            // Step 3. Configure claims mapping
            await ConfigureClaimsMapping(galleryAppsRepository, spoId);
            // Step 4. Configure signing certificate
            await ConfigureSigningCertificate(galleryAppsRepository, spoId);
        }

        private static async Task<Beta.ApplicationServicePrincipal> CreateApplicationTemplate(GalleryAppsRepository coreHelper)
        {
            Logger.Info("Enter the name of the application you want to create?");
            var appName = InputProvider.ReadInput();

            // Using the appName provided, search for applications in the Gallery that matches the appName
            Beta.IGraphServiceApplicationTemplatesCollectionPage appTemplatesResponse = await coreHelper.GetGalleryAppsByNameAsync(appName);

            DisplayGalleryResults(appTemplatesResponse);

            Logger.Info("Enter the id of the application you want to create");
            int selectedAppTemplateId = Convert.ToInt32(InputProvider.ReadInput());

            // Create application template with appTemplateID and appDisplayName
            var appTemplateId = appTemplatesResponse[selectedAppTemplateId].Id;
            var appDisplayName = appTemplatesResponse[selectedAppTemplateId].DisplayName + " Automated";
            Beta.ApplicationServicePrincipal applicationCreated = await coreHelper.CreateApplicationTemplate(appTemplateId, appDisplayName);
            return applicationCreated;
        }

        private static async Task<string> ConfigureSingleSignOn(GalleryAppsRepository coreHelper, Beta.ApplicationServicePrincipal galleryApp)
        {
            //Create a service principal resource type with the desired configuration
            var servicePrincipal = new ServicePrincipal
            {
                PreferredSingleSignOnMode = "saml",
                LoginUrl = "https://salesforce.com"
            };
            //Create the webApplication resource type with the desired configuration
            var web = new WebApplication
            {
                RedirectUris = new string[] { "https://signin.salesforce.com/saml" }
            };
            //Create an application resource type with the desired configuration
            var application = new Application
            {
                Web = web,
                IdentifierUris = new string[] { "https://testing.sdk.com/identifier" }
            };

            string spoId = galleryApp.ServicePrincipal.AdditionalData.First(x => x.Key == "objectId").Value.ToString();
            string appoId = galleryApp.Application.AdditionalData.First(x => x.Key == "objectId").Value.ToString();
            //var spoId = "ee16cffa-2fe3-45a0-86c1-867c3dd83352";
            //var appoId = "03ea6316-3b80-41d1-b8a6-9b337f3b2491";

            //Send servicePrincipal and Application to configure the applicationTemplate
            await coreHelper.ConfigureApplicationTemplate(servicePrincipal, application, spoId, appoId);
            return spoId;
        }
        
        private static async Task ConfigureClaimsMapping(GalleryAppsRepository coreHelper, string spoId)
        {
            // Read and assign the claims mapping policy definition
            string policyDefinition = System.IO.File.ReadAllText("./Files/claimsMappingPolicy.txt");

            var claimsMappingPolicy = new ClaimsMappingPolicy
            {
                Definition = new List<string>()
                {
                    policyDefinition
                },
                DisplayName = "automated-salesforce"
            };

            // Create and assign claims mapping policy
            await coreHelper.ConfigureClaimsMappingPolicy(claimsMappingPolicy, spoId);
        }
        
        private static async Task ConfigureSigningCertificate(GalleryAppsRepository galleryAppsRepository, string spoId)
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

            List<Beta.KeyCredential> keyCredentials = new List<Beta.KeyCredential>()
            {
                privateKey,
                publicKey
            };
            List<Beta.PasswordCredential> passwordCredentials = new List<Beta.PasswordCredential>()
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

            await galleryAppsRepository.ConfigureSelfSignedCertificate(spKeyCredentials, spoId);
        }

        /// <summary>
        /// Display in the console the result of searching in the Azure AD Gallery
        /// </summary>
        /// <param name="applicationTemplates"></param>
        private static void DisplayGalleryResults(Beta.IGraphServiceApplicationTemplatesCollectionPage applicationTemplates)
        {
            var count = 0;
            Logger.Info("id | appId | appName ");
            foreach (var applicationTemplate in applicationTemplates)
            {
                Logger.Info(count + " - " + applicationTemplate.Id + " - " + applicationTemplate.DisplayName);
                count++;
            }
        }
    }
}
