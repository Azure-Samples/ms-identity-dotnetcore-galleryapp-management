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
extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using Microsoft.Graph;
using daemon_console.Authentication;
using daemon_core;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates; //Only import this if you are using certificate
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application
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
                RunAsync(new ConsoleLogger(), new ConsoleInputProvider()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);                
            }            
        }

        public static async Task RunAsync(ILogger logger, IInputProvider inputProvider)
        {
            Logger = logger;
            InputProvider = inputProvider;
            
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            var authProvider = new ClientCredentialProvider(config, Logger);
            GalleryApps coreHelper = new GalleryApps(authProvider);
            Logger.Info("Enter the name of the application you want to create?");
            var appName = InputProvider.ReadInput();
            Beta.IGraphServiceApplicationTemplatesCollectionPage appTemplatesResponse = await coreHelper.GetGalleryAppsByNameAsync(appName);
            DisplayGalleryResults(appTemplatesResponse);
            Logger.Info("Enter the id of the application you want to create");
            int selectedAppTemplateId = Convert.ToInt32(InputProvider.ReadInput());
            Beta.ApplicationServicePrincipal applicationCreated = await coreHelper.createApplicationTemplate(appTemplatesResponse[selectedAppTemplateId].Id
                , appTemplatesResponse[selectedAppTemplateId].DisplayName + " Automated", Logger);

            //Create a service principal resource type
            var servicePrincipal = new Beta.ServicePrincipal
            {         
                PreferredSingleSignOnMode = "saml"
            };
            //Create the webApplication resource type
            var web = new WebApplication
            {
                RedirectUris = new string[] {"https://signin.salesforce.com/saml"}
            };
            //Create an application resource type
            var application = new Application
            {
                Web = web,
                IdentifierUris = new string[] { "https://signin.salesforce.com/saml" }
            };


            // Send servicePrincipal and Application to configure the applicationTemplate
            await coreHelper.configureApplicationTemplate(servicePrincipal, application,
                "7b7c4134-55ce-4e0c-a24e-3877d590e4ee", "14391f45-f6db-4053-9eaa-9bf64c4fee8c", Logger);

            // Create claims mapping policy definition

            string policyDefinition = System.IO.File.ReadAllText("C:/Users/luleon/ms-identity-dotnetcore-galleryapp-management/src/daemon-console/Files/claimsMappingPolicy.txt");

            var claimsMappingPolicy = new ClaimsMappingPolicy
            {
                Definition = new List<string>()
                {
                    policyDefinition
                },
                DisplayName = "automated-salesforce"
            };


            // Create and assign claims mapping policy

            ClaimsMappingPolicy claimsMappingPolicyCreated =   await coreHelper.configureClaimsMappingPolicy(claimsMappingPolicy, Logger);
            
            //AuthenticationResult token = await AcquireTokenHelper.GetAcquiredToken(Logger, config);


            //if (token != null)
            //{
            //    Logger.Info("Enter the name of the application you want to create?");
            //    var appName = InputProvider.ReadInput();
            //    JObject appTemplatesResponse = await new GalleryApps().GetGalleryAppsByNameAsync(appName, config, token);
            //    var appId = DisplayGalleryResults(appTemplatesResponse);
            //}
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        //private static string DisplayGalleryResults(JObject response)
        //{  
        //    JEnumerable<JToken> appSearchResults = response["value"].Children();
        //    IList<GalleryApp> searchResults = new List<GalleryApp>();
        //    foreach (JToken appSearchResult in appSearchResults)
        //    {
        //        GalleryApp galleryApp = appSearchResult.ToObject<GalleryApp>();
        //        searchResults.Add(galleryApp);
        //    }

        //    Logger.Info("Enter the id of the application you want to create");
        //    Logger.Info("id | appId | appName ");
        //    for (int i = 0; i < searchResults.Count; i++)
        //    {
        //        Logger.Info(i + " - " + searchResults[i].toString());                
        //    }
        //    string searchResultId = InputProvider.ReadInput();
        //    return searchResults[int.Parse(searchResultId)].id;
        //}
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
