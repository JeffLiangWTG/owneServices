using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.Tests
{
	sealed class TradeGroupsLandTests
	{
		[Test]
		public void TestLandTradeGroupsList()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<LandgruppeListe>(mockHttpMessageHandler.ToHttpClient(), tradeGroupUri, EmbeddedSchemaResources.TradeGroupLandgruppeSchema);

			var countryList = xmlData.CountryGroup.GroupBy(x => new
			{
				code = x.CountryGroupCode,
				fromdate = x.DateStart
			});
			foreach (var refCode in countryList)
			{
				sb.Append(refCode.Key.code + ",");
			}
			const string expectedCountryGroupList = "AKOR,ALD1,ALD2,ALLE,EU,RAEF,RAF,RAL,RALL,RAR,RAU,RAZ,RBA,RBD,RBEF,RBJ,RBO,RBR,RBY,RCH,RCN,RDKU,RDO,REFT,REG,RET,RFO,RGB,RGE,RGG,RGH,RGL,RGM,RGS2,RGSP,RHK,RHN,RHR,RID,RIM,RIN,RIR,RISL,RIT,RJE,RJP,RKE,RKEU,RKH,RKOR,RKR,RLB,RLD1,RLD2,RLK,RLV,RMA,RMD,RME,RMG,RMK,RMUL,RMX,RMY,RNG,RPE,RPK,RPT,RRIS,RRS,RRU,RSD,RSEU,RSL,RSN,RSNT,RSY,RTH,RTR,RUA,RUEO,RUG,RUS,RUZ,RVN,RXK,RZA,TAL,TALL,TBA,TCA,TCL,TCO,TCR,TEC,TEF,TEFT,TEG,TGB,TGCC,TGE,TGS1,TGS2,TGS7,TGS8,TGS9,TGSP,TH,THK,TI,TID,TIL,TIST,TJO,TKR,TLB,TMA,TME,TMK,TMX,TOES,TPA,TPE,TPH,TRS,TSAC,TSG,TTN,TTYR,TUA,TXI,UALL,UEF,XALL,XAVF,XFO,XGL,XHAI,XRUS,XUEO,";
			Assert.That(sb.ToString(), Is.EqualTo(expectedCountryGroupList));
		}

		[Test]
		public void TestLandTradeGroupsNumberOfEntries()
		{
			var (_, xmlData, _) = DownloadContent.Download<LandgruppeListe>(mockHttpMessageHandler.ToHttpClient(), tradeGroupUri, EmbeddedSchemaResources.TradeGroupLandgruppeSchema);
			Assert.That(xmlData.CountryGroup.Length, Is.EqualTo(140));
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();
			var tradeGroupUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceLandgruppeFilename;
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_landgruppe.json");
			mockHttpMessageHandler.When(tradeGroupUrl).Respond("application/json", queryResponse);
			const string resourceJsonUrl = "https://data.toll.no/dataset/bdbbe959-a7d6-4d3e-8cb5-e9810447feca/resource/b99f9f38-6402-4429-bd09-80b5523dd7ed/download/landgruppe.xml";
			var xmlresponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Input.landgruppe.xml");
			mockHttpMessageHandler.When(resourceJsonUrl).Respond("application/xml", xmlresponse);
			tradeGroupUri = new Uri(tradeGroupUrl);
		}
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri tradeGroupUri;
	}
}
