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
        public async Task<JObject> GetGalleryAppsByNameAsync(string appName, AuthenticationConfig config, Microsoft.Identity.Client.AuthenticationResult token) 
        {
            var httpClient = new HttpClient();
            var apiCaller = new ProtectedApiCallHelper(httpClient);
            JObject appTemplatesResponse = await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}beta/applicationTemplates?$search=\"{appName}\"&$filter='displayName' ne 'Custom' and categories/any()&$top=50&skip=0&$count=true", token.AccessToken);
            return appTemplatesResponse;
        }
    }
}
