using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefVesselZZ : IDataSetStorage
	{
		Guid ZZO_PK { get; set; }
		string ZZO_Code { get; set; }
		string ZZO_RadioCallSign { get; set; }
		string ZZO_VesselType { get; set; }
		string ZZO_RN_NKCountryOfReg { get; set; }
		string ZZO_LloydsNumber { get; set; }
		string ZZO_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCarrierVesselPivot> RefCarrierVesselPivots { get; }
		IEnumerable<IRefVesselArrival> RefVesselArrivals { get; }
	}
}
