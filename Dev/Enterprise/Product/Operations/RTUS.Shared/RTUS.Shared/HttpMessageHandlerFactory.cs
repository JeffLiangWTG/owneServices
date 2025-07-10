using System;
using System.Net;
using System.Net.Http;
using CargoWise.Application;
using Enterprise.TransportCommon.Integration;

namespace Enterprise.RTUS.Shared
{
	public static class HttpMessageHandlerFactory
	{
		public static HttpMessageHandler GetHandler()
		{
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			var proxyAddress = transportRegistry.OrganisationRTUSWebProxy.Value;
			return !string.IsNullOrEmpty(proxyAddress) && Uri.IsWellFormedUriString(proxyAddress, UriKind.Absolute)
				? new HttpClientHandler { UseProxy = true, Proxy = GetProxy(transportRegistry, proxyAddress) }
				: new HttpClientHandler();
		}

		static IWebProxy GetProxy(ITransportRegistry transportRegistry, string proxyAddress)
		{
			var proxyCredentials = transportRegistry.OrganisationRTUSWebProxyCredentials;
			return string.IsNullOrEmpty(proxyCredentials.UserName)
				? new WebProxy(proxyAddress)
				: new WebProxy(proxyAddress) { Credentials = new NetworkCredential(proxyCredentials.UserNameWithoutServer, proxyCredentials.Password, proxyCredentials.ServerName) };
		}
	}
}
