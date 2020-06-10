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
        public GalleryApps(IAuthenticationProvider authProvider)
        {
            _graphClient = new GraphServiceClient(authProvider);
            _graphBetaClient = new Beta.GraphServiceClient(authProvider);
        }
        public async Task<JObject> GetGalleryAppsByNameAsync(string appName, AuthenticationConfig config, Microsoft.Identity.Client.AuthenticationResult token) 
        {
            var httpClient = new HttpClient();
            var apiCaller = new ProtectedApiCallHelper(httpClient);
            JObject appTemplatesResponse = await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}beta/applicationTemplates?$search=\"{appName}\"&$filter='displayName' ne 'Custom' and categories/any()&$top=50&skip=0&$count=true", token.AccessToken);
            return appTemplatesResponse;
        }
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
        public async Task<Beta.ApplicationServicePrincipal> createApplicationTemplate(string id, string appDisplayName, ILogger logger)
        {
            var result = await _graphBetaClient.ApplicationTemplates[id]
                .Instantiate(appDisplayName)
                .Request()
                .PostAsync();

            logger.Info("applicationTemplate Created");
            return result;
        }
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
        public async Task<ClaimsMappingPolicy> configureClaimsMappingPolicy(ClaimsMappingPolicy claimsMappingPolicy, ILogger logger)
        {
            var result = await _graphClient.Policies.ClaimsMappingPolicies
                .Request()
                .AddAsync(claimsMappingPolicy);

            logger.Info("Claims mapping policy created. Name: " + result.DisplayName + " Id: " + result.Id);          

            return result;
        }
    }
}
