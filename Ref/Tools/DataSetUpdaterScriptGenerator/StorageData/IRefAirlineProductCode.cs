using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefAirlineProductCode : IDataSetStorage
	{
		Guid RAR_PK { get; set; }
		string RAR_AirlineID { get; set; }
		string RAR_Code { get; set; }
		string RAR_Description { get; set; }
		IEnumerable<IRefAirlineProductCodeCommodityCodePivot> RefAirlineProductCodeCommodityCodePivots { get; }
	}
}
