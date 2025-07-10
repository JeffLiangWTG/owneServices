using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefDocOrgCusCodeUpdaterInfo_3 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefDocOrgCusCode",
			RelatedFKColumnNames = new Dictionary<string, string>(),
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
		};
	}
}
