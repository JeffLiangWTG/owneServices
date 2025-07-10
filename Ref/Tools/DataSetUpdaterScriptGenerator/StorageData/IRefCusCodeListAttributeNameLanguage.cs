using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeListAttributeNameLanguage : IDataSetStorage
	{
		Guid ZXH_PK { get; set; }
		string ZXH_ZX6_NKLanguage { get; set; }
		Guid ZXH_ZXE_CodeListAttributeName { get; set; }
		string ZXH_Description { get; set; }
		string ZXH_Name { get; set; }
		string ZXH_ColumnCaption { get; set; }
	}
}
