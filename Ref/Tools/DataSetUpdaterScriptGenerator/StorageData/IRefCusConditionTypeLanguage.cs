using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionTypeLanguage : IDataSetStorage
	{
		Guid ZXW_PK { get; set; }
		Guid ZXW_ZX2_ConditionType { get; set; }
		string ZXW_ZX6_NKLanguage { get; set; }
		string ZXW_Description { get; set; }
	}
}
