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

namespace daemon_test
{
    using daemon_console;
    using daemon_core;
    using daemon_core.Authentication;
    using Moq;
    using Xunit;

    public class DaemonConsoleIntegrationTest
    {
        private readonly Mock<ILogger> mockLogger;
        private readonly Mock<IInputProvider> mockInputProvider;

        public DaemonConsoleIntegrationTest()
        {
            mockLogger = new Mock<ILogger>();
            mockInputProvider = new Mock<IInputProvider>();          
        }

        [Fact]
        public async void RunAsyncIntegrationTest()
        {
            // Arrange
            ClientCredentialProvider clientCredentialProvider = new ClientCredentialProvider(
                AuthenticationConfig.ReadFromJsonFile("appsettings.json"),
                mockLogger.Object);

            GalleryAppsRepository galleryAppsRepository = new GalleryAppsRepository(clientCredentialProvider, mockLogger.Object);

            mockInputProvider
                .SetupSequence(input => input.ReadInput())
                .Returns("Salesforce")
                .Returns("1");

            // Act
            await Program.RunAsync(mockLogger.Object, mockInputProvider.Object, galleryAppsRepository);

            // Assert
            mockLogger.Verify(logger => logger.Info(It.Is<string>(msg => msg == "Token acquired")));
            mockLogger.Verify(logger => logger.Info(It.Is<string>(msg => msg == "servicePrincipal updated with new keyCredentials")));
        }
    }
}
