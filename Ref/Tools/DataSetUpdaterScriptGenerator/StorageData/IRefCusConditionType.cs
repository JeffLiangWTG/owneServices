using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionType : IDataSetStorage
	{
		Guid ZX2_PK { get; set; }
		string ZX2_ConditionClass { get; set; }
		string ZX2_ConditionType { get; set; }
		string ZX2_Description { get; set; }
		string ZX2_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusCondition> RefCusCondition { get; }
		IEnumerable<IRefCusConditionTypeLanguage> RefCusConditionTypeLanguage { get; }
	}
}
