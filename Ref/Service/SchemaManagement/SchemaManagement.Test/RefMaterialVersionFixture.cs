using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	class RefMaterialVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefMaterial
				{
					RMC_PK = Guid.NewGuid(),
					RMC_Code = "AA",
					RMC_Group = "CEDEX",
					RMC_Description = "Description A"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefMaterial material)
			{
				material.RMC_Description = "Modified";
			}
			return true;
		}
	}
}
