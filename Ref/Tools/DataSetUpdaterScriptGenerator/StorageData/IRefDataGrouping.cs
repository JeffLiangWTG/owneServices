using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefDataGrouping : IDataSetStorage
	{
		Guid ZZZ_PK { get; set; }
		string ZZZ_DataGrouping { get; set; }
		string ZZZ_Description { get; set; }
		Nullable<Guid> ZZZ_ZZZ_Grouping { get; set; }

		IEnumerable<IRefDataGrouping> RefDataGrouping1 { get; }
	}
}
