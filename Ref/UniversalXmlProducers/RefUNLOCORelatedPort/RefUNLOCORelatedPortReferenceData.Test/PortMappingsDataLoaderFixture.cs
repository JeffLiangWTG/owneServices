using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Business;
using CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Test
{
	[TestFixture]
	public class PortMappingsDataLoaderFixture
	{
		[Test]
		public void TestSuccessfulResponse()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = AppConfigurationProvider.AppConfiguration.PortMappingsApiUrl;
				mockHttp.When(HttpMethod.Get, url).Respond("application/json", NormalJSON);

				using (var client = mockHttp.ToHttpClient())
				{
					var result = PortMappingsDataLoader.GetPortMappingsAsync(client, url).Result;
					Assert.AreEqual(2, result.Count);
					Assert.AreEqual(new[] { "AEMSA", "AERKT" }, result[1].relatedUnlocos);
				}
			}
		}

		[Test]
		public void TestUnsuccessfulStatusCode()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = AppConfigurationProvider.AppConfiguration.PortMappingsApiUrl;
				mockHttp.When(HttpMethod.Get, url).Respond(HttpStatusCode.Unauthorized, "application/json", NormalJSON);

				using (var client = mockHttp.ToHttpClient())
				{
					var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await PortMappingsDataLoader.GetPortMappingsAsync(client, url));
				}
			}
		}

		[Test]
		public void TestFailedDeserialisation()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = AppConfigurationProvider.AppConfiguration.PortMappingsApiUrl;
				mockHttp.When(HttpMethod.Get, url).Respond("application/json", MissingItemsJSON);

				using (var client = mockHttp.ToHttpClient())
				{
					var exception = Assert.ThrowsAsync<InvalidDataException>(async () => await PortMappingsDataLoader.GetPortMappingsAsync(client, url));
					Assert.AreEqual("Failed to deserialise port mappings: no \"items\" property.", exception.Message);
				}
			}
		}

		[Test]
		public void TestUNLOCOBlacklist()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = AppConfigurationProvider.AppConfiguration.PortMappingsApiUrl;
				mockHttp.When(HttpMethod.Get, url).Respond("application/json", BlacklistedUnlocosJSON);

				using (var client = mockHttp.ToHttpClient())
				{
					var result = PortMappingsDataLoader.GetPortMappingsAsync(client, url).Result;
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual(new[] { "AUSYD", "NZAKL" }, result[0].relatedUnlocos);
				}
			}
		}

		#region JSON

		const string NormalJSON = @"
{
	""totalItems"": 2,
	""totalPages"": 1,
	""items"": [
		{ ""relatedUnlocos"": [""AEAUH"", ""AEDXB"", ""AEJEA"", ""AEKHL""] },
		{ ""relatedUnlocos"": [""AEMSA"", ""AERKT""] }
	]
}";

		const string MissingItemsJSON = @"
{
	""totalItems"": 1,
	""totalPages"": 1,
	""weirdItems"": [ { ""relatedUnlocos"": [""AEMSA"", ""AERKT""] } ]
}";

		const string BlacklistedUnlocosJSON = @"
{
	""totalItems"": 2,
	""totalPages"": 1,
	""items"": [
		{ ""relatedUnlocos"": [""NZAN3"", ""CNFCG""] },
		{ ""relatedUnlocos"": [""NZAN3"", ""AERKT""] },
		{ ""relatedUnlocos"": [""CNFCG"", ""AERKT""] },
		{ ""relatedUnlocos"": [""NZAN3"", ""CNFCG"", ""AUSYD"", ""NZAKL""] },
	]
}";

		#endregion
	}
}
