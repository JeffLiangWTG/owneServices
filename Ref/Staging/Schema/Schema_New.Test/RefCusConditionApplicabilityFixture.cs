using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusConditionApplicabilityFixture
	{
		[Test]
		public void Constructor()
		{
			var cond = new RefCusCondition
			{
				ZX1_ZX2_NKConditionType = "AAA",
				ZX1_ZX2_ZZZ_NKDataGrouping = "BBB",
				ZX1_ZZ1_Tariff = Guid.NewGuid(),
				ZX1_ZZ5_Nomenclature = Guid.NewGuid(),
				ZX1_StartDate = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZX1_EndDate = new DateTime(2009, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZX1_Source = "CCC",
				ZX1_Comment = "DDD",
				ZX1_IsImport = true,
				ZX1_IsExport = false,
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_ZZZ_NKDataGrouping = "EEE",
				ZX1_ZZS_NKPreference = "FFF",
				ZX1_ZZS_ZZZ_NKDataGrouping = "GGG",
				ZX1_LogicalANDWithinGroup = new byte(),
				ZX1_ZY7_NKConditionCode = "HHH",
				ZX1_AdditionalComment = "III",
				RefCusConditionValues = new HashSet<RefCusConditionValue> { new RefCusConditionValue(), new RefCusConditionValue() },
				RefCusConditionLanguages = new HashSet<RefCusConditionLanguage> { new RefCusConditionLanguage(), new RefCusConditionLanguage(), new RefCusConditionLanguage() }
			};
			var app = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "C999",
				ZZT_OrderNumber = "S001",
				ZZT_StartDate = new DateTime(2002, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZZT_EndDate = new DateTime(2008, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZZT_ZZA_NKTradeGroup = "EEE",
				ZZT_ZZA_NKSecondTradeGroup = null,
				RefCusExcludedTradeGroups = new HashSet<RefCusExcludedTradeGroup> { new RefCusExcludedTradeGroup() }
			};
			var condApp = new RefCusConditionApplicability(cond, app);

			Assert.That(condApp.S07_ZZ1_Tariff == cond.ZX1_ZZ1_Tariff);
			Assert.That(condApp.S07_ZX2_NKConditionType == cond.ZX1_ZX2_NKConditionType);
			Assert.That(condApp.S07_ZX2_ZZZ_NKDataGrouping == cond.ZX1_ZX2_ZZZ_NKDataGrouping);
			Assert.That(condApp.S07_ZZ5_Nomenclature == cond.ZX1_ZZ5_Nomenclature);
			Assert.That(condApp.S07_StartDate == (app.ZZT_StartDate > cond.ZX1_StartDate ? app.ZZT_StartDate : cond.ZX1_StartDate));
			Assert.That(condApp.S07_EndDate == (app.ZZT_EndDate < cond.ZX1_EndDate ? app.ZZT_EndDate : cond.ZX1_EndDate));
			Assert.That(condApp.S07_Source == cond.ZX1_Source);
			Assert.That(condApp.S07_Comment == cond.ZX1_Comment);
			Assert.That(condApp.S07_IsImport == cond.ZX1_IsImport);
			Assert.That(condApp.S07_IsExport == cond.ZX1_IsExport);
			Assert.That(condApp.S07_ConditionValueTrueMeansStop == cond.ZX1_ConditionValueTrueMeansStop);
			Assert.That(condApp.S07_ZZZ_NKDataGrouping == cond.ZX1_ZZZ_NKDataGrouping);
			Assert.That(condApp.S07_ZZS_NKPreference == cond.ZX1_ZZS_NKPreference);
			Assert.That(condApp.S07_ZZS_ZZZ_NKDataGrouping == cond.ZX1_ZZS_ZZZ_NKDataGrouping);
			Assert.That(condApp.S07_LogicalANDWithinGroup == cond.ZX1_LogicalANDWithinGroup);
			Assert.That(condApp.S07_ZY7_NKConditionCode == cond.ZX1_ZY7_NKConditionCode);
			Assert.That(condApp.S07_AdditionalComment == cond.ZX1_AdditionalComment);
			Assert.That(condApp.S07_ZZA_NKTradeGroup == app.ZZT_ZZA_NKTradeGroup);
			Assert.That(condApp.S07_AdditionalCode == app.ZZT_AdditionalCode);
			Assert.That(condApp.S07_OrderNumber == app.ZZT_OrderNumber);
			Assert.That(condApp.S07_ZZA_NKSecondTradeGroup == app.ZZT_ZZA_NKSecondTradeGroup);

			Assert.That(condApp.RefCusConditionApplicabilityValues.Count == 2);
			Assert.That(condApp.RefCusConditionApplicabilityLanguages.Count == 3);
			Assert.That(condApp.RefCusExcludedTradeGroupNews.Count == 1);
		}
	}
}
