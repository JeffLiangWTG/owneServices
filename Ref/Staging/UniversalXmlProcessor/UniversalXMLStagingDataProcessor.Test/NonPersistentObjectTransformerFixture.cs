using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class NonPersistentObjectTransformerFixture
	{
		[Test]
		public void TransformNonPersistentObjects()
		{
			var mockSafeDataProvider = new Mock<ISafeDataProvider>();
			var addedObjects = GetAddedObjects();
			var updatedObjects = GetUpdatedObjects();
			var deletedObjects = GetDeletedObjects();
			var allPersist = addedObjects.Union(updatedObjects).Union(deletedObjects);
			mockSafeDataProvider.Setup(x => x.GetAllPersistentObjects()).Returns(allPersist);
			var transformer = new NonPersistentObjectTransformer(mockSafeDataProvider.Object);
			var updaterResults = GenerateSafeObjectUpdaterResults();

			var actualResult = transformer.TransformNonPersistentObjects(updaterResults);
			Assert.That(actualResult.Length, Is.EqualTo(21));
			Assert.That(!actualResult.Any(x => x.ParentCode == "S01" || x.ParentCode == "S02" || x.ParentCode == "S03"));
			CollectionAssert.AreEquivalent(ExpectedResults.Select(x => x.ParentPK), actualResult.Select(x => x.ParentPK));
			CollectionAssert.AreEquivalent(ExpectedResults.Where(x => x.Action == ResultAction.Insert).Select(x => x.ParentCode),
				actualResult.Where(x => x.Action == ResultAction.Insert).Select(x => x.ParentCode));
			CollectionAssert.AreEquivalent(ExpectedResults.Where(x => x.Action == ResultAction.Update).Select(x => x.ParentCode),
			actualResult.Where(x => x.Action == ResultAction.Update).Select(x => x.ParentCode));
			CollectionAssert.AreEquivalent(ExpectedResults.Where(x => x.Action == ResultAction.Expire).Select(x => x.ParentCode),
			actualResult.Where(x => x.Action == ResultAction.Expire).Select(x => x.ParentCode));
		}

		[Test]
		public void TransformNonPersistentObjects_Added()
		{
			var mockSafeDataProvider = new Mock<ISafeDataProvider>();
			var addedObjects = GetAddedObjects();
			mockSafeDataProvider.Setup(x => x.GetAllPersistentObjects()).Returns(addedObjects);
			var transformer = new NonPersistentObjectTransformer(mockSafeDataProvider.Object);
			var updaterResults = GenerateSafeObjectUpdaterResults();

			var actualResult = transformer.TransformNonPersistentObjects(updaterResults);
			Assert.That(actualResult.Count(x => x.Action == ResultAction.Insert), Is.EqualTo(7));
		}

		[Test]
		public void TransformNonPersistentObjects_Updated()
		{
			var mockSafeDataProvider = new Mock<ISafeDataProvider>();
			var updatedObjects = GetUpdatedObjects();
			mockSafeDataProvider.Setup(x => x.GetAllPersistentObjects()).Returns(updatedObjects);
			var transformer = new NonPersistentObjectTransformer(mockSafeDataProvider.Object);
			var updaterResults = GenerateSafeObjectUpdaterResults();

			var actualResult = transformer.TransformNonPersistentObjects(updaterResults);
			Assert.That(actualResult.Count(x => x.Action == ResultAction.Update), Is.EqualTo(7));
		}

		[Test]
		public void TransformNonPersistentObjects_Deleted()
		{
			var mockSafeDataProvider = new Mock<ISafeDataProvider>();
			var deletedObjects = GetDeletedObjects();
			mockSafeDataProvider.Setup(x => x.GetAllPersistentObjects()).Returns(deletedObjects);
			var transformer = new NonPersistentObjectTransformer(mockSafeDataProvider.Object);
			var updaterResults = GenerateSafeObjectUpdaterResults();

			var actualResult = transformer.TransformNonPersistentObjects(updaterResults);
			Assert.That(actualResult.Count(x => x.Action == ResultAction.Expire), Is.EqualTo(7));
		}

		[Test]
		public void TransformNonPersistentObjects_CloneActionPK()
		{
			var mockSafeDataProvider = new Mock<ISafeDataProvider>();
			var persistentObjects = GetAddedObjects_Clone();
			mockSafeDataProvider.Setup(x => x.GetAllPersistentObjects()).Returns(persistentObjects);
			var transformer = new NonPersistentObjectTransformer(mockSafeDataProvider.Object);
			var updaterResults = GenerateSafeObjectUpdaterResults_Clone();
			var actualResult = transformer.TransformNonPersistentObjects(updaterResults);
			Assert.That(actualResult.Count(x => x.NewRecordForCloneActionPK.HasValue), Is.EqualTo(5));
			Assert.That(actualResult.Select(x => x.NewRecordForCloneActionPK).Distinct(), Is.EquivalentTo(new Guid[] { Added_ZZ1_PK, Added_ZZ2_PK1, Added_ZX1_PK1 }));
		}

		IEnumerable<object> GetAddedObjects()
		{
			var result = new List<object>
			{
				new RefCusTariff { ZZ1_PK = Added_ZZ1_PK }
			};

			var app1 = new RefCusApplicability { ZZT_PK = Added_ZZT_PK1 };
			var app2 = new RefCusApplicability { ZZT_PK = Added_ZZT_PK2 };
			var rate1 = new RefCusRate { ZZ2_PK = Added_ZZ2_PK1 };
			var rate2 = new RefCusRate { ZZ2_PK = Added_ZZ2_PK2 };
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_PK = Added_S01_PK1 };
			_ = new RefCusRateApplicability(rate2, app2, safeRepository) { S01_PK = Added_S01_PK2 };
			result.Add(app1);
			result.Add(app2);
			result.Add(rate1);
			result.Add(rate2);

			var uom = new RefCusRateUOM { ZXG_PK = Added_ZXG_PK };
			_ = new RefCusRateApplicabilityUOM(uom, rateApp1) { S02_PK = Added_S02_PK };
			result.Add(uom);

			var group = new RefCusExcludedTradeGroup { ZZC_PK = Added_ZZC_PK };
			var groupNew = new RefCusExcludedTradeGroupNew(group, rateApp1) { S03_PK = Added_S03_PK };
			result.Add(group);
			return result;
		}

		IEnumerable<object> GetUpdatedObjects()
		{
			var result = new List<object>
			{
				new RefCusTariff { ZZ1_PK = Updated_ZZ1_PK }
			};

			var app1 = new RefCusApplicability { ZZT_PK = Updated_ZZT_PK1 };
			var app2 = new RefCusApplicability { ZZT_PK = Updated_ZZT_PK2 };
			var rate1 = new RefCusRate { ZZ2_PK = Updated_ZZ2_PK1 };
			var rate2 = new RefCusRate { ZZ2_PK = Updated_ZZ2_PK2 };
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_PK = Updated_S01_PK1 };
			_ = new RefCusRateApplicability(rate2, app2, safeRepository) { S01_PK = Updated_S01_PK2 };
			result.Add(app1);
			result.Add(app2);
			result.Add(rate1);
			result.Add(rate2);

			var uom = new RefCusRateUOM { ZXG_PK = Updated_ZXG_PK };
			_ = new RefCusRateApplicabilityUOM(uom, rateApp1) { S02_PK = Updated_S02_PK };
			result.Add(uom);

			var group = new RefCusExcludedTradeGroup { ZZC_PK = Updated_ZZC_PK };
			_ = new RefCusExcludedTradeGroupNew(group, rateApp1) { S03_PK = Updated_S03_PK };
			result.Add(group);
			return result;
		}

		IEnumerable<object> GetDeletedObjects()
		{
			var result = new List<object>
			{
				new RefCusTariff { ZZ1_PK = Deleted_ZZ1_PK }
			};

			var app1 = new RefCusApplicability { ZZT_PK = Deleted_ZZT_PK1 };
			var app2 = new RefCusApplicability { ZZT_PK = Deleted_ZZT_PK2 };
			var rate1 = new RefCusRate { ZZ2_PK = Deleted_ZZ2_PK1 };
			var rate2 = new RefCusRate { ZZ2_PK = Deleted_ZZ2_PK2 };
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_PK = Deleted_S01_PK1 };
			_ = new RefCusRateApplicability(rate2, app2, safeRepository) { S01_PK = Deleted_S01_PK2 };
			result.Add(app1);
			result.Add(app2);
			result.Add(rate1);
			result.Add(rate2);

			var uom = new RefCusRateUOM { ZXG_PK = Deleted_ZXG_PK };
			_ = new RefCusRateApplicabilityUOM(uom, rateApp1) { S02_PK = Deleted_S02_PK };
			result.Add(uom);

			var group = new RefCusExcludedTradeGroup { ZZC_PK = Deleted_ZZC_PK };
			_ = new RefCusExcludedTradeGroupNew(group, rateApp1) { S03_PK = Deleted_S03_PK };
			result.Add(group);
			return result;
		}

		SafeObjectUpdaterResult[] GenerateSafeObjectUpdaterResults()
		{
			return
			[
				new SafeObjectUpdaterResult{ParentPK = Added_S01_PK1, ParentCode = "S01",  Action =  ResultAction.Insert, ExpirableAncestorPK = Added_ZZ1_PK},
				new SafeObjectUpdaterResult{ParentPK = Added_S01_PK2, ParentCode = "S01",  Action =  ResultAction.Insert, ExpirableAncestorPK = Added_ZZ1_PK},
				new SafeObjectUpdaterResult{ParentPK = Added_S02_PK, ParentCode = "S02",  Action =  ResultAction.Insert, ExpirableAncestorPK = Added_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Added_S03_PK, ParentCode = "S03",  Action =  ResultAction.Insert, ExpirableAncestorPK = Added_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Added_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Insert, ExpirableAncestorPK = null},

				new SafeObjectUpdaterResult{ParentPK = Updated_S01_PK1, ParentCode = "S01",  Action =  ResultAction.Update, ExpirableAncestorPK = Updated_ZZ1_PK},
				new SafeObjectUpdaterResult{ParentPK = Updated_S01_PK2, ParentCode = "S01",  Action =  ResultAction.Update, ExpirableAncestorPK = Updated_ZZ1_PK},
				new SafeObjectUpdaterResult{ParentPK = Updated_S02_PK, ParentCode = "S02",  Action =  ResultAction.Update, ExpirableAncestorPK = Updated_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Updated_S03_PK, ParentCode = "S03",  Action =  ResultAction.Update, ExpirableAncestorPK = Updated_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Updated_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Update, ExpirableAncestorPK = null},

				new SafeObjectUpdaterResult{ParentPK = Deleted_S01_PK1, ParentCode = "S01",  Action =  ResultAction.Expire, ExpirableAncestorPK = Deleted_ZZ1_PK},
				new SafeObjectUpdaterResult{ParentPK = Deleted_S01_PK2, ParentCode = "S01",  Action =  ResultAction.Expire, ExpirableAncestorPK = Deleted_ZZ1_PK},
				new SafeObjectUpdaterResult{ParentPK = Deleted_S02_PK, ParentCode = "S02",  Action =  ResultAction.Expire, ExpirableAncestorPK = Deleted_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Deleted_S03_PK, ParentCode = "S03",  Action =  ResultAction.Expire, ExpirableAncestorPK = Deleted_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Deleted_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Expire, ExpirableAncestorPK = null},
			];
		}

		IEnumerable<object> GetAddedObjects_Clone()
		{
			var result = new List<object>
			{
				new RefCusTariff { ZZ1_PK = Added_ZZ1_PK }
			};

			var app1 = new RefCusApplicability { ZZT_PK = Added_ZZT_PK1 };
			var app2 = new RefCusApplicability { ZZT_PK = Added_ZZT_PK2 };
			var rate = new RefCusRate { ZZ2_PK = Added_ZZ2_PK1 };
			var cond = new RefCusCondition { ZX1_PK = Added_ZX1_PK1 };
			rate.RefCusApplicabilities.Add(app1);
			cond.RefCusApplicabilities.Add(app2);
			// we need to create Non-Persistent objs to link non-persisted and persisted objects
			_ = new RefCusRateApplicability(rate, app1, safeRepository) { S01_PK = Added_S01_PK1 };
			_ = new RefCusConditionApplicability(cond, app2, safeRepository) { S07_PK = Added_S07_PK1 };
			result.Add(app1);
			result.Add(app2);
			result.Add(rate);
			result.Add(cond);

			return result;
		}

		SafeObjectUpdaterResult[] GenerateSafeObjectUpdaterResults_Clone()
		{
			return
			[
				new SafeObjectUpdaterResult{ParentPK = Added_S01_PK1, ParentCode = "S01",  Action =  ResultAction.Insert, NewRecordForCloneActionPK = Added_S01_PK1},
				new SafeObjectUpdaterResult{ParentPK = Added_S07_PK1, ParentCode = "S07",  Action =  ResultAction.Insert, NewRecordForCloneActionPK = Added_S07_PK1},
				new SafeObjectUpdaterResult{ParentPK = Added_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Insert, NewRecordForCloneActionPK = Added_ZZ1_PK},
			];
		}

		readonly SafeObjectUpdaterResult[] ExpectedResults =
		[
			new() {ParentPK = Added_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Insert},
			new() {ParentPK = Added_ZZT_PK1, ParentCode = "ZZT",  Action =  ResultAction.Insert},
			new() {ParentPK = Added_ZZT_PK2, ParentCode = "ZZT",  Action =  ResultAction.Insert},
			new() {ParentPK = Added_ZZ2_PK1, ParentCode = "ZZ2",  Action =  ResultAction.Insert},
			new() {ParentPK = Added_ZZ2_PK2, ParentCode = "ZZ2",  Action =  ResultAction.Insert},
			new() {ParentPK = Added_ZXG_PK, ParentCode = "ZXG",  Action =  ResultAction.Insert},
			new() {ParentPK = Added_ZZC_PK, ParentCode = "ZZC",  Action =  ResultAction.Insert},

			new() {ParentPK = Updated_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Update},
			new() {ParentPK = Updated_ZZT_PK1, ParentCode = "ZZT",  Action =  ResultAction.Update},
			new() {ParentPK = Updated_ZZT_PK2, ParentCode = "ZZT",  Action =  ResultAction.Update},
			new() {ParentPK = Updated_ZZ2_PK1, ParentCode = "ZZ2",  Action =  ResultAction.Update},
			new() {ParentPK = Updated_ZZ2_PK2, ParentCode = "ZZ2",  Action =  ResultAction.Update},
			new() {ParentPK = Updated_ZXG_PK, ParentCode = "ZXG",  Action =  ResultAction.Update},
			new() {ParentPK = Updated_ZZC_PK, ParentCode = "ZZC",  Action =  ResultAction.Update},

			new() {ParentPK = Deleted_ZZ1_PK, ParentCode = "ZZ1",  Action =  ResultAction.Expire},
			new() {ParentPK = Deleted_ZZT_PK1, ParentCode = "ZZT",  Action =  ResultAction.Expire},
			new() {ParentPK = Deleted_ZZT_PK2, ParentCode = "ZZT",  Action =  ResultAction.Expire},
			new() {ParentPK = Deleted_ZZ2_PK1, ParentCode = "ZZ2",  Action =  ResultAction.Expire},
			new() {ParentPK = Deleted_ZZ2_PK2, ParentCode = "ZZ2",  Action =  ResultAction.Expire},
			new() {ParentPK = Deleted_ZXG_PK, ParentCode = "ZXG",  Action =  ResultAction.Expire},
			new() {ParentPK = Deleted_ZZC_PK, ParentCode = "ZZC",  Action =  ResultAction.Expire},
		];

		static readonly Guid Added_S01_PK1 = Guid.NewGuid();
		static readonly Guid Added_S01_PK2 = Guid.NewGuid();
		static readonly Guid Added_S02_PK = Guid.NewGuid();
		static readonly Guid Added_S03_PK = Guid.NewGuid();
		static readonly Guid Added_ZZ1_PK = Guid.NewGuid();
		static readonly Guid Added_ZZT_PK1 = Guid.NewGuid();
		static readonly Guid Added_ZZT_PK2 = Guid.NewGuid();
		static readonly Guid Added_ZZ2_PK1 = Guid.NewGuid();
		static readonly Guid Added_ZZ2_PK2 = Guid.NewGuid();
		static readonly Guid Added_ZXG_PK = Guid.NewGuid();
		static readonly Guid Added_ZZC_PK = Guid.NewGuid();

		static readonly Guid Added_ZX1_PK1 = Guid.NewGuid();
		static readonly Guid Added_S07_PK1 = Guid.NewGuid();

		static readonly Guid Updated_S01_PK1 = Guid.NewGuid();
		static readonly Guid Updated_S01_PK2 = Guid.NewGuid();
		static readonly Guid Updated_S02_PK = Guid.NewGuid();
		static readonly Guid Updated_S03_PK = Guid.NewGuid();
		static readonly Guid Updated_ZZ1_PK = Guid.NewGuid();
		static readonly Guid Updated_ZZT_PK1 = Guid.NewGuid();
		static readonly Guid Updated_ZZT_PK2 = Guid.NewGuid();
		static readonly Guid Updated_ZZ2_PK1 = Guid.NewGuid();
		static readonly Guid Updated_ZZ2_PK2 = Guid.NewGuid();
		static readonly Guid Updated_ZXG_PK = Guid.NewGuid();
		static readonly Guid Updated_ZZC_PK = Guid.NewGuid();

		static readonly Guid Deleted_S01_PK1 = Guid.NewGuid();
		static readonly Guid Deleted_S01_PK2 = Guid.NewGuid();
		static readonly Guid Deleted_S02_PK = Guid.NewGuid();
		static readonly Guid Deleted_S03_PK = Guid.NewGuid();
		static readonly Guid Deleted_ZZ1_PK = Guid.NewGuid();
		static readonly Guid Deleted_ZZT_PK1 = Guid.NewGuid();
		static readonly Guid Deleted_ZZT_PK2 = Guid.NewGuid();
		static readonly Guid Deleted_ZZ2_PK1 = Guid.NewGuid();
		static readonly Guid Deleted_ZZ2_PK2 = Guid.NewGuid();
		static readonly Guid Deleted_ZXG_PK = Guid.NewGuid();
		static readonly Guid Deleted_ZZC_PK = Guid.NewGuid();

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			Constants.SetConfigFileForTest("CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test.config.json");
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
