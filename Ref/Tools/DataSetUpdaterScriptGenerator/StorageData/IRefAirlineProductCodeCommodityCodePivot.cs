using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefAirlineProductCodeCommodityCodePivot
		: IDataSetStorage
	{
		Guid RPC_PK { get; set; }
		Guid RPC_RAR { get; set; }
		Guid RPC_RAC { get; set; }
		string RPC_AirlineID { get; set; }
	}
}
