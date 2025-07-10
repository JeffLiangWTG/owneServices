using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusMapTypeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusMapType",
			RelatedFKColumnNames = new Dictionary<string, string>(),
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
		};
	}
}
