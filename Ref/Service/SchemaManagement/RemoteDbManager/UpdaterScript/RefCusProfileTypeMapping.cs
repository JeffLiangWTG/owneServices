using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusProfileTypeMapping
	{
		public static DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusProfileType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTariffType", "XXX_ZZI_TariffType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTariffType", new DataTableMapping { TableName = "#TempRefCusTariffType" }
				}
			}
		};
	}
}
