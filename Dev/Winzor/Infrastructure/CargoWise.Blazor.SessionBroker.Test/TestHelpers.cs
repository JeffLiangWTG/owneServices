using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using CargoWiseNext.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Model;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public static class TestHelpers
	{
		/// <summary>
		/// Simulate a user clicking the "launch" button on the debug page - launches a CW Blazor process and adds to the YARP cluster
		/// </summary>
		/// <param name="factory">Integration test host factory</param>
		/// <returns>An HttpClient that can connect to the new BlazorApp instance, and an HttpResponseMessage that allows you to access the affinity cookie</returns>
		public static async Task<(HttpClient, HttpResponseMessage)> CreateTestNodeInCluster(WebApplicationFactory<Startup> factory)
		{
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var tokenGenerator = new DebugClientTokenGenerator(cargoWiseOptions);
			var token = tokenGenerator.GenerateClientToken();
			var client = factory.CreateClient();
			var page = await client.GetAsync($"/?{QueryParameters.ClientToken}=" + token);
			UpdatedestinationStates(factory, DestinationHealth.Healthy);
			return (client, page);
		}

		public static string GetAffinityCookie(HttpResponseMessage response, Uri baseAddress)
		{
			var parsedHeaders = response.RequestMessage.Headers.TryGetValues(HeaderNames.Cookie, out var headers);
			Assert.That(parsedHeaders, Is.True);
			var cookies = ParseCookies(headers.ToList(), baseAddress);
			var affinityCookie = cookies.GetCookies(baseAddress).Single(c => c.Name == AffinityCookieName);
			return affinityCookie.Value;
		}

		static CookieContainer ParseCookies(IList<string> setCookieHeaders, Uri requestUri)
		{
			var result = new CookieContainer();
			foreach (var cookie in CookieHeaderValue.ParseList(setCookieHeaders))
			{
				result.Add(requestUri, new Cookie(cookie.Name.Value, cookie.Value.Value));
			}

			return result;
		}

		public static void UpdatedestinationStates(WebApplicationFactory<Startup> factory, DestinationHealth health)
		{
			var proxyStateLookup = factory.Services.GetService<IProxyStateLookup>();
			var proxyState = proxyStateLookup.GetClusters().Single();
			foreach (var destination in proxyState.Destinations.Values)
			{
				destination.Health.Active = health;
			}
		}

		internal const string AffinityCookieName = ".Yarp.ReverseProxy.Affinity";
	}
}
