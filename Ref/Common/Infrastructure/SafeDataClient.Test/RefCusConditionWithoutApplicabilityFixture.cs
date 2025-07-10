using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	internal class RefCusConditionWithoutApplicabilityFixture
	{
		[Test]
		public void RefCusConditionWithoutApplicabilityConstructor()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_TariffCode = "0001" };
			var nomenclatureGroup = new RefCusNomenclatureGroup { ZZ5_PK = Guid.NewGuid(), ZZ5_Value = "1" };
			var conditionValue = new RefCusConditionValue { ZX3_PK = Guid.NewGuid(), ZX3_Value = "A" };
			var conditionLanguage = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid(), ZXJ_ZX6_NKLanguage = "EN" };

			var condition = new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_ZZ1_Tariff = tariff.ZZ1_PK,
				ZX1_StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZX1_EndDate = new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZX1_ZX2_ConditionType = Guid.NewGuid(),
				ZX1_ZZ5_Nomenclature = Guid.NewGuid(),
				ZX1_Source = "AAA",
				ZX1_Comment = "BBB",
				ZX1_IsImport = true,
				ZX1_IsExport = false,
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_ZZS_Preference = Guid.NewGuid(),
				ZX1_ZZZ_NKDataGrouping = "CCC",
				ZX1_LogicalANDWithinGroup = 1,
				ZX1_ZY7_NKConditionCode = "DDD",
				ZX1_AdditionalComment = "EEE",
				ZX1_DataSetCode = "ZZ1",
				ZX1_DataSetPK = tariff.ZZ1_PK
			};
			condition.RefCusTariff = tariff;
			condition.RefCusNomenclatureGroup = nomenclatureGroup;
			condition.RefCusConditionValues.Add(conditionValue);
			condition.RefCusConditionLanguages.Add(conditionLanguage);

			var safeRepository = new Mock<ISafeRepository>().Object;
			var result = new RefCusConditionWithoutApplicability(condition, safeRepository);
			Assert.AreEqual(condition.ZX1_ZZ1_Tariff, result.ZX1_ZZ1_Tariff);
			Assert.AreEqual(condition.ZX1_ZX2_ConditionType, result.ZX1_ZX2_ConditionType);
			Assert.AreEqual(condition.ZX1_ZZ5_Nomenclature, result.ZX1_ZZ5_Nomenclature);
			Assert.AreEqual(condition.ZX1_ZZS_Preference, result.ZX1_ZZS_Preference);
			Assert.AreEqual(condition.ZX1_StartDate, result.ZX1_StartDate);
			Assert.AreEqual(condition.ZX1_EndDate, result.ZX1_EndDate);
			Assert.AreEqual(condition.ZX1_Source, result.ZX1_Source);
			Assert.AreEqual(condition.ZX1_Comment, result.ZX1_Comment);
			Assert.AreEqual(condition.ZX1_ZZZ_NKDataGrouping, result.ZX1_ZZZ_NKDataGrouping);
			Assert.AreEqual(condition.ZX1_ZY7_NKConditionCode, result.ZX1_ZY7_NKConditionCode);
			Assert.AreEqual(condition.ZX1_AdditionalComment, result.ZX1_AdditionalComment);
			Assert.AreEqual(condition.ZX1_DataSetCode, result.ZX1_DataSetCode);
			Assert.AreEqual(condition.ZX1_DataSetPK, result.ZX1_DataSetPK);

			Assert.AreEqual(condition.RefCusTariff, result.RefCusTariff);
			Assert.AreEqual(condition.RefCusNomenclatureGroup, result.RefCusNomenclatureGroup);
			Assert.AreEqual(condition.RefCusConditionValues, result.RefCusConditionValues);
			Assert.AreEqual(condition.RefCusConditionLanguages, result.RefCusConditionLanguages);
			Assert.AreEqual(0, condition.RefCusApplicabilities.Count);
		}

		[Test]
		public void BuildNonPersistentObjects_Tariff()
		{
			var tariff = new RefCusTariff();
			var condition1 = new RefCusCondition { ZX1_Comment = "1" };
			var condition2 = new RefCusCondition { ZX1_Comment = "2" };
			var condition3 = new RefCusCondition { ZX1_Comment = "3" };
			var app1 = new RefCusApplicability { ZZT_OrderNumber = "1" };
			condition1.RefCusApplicabilities.Add(app1);
			tariff.RefCusConditions.Add(condition1);
			tariff.RefCusConditions.Add(condition2);
			tariff.RefCusConditions.Add(condition3);

			tariff.BuildNonPersistentObjects(new Mock<ISafeRepository>().Object);
			Assert.AreEqual(2, tariff.RefCusConditionWithoutApplicabilities.Count);
			Assert.True(tariff.RefCusConditionWithoutApplicabilities.Any(x => x.ZX1_Comment == "2"));
			Assert.True(tariff.RefCusConditionWithoutApplicabilities.Any(x => x.ZX1_Comment == "3"));
		}

		[Test]
		public void BuildNonPersistentObjects_NomenclatureGroup()
		{
			var nomenclatureGroup = new RefCusNomenclatureGroup();
			var condition1 = new RefCusCondition { ZX1_Comment = "1" };
			var condition2 = new RefCusCondition { ZX1_Comment = "2" };
			var condition3 = new RefCusCondition { ZX1_Comment = "3" };
			var app1 = new RefCusApplicability { ZZT_OrderNumber = "1" };
			condition1.RefCusApplicabilities.Add(app1);
			nomenclatureGroup.RefCusConditions.Add(condition1);
			nomenclatureGroup.RefCusConditions.Add(condition2);
			nomenclatureGroup.RefCusConditions.Add(condition3);

			nomenclatureGroup.BuildNonPersistentObjects(new Mock<ISafeRepository>().Object);
			Assert.AreEqual(2, nomenclatureGroup.RefCusConditionWithoutApplicabilities.Count);
			Assert.True(nomenclatureGroup.RefCusConditionWithoutApplicabilities.Any(x => x.ZX1_Comment == "2"));
			Assert.True(nomenclatureGroup.RefCusConditionWithoutApplicabilities.Any(x => x.ZX1_Comment == "3"));
		}
	}
}
