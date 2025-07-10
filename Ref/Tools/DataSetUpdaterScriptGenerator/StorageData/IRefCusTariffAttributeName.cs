using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffAttributeName : IDataSetStorage
	{
		Guid ZY6_PK { get; set; }
		string ZY6_Name { get; set; }
		string ZY6_Description { get; set; }
		string ZY6_ZZI_NKTariffType { get; set; }
		string ZY6_ZZZ_NKDataGrouping { get; set; }
		string ZY6_ColumnCaption { get; set; }
	}
}
