using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusTradeGroupVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var group = new RefCusTradeGroup
			{
				ZZA_PK = Guid.NewGuid(),
				ZZA_Description = "A",
				ZZA_StartDate = new DateTime(1900, 01, 01),
				ZZA_EndDate = new DateTime(2079, 06, 06),
				ZZA_TradeGroup = "ZT",
				ZZA_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(group);
			result.Add(new RefCusTradeGroupLanguage
			{
				ZXD_PK = Guid.NewGuid(),
				ZXD_Description = "AAA",
				ZXD_ZX6_NKLanguage = "EN",
				ZXD_ZZA_TradeGroup = group.ZZA_PK
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusTradeGroup group)
			{
				group.ZZA_Description = "XX";
			}
			if (data is RefCusTradeGroupLanguage language)
			{
				language.ZXD_Description = "XX";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusTradeGroup
			{
				ZZA_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZA_Description = "B",
				ZZA_StartDate = new DateTime(1900, 01, 01),
				ZZA_EndDate = new DateTime(2079, 06, 06),
				ZZA_TradeGroup = "BB",
				ZZA_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusTradeGroupLanguage language)
			{
				language.ZXD_ZZA_TradeGroup = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
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
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
