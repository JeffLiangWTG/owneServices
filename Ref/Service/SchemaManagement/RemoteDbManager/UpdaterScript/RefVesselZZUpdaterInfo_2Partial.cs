using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefVesselZZUpdaterInfo_2 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefVesselZZ",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefVesselArrivals", "ZYA_ZZO_Vessel" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefVesselArrivals", new DataTableMapping { TableName = "#TempRefVesselArrival" }
				}
			}
		};
	}
}
