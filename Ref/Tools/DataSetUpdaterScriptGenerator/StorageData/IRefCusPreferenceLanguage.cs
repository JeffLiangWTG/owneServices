using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusPreferenceLanguage : IDataSetStorage
	{
		Guid ZX9_PK { get; set; }
		string ZX9_ZX6_NKLanguage { get; set; }
		Guid ZX9_ZZS_Preference { get; set; }
		string ZX9_Description { get; set; }
	}
}
