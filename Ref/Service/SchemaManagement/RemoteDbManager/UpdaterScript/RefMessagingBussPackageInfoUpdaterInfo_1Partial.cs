using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefMessagingBussPackageInfoUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefMessagingBussPackageInfo",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefMessagingBussPackageVersions", "ZMV_ZMP_PackageInfo" },
				{ "RefMessagingBussCarrierInfoes", "ZMC_ZMP_PackageInfo" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefMessagingBussPackageVersions", new DataTableMapping { TableName = "#TempRefMessagingBussPackageVersion" }
				},
				{
					"RefMessagingBussCarrierInfoes", new DataTableMapping { TableName = "#TempRefMessagingBussCarrierInfo" }
				}
			}
		};
	}
}
