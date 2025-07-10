using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefAirlineProductCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var refAirlineProductCode = new RefAirlineProductCode();
			refAirlineProductCode.SetDefaultValues();
			refAirlineProductCode.RAR_PK = Guid.NewGuid();
			refAirlineProductCode.RAR_AirlineID = "123";
			refAirlineProductCode.RAR_Code = "ABC";
			refAirlineProductCode.RAR_Description = "DEF";
			result.Add(refAirlineProductCode);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefAirlineProductCode refAirlineProductCode)
			{
				refAirlineProductCode.RAR_Description = "XYZ";
			}
			return true;
		}
	}
}
