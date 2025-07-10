using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCarrierVesselPivot : IDataSetStorage
	{
		Guid ZZQ_PK { get; set; }
		Guid ZZQ_ZZ4 { get; set; }
		Guid ZZQ_ZZO { get; set; }
	}
}
