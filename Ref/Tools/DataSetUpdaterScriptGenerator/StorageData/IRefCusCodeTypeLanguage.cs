using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeTypeLanguage : IDataSetStorage
	{
		Guid ZXI_PK { get; set; }
		string ZXI_ZX6_NKLanguage { get; set; }
		Guid ZXI_ZZK_CodeType { get; set; }
		string ZXI_Description { get; set; }
	}
}
