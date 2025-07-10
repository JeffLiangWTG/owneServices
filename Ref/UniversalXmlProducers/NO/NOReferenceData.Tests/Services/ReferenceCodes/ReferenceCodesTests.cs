using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes.Tests
{
	sealed class ReferenceCodesTests
	{
		[Test]
		public void TestReferenceCodeList()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<referanserListe>(mockHttpMessageHandler.ToHttpClient(), referenceCodeUri, EmbeddedSchemaResources.ReferenceCodesSchema);

			var referenceList = xmlData.Reference.GroupBy(x => new
			{
				code = x.Code,
				fromdate = x.DateStart
			});
			foreach (var refCode in referenceList)
			{
				sb.Append(refCode.Key.code + ",");
			}
			const string expectedReferenceList = "AFB,B3,BOK,CE,CIT,DA,DEL,DOK,F1,FD,FLY,FOR,GEN,HST,I1,I2,KJT,KVT,L3,M2,MIL,MR,N2,P2,R1,S1,S3,S6,SER,TB,TXT,UND,UNI,USK,V3,V5,VEC,VT,";
			Assert.That(sb.ToString(), Is.EqualTo(expectedReferenceList));
		}

		[Test]
		public void TestReferenceCodeNumberOfEntries()
		{
			var (_, xmlData, _) = DownloadContent.Download<referanserListe>(mockHttpMessageHandler.ToHttpClient(), referenceCodeUri, EmbeddedSchemaResources.ReferenceCodesSchema);
			Assert.That(xmlData.Reference.Length, Is.EqualTo(38));
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();
			var referenceCodeUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceImportReferenceFilename;
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_innfoerselsreferanse_request.json");
			mockHttpMessageHandler.When(referenceCodeUrl).Respond("application/json", queryResponse);
			var innfoerselsreferanse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Input.innfoerselsreferanse.xml");
			const string resourceJsonUrl = "https://data.toll.no/dataset/66d203e0-c998-46bf-b702-190678c65489/resource/3b896b6a-771f-4e43-a43f-35a0846f1fe6/download/innfoerselsreferanse.xml";
			mockHttpMessageHandler.When(resourceJsonUrl).Respond("application/xml", innfoerselsreferanse);
			referenceCodeUri = new Uri(referenceCodeUrl);
		}
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri referenceCodeUri;
	}
}
