using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	class RefDamageVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefDamage
				{
					RFM_PK = Guid.NewGuid(),
					RFM_Code = "BB",
					RFM_Group = "CEDEX",
					RFM_Description = "Description A"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefDamage refDamage)
			{
				refDamage.RFM_Description = "Modified";
			}
			return true;
		}
	}
}
