using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusRateTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var type = new RefCusRateType
			{
				ZZR_PK = Guid.NewGuid(),
				ZZR_RateType = "EXC",
				ZZR_Description = "Excise",
				ZZR_IsPayable = true,
				ZZR_CustomsValueFormula = "",
				ZZR_ZZZ_NKDataGrouping = "ZA",
				ZZR_RX_NKFormulaCurrency = "",
				ZZR_IsExport = true
			};
			result.Add(type);
			var rateCode = new RefCusRateCode
			{
				ZY1_PK = Guid.NewGuid(),
				ZY1_ZZR_RateType = type.ZZR_PK,
				ZY1_RateCode = "NEW",
				ZY1_Description = "NEW",
				ZY1_InternalUse = false
			};
			result.Add(rateCode);
			var rateCodeLanguage = new RefCusRateCodeLanguage
			{
				ZXC_PK = Guid.NewGuid(),
				ZXC_ZY1_RateCode = rateCode.ZY1_PK,
				ZXC_Description = "Test",
				ZXC_ZX6_NKLanguage = "EN"
			};
			result.Add(rateCodeLanguage);
			var typeLanguage = new RefCusRateTypeLanguage
			{
				ZXT_PK = Guid.NewGuid(),
				ZXT_ZZR_RateType = type.ZZR_PK,
				ZXT_ZX6_NKLanguage = "EN",
				ZXT_Description = "Description"
			};
			result.Add(typeLanguage);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusRateType type)
			{
				type.ZZR_Description = "XX";
			}
			if (data is RefCusRateCode code)
			{
				code.ZY1_Description = "New2";
			}
			if (data is RefCusRateCodeLanguage codeLanguage)
			{
				codeLanguage.ZXC_Description = "Test2";
			}
			if (data is RefCusRateTypeLanguage typeLanguage)
			{
				typeLanguage.ZXT_Description = "Description2";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusRateType
			{
				ZZR_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZR_RateType = "BBB",
				ZZR_Description = "BBB",
				ZZR_IsPayable = true,
				ZZR_CustomsValueFormula = "",
				ZZR_ZZZ_NKDataGrouping = "ZA",
				ZZR_RX_NKFormulaCurrency = "",
				ZZR_IsExport = true
			},
			new RefCusRateCode
			{
				ZY1_PK = Guid.Parse("79D43F75-C5A1-4D3A-9C1A-48814D635DBA"),
				ZY1_ZZR_RateType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZY1_RateCode = "CCC",
				ZY1_Description = "CCC",
				ZY1_InternalUse = false
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusRateCode code)
			{
				code.ZY1_ZZR_RateType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCusRateCodeLanguage codeLanguage)
			{
				codeLanguage.ZXC_ZY1_RateCode = Guid.Parse("79D43F75-C5A1-4D3A-9C1A-48814D635DBA");
				return true;
			}
			if (data is RefCusRateTypeLanguage typeLanguage)
			{
				typeLanguage.ZXT_ZZR_RateType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefLanguageTypes.Add(new RefLanguageType()
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "EN",
					ZX6_Language = "EN"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
