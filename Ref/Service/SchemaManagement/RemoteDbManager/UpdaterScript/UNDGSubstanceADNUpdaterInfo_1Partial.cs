using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class UNDGSubstanceADNUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempUNDGSubstanceADN",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "UNDGAttributeZZs", "DAZ_ParentPK" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"UNDGAttributeZZs", new DataTableMapping { TableName = "#TempUNDGAttributeZZ" }
				}
			}
		};
	}
}
