using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionValue : IDataSetStorage
	{
		Guid ZX3_PK { get; set; }
		Guid ZX3_ZX4_ValueType { get; set; }
		Guid ZX3_ZX1_Condition { get; set; }
		string ZX3_Value { get; set; }
		byte ZX3_LogicalORWithinGroup { get; set; }
	}
}
