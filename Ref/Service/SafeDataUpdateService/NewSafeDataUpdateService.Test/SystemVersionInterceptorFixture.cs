using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class SystemVersionInterceptorFixture
	{

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void ReaderExecuting()
		{
			const string NON_TEMPORAL_TABLE = "NON_TEMPORAL_TABLE";
			const string expectSql = "FOR SYSTEM_TIME AS OF";
			var temporalTableNames = new List<string>();
			var nonTemporalTableNames = new List<string>();
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"select name,temporal_type_desc
from sys.tables
WHERE temporal_type_desc  <> 'HISTORY_TABLE'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var temporalTypeDesc = reader.GetString(1);
							if (temporalTypeDesc.Equals(NON_TEMPORAL_TABLE, StringComparison.OrdinalIgnoreCase))
							{
								nonTemporalTableNames.Add(reader.GetString(0));
							}
							else
							{
								temporalTableNames.Add(reader.GetString(0));
							}
						}
					}
				}
				temporalTableNames.AddRange(UserViews);
				var systemVersionContext = new SystemVersionContext();
				systemVersionContext.SystemVersionUTC = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
				var systemVersionInterceptor = new SystemVersionInterceptor(systemVersionContext);
				Assert.Multiple(() =>
				{
					using (var cmd = conn.CreateCommand())
					{
						foreach (var tableName in temporalTableNames)
						{
							cmd.CommandText = $"SELECT [Extent1].* FROM [{tableName}] AS [Extent1] WHERE 1 = 1";
							systemVersionInterceptor.ReaderExecuting(cmd, default, default);
							Assert.IsTrue(cmd.CommandText.Contains(expectSql), $"{tableName} should be Temporal Table");
						}
						foreach (var tableName in nonTemporalTableNames)
						{
							cmd.CommandText = $"SELECT [Extent1].* FROM [{tableName}] AS [Extent1] WHERE 1 = 1";
							systemVersionInterceptor.ReaderExecuting(cmd, default, default);
							Assert.IsFalse(cmd.CommandText.Contains(expectSql), $"{tableName} should be Non-TemporalTable");
						}
					}
				});
			}
		}

		static readonly string[] UserViews =
		[
			"RefVesselUserView",
			"RefUNLOCOUserView",
			"RefPortPolygonUserView",
			"RefAccTaxRateUserView",
			"RefCusCodeListUserView",
			"RefCusProcedureUserView",
			"RefShippingLineUserView"
		];
	}
}
