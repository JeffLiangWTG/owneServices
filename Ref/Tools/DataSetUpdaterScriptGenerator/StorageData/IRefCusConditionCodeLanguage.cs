using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionCodeLanguage : IDataSetStorage
	{
		Guid ZY8_PK { get; set; }
		Guid ZY8_ZY7_ConditionCode { get; set; }
		string ZY8_ZX6_NKLanguage { get; set; }
		string ZY8_Description { get; set; }
	}
}
