extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace daemon_core
{
    public class GalleryApps
    {
        // Graph client
        private static GraphServiceClient _graphClient;
        private static Beta.GraphServiceClient _graphBetaClient;
        /// <summary>
        /// Initialize the graph clients using an authProvider
        /// </summary>
        /// <param name="authProvider"></param>
        public GalleryApps(IAuthenticationProvider authProvider)
        {
            _graphClient = new GraphServiceClient(authProvider);
            _graphBetaClient = new Beta.GraphServiceClient(authProvider);
        }
        public async Task<JObject> GetGalleryAppsByNameAsync(string appName, IAuthenticationConfig config, Microsoft.Identity.Client.AuthenticationResult token) 
        {
            var httpClient = new HttpClient();
            var apiCaller = new ProtectedApiCallHelper(httpClient);
            JObject appTemplatesResponse = await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}beta/applicationTemplates?$search=\"{appName}\"&$filter='displayName' ne 'Custom' and categories/any()&$top=50&skip=0&$count=true", token.AccessToken);
            return appTemplatesResponse;
        }
        /// <summary>
        /// Search and retrieve the gallery applications that startWith the param "appName"
        /// </summary>
        /// <param name="appName"></param>
        /// <returns>A list with the gallery apps that matches the search</returns>
        public async Task<Beta.IGraphServiceApplicationTemplatesCollectionPage> GetGalleryAppsByNameAsync(string appName)
        {
            Beta.IGraphServiceApplicationTemplatesCollectionPage galleryApps;
            galleryApps = await _graphBetaClient.ApplicationTemplates
                    .Request()
                    .Select(m => new
                    {
                        m.DisplayName,
                        m.Id
                    })
                    .Filter($"startswith(displayName,'{appName}')")
                    .GetAsync();
            return galleryApps;
        }
        /// <summary>
        /// Create the application based on the applicationTemplate
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appDisplayName"></param>
        /// <param name="logger"></param>
        /// <returns>The application and service principal created in the ApplicationServicePrincipal resource type</returns>
        public async Task<Beta.ApplicationServicePrincipal> createApplicationTemplate(string id, string appDisplayName, ILogger logger)
        {
            var result = await _graphBetaClient.ApplicationTemplates[id]
                .Instantiate(appDisplayName)
                .Request()
                .PostAsync();

            logger.Info("applicationTemplate created with spoId =" + result.ServicePrincipal.Id + "app object Id ="+ result.Application.Id);
            return result;
        }
        /// <summary>
        /// Configure single sign-on settings for application and service principal 
        /// </summary>
        /// <param name="servicePrincipal"></param>
        /// <param name="application"></param>
        /// <param name="spId"></param>
        /// <param name="appId"></param>
        /// <param name="loger"></param>
        public async Task configureApplicationTemplate(Beta.ServicePrincipal servicePrincipal, Application application, string spId, string appId, ILogger loger)
        {
            _ = await _graphBetaClient.ServicePrincipals[spId]
                .Request()
                .UpdateAsync(servicePrincipal);
            loger.Info("servicePrincipal updated");

            _ = await _graphClient.Applications[appId]
                .Request()
                .UpdateAsync(application);

            loger.Info("Application updated");
        }
        /// <summary>
        /// Create claims mapping policy and assign it to the service principal
        /// </summary>
        /// <param name="claimsMappingPolicy"></param>
        /// <param name="logger"></param>
        /// <returns>Assigned claims mapping policy </returns>
        public async Task<ClaimsMappingPolicy> configureClaimsMappingPolicy(ClaimsMappingPolicy claimsMappingPolicy, string spoId, ILogger logger)
        {
            var result = await _graphClient.Policies.ClaimsMappingPolicies
                .Request()
                .AddAsync(claimsMappingPolicy);

            logger.Info("Claims mapping policy created. Name: " + result.DisplayName + " Id: " + result.Id);

            //var assignedPolicy = new ClaimsMappingPolicy
            //{
            //    AdditionalData = new Dictionary<string, object>()
            //        {
            //        {"@odata.id",$"https://graph.microsoft.com/beta/policies/claimsMappingPolicies/{result.Id}"}
            //        }
            //};

            //_ = await _graphClient.ServicePrincipals[spoId].ClaimsMappingPolicies
            //    .Request()
            //    .AddAsync(claimsMappingPolicy);

            return result;

        }
        
    }
}
