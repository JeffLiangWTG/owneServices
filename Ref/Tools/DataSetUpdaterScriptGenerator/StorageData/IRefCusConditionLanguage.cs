using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionLanguage : IDataSetStorage
	{
		Guid ZXJ_PK { get; set; }
		string ZXJ_ZX6_NKLanguage { get; set; }
		string ZXJ_Comment { get; set; }
		string ZXJ_Source { get; set; }
		Guid ZXJ_ZX1_Condition { get; set; }
		string ZXJ_AdditionalComment { get; set; }
	}
}
