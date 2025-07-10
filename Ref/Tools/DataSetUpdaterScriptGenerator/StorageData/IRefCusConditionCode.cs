using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConditionCode : IDataSetStorage
	{
		Guid ZY7_PK { get; set; }
		string ZY7_ConditionCode { get; set; }
		string ZY7_Description { get; set; }
		string ZY7_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusConditionCodeLanguage> RefCusConditionCodeLanguage { get; }
	}
}
