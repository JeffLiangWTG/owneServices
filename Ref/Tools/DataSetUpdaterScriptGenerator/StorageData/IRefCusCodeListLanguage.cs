using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeListLanguage : IDataSetStorage
	{
		Guid ZXA_PK { get; set; }
		string ZXA_ZX6_NKLanguage { get; set; }
		Guid ZXA_ZZD_CodeList { get; set; }
		string ZXA_Description { get; set; }
	}
}
