using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefAccElectronicProcessingFeeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefAccElectronicProcessingFee()
				{
					EPF_PK = Guid.NewGuid(),
					EPF_SystemCode = "TST",
					EPF_Category = "AA",
					EPF_Code = "BB",
					EPF_Currency = "CC",
					EPF_Price = 10,
					EPF_ValidFrom = DateTime.Now,
					EPF_Description = "Des1",
					EPF_CountryCode = "AU",
					EPF_JobDirection = "DD"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefAccElectronicProcessingFee)data).EPF_Description = "Des2";
			return true;
		}
	}
}
