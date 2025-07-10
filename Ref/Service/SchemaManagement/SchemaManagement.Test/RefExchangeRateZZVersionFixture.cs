using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefExchangeRateZZVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefExchangeRateZZ()
				{
					ZZN_PK = Guid.NewGuid(),
					ZZN_RN_NKCountry = "ZA",
					ZZN_ExRateType = "BNB",
					ZZN_Rate = 1,
					ZZN_RX_NKExCurrency = "ZAD",
					ZZN_AsPublished = "Test",
					ZZN_StartDate = new DateTime(2000,1,1),
					ZZN_EndDate = new DateTime(2010,1,1)
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefExchangeRateZZ)data).ZZN_AsPublished = "New";
			return true;
		}
	}
}
