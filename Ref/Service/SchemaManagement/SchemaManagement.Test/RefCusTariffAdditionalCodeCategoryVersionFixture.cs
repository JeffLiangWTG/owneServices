using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusTariffAdditionalCodeCategoryVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefCusTariffAdditionalCodeCategory()
				{
					ZY3_PK = Guid.NewGuid(),
					ZY3_Category = "CAT",
					ZY3_Description = "TT",
					ZY3_ZZZ_NKDataGrouping = "ZA"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefCusTariffAdditionalCodeCategory)data).ZY3_Description = "XX";
			return true;
		}
	}
}
