using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefAirlineVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var refAirline = new RefAirline();
			refAirline.SetDefaultValues();
			refAirline.RM_PK = Guid.NewGuid();
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			refAirline.RM_ThreeLetterCode = "ABC";
			refAirline.RM_TwoCharacterCode = "AB";
			refAirline.RM_AirlineCountry = "Australia";
			refAirline.RM_AirlineName1 = "Name1";
			result.Add(refAirline);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefAirline refAirline)
			{
				refAirline.RM_AirlineCountry = "USA";
			}
			return true;
		}
	}
}
