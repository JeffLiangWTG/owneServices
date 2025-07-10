using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefVesselZZVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var vesselZZ = new RefVesselZZ
			{
				ZZO_PK = Guid.NewGuid(),
				ZZO_Code = "TEST",
				ZZO_RadioCallSign = "TST",
				ZZO_VesselType = "CV",
				ZZO_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(vesselZZ);
			result.Add(new RefVesselArrival
			{
				ZYA_PK = Guid.NewGuid(),
				ZYA_ZZO_Vessel = vesselZZ.ZZO_PK,
				ZYA_VoyageNumber = "1",
				ZYA_ArrivalDate = new DateTime(2025, 1, 1),
				ZYA_ArrivalPort = "AA"
			});
			return [.. result];
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefVesselZZ vesselZZ)
			{
				vesselZZ.ZZO_LloydsNumber = "Updated";
			}
			if (data is RefVesselArrival vesselArrival)
			{
				vesselArrival.ZYA_ArrivalPort = "BB";
			}
			return true;
		}
	}
}
