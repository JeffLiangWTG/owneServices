using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusConditionTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var conditionType = new RefCusConditionType
			{
				ZX2_PK = Guid.NewGuid(),
				ZX2_Description = "DESC",
				ZX2_ZZZ_NKDataGrouping = "ZA",
				ZX2_ConditionClass = "CLASS",
				ZX2_ConditionType = "tp1234",
			};
			result.Add(conditionType);
			result.Add(new RefCusConditionTypeLanguage
			{
				ZXW_Description = "D",
				ZXW_PK = Guid.NewGuid(),
				ZXW_ZX2_ConditionType = conditionType.ZX2_PK,
				ZXW_ZX6_NKLanguage = "EN"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusConditionType conditionType)
			{
				conditionType.ZX2_Description = "DES";
			}
			else if (data is RefCusConditionTypeLanguage conditionTypeLanguage)
			{
				conditionTypeLanguage.ZXW_Description = "DES";
			}
			return true;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				context.SaveChanges();
			}
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusConditionType
			{
				ZX2_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZX2_Description = "BBBB",
				ZX2_ZZZ_NKDataGrouping = "ZA",
				ZX2_ConditionClass = "RATE",
				ZX2_ConditionType = "BBCDDE",
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusConditionTypeLanguage language)
			{
				language.ZXW_ZX2_ConditionType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
