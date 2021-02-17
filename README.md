---
page_type: sample
languages:
- csharp
products:
- azure
- azure-active-directory
---

# Automate SAML-based SSO app configuration using MS Graph API SDK .Net

<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

This sample demonstrates how to create and configure single sign-on for [Azure AD gallery apps](https://docs.microsoft.com/azure/active-directory/saas-apps/tutorial-list) using Microsoft Graph API SDK for .Net.

## About the sample

The sample contains a daemon-console application that prompts the users of what application to create from the Azure AD Gallery. Also, it orchestrates the actions to configure single sign-on for the application. 

The daemon-core applications contain the logic to create and configure single sign-on for an Azure AD gallery app. Using the MS Graph SDK .net, the daemon-core application can:

- Search and create an Azure AD gallery app
- Configure basic SAML properties like Identifier, Reply URL and preferredSSOMode
- Create and assign claims mapping policies to the application
- Create and assign a signing certificate to sign the Azure AD token

For more information about the MS Graph APIs used for automating this scenario, see [Automate SAML-based SSO app configuration with Microsoft Graph API](https://docs.microsoft.com/graph/application-saml-sso-configure-api).

This sample uses:

- [Microsoft Authentication Library for .NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) (MSAL.NET) to obtain an access token.
- The [Microsoft Graph Client Library for .NET](https://github.com/microsoftgraph/msgraph-sdk-dotnet) (SDK) to call Microsoft Graph.

## Prerequisites

- Visual Studio 2019 installed on your development computer.
- .NET Core 3.1 or later (for example for Windows) installed. You can follow the instructions at [.NET and C# - Get Started in 10 Minutes](https://www.microsoft.com/net/core). In addition to developing on Windows, you can develop on [Linux](https://www.microsoft.com/net/core#linuxredhat), [Mac](https://www.microsoft.com/net/core#macos), or [Docker](https://www.microsoft.com/net/core#dockercmd).
- A [work, school or personal account](https://dev.office.com/devprogram). A tenant administrator account is required to grant application permissions.
- The application ID and key from the application that you [register on the Azure Portal](#register-the-app).

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/ms-identity-dotnetcore-galleryapp-management.git
```

Go to the `"src"` folder

```Shell
cd "src"
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample with your Azure Active Directory tenant

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Azure AD tenant.
1. In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.

#### Register the client app

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
2. Select **New registration**.
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   - Select **Register** to create the application.
4. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
5. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (for instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
6. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Application permissions** section, ensure that the right permissions are checked: **Application.ReadWrite.All**, **Policy.ReadWrite.ApplicationConfiguration**
   - Select the **Add permissions** button

7. At this stage permissions are assigned correctly but the client app does not allow interaction. 
   Therefore no consent can be presented via a UI and accepted to use the service app. 
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the
   requested permissions for all account in the tenant.
   You need to be an Azure AD tenant admin to do this.

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the client project

> Note: if you used the setup scripts, the changes below will have been applied for you, with the exception of the national cloud specific steps.

1. Open the `src\daemon-console\appsettings.json` file.
1. If you are connecting to a national cloud, change the instance to the correct Azure AD endpoint. [See this reference for a list of Azure AD endpoints.](https://docs.microsoft.com/graph/deployments#app-registration-and-token-service-root-endpoints)
1. Find the app key `Tenant` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `daemon-console` application copied from the Azure portal.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `daemon-console` app, in the Azure portal.
1. If you are connecting to a national cloud, open the 'daemon-console\Program.cs' file.
1. Change the graph endpoint on lines in which there is a "graph.microsoft.com" reference. [See this reference for more info on which graph endpoint to use.](https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints)

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

Start the application, it will display the users in the tenant.

## Troubleshooting

### Did you forget to provide admin consent? This is needed for daemon apps

If you get an error when calling the API `Insufficient privileges to complete the operation.`, this is because the tenant administrator has not granted permissions
to the application. See step 6 of [Register the client app (daemon-console)](#register-the-client-app-daemon-console) above.

You will typically see, on the output window, something like the following:

```Json
Failed to call the Web Api: Forbidden
Content: {
  "error": {
    "code": "Authorization_RequestDenied",
    "message": "Insufficient privileges to complete the operation.",
    "innerError": {
      "request-id": "<a guid>",
      "date": "<date>"
    }
  }
}
```

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

If you find a bug in msal.Net, please raise the issue on [MSAL.NET GitHub Issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information about connecting all your applications with Azure AD:
- [Application management documentation](https://docs.microsoft.com/azure/active-directory/manage-apps)
- [Setup SAML-based single sign-on](https://docs.microsoft.com/en-us/azure/active-directory/manage-apps/add-application-portal-setup-sso)

For more information, see MSAL.NET's conceptual documentation:

- [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
- [Acquiring a token for an application with client credential flows](https://aka.ms/msal-net-client-credentials)

For more information about Microsoft Graph SDK:
- [Microsoft Graph SDKs overview](https://docs.microsoft.com/graph/sdks/sdks-overview)

For more information about the underlying protocol:

- [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
