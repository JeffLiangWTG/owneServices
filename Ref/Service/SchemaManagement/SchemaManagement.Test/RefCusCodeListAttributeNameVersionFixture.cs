using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	class RefCusCodeListAttributeNameVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var attr = new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "AA",
				ZXE_Description = "AA",
				ZXE_ZZZ_NKDataGrouping = "ZA",
				ZXE_ZZK_NKCodeType = "PKG",
				ZXE_ValueDataType = "",
				ZXE_ColumnCaption = ""
			};
			result.Add(attr);
			result.Add(new RefCusCodeListAttributeNameLanguage
			{
				ZXH_PK = Guid.NewGuid(),
				ZXH_Description = "Des",
				ZXH_ZX6_NKLanguage = "EN",
				ZXH_ZXE_CodeListAttributeName = attr.ZXE_PK,
				ZXH_Name = "Des",
				ZXH_ColumnCaption = "Se"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusCodeListAttributeName attr)
			{
				attr.ZXE_Description = "BB";
			}
			if (data is RefCusCodeListAttributeNameLanguage language)
			{
				language.ZXH_Description = "CCC";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZXE_Name = "BB",
				ZXE_Description = "BB",
				ZXE_ZZZ_NKDataGrouping = "ZA",
				ZXE_ZZK_NKCodeType = "PKG",
				ZXE_ValueDataType = "",
				ZXE_ColumnCaption = ""
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			var language = data as RefCusCodeListAttributeNameLanguage;
			if (language != null)
			{
				language.ZXH_ZXE_CodeListAttributeName = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefCusCodeTypes.Add(new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = "PKG",
					ZZK_Description = "Package",
					ZZK_MaxLength = 0,
					ZZK_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				context.SaveChanges();
			}
		}
	}
}
