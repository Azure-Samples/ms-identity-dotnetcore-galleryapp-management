using System;
using Xunit;
using daemon_console;
using daemon_core;
using System.Collections.Generic;
using System.Linq;

namespace daemon_test
{
    public class DaemonConsoleIntegrationTest
    {
        [Fact]
        public async void RunAsyncIntegrationTest()
        {
            // Arrange
            var logger = new MockLogger();

            // Act
            await Program.RunAsync(logger, new MockInputProvider());

            // Assert
            Assert.True(logger.Messages.Count() > 0);
            Assert.Contains(logger.Messages, m => m == "Token acquired");
        }

        class MockLogger : ILogger
        {
            public IEnumerable<string> Messages => _messages;

            private IList<string> _messages = new List<string>();

            public MockLogger()
            {
            }

            public void Error(string message)
            {                
            }

            public void Info(string message)
            {
                _messages.Add(message);
            }
        }

        class MockInputProvider : IInputProvider
        {
            private int count;

            public string ReadInput()
            {
                count++;
                return count <= 1 ? "salesforce" : "1";
            }
        }
    }
}
