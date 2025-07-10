using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusTariffTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var type = new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_TariffType = "1P1",
				ZZI_Description = "Schedule 1 Part 1",
				ZZI_ZZZ_NKDataGrouping = "ZA",
				ZZI_HasFormulaSpecificQuestions = false,
				ZZI_ZZR_RateType = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F"),
				ZZI_ZZ9_NKNomenclatureGroupType = "",
			};
			result.Add(type);
			var typeLanguage = new RefCusTariffTypeLanguage
			{
				ZXK_PK = Guid.NewGuid(),
				ZXK_ZZI_TariffType = type.ZZI_PK,
				ZXK_ZX6_NKLanguage = "EN",
				ZXK_Description = "Test"
			};
			result.Add(typeLanguage);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusTariffType type)
			{
				type.ZZI_Description = "XX";
			}
			if (data is RefCusTariffTypeLanguage typeLanguage)
			{
				typeLanguage.ZXK_Description = "New Test";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusTariffType
			{
				ZZI_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZI_TariffType = "BBB",
				ZZI_Description = "BBBB",
				ZZI_ZZZ_NKDataGrouping = "ZA",
				ZZI_HasFormulaSpecificQuestions = false,
				ZZI_ZZR_RateType = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F"),
				ZZI_ZZ9_NKNomenclatureGroupType = "",
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusTariffTypeLanguage language)
			{
				language.ZXK_ZZI_TariffType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var rateTypeGuid = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F");
				var rateType = context.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypeGuid,
					ZZR_RateType = "EXC",
					ZZR_Description = "Excise",
					ZZR_IsPayable = true,
					ZZR_ZZZ_NKDataGrouping = "ZA",
					ZZR_CustomsValueFormula = "",
					ZZR_RX_NKFormulaCurrency = string.Empty
				});
				context.RefLanguageTypes.Add(new RefLanguageType()
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Language = "EN",
					ZX6_Description = "EN"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
