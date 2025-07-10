using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefAccElectronicProcessingFeeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefAccElectronicProcessingFee",
			RelatedFKColumnNames = new Dictionary<string, string>(),
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
		};
	}
}
