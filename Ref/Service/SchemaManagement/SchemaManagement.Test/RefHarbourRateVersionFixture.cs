using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefHarbourRateVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefHarbourRate()
				{
					ZXF_PK = Guid.NewGuid(),
					ZXF_ZZZ_NKDataGrouping = "ZA",
					ZXF_Type = "TT",
					ZXF_Commodity = "TTT",
					ZXF_Mode = "CON",
					ZXF_Port = "PORT",
					ZXF_RateFormula = "FORMULA",
					ZXF_StartDate = DateTime.Now.AddYears(-1),
					ZXF_EndDate = DateTime.Now,
					ZXF_PortTaxType = "TAT"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefHarbourRate)data).ZXF_RateFormula = "FORMULA2";
			return true;
		}
	}
}
