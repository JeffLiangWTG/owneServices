using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Hawking.eHub.Api.Controllers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog;
using Unity;
using Xunit;

namespace Hawking.eHub.Api.Tests
{
    public class XsltControllerFixtures
    {
        IUnityContainer unityContainer;
        readonly IConfiguration configuration;
        readonly Uri baseUri;

        public XsltControllerFixtures()
        {
            Unity.Config.ApplicationConfig.Initialise();

            unityContainer = Unity.DependencyFactory.Container;
            unityContainer.RegisterType<XsltController>();
            unityContainer.RegisterInstance<ILogger>(new Mock<ILogger>().Object);

            configuration = unityContainer.Resolve<IConfiguration>();
            baseUri = new Uri(configuration[$"{nameof(XsltController)}:Url"]);
        }

        [Fact]
        public async void TestTransform()
        {
            const string message = "The rabbit runs faster than the fox, because the rabbit is running for his life while the fox is only running for his dinner.";
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, message);

            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(baseUri, "Transform")
                };

                var parameters = new Dictionary<string, string>
                {
                    { "senderID", "sender1" },
                    { "recipientID", "recipient1" },
                    { "sourceMessageType", "sourceMessageType1" }
                };

                using (var stream = new FileStream(tempFile, FileMode.Open))
                {
                    var httpContent = new StreamContent(stream);
                    var response = await client.PostAsync(
                        QueryHelpers.AddQueryString(client.BaseAddress.AbsoluteUri, parameters),
                        httpContent,
                        System.Threading.CancellationToken.None);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var resultStream = await response.Content.ReadAsStreamAsync())
                        using (var streamReader = new StreamReader(resultStream))
                        {
                            var result = await streamReader.ReadToEndAsync();
                            Assert.Equal(message, result);
                        }
                    }
                    else
                    {
                        Assert.False(true, response.Content.ToString());
                    }
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}
