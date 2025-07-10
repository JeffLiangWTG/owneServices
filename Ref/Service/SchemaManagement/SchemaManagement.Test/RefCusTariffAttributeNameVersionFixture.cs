using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusTariffAttributeNameVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();

			var tariffAttributeName = new RefCusTariffAttributeName
			{
				ZY6_PK = Guid.NewGuid(),
				ZY6_Name = "Test",
				ZY6_ZZI_NKTariffType = "1P1",
				ZY6_ZZZ_NKDataGrouping = "ZA",
				ZY6_Description = "Description",
				ZY6_ColumnCaption = "FT"
			};
			result.Add(tariffAttributeName);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			var tariffAttributeName = data as RefCusTariffAttributeName;
			tariffAttributeName.ZY6_Description = "Update Description";
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[]
			{
				new RefCusTariffAttributeName
				{
					ZY6_PK = Guid.Parse("C226C553-4772-4A81-B839-96643A2F065F"),
					ZY6_Name = "Test1",
					ZY6_ZZI_NKTariffType = "1P1",
					ZY6_ZZZ_NKDataGrouping = "ZA",
					ZY6_Description = "Description1",
					ZY6_ColumnCaption = "FT1",
				}
			};
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
				context.RefCusTariffTypes.Add(new RefCusTariffType()
				{
					ZZI_PK = Guid.NewGuid(),
					ZZI_TariffType = "1P1",
					ZZI_Description = "Schedule 1 Part 1",
					ZZI_ZZZ_NKDataGrouping = "ZA",
					ZZI_HasFormulaSpecificQuestions = false,
					ZZI_ZZR_RateType = rateTypeGuid,
					ZZI_ZZ9_NKNomenclatureGroupType = "",
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}

	}
}
