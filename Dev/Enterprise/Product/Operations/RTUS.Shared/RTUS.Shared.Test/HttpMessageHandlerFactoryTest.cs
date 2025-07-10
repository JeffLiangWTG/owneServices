using System;
using System.Net;
using System.Net.Http;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.RTUS.Shared.Testing
{
	class HttpMessageHandlerFactoryTest : TransactionedTestCase
	{
		public void TestGetHandler()
		{
			var handler = HttpMessageHandlerFactory.GetHandler();
			AssertType<HttpClientHandler>(handler);
			AssertNull(((HttpClientHandler)handler).Proxy);
		}

		public void TestGetHandler_InvalidUrl()
		{
			using (TransportRegistry.Instance.OrganisationRTUSWebProxy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var handler = HttpMessageHandlerFactory.GetHandler();
				AssertType<HttpClientHandler>(handler);
				AssertNull(((HttpClientHandler)handler).Proxy);
			}
		}

		public void TestGetHandler_ValidUrl()
		{
			using (TransportRegistry.Instance.OrganisationRTUSWebProxy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ProxyAddress"))
			{
				var handler = HttpMessageHandlerFactory.GetHandler();
				AssertType<HttpClientHandler>(handler);

				var httpClientHandler = (HttpClientHandler)handler;
				AssertEquals(true, httpClientHandler.UseProxy);
				AssertType<WebProxy>(httpClientHandler.Proxy);

				var webProxy = (WebProxy)httpClientHandler.Proxy;
				AssertEquals(new Uri("http://ProxyAddress"), webProxy.Address);
				AssertNull(webProxy.Credentials);
			}
		}

		public void TestGetHandler_WithWebProxyCredentials()
		{
			using (TransportRegistry.Instance.OrganisationRTUSWebProxyUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "John"))
			using (TransportRegistry.Instance.OrganisationRTUSWebProxyPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "1234"))
			using (TransportRegistry.Instance.OrganisationRTUSWebProxy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ProxyAddress"))
			{
				var handler = HttpMessageHandlerFactory.GetHandler();
				AssertType<HttpClientHandler>(handler);

				var httpClientHandler = (HttpClientHandler)handler;
				AssertEquals(true, httpClientHandler.UseProxy);

				var webProxy = (WebProxy)httpClientHandler.Proxy;
				AssertType<NetworkCredential>(webProxy.Credentials);
				var networkCredentials = (NetworkCredential)webProxy.Credentials;
				AssertEquals("John", networkCredentials.UserName);
				AssertEquals("", networkCredentials.Domain);
				AssertEquals("1234", networkCredentials.Password);
			}
		}

		public void TestGetHandler_WithWebProxyCredentials_WithDomain()
		{
			using (TransportRegistry.Instance.OrganisationRTUSWebProxyUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CORP\\John"))
			using (TransportRegistry.Instance.OrganisationRTUSWebProxyPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "1234"))
			using (TransportRegistry.Instance.OrganisationRTUSWebProxy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ProxyAddress"))
			{
				var handler = HttpMessageHandlerFactory.GetHandler();
				AssertType<HttpClientHandler>(handler);

				var httpClientHandler = (HttpClientHandler)handler;
				AssertEquals(true, httpClientHandler.UseProxy);

				var webProxy = (WebProxy)httpClientHandler.Proxy;
				AssertType<NetworkCredential>(webProxy.Credentials);
				var networkCredentials = (NetworkCredential)webProxy.Credentials;
				AssertEquals("John", networkCredentials.UserName);
				AssertEquals("CORP", networkCredentials.Domain);
				AssertEquals("1234", networkCredentials.Password);
			}
		}
	}
}
