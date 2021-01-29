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
        private static readonly ILogger Logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            try
            {
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<ILogger, ConsoleLogger>()
                    .AddSingleton(AuthenticationConfig.ReadFromJsonFile("appsettings.json"))
                    .AddSingleton<IAuthenticationProvider, ClientCredentialProvider>()
                    .AddSingleton<GalleryAppsProcessor>()
                    .AddSingleton<GalleryAppsRepository>()
                    .BuildServiceProvider();

                var newGalleryAppDetails = NewGalleryAppDetails(serviceProvider.GetService<GalleryAppsRepository>()).GetAwaiter().GetResult();
                serviceProvider.GetService<GalleryAppsProcessor>().CreateGalleryAppAsync(newGalleryAppDetails).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        private static async Task<NewGalleryAppDetails> NewGalleryAppDetails(GalleryAppsRepository galleryAppsRepository)
        {
            Logger.Info("Enter the name of the application you want to create:");
            var appName = Console.ReadLine();

            // Using the appName provided, search for applications in the Gallery that matches the appName
            Beta.IGraphServiceApplicationTemplatesCollectionPage appTemplatesResponse = await galleryAppsRepository.GetByNameAsync(appName);
            DisplayGalleryResults(appTemplatesResponse);

            Logger.Info("Select the application you want to create:");
            var selectedAppTemplateId = Convert.ToInt32(Console.ReadLine());

            // Create application template with appTemplateID and appDisplayName
            var newGalleryAppDetailsBuilder = new NewGalleryAppDetails.Builder(appTemplatesResponse[selectedAppTemplateId].Id);
            newGalleryAppDetailsBuilder.DisplayName(appTemplatesResponse[selectedAppTemplateId].DisplayName + " Automated");

            newGalleryAppDetailsBuilder.PreferredSsoMode(PreferredSso.SAML);

            const string defaultLoginUrl = "https://example.com";
            Logger.Info($"Please enter the loginUrl (or press enter to use '{defaultLoginUrl}')");
            newGalleryAppDetailsBuilder.LoginUrl(ReadAnswerOrUseDefault(defaultLoginUrl));

            const string defaultReplyUrl = "https://example.com/replyurl";
            Logger.Info($"Please enter the replyUrl (or press enter to use '{defaultReplyUrl}')");
            newGalleryAppDetailsBuilder.ReplyUrl(ReadAnswerOrUseDefault(defaultReplyUrl));

            const string defaultIdentifierUri = "https://example.com/identifier2";
            Logger.Info($"Please enter the identifierUri (or press enter to use '{defaultIdentifierUri}')");
            newGalleryAppDetailsBuilder.IdentifierUri(ReadAnswerOrUseDefault(defaultIdentifierUri));

            Logger.Info("This tool will read the claims mapping policy from this location ./Files/claimsMappingPolicy.json");
            newGalleryAppDetailsBuilder.ClaimsMappingPolicyPath("./Files/claimsMappingPolicy.json");

            return newGalleryAppDetailsBuilder.Build();
        }

        private static string ReadAnswerOrUseDefault(string defaultAnswer)
        {
            var answer = Console.ReadLine();
            return string.IsNullOrEmpty(answer) ? defaultAnswer : answer;
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
