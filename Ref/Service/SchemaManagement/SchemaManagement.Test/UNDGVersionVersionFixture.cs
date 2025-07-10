using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGVersionVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return
			[
				new UNDGVersion
				{
					DV_PK = Guid.NewGuid(),
					DV_Name = "version 1",
					DV_Standard = "ADN",
					DV_IsActive = true
				}
			];
		}

		protected override bool UpdateData(object data)
		{
			((UNDGVersion)data).DV_Name = "version 2";
			return true;
		}
	}
}
