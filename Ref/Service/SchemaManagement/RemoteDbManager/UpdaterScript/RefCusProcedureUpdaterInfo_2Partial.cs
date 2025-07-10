using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusProcedureUpdaterInfo_2 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusProcedure",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusProcedureAttributes", "ZXB_ZZ6_ProcedureCode" },
				{ "RefCusProcedureLanguages", "ZXV_ZZ6_Procedure" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusProcedureAttributes", new DataTableMapping { TableName = "#TempRefCusProcedureAttribute" }
				},
				{
					"RefCusProcedureLanguages", new DataTableMapping { TableName = "#TempRefCusProcedureLanguage" }
				}
			}
		};
	}
}
