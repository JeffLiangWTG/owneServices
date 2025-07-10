using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Business;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

public class AuditLogsHelperForTesting
{
	public AuditLogsHelperForTesting(IBusinessObjectFactoryInternals factory, ITableSchema schema)
		: this (((IDbConnected)factory).Connection as AdminConnection, schema)
	{ }

	public AuditLogsHelperForTesting(AdminConnection connection, ITableSchema schema)
	{
		EnableCdc(connection, schema.SqlSchemaName, schema.TableName);
	}

	public AuditEventCollection GetAuditLogCollection(BusinessObject businessObject, ZDateTime? fromDate = null, ZDateTime? toDate = null)
	{
		if ((businessObject.Factory as IDbConnected).Connection is not AdminConnection mainDbConnection)
		{
			throw new ArgumentException("The Db connection must be AdminConnection");
		}

		ObjectFactory.Get<IAuditTsqlScriptRunnerHelperForTest>().RunAETWithAdminDbConnection(mainDbConnection);

		var auditEventCollection = new AuditEventCollection(businessObject.Factory, new AuditMasterTable(businessObject));
		auditEventCollection.Reload(sourceEntity: new AuditEntity(businessObject.PKSchemaColumn, null), changeUserCode: null, utcTimeFrom: fromDate ?? MinFromDate, utcTimeTo: toDate ?? MaxToDate);

		return auditEventCollection;
	}

	public void EnableCdc(AdminConnection connection, string testSchemaName, string testTableName)
	{
		ObjectFactory.Get<IAuditTsqlScriptRunnerHelperForTest>().EnableCdc(connection, testSchemaName, testTableName);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
	public void DeleteAllLogs(BusinessObject businessObject)
	{
		var sql = string.Format("DELETE FROM {0}.{1}.{2} where {3}_PK= @ID", Db.AuditDatabaseName, businessObject.PKSchemaColumn.TableSchema.SqlSchemaName, businessObject.TableName,businessObject.TablePrefix);
		using var cmd = (businessObject.Factory as IDbConnected).Connection.Command(sql);
		cmd.AddParameter("@ID", SqlDbType.UniqueIdentifier, businessObject.PK.ToGuid());
		cmd.ExecuteNonQuery();
	}

	public static void AssertColumnsExistsInAuditDb(BusinessObjectFactory factory, string sqlSchema, string tableName, params SchemaColumn[] columns)
	{
		var sqlText = String.Format(CultureInfo.InvariantCulture, @"
					SELECT c.name
					FROM
						[{0}].sys.columns c
						INNER JOIN [{0}].sys.tables t ON t.object_id = c.object_id
						INNER JOIN [{0}].sys.schemas s ON s.schema_id = t.schema_id
					WHERE
						s.name = '{1}'
						AND t.name = '{2}'",
			Db.AuditDatabaseName,
			sqlSchema,
			tableName);

		var connection = ((IDbConnected)factory).Connection;
		using var cmd = connection.Command(sqlText);
		using var reader = cmd.ExecuteReader();

		var result = new List<string>();
		while (reader.Read())
		{
			result.Add(reader.GetString(0));
		}

		foreach (var column in columns)
		{
			Assertion.AssertCollectionContains(String.Format(CultureInfo.InvariantCulture, "Column [{0}].[{1}].[{2}] exists in the audit database.", sqlSchema, tableName, column.Name), column.Name, result);
		}
	}

	internal static readonly ZDateTime MinFromDate = new ZDateTime(2010, 1, 1);
	internal static readonly ZDateTime MaxToDate = new ZDateTime(2090, 1, 1);
}