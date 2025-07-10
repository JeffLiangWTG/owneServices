using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionValueTypeLanguage : IDataSetStorage
	{
		Guid ZXX_PK { get; set; }
		Guid ZXX_ZX4_ValueType { get; set; }
		string ZXX_ZX6_NKLanguage { get; set; }
		string ZXX_Description { get; set; }
	}
}
