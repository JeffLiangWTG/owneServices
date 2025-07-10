using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusConditionValueTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var conditionValueType = new RefCusConditionValueType
			{
				ZX4_PK = Guid.NewGuid(),
				ZX4_Description = "DESC",
				ZX4_ZZZ_NKDataGrouping = "ZA",
				ZX4_ValueType = "1",
				ZX4_IsFormula = true
			};
			result.Add(conditionValueType);
			result.Add(new RefCusConditionValueTypeLanguage
			{
				ZXX_Description = "D",
				ZXX_PK = Guid.NewGuid(),
				ZXX_ZX4_ValueType = conditionValueType.ZX4_PK,
				ZXX_ZX6_NKLanguage = "EN"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusConditionValueType conditionValueType)
			{
				conditionValueType.ZX4_Description = "DES";
			}
			else if (data is RefCusConditionValueTypeLanguage conditionValueTypeLanguage)
			{
				conditionValueTypeLanguage.ZXX_Description = "DES";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusConditionValueType
			{
				ZX4_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZX4_Description = "BBB",
				ZX4_ZZZ_NKDataGrouping = "ZA",
				ZX4_ValueType = "B",
				ZX4_IsFormula = true
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			var language = data as RefCusConditionValueTypeLanguage;
			if (language != null)
			{
				language.ZXX_ZX4_ValueType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefLanguageTypes.Add(new RefLanguageType { ZX6_PK = Guid.NewGuid(), ZX6_Description = "English", ZX6_Language = "EN" });
				context.SaveChanges();
			}
		}
	}
}
