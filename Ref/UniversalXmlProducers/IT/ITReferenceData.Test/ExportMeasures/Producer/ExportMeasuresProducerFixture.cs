using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class ExportMeasuresProducerFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: null, loader: loaderMock.Object, mapper: mapperMock.Object, tariffLoader: tariffLoaderMock.Object, tradeGroupLookup: tradeGroupLookupMock.Object, additionalCodeLookup: additionalCodeLookupMock.Object, publicationTimeLoader: publicationTimeLoaderMock.Object), "When logger is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: loggerMock.Object, loader: null, mapper: mapperMock.Object, tariffLoader: tariffLoaderMock.Object, tradeGroupLookup: tradeGroupLookupMock.Object, additionalCodeLookup: additionalCodeLookupMock.Object, publicationTimeLoader: publicationTimeLoaderMock.Object), "When loader is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: loggerMock.Object, loader: loaderMock.Object, mapper: null, tariffLoader: tariffLoaderMock.Object, tradeGroupLookup: tradeGroupLookupMock.Object, additionalCodeLookup: additionalCodeLookupMock.Object, publicationTimeLoader: publicationTimeLoaderMock.Object), "When mapper is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: loggerMock.Object, loader: loaderMock.Object, mapper: mapperMock.Object, tariffLoader: null, tradeGroupLookup: tradeGroupLookupMock.Object, additionalCodeLookup: additionalCodeLookupMock.Object, publicationTimeLoader: publicationTimeLoaderMock.Object), "When tariffLoader is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: loggerMock.Object, loader: loaderMock.Object, mapper: mapperMock.Object, tariffLoader: tariffLoaderMock.Object, tradeGroupLookup: null, additionalCodeLookup: additionalCodeLookupMock.Object, publicationTimeLoader: publicationTimeLoaderMock.Object), "When tradeGroupLookup is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: loggerMock.Object, loader: loaderMock.Object, mapper: mapperMock.Object, tariffLoader: tariffLoaderMock.Object, tradeGroupLookup: tradeGroupLookupMock.Object, additionalCodeLookup: null, publicationTimeLoader: publicationTimeLoaderMock.Object), "When additionalCodeLookup is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresProducer(logger: loggerMock.Object, loader: loaderMock.Object, mapper: mapperMock.Object, tariffLoader: tariffLoaderMock.Object, tradeGroupLookup: tradeGroupLookupMock.Object, additionalCodeLookup: additionalCodeLookupMock.Object, publicationTimeLoader: null), "When publicationTimeLoader is null");
		}

		[Test]
		public async Task ProduceEntities()
		{
			var producer = new ExportMeasuresProducer(loggerMock.Object, loaderMock.Object, mapper, tariffLoaderMock.Object, tradeGroupLookupMock.Object, additionalCodeLookupMock.Object, publicationTimeLoaderMock.Object);
			var entities = (await producer.ProduceEntitiesAsync()).ToArray();

			var startDate = new DateTime(2025, 2, 10);

			Assert.AreEqual(3, entities.Length);

			Assert.AreEqual("22082086", entities[0].ZZ1_TariffCode);
			Assert.AreEqual(1, entities[0].RefCusTariffAdditionalCodes.Length);
			AssertAdditionalCode(entities[0].RefCusTariffAdditionalCodes[0], "U001", "Grappa piemontese o Grappa del Piemonte IG", startDate, ["1011", "1014"]);

			Assert.AreEqual("04069063", entities[1].ZZ1_TariffCode);
			Assert.AreEqual(2, entities[1].RefCusTariffAdditionalCodes.Length);
			AssertAdditionalCode(entities[1].RefCusTariffAdditionalCodes[0], "U001", "Grappa piemontese o Grappa del Piemonte IG", startDate, ["1011"]);
			AssertAdditionalCode(entities[1].RefCusTariffAdditionalCodes[1], "U017", "U017", startDate, ["1011"]);

			Assert.AreEqual("04069084", entities[2].ZZ1_TariffCode);
			Assert.AreEqual(1, entities[2].RefCusTariffAdditionalCodes.Length);
			AssertAdditionalCode(entities[2].RefCusTariffAdditionalCodes[0], "U002", "Grappa friulana o Grappa del Friuli IG", startDate, ["1014"]);
		}

		[Test]
		public async Task ProduceEntitiesWhenTradeGroupsAreEmpty()
		{
			tradeGroupLookupMock.Setup(x => x.LoadAsync(It.IsAny<Uri>())).Returns(Task.FromResult(false));
			var producer = new ExportMeasuresProducer(loggerMock.Object, loaderMock.Object, mapper, tariffLoaderMock.Object, tradeGroupLookupMock.Object, additionalCodeLookupMock.Object, publicationTimeLoaderMock.Object);
			var entities = (await producer.ProduceEntitiesAsync()).ToArray();
			CollectionAssert.IsEmpty(entities);
			loggerMock.Verify(x => x.Log("Failed to load trade groups"));
		}

		[Test]
		public async Task ProduceEntitiesWhenAdditionalCodesAreEmpty()
		{
			additionalCodeLookupMock.Setup(x => x.LoadAsync(It.IsAny<Uri>())).Returns(Task.FromResult(false));
			var producer = new ExportMeasuresProducer(loggerMock.Object, loaderMock.Object, mapper, tariffLoaderMock.Object, tradeGroupLookupMock.Object, additionalCodeLookupMock.Object, publicationTimeLoaderMock.Object);
			var entities = (await producer.ProduceEntitiesAsync()).ToArray();
			CollectionAssert.IsEmpty(entities);
			loggerMock.Verify(x => x.Log("Failed to load additional codes"));
		}


		[Test]
		public async Task ProduceEntitiesWhenTariffsAreEmpty()
		{
			tariffLoaderMock.Setup(x => x.LoadAsync(It.IsAny<Uri>())).Returns(Task.FromResult(false));
			var producer = new ExportMeasuresProducer(loggerMock.Object, loaderMock.Object, mapper, tariffLoaderMock.Object, tradeGroupLookupMock.Object, additionalCodeLookupMock.Object, publicationTimeLoaderMock.Object);
			var entities = (await producer.ProduceEntitiesAsync()).ToArray();
			CollectionAssert.IsEmpty(entities);
			loggerMock.Verify(x => x.Log("Failed to load tariffs"));
		}

		[SetUp]
		public void SetUp()
		{
			mapper = new ExportMeasureMapper();

			loggerMock = new Mock<ILogger>();

			loaderMock = new Mock<IExportMeasuresLoader>();
			loaderMock.Setup(x => x.DoHandshakeAsync()).Returns(Task.CompletedTask);
			loaderMock.Setup(x => x.GetMeasureInformationAsync(It.Is<string>(x => x == "22082086"))).ReturnsAsync([new MeasureInformation("ERGA OMNES", "U001"), new MeasureInformation("OECD", "U001")]);
			loaderMock.Setup(x => x.GetMeasureInformationAsync(It.Is<string>(x => x == "04069063"))).ReturnsAsync([new MeasureInformation("ERGA OMNES", "U001"), new MeasureInformation("ERGA OMNES", "U017")]);
			loaderMock.Setup(x => x.GetMeasureInformationAsync(It.Is<string>(x => x == "04069084"))).ReturnsAsync([new MeasureInformation("OECD", "U002"), new MeasureInformation("SADC EPA", "U001")]);
			loaderMock.Setup(x => x.GetMeasureInformationAsync(It.Is<string>(x => x == "04069085"))).ReturnsAsync([new MeasureInformation("SADC EPA", "U001")]);
			loaderMock.Setup(x => x.GetMeasureInformationAsync(It.IsNotIn(new string[] { "22082086", "04069063", "04069084", "04069085" }))).ReturnsAsync([]);

			mapperMock = new Mock<IExportMeasureMapper>();

			tariffLoaderMock = new Mock<ICusTariffLoader>();
			tariffLoaderMock.Setup(x => x.LoadAsync(It.IsAny<Uri>())).Returns(Task.FromResult(true));
			tariffLoaderMock.SetupGet(x => x.AllCodes).Returns(["22082086", "04069063", "04069084", "04069085", "04069086"]);

			tradeGroupLookupMock = new Mock<IDataLookup>();
			tradeGroupLookupMock.Setup(x => x.LoadAsync(It.IsAny<Uri>())).Returns(Task.FromResult(true));
			tradeGroupLookupMock.Setup(x => x.Lookup(It.Is<string>(x => x == "ERGA OMNES"))).Returns("1011");
			tradeGroupLookupMock.Setup(x => x.Lookup(It.Is<string>(x => x == "OECD"))).Returns("1014");
			tradeGroupLookupMock.Setup(x => x.Lookup(It.IsNotIn(new string[] { "ERGA OMNES", "OECD" }))).Returns(null as string);

			additionalCodeLookupMock = new Mock<IDataLookup>();
			additionalCodeLookupMock.Setup(x => x.LoadAsync(It.IsAny<Uri>())).Returns(Task.FromResult(true));
			additionalCodeLookupMock.Setup(x => x.Lookup(It.Is<string>(x => x == "U001"))).Returns("Grappa piemontese o Grappa del Piemonte IG");
			additionalCodeLookupMock.Setup(x => x.Lookup(It.Is<string>(x => x == "U002"))).Returns("Grappa friulana o Grappa del Friuli IG");
			additionalCodeLookupMock.Setup(x => x.Lookup(It.IsNotIn(new string[] { "U001", "U002" }))).Returns(null as string);

			publicationTimeLoaderMock = new Mock<IPublicationTimeLoader>();
			publicationTimeLoaderMock.Setup(x => x.GetDateTime()).Returns(new DateTime(2025, 2, 10));
		}

		void AssertAdditionalCode(RefCusTariffAdditionalCode additionalCode, string code, string description, DateTime startDate, string[] tradeGroupCodes)
		{
			Assert.AreEqual(code, additionalCode.ZY2_AdditionalCode);
			Assert.AreEqual(description, additionalCode.ZY2_Description);
			Assert.AreEqual("ESM", additionalCode.ZY2_ZY3_NKCategory);
			Assert.AreEqual(tradeGroupCodes.Length, additionalCode.RefCusApplicabilities.Length);

			for (var i = 0; i < additionalCode.RefCusApplicabilities.Length; i++)
			{
				AssertApplicability(additionalCode.RefCusApplicabilities[i], startDate, tradeGroupCodes[i]);
			}
		}

		void AssertApplicability(RefCusApplicability applicability, DateTime startDate, string tradeGroupCode)
		{
			Assert.AreEqual(startDate, applicability.ZZT_StartDate);
			Assert.AreEqual(tradeGroupCode, applicability.ZZT_ZZA_NKTradeGroup);
		}

		ExportMeasureMapper mapper;
		Mock<ILogger> loggerMock;
		Mock<IExportMeasuresLoader> loaderMock;
		Mock<IExportMeasureMapper> mapperMock;
		Mock<ICusTariffLoader> tariffLoaderMock;
		Mock<IDataLookup> tradeGroupLookupMock;
		Mock<IDataLookup> additionalCodeLookupMock;
		Mock<IPublicationTimeLoader> publicationTimeLoaderMock;
	}
}
