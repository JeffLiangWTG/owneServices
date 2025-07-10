using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffAdditionalCodeCategory : IDataSetStorage
	{
		Guid ZY3_PK { get; set; }
		string ZY3_Category { get; set; }
		string ZY3_Description { get; set; }
		string ZY3_ZZZ_NKDataGrouping { get; set; }
	}
}
