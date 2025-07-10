using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	public class DataPublisher : IDataPublisher
	{
		IDbConnection sqlConnection;
		public DataPublisher(IDbConnection sqlConnection)
		{
			this.sqlConnection = sqlConnection;
		}

		static List<short> DataSetIds;

		public void PublishData(int commandTimeoutInSeconds = 0)
		{
			PublishData(false, null, commandTimeoutInSeconds);
		}

		public void PublishData(bool isPush, IMessageFactory messageFactory, int commandTimeoutInSeconds = 0)
		{
			DataSetIds = new List<short>();
			var retry = 1;
			
				while (retry < ApplicationConfig.RetryCount)
				{
					try
					{
						UpdateDataSetInformation(sqlConnection, commandTimeoutInSeconds, isPush);
						break;
					}
					catch (SqlException ex) when (ex.Message.Contains("Execution Timeout Expired"))
					{
						Console.WriteLine($"{retry} trial(s) for UpdateDataSetInformation failed because of: " + ex.Message);
						retry++;
						Thread.Sleep(TimeSpan.FromSeconds(ApplicationConfig.RetryIntervalInSeconds));
					}
				}

				if (retry == ApplicationConfig.RetryCount)
				{
					UpdateDataSetInformation(sqlConnection, commandTimeoutInSeconds, isPush);
				}
				
				using (var trans = sqlConnection.BeginTransaction())
				{
					var cmd = sqlConnection.CreateCommand();
					cmd.CommandTimeout = commandTimeoutInSeconds;
					cmd.Transaction = trans;
					Publish(cmd, isPush);
					if (isPush)
					{
						IncludeDataSetIds(cmd);
					}
					trans.Commit();
				}
			if (isPush)
			{
				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandTimeout = commandTimeoutInSeconds;
					messageFactory.AddBatchToMessage(cmd, DataSetIds);
					messageFactory.SendMessage(cmd, ApplicationConfig.MessageBatchSize);
				}
			}
		}

		static void IncludeDataSetIds(IDbCommand cmd)
		{
			cmd.CommandText = $"SELECT DataSetId FROM {ApplicationConfig.TempDataSetIdTableName}";
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataSetId = Convert.ToInt16(reader.GetByte(0));
					if (!DataSetIds.Contains(dataSetId))
					{
						DataSetIds.Add(dataSetId);
					}
				}
			}
		}

		static int Publish(IDbCommand cmd, bool isPush)
		{
			var sqlText = $@"
DECLARE @now datetime2 = SYSUTCDATETIME();

IF OBJECT_ID('tempdb..{ApplicationConfig.TempDataSetIdTableName}') IS NOT NULL
BEGIN
	DROP TABLE {ApplicationConfig.TempDataSetIdTableName}
END
CREATE TABLE {ApplicationConfig.TempDataSetIdTableName} (DataSetId tinyint)

;with dataToPublish AS
(
	SELECT RVC_ParentPK
	FROM RefDbVersionControl
	JOIN RefDataSetInformation ON RVC_DataSetId = RDS_DataSetId AND RDS_IsPush = {(isPush ? "1" : "0")}
	WHERE RVC_IsPublished = 0
)

UPDATE v SET RVC_LastUpdatedUTC = @now, RVC_IsPublished = 1
OUTPUT inserted.RVC_DataSetId
INTO {ApplicationConfig.TempDataSetIdTableName}
FROM RefDbVersionControl v
JOIN dataToPublish v1 ON v1.RVC_ParentPK = v.RVC_ParentPK

UPDATE t SET RDS_LastUpdatedUTC = @now
FROM
RefDataSetInformation t
JOIN {ApplicationConfig.TempDataSetIdTableName} ON DataSetId = t.RDS_DataSetId
";
			cmd.CommandText = sqlText;
			return cmd.ExecuteNonQuery();
		}

		static void UpdateDataSetInformation(IDbConnection conn, int commandTimeoutInSeconds, bool isPush)
		{
			using (var trans = conn.BeginTransaction())
			{
				var cmd = conn.CreateCommand();
				cmd.CommandTimeout = commandTimeoutInSeconds;
				cmd.Transaction = trans;
				UpdateDataSetInformationCore(cmd, isPush);
				trans.Commit();
			}
		}

		static void UpdateDataSetInformationCore(IDbCommand cmd, bool isPush)
		{
			cmd.CommandText = $@"
UPDATE r SET r.RVC_DataSetId = RDS_DataSetId
FROM RefDbVersionControl r
JOIN RefDataSetInformation
ON (r.RVC_DataSetId IS NULL OR r.RVC_DataSetId <> RDS_DataSetId)
AND r.RVC_IsPublished = 0
AND r.RVC_ParentCode = RDS_DataSetTableCode
AND RDS_PriorityLevel = 0
AND RDS_IsPush = {(isPush ? "1" : "0")}";
			cmd.ExecuteNonQuery();

			var selectDataSetDefinitions = $@"
SELECT RDS_DataSetId, RDS_DataSetTableCode, RDS_TableName, RDD_ColumnName, RDD_ColumnValue
FROM RefDataSetInformationDefinition
JOIN RefDataSetInformation ON RDD_DataSetId = RDS_DataSetId AND RDS_IsPush = {(isPush ? "1" : "0")} AND RDS_PriorityLevel > 0
ORDER BY RDS_PriorityLevel ASC";

			var definitions = new List<(int dataSetId, string dataSetTableCode, string tableName, string columnName, string columnValue)>();

			cmd.CommandText = selectDataSetDefinitions;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					definitions.Add((reader.GetInt16(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4)));
				}
			}
#pragma warning disable CA2100
			foreach (var definitionGroup in definitions.GroupBy(x => x.dataSetId))
			{
				var baseUpdate = $@"
				UPDATE r SET r.RVC_DataSetId = {definitionGroup.Key}
				FROM RefDbVersionControl r";
				foreach (var definition in definitionGroup.GroupBy(x => new { x.tableName, x.dataSetTableCode }))
				{
					baseUpdate += $@"
				JOIN {definition.Key.tableName} ON (r.RVC_DataSetId IS NULL OR r.RVC_DataSetId <> {definitionGroup.Key}) AND r.RVC_ParentCode = '{definition.Key.dataSetTableCode}' AND r.RVC_ParentPK = {definition.Key.dataSetTableCode}_PK AND r.RVC_IsPublished = 0";
					foreach (var tableDefinition in definition)
					{
						baseUpdate += $" AND {tableDefinition.columnName}='{tableDefinition.columnValue}'";
					}
				}

				cmd.CommandText = baseUpdate;
				cmd.ExecuteNonQuery();
#pragma warning restore CA2100
			}
		}
	}
}
