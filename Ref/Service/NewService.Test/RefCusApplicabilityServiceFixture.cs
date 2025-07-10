using System;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusApplicabilityServiceFixture
	{
		[Test]
		public void GetSetData()
		{
			var excludedTradeGroupArray = CreateRefCusExcludedTradeGroups();
			var applicabilityArray = CreateRefCusApplicabilitie();
			var appIdx = 0;
			var exIdx = 0;
			var result = applicabilityService.GetSetData(applicabilityArray, excludedTradeGroupArray, preference.ZZS_PK, ref appIdx, ref exIdx);
			var applicabilityResults = result.SelectMany(a => a);
			Assert.True(result.GetType().IsArray);
			Assert.AreEqual(2, applicabilityResults.Count());
			Assert.AreEqual("1", applicabilityResults.ElementAt(0).ZZT_OrderNumber);
			Assert.AreEqual("A", applicabilityResults.ElementAt(0).ZZT_AdditionalCode);
			Assert.AreEqual("A", applicabilityResults.ElementAt(0).RefCusTradeGroup.ZZA_TradeGroup);
			Assert.Null(applicabilityResults.ElementAt(0).RefCusTradeGroup1);

			Assert.AreEqual("3", applicabilityResults.ElementAt(1).ZZT_OrderNumber);
			Assert.AreEqual("C", applicabilityResults.ElementAt(1).ZZT_AdditionalCode);
			Assert.Null(applicabilityResults.ElementAt(1).RefCusTradeGroup);
			Assert.AreEqual("B", applicabilityResults.ElementAt(1).RefCusTradeGroup1.ZZA_TradeGroup);
		}

		[Test]
		public void GetExcludedTradeGroupChunk()
		{
			CreateRefCusExcludedTradeGroups();
			var tradeGroupChunk = applicabilityService.GetExcludedTradeGroupChunk("ZZS", null, now, null, 20, null, null);
			Assert.AreEqual(2, tradeGroupChunk.Length);
			Assert.AreEqual("ZZS", tradeGroupChunk[0].ZZC_DataSetCode);
			Assert.AreEqual(preference.ZZS_PK, tradeGroupChunk[0].ZZC_DataSetPK);
		}

		[Test]
		public void GetCusApplicabilityChunk()
		{
			CreateRefCusApplicabilitie();
			var applicabilityChunk = applicabilityService.GetCusApplicabilityChunk("ZZS", null, now, null, 20, null, null);
			Assert.AreEqual(3, applicabilityChunk.Length);
			Assert.AreEqual("A", applicabilityChunk[0].ZZT_AdditionalCode);
			Assert.AreEqual("1", applicabilityChunk[0].ZZT_OrderNumber);
			Assert.AreEqual("B", applicabilityChunk[1].ZZT_AdditionalCode);
			Assert.AreEqual("2", applicabilityChunk[1].ZZT_OrderNumber);
			Assert.AreEqual("C", applicabilityChunk[2].ZZT_AdditionalCode);
			Assert.AreEqual("3", applicabilityChunk[2].ZZT_OrderNumber);
		}

		RefCusExcludedTradeGroup[] CreateRefCusExcludedTradeGroups()
		{
			var excludedTradeGroup1 = repo.Create(() => new RefCusExcludedTradeGroup
			{
				ZZC_DataSetCode = "ZZS",
				ZZC_DataSetPK = preference.ZZS_PK,
				ZZC_ZZA_TradeGroup = tradeGroup.ZZA_PK
			});
			var excludedTradeGroup2 = repo.Create(() => new RefCusExcludedTradeGroup
			{
				ZZC_DataSetCode = "ZZS",
				ZZC_DataSetPK = preference.ZZS_PK,
				ZZC_ZZA_TradeGroup = tradeGroup1.ZZA_PK
			});
			return new RefCusExcludedTradeGroup[] { excludedTradeGroup1, excludedTradeGroup2 };
		}

		RefCusApplicability[] CreateRefCusApplicabilitie()
		{
			var applicability1 = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_DataSetCode = "ZZS",
				ZZT_DataSetPK = preference.ZZS_PK,
				ZZT_AdditionalCode = "A",
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroup.ZZA_PK
			});
			var applicability2 = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_DataSetCode = "ZZS",
				ZZT_DataSetPK = preference.ZZS_PK,
				ZZT_AdditionalCode = "B",
				ZZT_OrderNumber = "2",
				ZZT_ZZA_TradeGroup = tradeGroup.ZZA_PK,
				ZZT_ZZA_SecondTradeGroup = Guid.Empty
			});
			var applicability3 = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_DataSetCode = "ZZS",
				ZZT_DataSetPK = preference.ZZS_PK,
				ZZT_AdditionalCode = "C",
				ZZT_OrderNumber = "3",
				ZZT_ZZA_SecondTradeGroup = tradeGroup1.ZZA_PK
			});
			return new RefCusApplicability[] { applicability1, applicability2, applicability3 };
		}

		DateTime now;
		ObjectReferenceDataRepository repo;
		RefCusTradeGroup tradeGroup;
		RefCusTradeGroup tradeGroup1;
		RefCusPreference preference;
		RefCusApplicabilityService applicabilityService;

		[SetUp]
		public void Setup()
		{
			now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
			tradeGroup = repo.Create(() => new RefCusTradeGroup
			{
				ZZA_PK = Guid.NewGuid(),
				ZZA_TradeGroup = "A"
			});
			tradeGroup1 = repo.Create(() => new RefCusTradeGroup
			{
				ZZA_PK = Guid.NewGuid(),
				ZZA_TradeGroup = "B"
			});
			preference = new RefCusPreference
			{
				ZZS_PK = Guid.NewGuid(),
				ZZS_Description = "Description"
			};
			var conditionType = repo.Create(() => new RefCusConditionType
			{
				ZX2_PK = Guid.NewGuid(),
				ZX2_ConditionType = "A"
			});
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = preference.ZZS_PK,
				ZX1_DataSetCode = "ZZS",
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tradeGroup.ZZA_PK, RVC_DataSetId = 12, RVC_ParentCode = "ZZA", RVC_LastUpdatedUTC = now, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tradeGroup1.ZZA_PK, RVC_DataSetId = 12, RVC_ParentCode = "ZZA", RVC_LastUpdatedUTC = now, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = preference.ZZS_PK, RVC_DataSetId = 20, RVC_ParentCode = "ZZS", RVC_LastUpdatedUTC = now, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = conditionType.ZX2_PK, RVC_DataSetId = 25, RVC_ParentCode = "ZX2", RVC_LastUpdatedUTC = now, RVC_IsPublished = true });

			applicabilityService = new RefCusApplicabilityService(repo);
		}
	}
}
