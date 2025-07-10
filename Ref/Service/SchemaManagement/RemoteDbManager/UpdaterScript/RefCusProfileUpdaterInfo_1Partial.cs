using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusProfileUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusProfile",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusProfileType", "XX0_XXX_ProfileType" },
				{ "RefCusProfileAttributes", "XXY_XX0_Profile" },
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusProfileType", RefCusProfileTypeMapping.Mapping
				},
				{
					"RefCusProfileAttributes",  new DataTableMapping { TableName = "#TempRefCusProfileAttribute" }
				},
			}
		};
	}
}
