using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCurrencyVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[] { new RefCurrency
			{
				RX_PK = Guid.NewGuid(),
				RX_Code = "ANY",
				RX_Desc = "DESC",
				RX_IsActive = true,
				RX_ISOSubUnitRatio = 1,
				RX_SubUnitName = "SUB",
				RX_SubUnitRatio = 50,
				RX_Symbol = "$",
				RX_UnitName = "UN NAME"
			} };
		}

		protected override bool UpdateData(object data)
		{
			((RefCurrency)data).RX_Desc = "XX";
			return true;
		}
	}
}
