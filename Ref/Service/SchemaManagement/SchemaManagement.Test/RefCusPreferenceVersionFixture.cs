using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusPreferenceVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var preference = new RefCusPreference
			{
				ZZS_PK = Guid.NewGuid(),
				ZZS_Description = "PRE",
				ZZS_Preference = "P",
				ZZS_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(preference);
			result.Add(new RefCusPreferenceLanguage
			{
				ZX9_PK = Guid.NewGuid(),
				ZX9_ZZS_Preference = preference.ZZS_PK,
				ZX9_ZX6_NKLanguage = "EN",
				ZX9_Description = "English"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusPreference preference)
			{
				preference.ZZS_Description = "RE";
			}
			if (data is RefCusPreferenceLanguage language)
			{
				language.ZX9_Description = "XX";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusPreference
			{
				ZZS_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZS_Description = "BBB",
				ZZS_Preference = "B",
				ZZS_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusPreferenceLanguage language)
			{
				language.ZX9_ZZS_Preference = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
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
