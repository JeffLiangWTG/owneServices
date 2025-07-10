using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefHarbourRate : IDataSetStorage
	{
		Guid ZXF_PK { get; set; }
		string ZXF_Type { get; set; }
		string ZXF_Port { get; set; }
		string ZXF_Mode { get; set; }
		string ZXF_Commodity { get; set; }
		DateTime ZXF_StartDate { get; set; }
		DateTime ZXF_EndDate { get; set; }
		string ZXF_RateFormula { get; set; }
		string ZXF_ZZZ_NKDataGrouping { get; set; }
		string ZXF_PortTaxType { get; set; }
	}
}
