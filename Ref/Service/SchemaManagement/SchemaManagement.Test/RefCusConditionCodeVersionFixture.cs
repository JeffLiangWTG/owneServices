using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusConditionCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var conditionCode = new RefCusConditionCode
			{
				ZY7_PK = Guid.NewGuid(),
				ZY7_Description = "DESC",
				ZY7_ConditionCode = "ABC",
				ZY7_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(conditionCode);
			result.Add(new RefCusConditionCodeLanguage
			{
				ZY8_Description = "D",
				ZY8_PK = Guid.NewGuid(),
				ZY8_ZY7_ConditionCode = conditionCode.ZY7_PK,
				ZY8_ZX6_NKLanguage = "EN"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusConditionCode conditionCode)
			{
				conditionCode.ZY7_Description = "DES";
			}
			else if (data is RefCusConditionCodeLanguage conditionCodeLanguage)
			{
				conditionCodeLanguage.ZY8_Description = "DES";
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
			return new object[] { new RefCusConditionCode
			{
				ZY7_PK = Guid.Parse("329CD9F0-5B4E-47B8-9DF4-285BFBD35537"),
				ZY7_Description = "Description",
				ZY7_ZZZ_NKDataGrouping = "ZA",
				ZY7_ConditionCode = "BCD",
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusConditionCodeLanguage language)
			{
				language.ZY8_ZY7_ConditionCode = Guid.Parse("329CD9F0-5B4E-47B8-9DF4-285BFBD35537");
				return true;
			}
			return false;
		}
	}
}
