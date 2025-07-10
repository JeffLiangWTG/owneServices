using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	// how to test this?
	// * WebApplicationFactory is not a good fit - it doesn't populate IServerAddressesFeature so you can't get an address to register
	// * Executing the session broker process seems bad - you can't control the http client and verify that it is behaving correctly
	// ....which leaves us with mocking?
	// Mocking allows fine-grained control over what the http client does, including verifying that it isn't used if no address is provided

	public class VersionBrokerRegistrationTests
	{
#pragma warning disable CA2000 // Dispose objects before losing scope - the NSubstitute method of setting expectations really confuses the analyzer
		[Test]
		public async Task TestSuccessfulRegistration()
		{
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("VersionBrokerRegistrationCallback", "http://www.example.com"),
				}).Build();

			var server = CreateMockServer("http://[::]:12345");

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
			httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

			var loggerMock = new Mock<ILogger<VersionBrokerRegistration>>();
			var vbr = new VersionBrokerRegistration(config, server, httpClientFactoryMock.Object, loggerMock.Object);
			await vbr.RegisterAsync();

			Assert.That(handler.RequestUri, Is.EqualTo("http://www.example.com/"));
			Assert.That(handler.RequestMethod, Is.EqualTo(HttpMethod.Post));
			Assert.That(handler.RequestBody, Is.EqualTo("http://localhost:12345"));
			Assert.That(handler.NumberOfCalls, Is.EqualTo(1));
		}

		[Test]
		public async Task TestNoRegistrationUrlProvided()
		{
			var config = new ConfigurationBuilder().Build();

			var server = CreateMockServer("http://[::]:12345");

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
			httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

			var loggerMock = new Mock<ILogger<VersionBrokerRegistration>>();
			var vbr = new VersionBrokerRegistration(config, server, httpClientFactoryMock.Object, loggerMock.Object);
			await vbr.RegisterAsync();

			Assert.That(handler.RequestUri, Is.Null);
			Assert.That(handler.RequestBody, Is.Null);
			Assert.That(handler.NumberOfCalls, Is.EqualTo(0));
		}

		[Test]
		public void TestFailedRegistrationThrowsException()
		{
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("VersionBrokerRegistrationCallback", "http://www.example.com"),
				}).Build();

			var server = CreateMockServer("http://[::]:12345");

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError);
			httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

			var loggerMock = new Mock<ILogger<VersionBrokerRegistration>>();
			var vbr = new VersionBrokerRegistration(config, server, httpClientFactoryMock.Object, loggerMock.Object);

			Assert.That(async () => await vbr.RegisterAsync(), Throws.InstanceOf<HttpRequestException>());
		}

		[Test]
		public void TestFailedRegistrationExitsProcess()
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					Arguments = "--VersionBrokerRegistrationCallback x",
					FileName = @"..\SessionBroker\CargoWise.Blazor.SessionBroker.exe",
				},
			};
			process.Start();
			Assert.That(process.WaitForExit(30000), "registration should fail and the process should exit");
		}

		[Test]
		public void TestEmptyRegistrationUrlIsInvalid()
		{
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("VersionBrokerRegistrationCallback", ""),
				}).Build();

			var server = CreateMockServer("http://[::]:12345");

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError);
			httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

			var loggerMock = new Mock<ILogger<VersionBrokerRegistration>>();
			var vbr = new VersionBrokerRegistration(config, server, httpClientFactoryMock.Object, loggerMock.Object);

			Assert.That(async () => await vbr.RegisterAsync(), Throws.ArgumentException);
		}

		IServer CreateMockServer(string listeningAddress)
		{
			var result = new Mock<IServer>();
			var features = new FeatureCollection();
			var addressFeature = new Mock<IServerAddressesFeature>();
			addressFeature.SetupGet(feature => feature.Addresses).Returns(new[] { listeningAddress });
			features[typeof(IServerAddressesFeature)] = addressFeature.Object;
			result.Setup(server => server.Features).Returns(features);
			return result.Object;
		}
#pragma warning restore CA2000 // Dispose objects before losing scope
	}
}
