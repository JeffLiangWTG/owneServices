using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefAirlineCommodityCode : IDataSetStorage
	{
		Guid RAC_PK { get; set; }
		string RAC_AirlineID { get; set; }
		string RAC_Code { get; set; }
		string RAC_Description { get; set; }
		string RAC_SpecialHandlingCodes { get; set; }
		IEnumerable<IRefAirlineProductCodeCommodityCodePivot> RefAirlineProductCodeCommodityCodePivots { get; }
	}
}
