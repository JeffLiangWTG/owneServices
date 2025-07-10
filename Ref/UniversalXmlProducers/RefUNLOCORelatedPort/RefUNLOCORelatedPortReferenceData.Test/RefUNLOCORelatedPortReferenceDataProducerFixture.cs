using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Test
{
	[TestFixture]
	public class RefUNLOCORelatedPortReferenceDataProducerFixture
	{
		[Test]
		public void TestProduceXml()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = AppConfigurationProvider.AppConfiguration.PortMappingsApiUrl;
				mockHttp.When(HttpMethod.Get, url).Respond("application/json", PortMappingsJSON);

				using (var client = mockHttp.ToHttpClient())
				{
					AppConfigurationProvider.AppConfiguration.OutputPath = WriterOutputFilePath;
					RefUNLOCORelatedPortReferenceDataProducer.ProduceXml(client);

					mockHttp.Expect(HttpMethod.Get, url);
					Assert.IsTrue(File.Exists(WriterOutputFilePath));

					var outputXml = File.ReadAllText(WriterOutputFilePath);
					var match = Regex.Match(outputXml, @"<PublicationTime>(.*?)</PublicationTime>");
					var auCulture = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
					var today = DateTime.Today.ToString("yyyy-MM-dd", auCulture);
					Assert.IsTrue(match.Groups[1].Value.StartsWith(today));
				}
			}
		}

		string WriterOutputFilePath => GetTestFilePath("RelatedPortReferenceDataProducerOutput.xml");

		string GetTestFilePath(string fileName)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"TestFiles\{fileName}");
		}

		const string PortMappingsJSON = @"
{
	""totalItems"": 2,
	""totalPages"": 1,
	""items"": [
		{ ""relatedUnlocos"": [""AEAUH"", ""AEDXB"", ""AEJEA"", ""AEKHL""] },
		{ ""relatedUnlocos"": [""AEMSA"", ""AERKT""] }
	]
}";

		[TearDown]
		protected void TearDown()
		{
			if (File.Exists(WriterOutputFilePath))
			{
				File.Delete(WriterOutputFilePath);
			}
		}
	}
}
