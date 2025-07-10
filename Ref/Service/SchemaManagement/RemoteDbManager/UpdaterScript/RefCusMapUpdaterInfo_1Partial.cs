using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusMapUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusMap",
			RelatedFKColumnNames = new Dictionary<string, string>(),
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
		};
	}
}
