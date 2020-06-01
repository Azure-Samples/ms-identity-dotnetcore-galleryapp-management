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
            AuthenticationResult token = await AcquireTokenHelper.GetAcquiredToken(Logger, config);


            if (token != null)
            {
                Logger.Info("Enter the name of the application you want to create?");
                var appName = InputProvider.ReadInput();
                JObject appTemplatesResponse = await new GalleryApps().GetGalleryAppsByNameAsync(appName, config, token);
                var appId = DisplayGalleryResults(appTemplatesResponse);
            }
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static string DisplayGalleryResults(JObject response)
        {  
            JEnumerable<JToken> appSearchResults = response["value"].Children();
            IList<GalleryApp> searchResults = new List<GalleryApp>();
            foreach (JToken appSearchResult in appSearchResults)
            {
                GalleryApp galleryApp = appSearchResult.ToObject<GalleryApp>();
                searchResults.Add(galleryApp);
            }

            Logger.Info("Enter the id of the application you want to create");
            Logger.Info("id | appId | appName ");
            for (int i = 0; i < searchResults.Count; i++)
            {
                Logger.Info(i + " - " + searchResults[i].toString());                
            }
            string searchResultId = InputProvider.ReadInput();
            return searchResults[int.Parse(searchResultId)].id;
        }
    }
}
