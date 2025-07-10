using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefEquipmentGradeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefEquipmentGrade()
				{
					REG_PK = Guid.NewGuid(),
					REG_Code = "TST",
					REG_Description = "DESC",
					REG_IsActive = true
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefEquipmentGrade)data).REG_Description = "DESC_ONE";
			return true;
		}
	}
}
