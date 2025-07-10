using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class StagingDataWrapperFixture
	{
		[Test]
		public void GetValue()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_TariffCode = "001", ZZ1_ZZI_NKTariffType = "1P1" };
			var wrapper = new StagingDataWrapper(new Mock<IMetadataProvider>().Object, new Mock<IStagingDataProvider>().Object,
				overlappingCalculator, new Mock<IStagingDataWrapperFactory>().Object, tariff);
			var tariffTypePK = Guid.NewGuid();
			wrapper.SetWrapperValue("ZZ1_ZZI_TariffType", tariffTypePK);
			Assert.AreEqual("001", wrapper.GetWrapperValue(nameof(Stage.RefCusTariff.ZZ1_TariffCode)));
			Assert.AreEqual(tariffTypePK, wrapper.GetWrapperValue("ZZ1_ZZI_TariffType"));
		}

		[Test]
		public void GetRelatedEntityTypesAndFKs()
		{
			var factory = new Mock<IStagingDataWrapperFactory>();
			factory.Setup(x => x.GetExpriableKeyRelatedStagingEntityTypes(typeof(Stage.RefCusRate)))
				.Returns(new[] { typeof(Stage.RefCusApplicability) });
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetFKPropertyName(typeof(Stage.RefCusApplicability), typeof(Stage.RefCusRate)))
				.Returns(nameof(Stage.RefCusApplicability.ZZT_ZZ2_Rate));
			var wrapper = new StagingDataWrapper(new Mock<IMetadataProvider>().Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, new Stage.RefCusRate());
			var result = wrapper.GetRelatedEntityTypesAndFKs().ToArray();
			Assert.AreEqual(0, result.Length);
		}

		[Test]
		public void GetRelatedEntities()
		{
			var rate1 = new Stage.RefCusRate();
			var rate2 = new Stage.RefCusRate();
			var tariff = new Stage.RefCusTariff();
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties(nameof(Stage.RefCusTariff))).Returns(new[]
			{
				nameof(Stage.RefCusTariff.ZZ1_TariffCode), nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType),
				nameof(Stage.RefCusTariff.RefCusRates), nameof(Stage.RefCusTariff.RefCusTariffAttributes)
			});
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetRelatedEntities<Stage.RefCusTariff, Stage.RefCusRate>(tariff))
				.Returns(new[] { rate1, rate2 });
			stagingDataProvider.Setup(x => x.GetTypeFromTypeName(nameof(Stage.RefCusRate)))
				.Returns(typeof(Stage.RefCusRate));
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, tariff);
			factory.Setup(x => x.CreateWrapper(rate1)).Returns(new[] { new Mock<IStagingDataWrapper>().Object });
			factory.Setup(x => x.CreateWrapper(rate2)).Returns(new[] { new Mock<IStagingDataWrapper>().Object });
			var result = wrapper.GetRelatedEntities(nameof(Stage.RefCusRate)).ToArray();
			Assert.AreEqual(2, result.Length);
			wrapper.SetRelatedEntities(typeof(Stage.RefCusRate), new[] { new Mock<IStagingDataWrapper>().Object });
			result = wrapper.GetRelatedEntities(nameof(Stage.RefCusRate)).ToArray();
			Assert.AreEqual(1, result.Length);
		}

		[Test]
		public void GetDateTimeRange()
		{
			var dateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			var condition = new Stage.RefCusCondition { ZX1_StartDate = new DateTime(2017, 01, 01), ZX1_EndDate = new DateTime(2017, 11, 30) };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, condition);
			var result = wrapper.GetDateTimeRange();
			Assert.AreEqual(dateTimeRange, result);
		}

		[Test]
		public void GetDateTimeRange_NonExpirableType()
		{
			var codeType = new Stage.RefCusCodeType { ZZK_CodeType = "Test" };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, codeType);
			var result = wrapper.GetDateTimeRange();
			Assert.IsNull(result);
		}

		[Test]
		public void GetWrapperStartDateTime()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_TariffCode = "001", ZZ1_ZZI_NKTariffType = "1P1", ZZ1_StartDate = new DateTime(2000, 01, 01), ZZ1_EndDate = new DateTime(2010, 01, 01) };
			var wrapper = new StagingDataWrapper(new Mock<IMetadataProvider>().Object, new Mock<IStagingDataProvider>().Object,
				overlappingCalculator, new Mock<IStagingDataWrapperFactory>().Object, tariff);

			Assert.AreEqual(new DateTime(2000, 01, 01), wrapper.GetWrapperStartDateTime("ZZ1"));
		}

		[Test]
		public void GetWrapperEndDateTime()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_TariffCode = "001", ZZ1_ZZI_NKTariffType = "1P1", ZZ1_StartDate = new DateTime(2000, 01, 01), ZZ1_EndDate = new DateTime(2010, 01, 01) };
			var wrapper = new StagingDataWrapper(new Mock<IMetadataProvider>().Object, new Mock<IStagingDataProvider>().Object,
				overlappingCalculator, new Mock<IStagingDataWrapperFactory>().Object, tariff);

			Assert.AreEqual(new DateTime(2010, 01, 01), wrapper.GetWrapperEndDateTime("ZZ1"));
		}

		[Test]
		public void GetWrapperOriginalPK()
		{
			var tariffPK = Guid.NewGuid();
			var tariff = new Stage.RefCusTariff { ZZ1_PK = tariffPK };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, tariff);
			Assert.That(wrapper.GetWrapperOriginalPK(), Is.EqualTo(tariffPK));
		}

		[Test]
		public void GetWrapperOriginalPK_RateApp()
		{
			var rateAppPK = Guid.NewGuid();
			var ratePK = Guid.NewGuid();
			var rate = new Stage.RefCusRate { ZZ2_PK = ratePK };
			var appPK = Guid.NewGuid();
			var app = new Stage.RefCusApplicability { ZZT_PK = appPK };
			var rateApp = new Stage.RefCusRateApplicability(rate, app) { S01_PK = rateAppPK };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, rateApp);
			Assert.That(wrapper.GetWrapperOriginalPK(), Is.EqualTo(appPK));
		}

		[Test]
		public void GetWrapperOriginalPK_CondApp()
		{
			var condAppPK = Guid.NewGuid();
			var condPK = Guid.NewGuid();
			var cond = new Stage.RefCusCondition { ZX1_PK = condPK };
			var appPK = Guid.NewGuid();
			var app = new Stage.RefCusApplicability { ZZT_PK = appPK };
			var condApp = new Stage.RefCusConditionApplicability(cond, app) { S07_PK = condAppPK };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, condApp);
			Assert.That(wrapper.GetWrapperOriginalPK(), Is.EqualTo(appPK));
		}

		[Test]
		public void GetStagingType()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_PK = Guid.Parse("02EA02CD-251D-46BE-AB8D-B719A1AEFF96") };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, tariff);
			Assert.That(wrapper.GetStagingType(), Is.EqualTo(typeof(Stage.RefCusTariff)));
		}

		[Test]
		public void GetStagingTypeName()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_PK = Guid.Parse("24D6C729-ACF5-426C-9AE4-663645654270") };
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var factory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new StagingDataWrapper(metadataProvider.Object, stagingDataProvider.Object,
				overlappingCalculator, factory.Object, tariff);
			Assert.That(wrapper.GetStagingTypeName(), Is.EqualTo(nameof(Stage.RefCusTariff)));
		}

		IOverlappingCalculator overlappingCalculator;
		[SetUp]
		public void SetUp()
		{
			overlappingCalculator = new OverlappingCalculator(false);
		}
	}
}
