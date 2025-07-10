using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionValueType : IDataSetStorage
	{
		Guid ZX4_PK { get; set; }
		string ZX4_ValueType { get; set; }
		string ZX4_Description { get; set; }
		bool ZX4_IsFormula { get; set; }
		string ZX4_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusConditionValue> RefCusConditionValue { get; }
		IEnumerable<IRefCusConditionValueTypeLanguage> RefCusConditionValueTypeLanguage { get; }
	}
}
