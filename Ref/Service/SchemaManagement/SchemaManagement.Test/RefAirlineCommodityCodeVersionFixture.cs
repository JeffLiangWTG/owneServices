using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefAirlineCommodityCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var refAirlineProductCode = new RefAirlineCommodityCode();
			refAirlineProductCode.SetDefaultValues();
			refAirlineProductCode.RAC_PK = Guid.NewGuid();
			refAirlineProductCode.RAC_AirlineID = "123";
			refAirlineProductCode.RAC_Code = "ABC";
			refAirlineProductCode.RAC_Description = "DEF";
			refAirlineProductCode.RAC_SpecialHandlingCodes = "QWE";
			result.Add(refAirlineProductCode);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefAirlineCommodityCode refAirlineProductCode)
			{
				refAirlineProductCode.RAC_Description = "XYZ";
				refAirlineProductCode.RAC_SpecialHandlingCodes = "ASD";
			}
			return true;
		}
	}
}
