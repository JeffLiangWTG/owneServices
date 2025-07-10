using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusTaxOrFeeTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();

			var taxOrFeeType = new RefCusTaxOrFeeType
			{
				ZX0_Description = "ANY",
				ZX0_PK = Guid.NewGuid(),
				ZX0_TaxOrFeeType = "TYP"
			};
			result.Add(taxOrFeeType);
			var taxOrFee = new RefCusTaxOrFee
			{
				ZZF_PK = Guid.NewGuid(),
				ZZF_ZX0_NKTaxOrFeeType = "TYP",
				ZZF_Code = "COD",
				ZZF_Description = "DESC",
				ZZF_Maximum = 1,
				ZZF_Minimum = 0,
				ZZF_Value = 20,
				ZZF_StartDate = DateTime.Today,
				ZZF_EndDate = DateTime.MaxValue,
				ZZF_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(taxOrFee);
			result.Add(new RefCusTaxOrFeeLanguage
			{
				ZXU_PK = Guid.NewGuid(),
				ZXU_Description = "D",
				ZXU_ZX6_NKLanguage = "EN",
				ZXU_ZZF_TaxOrFee = taxOrFee.ZZF_PK
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusTaxOrFeeType taxOrFeeType)
			{
				taxOrFeeType.ZX0_Description = "TEP";
			}
			else if (data is RefCusTaxOrFee taxOrFee)
			{
				taxOrFee.ZZF_Value = 10;
			}
			else if (data is RefCusTaxOrFeeLanguage taxOrFeeLanguage)
			{
				taxOrFeeLanguage.ZXU_Description = "DES";
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
	}
}
