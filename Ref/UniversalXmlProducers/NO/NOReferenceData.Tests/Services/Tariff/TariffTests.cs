using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tolltariffstruktur;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tests
{
	sealed class TariffTests
	{
		[Test]
		public void TestCustomsTariffStructureContent()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<CustomsTariffStructure>(mockHttpMessageHandler.ToHttpClient(), customstariffstructureUri, EmbeddedSchemaResources.CustomstariffStructureSchema);

			foreach (var chapterCode in xmlData.sections.GroupBy(x => new { x.id }))
			{
				sb.Append(chapterCode.Key.id).Append(',');
			}

			const string expected = "01,02,04,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void TestTolltariffStrukturContent()
		{
			var sb = new StringBuilder();
			var customstariffNoData = XmlHelper.ReadDeserializedManifestResourceContent<TolltariffStruktur>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.tolltariffstruktur.xml");

			foreach (var chapterCode in customstariffNoData.avsnitt.GroupBy(x => new { x.id }))
			{
				sb.Append(chapterCode.Key.id).Append(',');
			}

			const string expected = "01,02,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void TestInnfoerselsAvgiftContent()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<AvgiftListe>(mockHttpMessageHandler.ToHttpClient(), innfoerselsavgiftUri, EmbeddedSchemaResources.AvgiftsListeSchema);

			foreach (var goodsCode in xmlData.Goods.GroupBy(x => new { x.id }))
			{
				sb.Append(goodsCode.Key.id).Append(',');
			}

			const string expected = "00000011,00000033,00000044,00000055,01012100,01012902,01012908,01013000,01019000,01022100,01022900,14049000,21069060,22030040,22071019,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void TestUtfoerselsAvgiftContent()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<AvgiftListe>(mockHttpMessageHandler.ToHttpClient(), utfoerselsavgiftUri, EmbeddedSchemaResources.AvgiftsListeSchema);

			foreach (var goodsCode in xmlData.Goods.GroupBy(x => new { x.id }).Take(10))
			{
				sb.Append(goodsCode.Key.id).Append(',');
			}

			const string expected = "03011100,03011900,03019100,03019200,03019300,03019400,03019500,03019901,03019902,03019903,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void TestVarenummerContent()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<VarenummerListe>(mockHttpMessageHandler.ToHttpClient(), varenummerUri, EmbeddedSchemaResources.VarenummerSchema);

			foreach (var itemNumbers in xmlData.ItemNumber.GroupBy(x => new { x.id }).Take(10))
			{
				sb.Append(itemNumbers.Key.id).Append(',');
			}

			const string expected = "00000011,00000022,00000033,00000044,00000055,00000077,00000088,00000099,01012100,01012902,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void TestTollSatsContent()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<AvgiftListe>(mockHttpMessageHandler.ToHttpClient(), tollsatsUri, EmbeddedSchemaResources.TollsatsSchema);

			foreach (var goodsId in xmlData.Goods.GroupBy(x => new { x.id }).Take(10))
			{
				sb.Append(goodsId.Key.id).Append(',');
			}

			const string expected = "00000011,00000033,00000044,00000055,01012100,01012902,01012908,01013000,01019000,01022100,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void TestRaavareTollSatsContent()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<AvgiftListe>(mockHttpMessageHandler.ToHttpClient(), raavareTollsatsUri, EmbeddedSchemaResources.TollsatsSchema);

			foreach (var goodsId in xmlData.Goods.GroupBy(x => new { x.id }).Take(10))
			{
				sb.Append(goodsId.Key.id).Append(',');
			}

			const string expected = "04032040,17049092,17049099,18062012,18062090,18063100,18063200,18069010,18069022,18069091,";
			Assert.That(sb.ToString(), Is.EqualTo(expected));
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();

			var customstariffstructureUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceCustomstariffstructureFilename;
			var customstariffstructureResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_customstariffstructure.json");
			mockHttpMessageHandler.When(customstariffstructureUrl).Respond("application/json", customstariffstructureResponse);
			var customstariffstructure = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.customstariffstructure.xml");
			var customstariffstructureJsonUrl = "https://data.toll.no/dataset/aeb26da2-f75a-440d-8d16-44de39775e43/resource/c91c5587-5370-41c7-abd9-16063b0e8119/download/customstariffstructure.xml";
			mockHttpMessageHandler.When(customstariffstructureJsonUrl).Respond("application/xml", customstariffstructure);
			customstariffstructureUri = new Uri(customstariffstructureUrl);

			var innfoerselsavgiftUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceInnfoerselsavgiftFilename;
			var innfoerselsavgiftResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_innfoerselsavgift.json");
			mockHttpMessageHandler.When(innfoerselsavgiftUrl).Respond("application/json", innfoerselsavgiftResponse);
			var innfoerselsavgift = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.innfoerselsavgift.xml");
			var innfoerselsavgiftJsonUrl = "https://data.toll.no/dataset/3f2f8428-4c7b-4fb4-8cfc-099d000cfb18/resource/edf779dd-c5fd-4231-904e-524ede14c1c3/download/innfoerselsavgift.xml";
			mockHttpMessageHandler.When(innfoerselsavgiftJsonUrl).Respond("application/xml", innfoerselsavgift);
			innfoerselsavgiftUri = new Uri(innfoerselsavgiftUrl);

			var utfoerselsavgiftUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceUtfoerselsavgiftFilename;
			var utfoerselsavgiftResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_utfoerselsavgift.json");
			mockHttpMessageHandler.When(utfoerselsavgiftUrl).Respond("application/json", utfoerselsavgiftResponse);
			var utfoerselsavgift = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.utfoerselsavgift.xml");
			var utfoerselsavgiftJsonUrl = "https://data.toll.no/dataset/5573dbd3-b4ad-4611-b563-b3395a6f01ee/resource/64f7c54d-721d-44c2-bef0-97235f4dbf5d/download/utfoerselsavgift.xml";
			mockHttpMessageHandler.When(utfoerselsavgiftJsonUrl).Respond("application/xml", utfoerselsavgift);
			utfoerselsavgiftUri = new Uri(utfoerselsavgiftUrl);

			var varenummerUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceVarenummerFilename;
			var varenummerResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_varenummer.json");
			mockHttpMessageHandler.When(varenummerUrl).Respond("application/json", varenummerResponse);
			var varenummer = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.varenummer.xml");
			var varenummerJsonUrl = "https://data.toll.no/dataset/a1f67530-1042-45c7-acde-34f669a9a2ee/resource/d1cc4f02-fbe4-4993-bddf-aed3449b51f6/download/varenummer.xml";
			mockHttpMessageHandler.When(varenummerJsonUrl).Respond("application/xml", varenummer);
			varenummerUri = new Uri(varenummerUrl);

			var tollsatsUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceTollsatsFilename;
			var tollsatsResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_tollsats.json");
			mockHttpMessageHandler.When(tollsatsUrl).Respond("application/json", tollsatsResponse);
			var tollsats = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.tollavgiftssats.xml");
			var tollsatsJsonUrl = "https://data.toll.no/dataset/d7e08b68-8fdf-46c6-b080-b3ae249dd626/resource/9751f914-1f5a-4bbb-b925-f1298a5c2eaf/download/tollavgiftssats.xml";
			mockHttpMessageHandler.When(tollsatsJsonUrl).Respond("application/xml", tollsats);
			tollsatsUri = new Uri(tollsatsUrl);

			var raavareTollsatsUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceRaavaretollsatsFilename;
			var raavreTollsatsResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_raavaretollavgiftssats.json");
			mockHttpMessageHandler.When(raavareTollsatsUrl).Respond("application/json", raavreTollsatsResponse);
			var raavareTollsats = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.raavaretollavgiftssats.xml");
			var raavareTollsatsJsonUrl = "https://data.toll.no/dataset/99e0e7b4-a153-4da3-a722-3345191711e9/resource/83e74fd2-5c84-42ab-903b-298973689d5c/download/raavaretollavgiftssats.xml";
			mockHttpMessageHandler.When(raavareTollsatsJsonUrl).Respond("application/xml", raavareTollsats);
			raavareTollsatsUri = new Uri(raavareTollsatsUrl);
		}
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri customstariffstructureUri;
		Uri innfoerselsavgiftUri;
		Uri utfoerselsavgiftUri;
		Uri varenummerUri;
		Uri tollsatsUri;
		Uri raavareTollsatsUri;
	}
}

