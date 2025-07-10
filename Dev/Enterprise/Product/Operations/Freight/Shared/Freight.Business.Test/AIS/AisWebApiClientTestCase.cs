using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.Freight.AIS.Testing
{
	sealed class AisWebApiClientTestCase : TransactionedTestCase
	{
		public void TestConstructor_CreatesProperlyConfiguredHttpClient()
		{
			// Arrange
			const string routeVisualizerApi = "http://api.wtg.local/v1";

			HttpMessageHandler httpMessageHandler = null;
			var timeout = TimeSpan.Zero;
			var httpClient = new HttpClient();

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			httpClientFactoryMock
				.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>()))
				.Returns(httpClient)
				.Callback<HttpMessageHandler, TimeSpan>((h, t) =>
				{
					httpMessageHandler = h;
					timeout = t;
				});
			using (ObjectFactory.Substitute(Mock.Of<IVesselMovementsUrlGenerator>()))
			using (ObjectFactory.Substitute(httpClientFactoryMock.Object))
			using (FreightDataRegistry.Instance.RouteVisualizerApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, routeVisualizerApi))
			{
				// Act
				var apiClient = new AisWebApiClient();

				// Assert
				CombineAssertions(() =>
				{
					AssertDelegatedHandlerTypes(httpMessageHandler, typeof(ResilientDelegatingHandler), typeof(AisAuthorizationDelegatingHandler));
					AssertEquals("HTTP Client Timeout must be set to 5 seconds.", TimeSpan.FromSeconds(10), timeout);
					AssertEquals("HTTP Client Base Address must be set to RouteVisualizerApiUrl Registry value.", routeVisualizerApi, httpClient.BaseAddress);
				});
			}
		}

		public void TestDispose_DisposesHttpClient()
		{
			// Arrange
			var isDisposed = false;

			var httpClientMock = new Mock<HttpClient>();
			httpClientMock.Protected()
				.Setup("Dispose", new object[] { true })
				.Callback(() => isDisposed = true);

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			httpClientFactoryMock
				.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>()))
				.Returns(httpClientMock.Object);
			using (ObjectFactory.Substitute(httpClientFactoryMock.Object))
			{
				var apiClient = new AisWebApiClient();

				// Act
				apiClient.Dispose();

				// Assert
				Assert("HTTP Client must be disposed on object disposal.", isDisposed);
			}
		}

		void AssertDelegatedHandlerTypes(HttpMessageHandler actualHttpMessageHandler, params Type[] expectedDelegatedHandlerTypes)
		{
			var actualDelegatedHandlerTypes = new List<Type>();
			while (actualHttpMessageHandler is DelegatingHandler delegatingHandler)
			{
				actualDelegatedHandlerTypes.Add(delegatingHandler.GetType());
				actualHttpMessageHandler = delegatingHandler.InnerHandler;
			}

			var message = $"Expected chain of delegated handlers is {string.Join(" --> ", expectedDelegatedHandlerTypes.AsEnumerable())}, but actual chain is {string.Join(" --> ", actualDelegatedHandlerTypes)}";
			AssertContainsExactElementsInExactOrder(message, expectedDelegatedHandlerTypes, actualDelegatedHandlerTypes);
		}
	}
}
