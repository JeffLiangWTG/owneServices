using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusMap : IDataSetStorage
	{
		Guid ZZM_PK { get; set; }
		string ZZM_ZZP_NKMapType { get; set; }
		string ZZM_CW1orCommercialValue { get; set; }
		string ZZM_CustomsValue { get; set; }
		DateTime ZZM_StartDate { get; set; }
		DateTime ZZM_EndDate { get; set; }
		string ZZM_ZZZ_NKDataGrouping { get; set; }
	}
}
