using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefAccTaxRateVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefAccTaxRate()
				{
					ZAT_PK = Guid.NewGuid(),
					ZAT_RN_NKCountry = "AU",
					ZAT_ReferenceRateType = "RateType",
					ZAT_StartDate = DateTime.Now.AddYears(-1),
					ZAT_EndDate = DateTime.Now,
					ZAT_RateDenominator = 1,
					ZAT_RateNumerator = 2
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefAccTaxRate)data).ZAT_RateNumerator = 33;
			return true;
		}
	}
}
