using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusNomenclatureGroupTypeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusNomenclatureGroupType",
			RelatedFKColumnNames = new Dictionary<string, string>(),
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
		};
	}
}
