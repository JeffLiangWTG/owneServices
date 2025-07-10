using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefVesselArrival : IDataSetStorage
	{
		Guid ZYA_PK { get; set; }
		Guid ZYA_ZZO_Vessel { get; set; }
		string ZYA_VoyageNumber { get; set; }
		DateTime ZYA_ArrivalDate { get; set; }
		string ZYA_ArrivalPort { get; set; }
	}
}
