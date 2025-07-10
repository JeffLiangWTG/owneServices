using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusNomenclatureGroupVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var group = new RefCusNomenclatureGroup
			{
				ZZ5_PK = Guid.NewGuid(),
				ZZ5_ZZZ_NKDataGrouping = "ZA",
				ZZ5_CompositeKey = "FF",
				ZZ5_StartDate = DateTime.Now.AddDays(-1),
				ZZ5_EndDate = DateTime.Now.AddDays(1),
				ZZ5_Description = "DD",
				ZZ5_Value = "F",
				ZZ5_ZZ9_NKNomenclatureGroupType = "E",
			};
			result.Add(group);
			result.Add(new RefCusNomenclatureLanguage
			{
				ZX8_PK = Guid.NewGuid(),
				ZX8_ZZ5_NomenclatureGroup = group.ZZ5_PK,
				ZX8_ZX6_NKLanguage = "EN",
				ZX8_Description = "English"
			});
			result.Add(new RefCusNomenclatureGroupNote
			{
				ZZL_PK = Guid.NewGuid(),
				ZZL_ZX6_NKLanguage = "EN",
				ZZL_Note = "HE",
				ZZL_NoteType = "T",
				ZZL_ZZZ_NKDataGrouping = "ZA",
				ZZL_ZZ5_NomenclatureGroup = group.ZZ5_PK,
			});
			var condition = new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_ZZ5_Nomenclature = group.ZZ5_PK,
				ZX1_ZX2_ConditionType = condTypeGuid,
				ZX1_ZZZ_NKDataGrouping = "ZA",
				ZX1_Comment = "A",
				ZX1_EndDate = new DateTime(2079, 06, 06),
				ZX1_StartDate = new DateTime(1900, 01, 01),
				ZX1_Source = "B",
				ZX1_DataSetPK = group.ZZ5_PK,
				ZX1_ZY7_NKConditionCode = "BBB",
				ZX1_AdditionalComment = "comment"
			};
			result.Add(condition);
			var applicability = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "A",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = group.ZZ5_PK
			};
			result.Add(applicability);
			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicability.ZZT_PK,
				ZZC_DataSetPK = group.ZZ5_PK
			});
			result.Add(new RefCusConditionValue
			{
				ZX3_LogicalORWithinGroup = 0,
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = condition.ZX1_PK,
				ZX3_Value = "T",
				ZX3_ZX4_ValueType = valueTypeGuid,
				ZX3_DataSetPK = group.ZZ5_PK
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusNomenclatureGroup group)
			{
				group.ZZ5_Description = "XX";
			}
			if (data is RefCusNomenclatureLanguage language)
			{
				language.ZX8_Description = "XX";
			}
			if (data is RefCusNomenclatureGroupNote note)
			{
				note.ZZL_Note = "GG";
			}
			if (data is RefCusCondition condition)
			{
				condition.ZX1_Source = "D";
			}
			if (data is RefCusApplicability applicability)
			{
				applicability.ZZT_OrderNumber = "4";
			}
			if (data is RefCusConditionValue value)
			{
				value.ZX3_Value = "S";
			}
			if (data is RefCusExcludedTradeGroup excluded)
			{
				return false;
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusNomenclatureGroup
			{
				ZZ5_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZ5_ZZZ_NKDataGrouping = "ZA",
				ZZ5_CompositeKey = "BB",
				ZZ5_StartDate = DateTime.Now.AddDays(-1),
				ZZ5_EndDate = DateTime.Now.AddDays(1),
				ZZ5_Description = "BB",
				ZZ5_Value = "B",
				ZZ5_ZZ9_NKNomenclatureGroupType = "B",
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusNomenclatureGroupNote note)
			{
				note.ZZL_ZZ5_NomenclatureGroup = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCusNomenclatureLanguage language)
			{
				language.ZX8_ZZ5_NomenclatureGroup = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		Guid condTypeGuid;
		Guid tradeGroupGuid;
		Guid valueTypeGuid;

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				condTypeGuid = Guid.NewGuid();
				context.RefCusConditionTypes.Add(new RefCusConditionType
				{
					ZX2_PK = condTypeGuid,
					ZX2_ConditionClass = "RATE",
					ZX2_ConditionType = "B",
					ZX2_Description = "C",
					ZX2_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefCusConditionCodes.Add(new RefCusConditionCode
				{
					ZY7_PK = Guid.NewGuid(),
					ZY7_ConditionCode = "BBB",
					ZY7_Description = "C",
					ZY7_ZZZ_NKDataGrouping = "ZA"
				});
				tradeGroupGuid = Guid.NewGuid();
				context.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_PK = tradeGroupGuid,
					ZZA_Description = "A",
					ZZA_StartDate = new DateTime(1900, 01, 01),
					ZZA_EndDate = new DateTime(2079, 06, 06),
					ZZA_TradeGroup = "ZT",
					ZZA_ZZZ_NKDataGrouping = "ZA"
				});
				valueTypeGuid = Guid.NewGuid();
				context.RefCusConditionValueTypes.Add(new RefCusConditionValueType
				{
					ZX4_PK = valueTypeGuid,
					ZX4_Description = "A",
					ZX4_ValueType = "B",
					ZX4_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
