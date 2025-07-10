using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class TemplateApplicationTableLocker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is SQL parameter")]
		public static bool Lock(DbConnection connection, ITableSchema tableSchema, SchemaGuidColumn lockColumn, IEnumerable<ZGuid> guids)
		{
			const string ParameterName = "@guids";
			var parameterValues = guids.Select(guid => guid.ToString());
			var sqlText =
				$@"SELECT Count({tableSchema.PK.Name})
				FROM {tableSchema.SqlSchemaName}.{tableSchema.TableName} with (UPDLOCK, HOLDLOCK)
				WHERE {lockColumn.Name} in (SELECT * FROM {ParameterName});";

			var count = (int)connection.ExecuteScalar(sqlText, parameters =>
			{
				parameters.AddTableValuedParameter(ParameterName, TVPHelper.TVP_uniqueidentifier, parameterValues);
			});

			return count > 0;
		}
	}
}
