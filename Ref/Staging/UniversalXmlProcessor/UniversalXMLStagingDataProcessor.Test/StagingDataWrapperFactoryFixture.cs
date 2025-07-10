using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class StagingDataWrapperFactoryFixture
	{
		[Test]
		public void CreateMultipleWrappers()
		{
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetRelatedEntityTypes(nameof(Stage.RefCusRate), It.IsAny<IMetadataProvider>()))
				.Returns(new[] { typeof(Stage.RefCusApplicability) });
			stagingDataProvider.Setup(x => x.GetFKPropertyName(typeof(Stage.RefCusApplicability), typeof(Stage.RefCusRate)))
				.Returns(nameof(Stage.RefCusApplicability.ZZT_ZZ2_Rate));
			stagingDataProvider.Setup(x => x.GetTypeFromTypeName(nameof(Stage.RefCusApplicability)))
				.Returns(typeof(Stage.RefCusApplicability));
			var rate = new Stage.RefCusRate();
			var app1 = new Stage.RefCusApplicability();
			app1.ZZT_ZZA_NKTradeGroup = "A";
			var app2 = new Stage.RefCusApplicability();
			app2.ZZT_ZZA_NKTradeGroup = "B";
			stagingDataProvider.Setup(x => x.GetRelatedEntities<Stage.RefCusRate, Stage.RefCusApplicability>(rate))
				.Returns(new[] { app1, app2 });
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusRate))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability) } });

			var safeProvider = new Mock<ISafeDataProvider>();
			var overlappingCalculator = new Mock<IOverlappingCalculator>();
			var factory = new StagingDataWrapperFactory(safeProvider.Object, metadataProvider.Object, stagingDataProvider.Object, overlappingCalculator.Object);
			var wrappers = factory.CreateWrapper(rate).ToArray();
			Assert.AreEqual(2, wrappers.Length);
			Assert.AreEqual("A", wrappers[0].GetRelatedEntities(nameof(Stage.RefCusApplicability)).ToArray()[0].GetWrapperValue(nameof(Stage.RefCusApplicability.ZZT_ZZA_NKTradeGroup)));
			Assert.AreEqual("B", wrappers[1].GetRelatedEntities(nameof(Stage.RefCusApplicability)).ToArray()[0].GetWrapperValue(nameof(Stage.RefCusApplicability.ZZT_ZZA_NKTradeGroup)));
		}

		[Test]
		public void CreateWrapper()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_ZZI_NKTariffType = "1P1" };
			var tariffType = new Safe.RefCusTariffType { ZZI_PK = Guid.NewGuid() };
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties("RefCusTariff")).Returns(new[]
			{
				"ZZ1_TariffCode", "ZZ1_ZZI_NKTariffType", "ZZ1_Description"
			});
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetRelatedTypeAndNKPropertyNames(It.IsAny<string>(), metadataProvider.Object))
				.Returns([Tuple.Create(typeof(Safe.RefCusTariffType), "ZZ1_ZZI_TariffType", new[] { "ZZ1_ZZI_NKTariffType" })]);
			var overlappingCalculator = new Mock<IOverlappingCalculator>();
			var factory = new StagingDataWrapperFactory(safeProvider.Object, metadataProvider.Object, new Mock<IStagingDataProvider>().Object, overlappingCalculator.Object);
			var wrapper = factory.CreateWrapper(tariff).FirstOrDefault();
			Assert.IsNull(wrapper.GetWrapperValue("ZZ1_ZZI_TariffType"));
			safeProvider.Setup(x => x.GetRelatedEntity<Safe.RefCusTariffType>(It.Is<(string PropertyName, object PropertyValue)[]>(t =>
				t[0].PropertyName == "ZZ1_ZZI_NKTariffType" && t[0].PropertyValue.ToString() == "1P1"
					))).Returns(tariffType);
			wrapper = factory.CreateWrapper(tariff).FirstOrDefault();
			Assert.That(wrapper.GetWrapperValue("ZZ1_ZZI_TariffType"), Is.EqualTo(tariffType.ZZI_PK));
		}
	}
}
