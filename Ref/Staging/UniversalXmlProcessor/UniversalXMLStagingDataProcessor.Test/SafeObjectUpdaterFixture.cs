using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Moq;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class SafeObjectUpdaterFixture
	{
		[Test]
		public void Create()
		{
			var metadataProvider = new Mock<IMetadataProvider>();
			var safeProvider = new Mock<ISafeDataProvider>();
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			var tariff = new Safe.RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadataProvider.Object)).Returns(tariff);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadataProvider.Object, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadataProvider.Object));
			Assert.AreEqual("ZZ1", results[0].ParentCode);
			Assert.AreEqual(tariff.ZZ1_PK, results[0].ParentPK);
			Assert.AreEqual(ResultAction.Insert, results[0].Action);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void DoNotUpdateUserOverrideData(bool userOverride)
		{
			var unloco = new Safe.RefUNLOCO { RL_Code = "AUSYD", RL_PortName = "SYDNEY" };
			var propertyNames = new[] {
				nameof(Safe.RefUNLOCO.RL_Code), nameof(Safe.RefUNLOCO.RL_PortName) };

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("AUSYD");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns("Sydney New");
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefUNLOCO));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefUNLOCO));

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefUNLOCO>(new[] { wrapper.Object, wrapper.Object }, metadata)).Returns(new[] { unloco });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefUNLOCO>(new object[0], It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefUNLOCO[]>() { { 0, new[] { unloco } } });
			safeProvider.Setup(x => x.IsUserOverride(unloco)).Returns(userOverride);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Update(unloco, wrapper.Object, metadata, IdenticalLevel.HasChange, 0), userOverride ? Times.Never() : Times.Once());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PopulateDPRForUserOverrideData(bool isData)
		{
			var unloco = new Safe.RefUNLOCO { RL_PK = Guid.NewGuid(), RL_Code = "AUSYD", RL_PortName = "SYDNEY" };
			var propertyNames = new[] {
				nameof(Safe.RefUNLOCO.RL_Code), nameof(Safe.RefUNLOCO.RL_PortName) };

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.SetupGet(x => x.IsData).Returns(isData);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("AUSYD");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns("Sydney New");
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefUNLOCO));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefUNLOCO));

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefUNLOCO>(new[] { wrapper.Object, wrapper.Object }, metadata)).Returns(new[] { unloco });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefUNLOCO>(new object[0], It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefUNLOCO[]>() { { 0, new[] { unloco } } });
			safeProvider.Setup(x => x.IsUserOverride(unloco)).Returns(true);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper.Object });
			Assert.AreEqual(1, results.Length);
			var dprResult = results.First();
			Assert.AreEqual(ResultAction.Update, dprResult.Action);
			Assert.AreEqual("RL", dprResult.ParentCode);
			Assert.AreEqual(unloco.RL_PK, dprResult.ParentPK);
			Assert.AreEqual(unloco.RL_PK, dprResult.DatasetPK);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void DoNotUpdateUserOverrideDataWhenWrapperIsDataIsFalse(bool userOverride)
		{
			var unloco = new Safe.RefUNLOCO { RL_Code = "AUSYD", RL_PortName = "SYDNEY" };
			var propertyNames = new[] {
				nameof(Safe.RefUNLOCO.RL_Code), nameof(Safe.RefUNLOCO.RL_PortName) };

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.SetupGet(x => x.IsData).Returns(false);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("AUSYD");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns("Sydney New");
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefUNLOCO));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefUNLOCO));

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefUNLOCO>(new[] { wrapper.Object, wrapper.Object }, metadata)).Returns(new[] { unloco });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefUNLOCO>(new object[0], It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefUNLOCO[]>() { { 0, new[] { unloco } } });
			safeProvider.Setup(x => x.IsUserOverride(unloco)).Returns(userOverride);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Update(unloco, wrapper.Object, metadata, IdenticalLevel.Identical, 0), userOverride ? Times.Never() : Times.Once());
		}

		[Test]
		public void UpdateNonExpirableType()
		{
			var unloco = new Safe.RefUNLOCO { RL_Code = "AUSYD", RL_PortName = "SYDNEY" };
			var propertyNames = new[] {
				nameof(Safe.RefUNLOCO.RL_Code), nameof(Safe.RefUNLOCO.RL_PortName) };
			DateTimeRange dummy = null;
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("AUSYD");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns("Sydney New");
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefUNLOCO));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefUNLOCO));
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(dummy);

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefUNLOCO>(new[] { wrapper.Object, wrapper.Object }, metadata)).Returns(new[] { unloco });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefUNLOCO>(new object[0], It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefUNLOCO[]>() { { 0, new[] { unloco } } });
			safeProvider.Setup(x => x.IsUserOverride(unloco)).Returns(false);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Update(unloco, wrapper.Object, metadata, IdenticalLevel.HasChange, 0), Times.Once());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Update(bool isData)
		{
			var datasetPK = Guid.NewGuid();
			var tariffTypePK = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff() { ZZ1_PK = datasetPK, ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset() };
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			wrapper1.SetupGet(x => x.IsData).Returns(isData);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(new DateTime(2000, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(new DateTime(2010, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			var wrapper2 = new Mock<IStagingDataWrapper>();
			wrapper2.SetupGet(x => x.IsData).Returns(isData);
			wrapper2.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("002");
			wrapper2.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper2.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(new DateTime(2000, 01, 01).ToUTCDateTimeOffset());
			wrapper2.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(new DateTime(2010, 01, 01).ToUTCDateTimeOffset());
			wrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper2.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper2.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var wrapper = new Mock<IStagingDataWrapper>().Object;
			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object, wrapper2.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, It.IsAny<bool>()))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.Identical);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper2.Object, metadata)).Returns((int)IdenticalLevel.HasChange);
			safeProvider.Setup(x => x.IsUserOverride(tariff)).Returns(false);
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, new List<object>(), metadata)).Returns(new Safe.RefCusTariff());
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper2.Object, null, new List<object>(), metadata)).Returns(new Safe.RefCusTariff());
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));
			safeProvider.Setup(x => x.GetSpecifiedDateTimeRangeObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(), It.IsAny<DateTimeRange>(), wrapper, metadata)).Returns(tariff);
			var deletedPKList = new List<Guid> { Guid.NewGuid() };
			safeProvider.Setup(x => x.DeleteConflictedRecordsByOrder<Safe.RefCusTariff>(0, It.IsAny<Dictionary<int, IEnumerable<object>>>())).Returns(deletedPKList);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper1.Object, wrapper2.Object });

			if (isData)
			{
				safeProvider.Verify(x => x.Update(tariff, wrapper1.Object, metadata, It.IsAny<IdenticalLevel>(), 0), Times.Never);
				safeProvider.Verify(x => x.Update(tariff, wrapper2.Object, metadata, IdenticalLevel.HasChange, 0));
				safeProvider.Verify(x => x.DeleteConflictedRecordsByOrder<Safe.RefCusTariff>(0, It.Is<Dictionary<int, IEnumerable<object>>>(y => y[0].First() == tariff)), Times.Exactly(2));
			}
			else
			{
				safeProvider.Verify(x => x.Update(tariff, wrapper1.Object, metadata, IdenticalLevel.Identical, 0));
				safeProvider.Verify(x => x.Update(tariff, wrapper2.Object, metadata, IdenticalLevel.Identical, 0));
			}

			Assert.AreEqual(datasetPK, results[0].DatasetPK);
			Assert.AreEqual("ZZ1", results[0].ParentCode);
			Assert.AreEqual(tariff.ZZ1_PK, results[0].ParentPK);
			Assert.AreEqual(ResultAction.Update, results[0].Action);
		}

		[Test]
		public void GetIdenticalLevelNoRelatedDataMatch()
		{
			var tariffTypePK = Guid.NewGuid();
			var tariffPK = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff() { ZZ1_PK = tariffPK, ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset() };

			var relationship = new Safe.RefCusTariffRelationship() { ZZH_ZZ1_Tariff = tariffPK, ZZH_ZZI_TariffType = tariffTypePK, ZZH_TariffCode = "111" };
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };

			var relationshipWrapper = new Mock<IStagingDataWrapper>();

			var wrapper = new Mock<IStagingDataWrapper>();
			Tuple<string, string>[] tuples = { Tuple.Create("RefCusTariffRelationship", "ZZH_ZZ1_Tariff") };
			wrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(tuples);
			wrapper.Setup(x => x.GetRelatedEntities("RefCusTariffRelationship")).Returns(new[] { relationshipWrapper.Object });

			var metadata = new Mock<IMetadataProvider>().Object;
			var safeProvider = new Mock<ISafeDataProvider>();

			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper.Object, metadata)).Returns((int)IdenticalLevel.Identical);
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariffRelationship>(It.IsAny<object[]>(), It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariffRelationship[]>() { { 0, new Safe.RefCusTariffRelationship[0] } });

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var result = updater.GetIdenticalLevel(tariff, typeof(Safe.RefCusTariff), wrapper.Object);

			Assert.AreEqual((int)IdenticalLevel.HasChange, result);
		}

		[Test]
		public void GetIdenticalLevelWhenDataIsReduced()
		{
			var condition = new Safe.RefCusCondition()
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_ZZ1_Tariff = Guid.NewGuid(),
				ZX1_Comment = "Condition B: Presentation of a certificate/licence/document",
				ZX1_EndDate = new DateTime(2023, 09, 30).ToUTCDateTimeOffset(),
				ZX1_IsExport = false,
				ZX1_IsImport = true,
				ZX1_StartDate = new DateTime(2022, 01, 30).ToUTCDateTimeOffset()
			};

			var conditionPK = Guid.NewGuid();
			var conditionValue1 = new Safe.RefCusConditionValue()
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = conditionPK,
				ZX3_Value = "Y922",
				ZX3_ZX4_ValueType = Guid.NewGuid()
			};

			var conditionValue2 = new Safe.RefCusConditionValue()
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = conditionPK,
				ZX3_Value = "999L",
				ZX3_ZX4_ValueType = Guid.NewGuid()
			};

			var conditionValue3 = new Mock<IStagingDataWrapper>();
			conditionValue3.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusConditionValue.ZX3_Value))).Returns("Y922");
			conditionValue3.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusConditionValue.ZX3_ZX1_Condition))).Returns(condition.ZX1_PK);

			var relationshipWrapper = new Mock<IStagingDataWrapper>();
			var wrapper = new Mock<IStagingDataWrapper>();
			Tuple<string, string>[] tuples = { Tuple.Create("RefCusConditionValue", "ZX3_ZX1_Condition") };
			wrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(tuples);
			wrapper.Setup(x => x.GetRelatedEntities("RefCusConditionValue")).Returns((new[] { conditionValue3.Object }));

			var metadata = new Mock<IMetadataProvider>().Object;
			var safeProvider = new Mock<ISafeDataProvider>();

			safeProvider.Setup(x => x.GetIdenticalLevel(condition, wrapper.Object, metadata)).Returns((int)IdenticalLevel.Identical);
			safeProvider.Setup(x => x.GetRelatedData(condition, nameof(Safe.RefCusConditionValue))).Returns(new[] { conditionValue1, conditionValue2 });

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var result = updater.GetIdenticalLevel(condition, typeof(Safe.RefCusCondition), wrapper.Object);

			Assert.AreEqual((int)IdenticalLevel.HasChange, result);
		}

		[TestCase(IdenticalLevel.Identical)]
		[TestCase(IdenticalLevel.MainContentIdentical)]
		public void UpdateNotIncludePublishedDate(IdenticalLevel identicalLevel)
		{
			var tariffTypePK = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff() { ZZ1_PK = Guid.NewGuid(), ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset() };
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(new DateTime(2000, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(new DateTime(2010, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue<DateTime?>(propertyNames[2])).Returns(new DateTime(2018, 01, 01));
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)identicalLevel);
			safeProvider.Setup(x => x.IsUserOverride(tariff)).Returns(false);
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, new List<object>(), metadata)).Returns(new Safe.RefCusTariff());
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper1.Object });
			safeProvider.Verify(x => x.Update(tariff, wrapper1.Object, metadata, identicalLevel, 0), identicalLevel == IdenticalLevel.Identical ? Times.Never() : Times.Once());
			Assert.AreEqual("ZZ1", results[0].ParentCode);
			Assert.AreEqual(tariff.ZZ1_PK, results[0].ParentPK);
		}

		[Test]
		public void InsertRelatedObject()
		{
			var tariff = new Safe.RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(It.IsAny<IStagingDataWrapper>(), null, new List<object>(), It.IsAny<IMetadataProvider>())).Returns(tariff);

			var rateWrapper1 = new Mock<IStagingDataWrapper>();
			rateWrapper1.SetupGet(x => x.IsData).Returns(true);
			rateWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			rateWrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));
			var tariffWrapper = new Mock<IStagingDataWrapper>();
			tariffWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			tariffWrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			tariffWrapper.SetupGet(x => x.IsData).Returns(true);
			tariffWrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusRate), nameof(Stage.RefCusRate.ZZ2_ZZ1_Tariff)) });
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusRate))).Returns(new[] { rateWrapper1.Object });
			tariffWrapper.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));
			safeProvider.Setup(x => x.Create<Safe.RefCusRate>(rateWrapper1.Object, tariff, new List<object>(), It.IsAny<IMetadataProvider>())).Returns(new Safe.RefCusRate());

			var updater = new SafeObjectUpdater(safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator, false);
			updater.Update(new[] { tariffWrapper.Object });
			safeProvider.Verify(x => x.Create<Safe.RefCusRate>(rateWrapper1.Object, tariff, It.IsAny<List<object>>(), It.IsAny<IMetadataProvider>()));
			rateWrapper1.Verify(x => x.SetWrapperValue(nameof(Safe.RefCusRate.ZZ2_ZZ1_Tariff), tariff.ZZ1_PK));
		}

		[Test]
		public void UpdaterResults()
		{
			var tariff = new Safe.RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(It.IsAny<IStagingDataWrapper>(), null, new List<object>(), It.IsAny<IMetadataProvider>())).Returns(tariff);

			var rateWrapper = new Mock<IStagingDataWrapper>();
			rateWrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			rateWrapper.SetupGet(x => x.IsData).Returns(true);
			rateWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			var uomWrapper = new Mock<IStagingDataWrapper>();
			uomWrapper.SetupGet(x => x.IsData).Returns(true);
			uomWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariffUOM));
			uomWrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			var tariffWrapper = new Mock<IStagingDataWrapper>();
			tariffWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			tariffWrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			tariffWrapper.SetupGet(x => x.IsData).Returns(true);
			tariffWrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusRate), nameof(Stage.RefCusRate.ZZ2_ZZ1_Tariff)),
				Tuple.Create(nameof(Stage.RefCusTariffUOM), nameof(Stage.RefCusTariffUOM.ZZ8_ZZ1_Tariff)) });
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusRate))).Returns(new[] { rateWrapper.Object });
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusTariffUOM))).Returns(new[] { uomWrapper.Object });
			tariffWrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var ratePK = Guid.NewGuid();
			var uomPK = Guid.NewGuid();
			safeProvider.Setup(x => x.Create<Safe.RefCusRate>(rateWrapper.Object, tariff, new List<object>(), It.IsAny<IMetadataProvider>())).Returns(new Safe.RefCusRate() { ZZ2_PK = ratePK });
			safeProvider.Setup(x => x.Create<Safe.RefCusTariffUOM>(uomWrapper.Object, tariff, new List<object>(), It.IsAny<IMetadataProvider>())).Returns(new Safe.RefCusTariffUOM() { ZZ8_PK = uomPK });

			DataProviderHelper.SetEnableExpirable(false);
			var updater = new SafeObjectUpdater(safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator, false);
			var results = updater.Update(new[] { tariffWrapper.Object });
			var rateResult = results.FirstOrDefault(x => x.ParentPK == ratePK);
			Assert.IsNotNull(rateResult.ExpirableAncestorPK, "We do store expirableAncestorPK for expirable types");
			var uomResult = results.FirstOrDefault(x => x.ParentPK == uomPK);
			Assert.AreEqual(tariff.ZZ1_PK, uomResult.ExpirableAncestorPK);
		}

		[Test]
		public void DeduplicationRelatedObject()
		{
			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusRate)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_RateFormula), Operation = Operations.GreaterThan } });
			var tariff = new Safe.RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rate = new Safe.RefCusRate() { ZZ2_PK = Guid.NewGuid() };
			var safeDateTimeRange = new DateTimeRange(new DateTime(2000, 01, 01, 0, 0, 0), new DateTime(2010, 01, 01, 0, 0, 0));
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(It.IsAny<IStagingDataWrapper>(), null, new List<object>(), metadata.Object)).Returns(tariff);
			safeProvider.Setup(x => x.Create<Safe.RefCusRate>(It.IsAny<IStagingDataWrapper>(), tariff, new List<object>(), metadata.Object)).Returns(rate);
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(new[] { rate },
				It.IsAny<IStagingDataWrapper>(), metadata.Object, false)).Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, new[] { rate } } });
			safeProvider.Setup(x => x.GetDateTimeRange(rate)).Returns(safeDateTimeRange);
			safeProvider.Setup(x => x.IsUserOverride(rate)).Returns(false);

			var rateWrapper1 = new Mock<IStagingDataWrapper>();
			rateWrapper1.SetupGet(x => x.IsData).Returns(true);
			rateWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			rateWrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));
			var rateWrapper2 = new Mock<IStagingDataWrapper>();
			rateWrapper2.SetupGet(x => x.IsData).Returns(true);
			rateWrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper2.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			rateWrapper2.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2001, 01, 01), new DateTime(2010, 01, 01)));
			var tariffWrapper = new Mock<IStagingDataWrapper>();
			tariffWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			tariffWrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			tariffWrapper.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));
			tariffWrapper.SetupGet(x => x.IsData).Returns(true);
			tariffWrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusRate), nameof(Stage.RefCusRate.ZZ2_ZZ1_Tariff)) });
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusRate))).Returns(new[] { rateWrapper1.Object, rateWrapper2.Object });

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata.Object, overlappingCalculator, false);
			var results = updater.Update(new[] { tariffWrapper.Object });
			safeProvider.Verify(x => x.Update(rate, rateWrapper2.Object, metadata.Object, IdenticalLevel.HasChange, 0));
		}

		[Test]
		public void ShouldNotDuplicateItemsToUpdateWhenHavingMoreThanOneWrapper()
		{
			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusCondition)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusCondition.ZX1_Comment), Operation = Operations.GreaterThan } });
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusApplicability)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability.ZZT_OrderNumber), Operation = Operations.GreaterThan } });
			var tariff = new Safe.RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var condition1 = new Safe.RefCusCondition();
			var condition2 = new Safe.RefCusCondition();
			var applicability1 = new Safe.RefCusApplicability();

			var applicabilityWrapper1 = new Mock<IStagingDataWrapper>();
			applicabilityWrapper1.SetupGet(x => x.IsData).Returns(true);
			applicabilityWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusApplicability));
			applicabilityWrapper1.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new Tuple<string, string>[0]);
			applicabilityWrapper1.Setup(x => x.SetWrapperValue(nameof(Stage.RefCusApplicability.ZZT_ZX1_Conditions), It.IsAny<object>()));
			applicabilityWrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));

			var conditionWrapper1 = new Mock<IStagingDataWrapper>();
			conditionWrapper1.SetupGet(x => x.IsData).Returns(true);
			conditionWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusCondition));
			conditionWrapper1.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] { Tuple.Create(nameof(Stage.RefCusApplicability), nameof(Stage.RefCusApplicability.ZZT_ZX1_Conditions)) });
			conditionWrapper1.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusApplicability))).Returns(new[] { applicabilityWrapper1.Object });
			conditionWrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));

			var conditionWrapper2 = new Mock<IStagingDataWrapper>();
			conditionWrapper2.SetupGet(x => x.IsData).Returns(true);
			conditionWrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusCondition));
			conditionWrapper2.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] { Tuple.Create(nameof(Stage.RefCusApplicability), nameof(Stage.RefCusApplicability.ZZT_ZX1_Conditions)) });
			conditionWrapper2.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusApplicability))).Returns(new[] { applicabilityWrapper1.Object });
			conditionWrapper2.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2001, 01, 01), new DateTime(2010, 01, 01)));

			var tariffWrapper = new Mock<IStagingDataWrapper>();
			tariffWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			tariffWrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			tariffWrapper.SetupGet(x => x.IsData).Returns(true);
			tariffWrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusCondition), nameof(Stage.RefCusCondition.ZX1_ZZ1_Tariff)) });
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusCondition))).Returns(new[] { conditionWrapper1.Object, conditionWrapper2.Object });
			tariffWrapper.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));

			var safeProvider = new Mock<ISafeDataProvider>();

			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(tariffWrapper.Object, null, It.IsAny<List<object>>(), metadata.Object)).Returns(tariff);
			safeProvider.Setup(x => x.Create<Safe.RefCusCondition>(conditionWrapper1.Object, tariff, It.IsAny<List<object>>(), metadata.Object)).Returns(condition1);
			safeProvider.Setup(x => x.Create<Safe.RefCusCondition>(conditionWrapper2.Object, tariff, It.IsAny<List<object>>(), metadata.Object)).Returns(condition2);
			safeProvider.Setup(x => x.Create<Safe.RefCusApplicability>(It.IsAny<IStagingDataWrapper>(), It.IsAny<object>(), It.IsAny<List<object>>(), metadata.Object)).Returns(applicability1);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata.Object, overlappingCalculator, false);
			updater.Update(new[] { tariffWrapper.Object });
			applicabilityWrapper1.Verify(x => x.SetWrapperValue(nameof(Stage.RefCusApplicability.ZZT_ZX1_Conditions), It.IsAny<object>()), Times.Exactly(2));
		}

		[Test]
		public void UpdateWithSeperatingWhenSafeObjHasMoreThanOneExpirableKeyRelatedEntities()
		{
			var datasetPK = Guid.NewGuid();
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var rate1 = new Safe.RefCusRate { ZZ2_PK = ratePK1, ZZ2_ZZ1_Tariff = datasetPK, ZZ2_DataSetPK = datasetPK, ZZ2_RateFormula = "VFD * 0.12", ZZ2_StartDate = new DateTime(1900, 01, 01).ToUTCDateTimeOffset(), ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app11 = new Safe.RefCusApplicability { ZZT_PK = appPK1, ZZT_ZZ2_Rate = ratePK1, ZZT_DataSetPK = datasetPK, ZZT_AdditionalCode = "AA", ZZT_StartDate = new DateTime(2020, 01, 01).ToUTCDateTimeOffset(), ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app12 = new Safe.RefCusApplicability { ZZT_PK = appPK2, ZZT_ZZ2_Rate = ratePK1, ZZT_DataSetPK = datasetPK, ZZT_AdditionalCode = "BB", ZZT_StartDate = new DateTime(2020, 01, 01).ToUTCDateTimeOffset(), ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var rate2 = new Safe.RefCusRate { ZZ2_PK = ratePK2, ZZ2_ZZ1_Tariff = datasetPK, ZZ2_DataSetPK = datasetPK, ZZ2_RateFormula = "0", ZZ2_StartDate = new DateTime(1900, 01, 01).ToUTCDateTimeOffset(), ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var listToSearch = new List<object> { rate1 };

			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusRate)))
				.Returns(new KeyProperty[] {
					new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_ZY1_NKRateCode)},
					new KeyProperty { Name = nameof(Stage.RefCusApplicability)}
				});
			metadata.Setup(x => x.GetProperties(nameof(Stage.RefCusRate))).Returns(
				new[] { nameof(Stage.RefCusApplicability) });

			var rateWrapper1 = new Mock<IStagingDataWrapper>();
			var appWrapper1 = new Mock<IStagingDataWrapper>();
			rateWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)));
			rateWrapper1.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusApplicability))).Returns(new[] { appWrapper1.Object });
			rateWrapper1.Setup(x => x.IsData).Returns(true);
			appWrapper1.Setup(x => x.IsData).Returns(true);

			var rateWrapper2 = new Mock<IStagingDataWrapper>();
			var appWrapper2 = new Mock<IStagingDataWrapper>();
			rateWrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper2.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRate));
			rateWrapper2.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)));
			rateWrapper2.Setup(x => x.GetRelatedEntities(nameof(Safe.RefCusApplicability))).Returns(new[] { appWrapper2.Object });
			rateWrapper2.Setup(x => x.IsData).Returns(true);
			appWrapper2.Setup(x => x.IsData).Returns(true);

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusRate>(It.IsAny<IStagingDataWrapper[]>(), metadata.Object)).Returns(listToSearch.Cast<Safe.RefCusRate>);
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(It.IsAny<object[]>(), rateWrapper1.Object, metadata.Object, false)).Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, listToSearch.Cast<Safe.RefCusRate>().ToArray() } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(It.IsAny<object[]>(), rateWrapper2.Object, metadata.Object, false)).Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, listToSearch.Cast<Safe.RefCusRate>().ToArray() } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusApplicability>(It.IsAny<object[]>(), appWrapper1.Object, metadata.Object, false)).Returns(new Dictionary<int, Safe.RefCusApplicability[]>() { { 0, new[] { app11 } } });
			safeProvider.Setup(x => x.GetRelatedData<Safe.RefCusRate>(rate1, nameof(Stage.RefCusApplicability))).Returns(new[] { app11, app12 }).Callback(() =>
			{
				safeProvider.Setup(x => x.GetRelatedData<Safe.RefCusRate>(rate1, nameof(Safe.RefCusApplicability))).Returns(new[] { app12 });
			});
			safeProvider.Setup(x => x.GetDateTimeRange(rate1)).Returns(new DateTimeRange(new DateTime(1900, 1, 1), new DateTime(2079, 6, 6)));
			safeProvider.Setup(x => x.Create<Safe.RefCusRate>(rateWrapper1.Object, null, listToSearch, metadata.Object)).Returns(rate2);
			safeProvider.Setup(x => x.GetIdenticalObjectFromList<Safe.RefCusRate>(It.IsAny<object[]>(), rateWrapper2.Object, metadata.Object)).Returns(rate2);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata.Object, overlappingCalculator, false);
			updater.Update(new[] { rateWrapper1.Object, rateWrapper2.Object });

			safeProvider.Verify(x => x.Update<Safe.RefCusRate>(rate1, rateWrapper2.Object, metadata.Object, It.IsAny<IdenticalLevel>(), It.IsAny<int>()));
		}

		[Test]
		public void UpdateWithSeperatingWhenSafeObjHasOnlyOneExpirableKeyRelatedEntities()
		{
			var datasetPK = Guid.NewGuid();
			var ratePK1 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var rate1 = new Safe.RefCusRate { ZZ2_PK = ratePK1, ZZ2_ZZ1_Tariff = datasetPK, ZZ2_DataSetPK = datasetPK, ZZ2_RateFormula = "VFD * 0.12", ZZ2_StartDate = new DateTime(1900, 01, 01).ToUTCDateTimeOffset(), ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app11 = new Safe.RefCusApplicability { ZZT_PK = appPK1, ZZT_ZZ2_Rate = ratePK1, ZZT_DataSetPK = datasetPK, ZZT_AdditionalCode = "AA", ZZT_StartDate = new DateTime(2020, 01, 01).ToUTCDateTimeOffset(), ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var listToSearch = new List<object> { rate1 };

			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusRate)))
				.Returns(new KeyProperty[] {
					new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_ZY1_NKRateCode)},
					new KeyProperty { Name = nameof(Stage.RefCusApplicability)}
				});
			metadata.Setup(x => x.GetProperties(nameof(Stage.RefCusRate))).Returns(
				new[] { nameof(Stage.RefCusApplicability) });

			var rateWrapper1 = new Mock<IStagingDataWrapper>();
			var appWrapper1 = new Mock<IStagingDataWrapper>();
			rateWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)));
			rateWrapper1.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusApplicability))).Returns(new[] { appWrapper1.Object });
			rateWrapper1.Setup(x => x.IsData).Returns(true);
			appWrapper1.Setup(x => x.IsData).Returns(true);

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusRate>(It.IsAny<IStagingDataWrapper[]>(), metadata.Object)).Returns(listToSearch.Cast<Safe.RefCusRate>);
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(It.IsAny<object[]>(), rateWrapper1.Object, metadata.Object, false)).Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, listToSearch.Cast<Safe.RefCusRate>().ToArray() } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusApplicability>(It.IsAny<object[]>(), appWrapper1.Object, metadata.Object, false)).Returns(new Dictionary<int, Safe.RefCusApplicability[]>() { { 0, new[] { app11 } } });
			safeProvider.Setup(x => x.GetRelatedData<Safe.RefCusRate>(rate1, nameof(Stage.RefCusApplicability))).Returns(new[] { app11 });
			safeProvider.Setup(x => x.GetDateTimeRange(rate1)).Returns(new DateTimeRange(new DateTime(1900, 1, 1), new DateTime(2079, 6, 6)));

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata.Object, overlappingCalculator, false);
			updater.Update(new[] { rateWrapper1.Object });

			safeProvider.Verify(x => x.Create<Safe.RefCusRate>(rateWrapper1.Object, null, It.IsAny<List<object>>(), metadata.Object), Times.Never);
			safeProvider.Verify(x => x.Update(rate1, rateWrapper1.Object, metadata.Object, It.IsAny<IdenticalLevel>(), 0), Times.Once);
		}

		[Test]
		public void ExpireObjHasChanges()
		{
			var metadata = new Mock<IMetadataProvider>().Object;
			var tariff = new Safe.RefCusTariff()
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "001",
				ZZ1_Description = "HELLO NEW WORLD"
			};
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_Description) };
			var safeDateTimeRange = new DateTimeRange(new DateTime(2016, 12, 31), new DateTime(2017, 12, 31));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff },
				It.IsAny<IStagingDataWrapper>(), metadata, false)).Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadata)).Returns(new Safe.RefCusTariff());
			safeProvider.Setup(x => x.IsExpirable<Safe.RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange)).Returns(true);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper.Object, metadata)).Returns(0);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var result = updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Expire(tariff, wrapper.Object, metadata));
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadata));
			Assert.AreEqual(ResultAction.Insert, result[1].Action);
			Assert.AreEqual(ResultAction.Expire, result[0].Action);
			Assert.AreEqual(tariff.ZZ1_PK, result[0].ParentPK);
		}

		[Test]
		public void ShouldNotDuplicateItemsToExpireWhenHavingMoreThanOneWrapper()
		{
			var metadata = new Mock<IMetadataProvider>().Object;
			var tariff = new Safe.RefCusTariff()
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "HELLO NEW WORLD"
			};
			var tariff2 = new Safe.RefCusTariff()
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "HELLO NEW WORLD"
			};
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_Description) };
			var wrapper = new Mock<IStagingDataWrapper>();
			var safeDateTimeRange = new DateTimeRange(new DateTime(2016, 12, 31), new DateTime(2017, 12, 31));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var wrapper2 = new Mock<IStagingDataWrapper>();
			wrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper2.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper2.SetupGet(x => x.IsData).Returns(true);
			wrapper2.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper2.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("HELLO WORLD");
			wrapper2.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			var safeProvider = new Mock<ISafeDataProvider>();

			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper.Object, wrapper2.Object }, metadata)).Returns(new[] { tariff, tariff2 });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(),
				wrapper.Object, metadata, false)).Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });

			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(),
				wrapper2.Object, metadata, false)).Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff2 } } });

			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(It.IsAny<IStagingDataWrapper>(), null, It.IsAny<List<object>>(), metadata)).Returns(new Safe.RefCusTariff());

			safeProvider.Setup(x => x.IsExpirable<Safe.RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange)).Returns(true);
			safeProvider.Setup(x => x.GetIdenticalLevel(It.IsAny<Safe.RefCusTariff>(), It.IsAny<IStagingDataWrapper>(), metadata)).Returns(0);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff2)).Returns(safeDateTimeRange);
			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			updater.Update(new[] { wrapper.Object, wrapper2.Object });
			safeProvider.Verify(x => x.Expire(It.IsAny<Safe.RefCusTariff>(), It.IsAny<IStagingDataWrapper>(), metadata), Times.Exactly(2));
		}

		[Test]
		public void UpdateEndDate_OnlyDescriptionHasChanges()
		{
			var metadata = new Mock<IMetadataProvider>().Object;
			var tariff = new Safe.RefCusTariff()
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "HELLO NEW WORLD"
			};
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_Description) };
			var wrapper = new Mock<IStagingDataWrapper>();
			var safeDateTimeRange = new DateTimeRange(new DateTime(2016, 12, 31), new DateTime(2017, 12, 31));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);
			safeProvider.Setup(x => x.IsExpirable<Safe.RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange)).Returns(true);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper.Object, metadata)).Returns(1);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Update(tariff, wrapper.Object, metadata, IdenticalLevel.MainContentIdentical, 0));
		}

		[Test]
		public void ExpireObj_ChangesInRelatedObj()
		{
			var metadata = new Mock<IMetadataProvider>().Object;
			var tariff = new Safe.RefCusTariff()
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "HELLO NEW WORLD",
			};
			var attr = new Safe.RefCusTariffAttribute();
			var rate = new Safe.RefCusRate();
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_Description) };
			var safeDateTimeRange = new DateTimeRange(new DateTime(2016, 12, 31), new DateTime(2017, 12, 31));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper.SetupGet(x => x.IsData).Returns(true);
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(new object[] { rate }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, new[] { rate } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariffAttribute>(new object[] { attr }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariffAttribute[]>() { { 0, new[] { attr } } });
			safeProvider.Setup(x => x.GetRelatedData(tariff, nameof(Safe.RefCusRate))).Returns(new[] { rate });
			safeProvider.Setup(x => x.GetRelatedData(tariff, nameof(Safe.RefCusTariffAttribute))).Returns(new[] { attr });
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);

			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadata)).Returns(new Safe.RefCusTariff());

			var attrWrapper = new Mock<IStagingDataWrapper>();
			attrWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariffAttribute));
			var rateWrapper = new Mock<IStagingDataWrapper>();
			rateWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			wrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusRate), nameof(Stage.RefCusRate.ZZ2_ZZ1_Tariff)),
				Tuple.Create(nameof(Stage.RefCusTariffAttribute), nameof(Stage.RefCusTariffAttribute.ZZ3_ZZ1_Tariff))
			});
			wrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusRate))).Returns(new[] { rateWrapper.Object });
			wrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusTariffAttribute))).Returns(new[] { attrWrapper.Object });

			safeProvider.Setup(x => x.IsExpirable<Safe.RefCusTariff>(It.IsAny<DateTimeRange>(), It.IsAny<DateTimeRange>())).Returns(true);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper.Object, metadata)).Returns(1);
			safeProvider.Setup(x => x.GetIdenticalLevel(rate, rateWrapper.Object, metadata)).Returns(0);
			safeProvider.Setup(x => x.GetIdenticalLevel(attr, attrWrapper.Object, metadata)).Returns(1);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Expire(tariff, wrapper.Object, metadata), Times.Never);
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadata), Times.Never);
			safeProvider.Verify(x => x.Update(tariff, wrapper.Object, metadata, IdenticalLevel.MainContentIdentical, 0));

			safeProvider.Setup(x => x.GetIdenticalLevel(attr, attrWrapper.Object, metadata)).Returns(0);
			updater.Update(new[] { wrapper.Object });
			safeProvider.Verify(x => x.Expire(tariff, wrapper.Object, metadata));
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper.Object, null, It.IsAny<List<object>>(), metadata));
		}

		[Test]
		public void InsertHistoricalData()
		{
			var tariffTypePK = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff() { ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset() };
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			var safeDateTimeRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(1999, 12, 31));
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(new DateTime(1990, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(new DateTime(1999, 12, 31).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, It.IsAny<bool>()))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.IsUserOverride(tariff)).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, It.IsAny<List<object>>(), metadata)).Returns(new Safe.RefCusTariff());

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);

			var results = updater.Update(new[] { wrapper1.Object });
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, It.IsAny<List<object>>(), metadata));
		}

		[TestCaseSource(nameof(UpdateHistoricalDataTestCases))]
		public void UpdateHistoricalData(DateTime stageStartDate, DateTime stageEndDate)
		{
			var tariffTypePK = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff() { ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset() };
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(stageStartDate.ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(stageEndDate.ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(stageStartDate, stageEndDate));

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, It.IsAny<bool>()))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.IsUserOverride(tariff)).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01)));
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, new List<object>(), metadata)).Returns(new Safe.RefCusTariff());

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper1.Object });
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, new List<object>(), metadata), Times.Never);
			safeProvider.Verify(x => x.Update(tariff, wrapper1.Object, metadata, It.IsAny<IdenticalLevel>(), 0));
		}

		protected static IEnumerable UpdateHistoricalDataTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(1999, 01, 01), new DateTime(2005, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_LateIsOverlapped_1"
				};
				yield return new TestCaseData(new DateTime(2000, 01, 01), new DateTime(2005, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_LateIsOverlapped_2"
				};
				yield return new TestCaseData(new DateTime(2001, 01, 01), new DateTime(2005, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_LateIsOverlapped_3"
				};
				yield return new TestCaseData(new DateTime(1999, 01, 01), new DateTime(2015, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_LateIsOverlapped_4"
				};
				yield return new TestCaseData(new DateTime(2000, 01, 01), new DateTime(2015, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_LateIsOverlapped_5"
				};
				yield return new TestCaseData(new DateTime(2005, 01, 01), new DateTime(2015, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_LateIsOverlapped_6"
				};
			}
		}

		[TestCaseSource(nameof(MiddleNoOverlappingTestCases))]
		public void InsertHistoricalData_MiddleNoOverlapping(DateTime stageStartDate, DateTime stageEndDate)
		{
			var tariffTypePK = Guid.NewGuid();
			var tariff1 = new Safe.RefCusTariff() { ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(1990, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset() };
			var tariff2 = new Safe.RefCusTariff() { ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2020, 01, 01).ToUTCDateTimeOffset() };
			var safeDateTimeRanges1 = new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01));
			var safeDateTimeRanges2 = new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01));
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(stageStartDate.ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(stageEndDate.ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(stageStartDate, stageEndDate));

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff1, tariff2 });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff1, tariff2 }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff2 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff1, tariff2 }, It.IsAny<IStagingDataWrapper>(), metadata, true))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff1, tariff2 } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff1, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff2, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.IsUserOverride(tariff1)).Returns(false);
			safeProvider.Setup(x => x.IsUserOverride(tariff2)).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff1)).Returns(safeDateTimeRanges1);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff2)).Returns(safeDateTimeRanges2);
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, It.IsAny<List<object>>(), metadata)).Returns(new Safe.RefCusTariff());

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);

			var results = updater.Update(new[] { wrapper1.Object });
			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, It.IsAny<List<object>>(), metadata));
		}

		static IEnumerable MiddleNoOverlappingTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(2001, 1, 1), new DateTime(2005, 1, 1))
				{
					TestName = "{m}_InsertHistoricalData_MiddleNoOverlapped_1"
				};
				yield return new TestCaseData(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1))
				{
					TestName = "{m}_InsertHistoricalData_MiddleNoOverlapped_2"
				};
			}
		}

		[TestCaseSource(nameof(MiddleOverlappingTestCases))]
		public void InsertHistoricalData_MiddleIsOverlapped(DateTime stageStartDate, DateTime stageEndDate)
		{
			var tariffTypePK = Guid.NewGuid();
			var tariffPK1 = Guid.NewGuid();
			var tariffPK2 = Guid.NewGuid();
			var tariff1 = new Safe.RefCusTariff() { ZZ1_PK = tariffPK1, ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(1990, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2000, 01, 01).ToUTCDateTimeOffset() };
			var tariff2 = new Safe.RefCusTariff() { ZZ1_PK = tariffPK2, ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK, ZZ1_StartDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset(), ZZ1_EndDate = new DateTime(2020, 01, 01).ToUTCDateTimeOffset() };
			var dateTimeRange1 = new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01));
			var dateTimeRange2 = new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01));
			var propertyNames = new[] {
				nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType), nameof(Safe.RefCusTariff.ZZ1_TariffCode), nameof(Safe.RefCusTariff.ZZ1_StartDate), nameof(Safe.RefCusTariff.ZZ1_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			var stagingTariffPK = Guid.NewGuid();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(tariffTypePK);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(stageStartDate.ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[3])).Returns(stageEndDate.ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue<Guid>("ZZ1_PK")).Returns(stagingTariffPK);
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(Convert.ToDateTime(stageStartDate), Convert.ToDateTime(stageEndDate)));
			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff1, tariff2 });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff1, tariff2 }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff2 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff1, tariff2 }, It.IsAny<IStagingDataWrapper>(), metadata, true))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff1, tariff2 } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff1, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff2, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.IsUserOverride(tariff1)).Returns(false);
			safeProvider.Setup(x => x.IsUserOverride(tariff2)).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff1)).Returns(dateTimeRange1);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff2)).Returns(dateTimeRange2);
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, new List<object>(), metadata)).Returns(new Safe.RefCusTariff());

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);

			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith($@"Overlapping is caused by new data from XML.
RefCusTariff has overlapping of Effective DateRange;
New data in StageDB:
[{stageStartDate.GetFormatString()}~{stageEndDate.GetFormatString()}, {stagingTariffPK}]
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2000/01/01 00:00:00, {tariffPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {tariffPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}"), () => updater.Update(new[] { wrapper1.Object }));

			safeProvider.Verify(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, null, new List<object>(), metadata), Times.Never);
			safeProvider.Verify(x => x.Update(tariff1, wrapper1.Object, metadata, It.IsAny<IdenticalLevel>(), 0), Times.Never);
			safeProvider.Verify(x => x.Update(tariff2, wrapper1.Object, metadata, It.IsAny<IdenticalLevel>(), 0), Times.Never);
		}

		protected static IEnumerable MiddleOverlappingTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(1985, 01, 01), new DateTime(2005, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_MiddleIsOverlapped_1"
				};
				yield return new TestCaseData(new DateTime(1990, 01, 01), new DateTime(2005, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_MiddleIsOverlapped_2"
				};
				yield return new TestCaseData(new DateTime(1995, 01, 01), new DateTime(2005, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_MiddleIsOverlapped_3"
				};
				yield return new TestCaseData(new DateTime(1999, 12, 31), new DateTime(2010, 01, 01))
				{
					TestName = "{m}_InsertHistoricalData_MiddleIsOverlapped_4"
				};
			}
		}

		[Test]
		public void InsertMultipleHistoricalData_MiddleIsOverlapped()
		{
			var stagingAppPk1 = Guid.NewGuid();
			var stagingAppPk2 = Guid.NewGuid();
			var wrapperEffectiveDateRange1 = new DateTimeRange(new DateTime(2020, 01, 01), new DateTime(2021, 01, 01));
			var wrapperEffectiveDateRange2 = new DateTimeRange(new DateTime(2021, 01, 01), new DateTime(2022, 01, 01));
			var safeRepository = new Mock<ISafeRepository>().Object;
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var rate1 = new Safe.RefCusRate { ZZ2_PK = ratePK1, ZZ2_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZ2_EndDate = new DateTimeOffset(2079, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var rate2 = new Safe.RefCusRate { ZZ2_PK = ratePK2, ZZ2_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZ2_EndDate = new DateTimeOffset(2079, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app1 = new Safe.RefCusApplicability { ZZT_PK = appPK1, ZZT_StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app2 = new Safe.RefCusApplicability { ZZT_PK = appPK2, ZZT_StartDate = new DateTimeOffset(2021, 6, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2022, 6, 1, 0, 0, 0, TimeSpan.Zero) };
			var rateApp1 = new Safe.RefCusRateApplicability(rate1, app1, safeRepository);
			var rateApp2 = new Safe.RefCusRateApplicability(rate2, app2, safeRepository);
			var rateApp3 = new Safe.RefCusRateApplicability(safeRepository);
			var dateTimeRange1 = new DateTimeRange(new DateTime(2025, 01, 01), new DateTime(2026, 01, 01));
			var dateTimeRange2 = new DateTimeRange(new DateTime(2021, 06, 01), new DateTime(2022, 06, 01));
			var propertyNames = new[] {
				nameof(Safe.RefCusRateApplicability.S01_AdditionalCode), nameof(Safe.RefCusRateApplicability.S01_RateFormula), nameof(Safe.RefCusRateApplicability.S01_StartDate), nameof(Safe.RefCusRateApplicability.S01_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperOriginalPK()).Returns(stagingAppPk1);
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRateApplicability));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRateApplicability));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(wrapperEffectiveDateRange1);
			var wrapper2 = new Mock<IStagingDataWrapper>();
			wrapper2.SetupGet(x => x.IsData).Returns(true);
			wrapper2.Setup(x => x.GetWrapperOriginalPK()).Returns(stagingAppPk2);
			wrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRateApplicability));
			wrapper2.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRateApplicability));
			wrapper2.Setup(x => x.GetDateTimeRange()).Returns(wrapperEffectiveDateRange2);
			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusRateApplicability>(new[] { wrapper1.Object, wrapper2.Object }, metadata)).Returns(new[] { rateApp1, rateApp2 });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRateApplicability>(new object[] { rateApp1, rateApp2 }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusRateApplicability[]>() { { 0, new[] { rateApp1 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRateApplicability>(new object[] { rateApp1, rateApp2 }, It.IsAny<IStagingDataWrapper>(), metadata, true))
				.Returns(new Dictionary<int, Safe.RefCusRateApplicability[]>() { { 0, new[] { rateApp1, rateApp2 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRateApplicability>(new object[] { rateApp1, rateApp2, rateApp3 }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusRateApplicability[]>() { { 0, new[] { rateApp1 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRateApplicability>(new object[] { rateApp1, rateApp2, rateApp3 }, It.IsAny<IStagingDataWrapper>(), metadata, true))
				.Returns(new Dictionary<int, Safe.RefCusRateApplicability[]>() { { 0, new[] { rateApp1, rateApp2, rateApp3 } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(rateApp1, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.GetIdenticalLevel(rateApp2, wrapper2.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.IsUserOverride(rateApp1)).Returns(false);
			safeProvider.Setup(x => x.IsUserOverride(rateApp2)).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(rateApp1)).Returns(dateTimeRange1);
			safeProvider.Setup(x => x.GetDateTimeRange(rateApp2)).Returns(dateTimeRange2);
			safeProvider.Setup(x => x.GetDateTimeRange(rateApp3)).Returns(wrapperEffectiveDateRange1);
			safeProvider.Setup(x => x.Create<Safe.RefCusRateApplicability>(wrapper1.Object, null, It.IsAny<List<object>>(), metadata)).Returns(rateApp3);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith($@"Overlapping is caused by new data from XML.
RefCusApplicability has overlapping of Effective DateRange;
New data in StageDB:
[{wrapperEffectiveDateRange2.StartDate.GetFormatString()}~{wrapperEffectiveDateRange2.EndDate.GetFormatString()}, {stagingAppPk2}]
Data processed but not yet committed to SafeDB:
[2020/01/01 00:00:00~2021/01/01 00:00:00]
Existing DateRanges in SafeDB:
[2021/06/01 00:00:00~2022/06/01 00:00:00, {appPK2}]
[2025/01/01 00:00:00~2026/01/01 00:00:00, {appPK1}]
ErrorCode: {ErrorCodes.OverlappingDateRange}"), () => updater.Update(new[] { wrapper1.Object, wrapper2.Object }));
			safeProvider.Verify(x => x.Create<Safe.RefCusRateApplicability>(wrapper1.Object, null, It.IsAny<List<object>>(), metadata), Times.Once);
		}

		[Test]
		public void UpdateIfWrapperStartDateWrapperEndDateAndSafeStartDateAreEqual()
		{
			var rate = new Safe.RefExchangeRateZZ() { ZZN_Rate = 1, ZZN_StartDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset(), ZZN_EndDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset() };

			var dateTimeRange = new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01));
			var propertyNames = new[] {
				nameof(Safe.RefExchangeRateZZ.ZZN_Rate), nameof(Safe.RefExchangeRateZZ.ZZN_StartDate), nameof(Safe.RefExchangeRateZZ.ZZN_EndDate) };
			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[0])).Returns(1);
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[1])).Returns(new DateTime(2010, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetWrapperValue(propertyNames[2])).Returns(new DateTime(2010, 01, 01).ToUTCDateTimeOffset());
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefExchangeRateZZ));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefExchangeRateZZ));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2010, 01, 01)));

			var metadata = new Mock<IMetadataProvider>().Object;

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefExchangeRateZZ>(new[] { wrapper1.Object }, metadata)).Returns(new[] { rate });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefExchangeRateZZ>(new object[] { rate }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefExchangeRateZZ[]>() { { 0, new[] { rate } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(rate, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.IsUserOverride(rate)).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(rate)).Returns(dateTimeRange);
			safeProvider.Setup(x => x.Create<Safe.RefExchangeRateZZ>(wrapper1.Object, null, new List<object>(), metadata)).Returns(new Safe.RefExchangeRateZZ());

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);

			var results = updater.Update(new[] { wrapper1.Object });

			safeProvider.Verify(x => x.Create<Safe.RefExchangeRateZZ>(wrapper1.Object, null, new List<object>(), metadata), Times.Never);
			safeProvider.Verify(x => x.Update(rate, wrapper1.Object, metadata, It.IsAny<IdenticalLevel>(), 0));
		}

		[Test]
		public void NewestIsNewerThanActualRecordBeingProcessedShouldNotProcessNewestObject()
		{
			var tariffType = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff
			{
				ZZ1_TariffCode = "7219349000",
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_CompositeKeyOnZZ5 = "15.72.03.19.3.4.20",
				ZZ1_Description = "Containing by weight less than 2,5 % of nickel",
				ZZ1_StartDate = new DateTime(2010, 01, 01, 0, 0, 0),
				ZZ1_EndDate = new DateTime(2079, 6, 6, 23, 59, 0),
				ZZ1_ZZI_TariffType = tariffType,
				ZZ1_PK = Guid.NewGuid()
			};

			var tariffInRange = new Safe.RefCusTariff
			{
				ZZ1_TariffCode = "7219349000",
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_CompositeKeyOnZZ5 = "15.72.03.19.3.4.20",
				ZZ1_Description = "Containing by weight less than 2,5 % of nickel",
				ZZ1_StartDate = tariff.ZZ1_StartDate.AddDays(-10),
				ZZ1_EndDate = tariff.ZZ1_StartDate,
				ZZ1_ZZI_TariffType = tariffType,
				ZZ1_PK = Guid.NewGuid()
			};

			var safeDateTimeRange = new DateTimeRange(new DateTime(2010, 01, 01, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0));
			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tariffType);
			wrapper1.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusTariff.ZZ1_StartDate))).Returns(tariffInRange.ZZ1_StartDate);
			wrapper1.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusTariff.ZZ1_EndDate))).Returns(tariffInRange.ZZ1_EndDate);
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(new DateTimeRange(tariffInRange.ZZ1_StartDate.DateTime, tariffInRange.ZZ1_EndDate.DateTime));

			var metadata = new Mock<IMetadataProvider>().Object;
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, It.IsAny<bool>()))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.MainContentIdentical);
			safeProvider.Setup(x => x.ShouldOverWriteAndNotExpire<Safe.RefCusTariff>()).Returns(false);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);

			safeProvider.Setup(x => x.GetSpecifiedDateTimeRangeObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(), It.IsAny<DateTimeRange>(), wrapper1.Object, metadata)).Returns(tariff);
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.Identical);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper1.Object });

			Assert.That(results.Length, Is.EqualTo(1));
			Assert.That(results.ElementAt(0).ParentPK, Is.EqualTo(tariff.ZZ1_PK));
		}

		[Test]
		public void IsExpirableSafeEndDateMightBeLesserThanStagingStartDate()
		{
			var tariffType = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff
			{
				ZZ1_TariffCode = "7219349000",
				ZZ1_Description = "A1",
				ZZ1_StartDate = new DateTime(2010, 01, 01, 0, 0, 0),
				ZZ1_EndDate = new DateTime(2021, 10, 7, 0, 0, 0),
				ZZ1_ZZI_TariffType = tariffType,
				ZZ1_PK = Guid.NewGuid()
			};
			var tariffDiff = new Safe.RefCusTariff
			{
				ZZ1_TariffCode = "7219349000",
				ZZ1_StartDate = new DateTime(2021, 10, 7, 0, 0, 0),
				ZZ1_EndDate = new DateTime(2079, 6, 6, 23, 59, 0),
				ZZ1_ZZI_TariffType = tariffType,
				ZZ1_Description = "A2",
				ZZ1_PK = Guid.NewGuid()
			};
			var safeDateTimeRange = new DateTimeRange(new DateTime(2010, 01, 01, 0, 0, 0), new DateTime(2021, 10, 7, 0, 0, 0));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2021, 10, 7, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0));

			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.SetupGet(x => x.IsData).Returns(true);
			wrapper1.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tariffType);
			wrapper1.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusTariff.ZZ1_StartDate))).Returns(tariffDiff.ZZ1_StartDate);
			wrapper1.Setup(x => x.GetWrapperValue(nameof(Safe.RefCusTariff.ZZ1_EndDate))).Returns(tariffDiff.ZZ1_EndDate);
			wrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper1.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var metadata = new Mock<IMetadataProvider>().Object;
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper1.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetIdenticalLevel(tariff, wrapper1.Object, metadata)).Returns((int)IdenticalLevel.HasChange);
			safeProvider.Setup(x => x.GetDateTimeRange(tariff)).Returns(safeDateTimeRange);
			safeProvider.Setup(x => x.IsExpirable<Safe.RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange)).Returns(true);
			safeProvider.Setup(x => x.Create<Safe.RefCusTariff>(wrapper1.Object, It.IsAny<object>(), It.IsAny<List<object>>(), metadata)).Returns(tariffDiff);

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, false);
			var results = updater.Update(new[] { wrapper1.Object });

			safeProvider.Verify(x => x.Expire(tariff, wrapper1.Object, metadata), Times.Never);
		}

		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Delete(bool isTariffData, bool isRateData)
		{
			var tariffPK = Guid.NewGuid();
			var tariffTypePK = Guid.NewGuid();
			var ratePK = Guid.NewGuid();
			var tariff = new Safe.RefCusTariff { ZZ1_PK = tariffPK, ZZ1_TariffCode = "001", ZZ1_ZZI_TariffType = tariffTypePK };
			var rate1 = new Safe.RefCusRate { ZZ2_PK = ratePK, ZZ2_ZZ1_Tariff = tariffPK, ZZ2_DataSetCode = "ZZ1", ZZ2_RateFormula = "0.1" };
			var rate2 = new Safe.RefCusRate { ZZ2_PK = ratePK, ZZ2_ZZ1_Tariff = tariffPK, ZZ2_DataSetCode = "ZZ1", ZZ2_RateFormula = "0.2" };
			var applicability1 = new Safe.RefCusApplicability { ZZT_PK = Guid.Parse("05E10A6B-6C4A-484C-A356-8EFE2093A160"), ZZT_ZZ2_Rate = ratePK, ZZT_DataSetCode = "ZZ2", ZZT_AdditionalCode = "A", ZZT_OrderNumber = "1" };
			var applicability2 = new Safe.RefCusApplicability { ZZT_PK = Guid.Parse("B83EA570-4DB2-45B0-AC85-3F0CE19C402F"), ZZT_ZZ2_Rate = ratePK, ZZT_DataSetCode = "ZZ2", ZZT_AdditionalCode = "B", ZZT_OrderNumber = "2" };

			var applicabilityWrapper = new Mock<IStagingDataWrapper>();
			applicabilityWrapper.SetupGet(x => x.IsData).Returns(true);
			applicabilityWrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusApplicability));
			applicabilityWrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusApplicability));

			var rateWrapper1 = new Mock<IStagingDataWrapper>();
			rateWrapper1.SetupGet(x => x.IsData).Returns(isRateData);
			rateWrapper1.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRate));
			rateWrapper1.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusApplicability), nameof(Stage.RefCusApplicability.ZZT_ZZ2_Rate)) });
			rateWrapper1.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusApplicability))).Returns(new[] { applicabilityWrapper.Object });

			var rateWrapper2 = new Mock<IStagingDataWrapper>();
			rateWrapper2.SetupGet(x => x.IsData).Returns(isRateData);
			rateWrapper2.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusRate));
			rateWrapper2.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusRate));

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.SetupGet(x => x.IsData).Returns(isTariffData);
			wrapper.Setup(x => x.GetStagingType()).Returns(typeof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapper.Setup(x => x.GetRelatedEntityTypesAndFKs(It.IsAny<bool>())).Returns(new[] {
				Tuple.Create(nameof(Stage.RefCusRate), nameof(Stage.RefCusRate.ZZ2_ZZ1_Tariff)) });
			wrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusRate))).Returns(new[] { rateWrapper1.Object, rateWrapper2.Object });

			var metadata = new Mock<IMetadataProvider>().Object;
			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(new[] { wrapper.Object }, metadata)).Returns(new[] { tariff });
			safeProvider.Setup(x => x.GetData<Safe.RefCusRate>(new[] { wrapper.Object }, metadata)).Returns(new[] { rate1, rate2 });
			safeProvider.Setup(x => x.GetData<Safe.RefCusApplicability>(new[] { wrapper.Object }, metadata)).Returns(new[] { applicability1, applicability2 });
			safeProvider.Setup(x => x.GetRelatedData(tariff, nameof(Safe.RefCusRate))).Returns(new[] { rate1, rate2 });
			safeProvider.Setup(x => x.GetRelatedData(rate1, nameof(Safe.RefCusApplicability))).Returns(new[] { applicability1, applicability2 });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(new object[] { tariff }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusTariff[]>() { { 0, new[] { tariff } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(new object[] { rate1, rate2 }, rateWrapper1.Object, metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, new[] { rate1 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusRate>(new object[] { rate1, rate2 }, rateWrapper2.Object, metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusRate[]>() { { 0, new[] { rate2 } } });
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusApplicability>(new object[] { applicability1, applicability2 }, It.IsAny<IStagingDataWrapper>(), metadata, false))
				.Returns(new Dictionary<int, Safe.RefCusApplicability[]>() { { 0, new[] { applicability1 } } });

			var updater = new SafeObjectUpdater(safeProvider.Object, metadata, overlappingCalculator, true);
			updater.Update(new[] { wrapper.Object });

			var tariffVerifyTime = isTariffData ? Times.Once() : Times.Never();
			var rateVerifyTime = isRateData ? Times.Once() : Times.Never();
			safeProvider.Verify(x => x.Delete<object>(tariff), tariffVerifyTime);
			safeProvider.Verify(x => x.Delete<object>(rate1), rateVerifyTime);
			safeProvider.Verify(x => x.Delete<object>(rate2), rateVerifyTime);
			safeProvider.Verify(x => x.Delete<object>(applicability1), Times.Once);
			safeProvider.Verify(x => x.Delete<object>(applicability2), Times.Never);
		}

		IOverlappingCalculator overlappingCalculator;
		[SetUp]
		public void SetUp()
		{
			overlappingCalculator = new OverlappingCalculator(false);
		}
	}
}
