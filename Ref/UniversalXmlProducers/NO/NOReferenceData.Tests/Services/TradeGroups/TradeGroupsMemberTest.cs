using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.MedlemsLand;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.Tests
{
	sealed class TradeGroupsMemberTest
	{
		[Test]
		public void TestMemberTradeGroupsList()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<MedlemLandListe>(mockHttpMessageHandler.ToHttpClient(), tradeGroupUri, EmbeddedSchemaResources.TradeGroupMedlemslandSchema);

			var countryList = xmlData.Member.GroupBy(x => new
			{
				code = x.CountryCode
			});
			foreach (var refCode in countryList)
			{
				sb.Append(refCode.Key.code + ",");
			}
			const string expectedCountryCodeList = "AD,AE,AF,AG,AI,AL,AM,AO,AQ,AR,AS,AT,AU,AW,AX,AZ,BA,BB,BD,BE,BF,BG,BH,BI,BJ,BM,BN,BO,BQ,BR,BS,BT,BV,BW,BY,BZ,CA,CC,CD,CF,CG,CH,CI,CK,CL,CM,CN,CO,CR,CU,CV,CW,CX,CY,CZ,DE,DJ,DK,DM,DO,DZ,EC,EE,EG,EH,ER,ES,ET,FI,FJ,FK,FM,FO,FR,GA,GB,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GR,GS,GT,GU,GW,GY,HK,HM,HN,HR,HT,HU,ID,IE,IL,IM,IN,IO,IQ,IR,IS,IT,JE,JM,JO,JP,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LT,LU,LV,LY,MA,MC,MD,ME,MG,MH,MK,ML,MM,MN,MO,MP,MQ,MR,MS,MT,MU,MV,MW,MX,MY,MZ,NA,NC,NE,NF,NG,NI,NL,NO,NP,NR,NU,NZ,OM,PA,PE,PF,PG,PH,PK,PL,PM,PN,PR,PS,PT,PW,PY,QA,RE,RO,RS,RU,RW,SA,SB,SC,SD,SE,SG,SH,SI,SJ,SK,SL,SM,SN,SO,SR,SS,ST,SV,SX,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TR,TT,TV,TW,TZ,UA,UG,UM,US,UY,UZ,VA,VC,VE,VG,VI,VN,VU,WF,WS,XB,XC,XK,YE,YT,ZA,ZM,ZW,";
			Assert.That(sb.ToString(), Is.EqualTo(expectedCountryCodeList), "Codelist from serialized data is different from xml-file");
		}

		[Test]
		public void TestMemberTradeGroupsNumEntries()
		{
			var (_, xmlData, _) = DownloadContent.Download<MedlemLandListe>(mockHttpMessageHandler.ToHttpClient(), tradeGroupUri, EmbeddedSchemaResources.TradeGroupMedlemslandSchema);
			Assert.That(xmlData.Member.Length, Is.EqualTo(250));
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();
			var tradeGroupUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceMedlemslandFilename;
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_medlemsland.json");
			mockHttpMessageHandler.When(tradeGroupUrl).Respond("application/json", queryResponse);
			var xmlresponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Input.medlemsland.xml");
			const string resourceJsonUrl = "https://data.toll.no/dataset/74471f11-419f-4534-8e7b-117f2c38570e/resource/a89fa5de-4596-4efc-adfe-83ca8767df3c/download/medlemsland.xml";
			mockHttpMessageHandler.When(resourceJsonUrl).Respond("application/xml", xmlresponse);
			tradeGroupUri = new Uri(tradeGroupUrl);
		}
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri tradeGroupUri;
	}
}
