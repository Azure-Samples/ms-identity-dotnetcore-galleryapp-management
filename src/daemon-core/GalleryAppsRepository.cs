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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Beta = BetaLib.Microsoft.Graph;

    /// <summary>
    /// Contains the core logic to create and configure Azure AD applications.
    /// </summary>
    public class GalleryAppsRepository
    {
        // Graph client
        private static GraphServiceClient _graphClient;
        private static Beta.GraphServiceClient _graphBetaClient;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the graph clients using an authProvider
        /// </summary>
        /// <param name="authProvider"></param>
        public GalleryAppsRepository(IAuthenticationProvider authProvider, ILogger logger)
        {
            _graphClient = new GraphServiceClient(authProvider);
            _graphBetaClient = new Beta.GraphServiceClient(authProvider);
            this.logger = logger;
        }
        /// <summary>
        /// Search and retrieve the gallery applications that startWith the param "appName"
        /// </summary>
        /// <param name="appName"></param>
        /// <returns>A list with the gallery apps that matches the search</returns>
        public async Task<Beta.IGraphServiceApplicationTemplatesCollectionPage> GetGalleryAppsByNameAsync(string appName)
        {
            Beta.IGraphServiceApplicationTemplatesCollectionPage galleryApps;
            var queryObjects = new List<QueryOption>
            {
                new QueryOption("search", $"\"{appName}\"")
            };

            galleryApps = await _graphBetaClient.ApplicationTemplates
                    .Request(queryObjects)
                    .Select(m => new
                    {
                        m.DisplayName,
                        m.Id
                    })
                    .Filter($"displayName ne 'Custom' and categories/any()")
                    .Top(5)
                    .Skip(0)
                    .GetAsync();
            return galleryApps;
        }
        /// <summary>
        /// Create the application based on the applicationTemplate
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appDisplayName"></param>
        /// <returns>The application and service principal created in the ApplicationServicePrincipal resource type</returns>
        public async Task<Beta.ApplicationServicePrincipal> CreateApplicationTemplate(string id, string appDisplayName)
        {
            var result = await _graphBetaClient.ApplicationTemplates[id]
                .Instantiate(appDisplayName)
                .Request()
                .PostAsync();

            string spoId = result.ServicePrincipal.AdditionalData.First(x => x.Key == "objectId").Value.ToString();
            string appoId = result.Application.AdditionalData.First(x => x.Key == "objectId").Value.ToString();

            logger.Info("applicationTemplate created with spoId =" + spoId + "app object Id =" + appoId);
            //logger.Info("applicationTemplate created with spoId =" + result.ServicePrincipal.Id + "app object Id =" + result.Application.Id);
            return result;
        }
        /// <summary>
        /// Configure single sign-on settings for application and service principal 
        /// </summary>
        /// <param name="servicePrincipal"></param>
        /// <param name="application"></param>
        /// <param name="spId"></param>
        /// <param name="appId"></param>
        public async Task ConfigureApplicationTemplate(ServicePrincipal servicePrincipal, Application application, string spId, string appId)
        {
            _ = await _graphClient.ServicePrincipals[spId]
                .Request()
                .UpdateAsync(servicePrincipal);
            logger.Info("servicePrincipal updated");

            _ = await _graphClient.Applications[appId]
                .Request()
                .UpdateAsync(application);

            logger.Info("Application updated");
        }
        /// <summary>
        /// Create claims mapping policy and assign it to the service principal
        /// </summary>
        /// <param name="claimsMappingPolicy"></param>
        /// <param name="spoId"></param>
        /// <returns>Assigned claims mapping policy </returns>
        public async Task<ClaimsMappingPolicy> ConfigureClaimsMappingPolicy(ClaimsMappingPolicy claimsMappingPolicy, string spoId)
        {
            var result = await _graphClient.Policies.ClaimsMappingPolicies
                .Request()
                .AddAsync(claimsMappingPolicy);

            logger.Info("Claims mapping policy created. Name: " + result.DisplayName + " Id: " + result.Id);

            var assignedPolicy = new ClaimsMappingPolicy
            {
                AdditionalData = new Dictionary<string, object>()
                    {
                    {"@odata.id",$"https://graph.microsoft.com/v1.0/policies/claimsMappingPolicies/{result.Id}"}
                    }
            };

            //_ = await _graphClient.ServicePrincipals[spoId].ClaimsMappingPolicies.References
            //    .Request()
            //    .AddAsync(assignedPolicy);

            return result;

        }
        /// <summary>
        /// Set the keyCredentials property in the servicePrincipal. It's expected that the servicePrincipal will have the
        /// keyCredential and the passwordCredential configured
        /// </summary>
        /// <param name="servicePrincipal"></param>
        /// <param name="spId"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task ConfigureSelfSignedCertificate(Beta.ServicePrincipal servicePrincipal, string spId)
        {
            _ = await _graphBetaClient.ServicePrincipals[spId]
               .Request()
               .UpdateAsync(servicePrincipal);
            logger.Info("servicePrincipal updated with new keyCredentials");

        }

    }
}
