using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	class RefMRComponentCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefMRComponentCode
				{
					RCC_PK = Guid.NewGuid(),
					RCC_Code = "AA",
					RCC_Group = "CEDEX",
					RCC_Description = "Description A",
					RCC_IsActive = true,
					RCC_Machinery = true,
					RCC_Structural = true,
					RCC_TankCleaning = true,
					RCC_TankRepair = true,
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefMRComponentCode componentCode)
			{
				componentCode.RCC_Description = "Modified";
			}
			return true;
		}
	}
}
