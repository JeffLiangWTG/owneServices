using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.OldAuditApi;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test.BiControllerTest;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.BusinessIntelligence.OldAuditApi
{
	class OldAuditApiControllerTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestCdcHistorySummary()
		{
			var auditDbName = Db.AuditDatabaseName;
			var lsn1 = "0x0000E59C00021AE80001";
			var lsn2 = "0x0000E59C00021AE80002";

			var testSQL1 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn1}, 'DBO', 'SomeTable', 200, 1907, '10 JUL 2019')";
			var testSQL2 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn2}, 'DBO', 'SomeOtherTable', 131, 1908, '10 AUG 2019')";

			using (var mainDbConnection = Db.NewAdminConnection())
			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.AuditServices.IsAllowed = true;
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				mainDbConnection.ExecuteNonQuery(testSQL1);
				mainDbConnection.ExecuteNonQuery(testSQL2);

				AssertRetrieveCdcHistorySummaryDetails("JSON", "201907100000", "201907110000", string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeTable\",\"NumberOfRows\":200,\"Batches\":1,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-07-10T00:00:00\"}}]", lsn1), 200); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("JSON", "201908100000", "201908110000", string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeOtherTable\",\"NumberOfRows\":131,\"Batches\":1,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-08-10T00:00:00\"}}]", lsn2), 131);// EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("CSV", "201907100000", "201907110000", string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeTable\",\"200\",\"1\",\"{0}\",\"10/07/2019 12:00:00 AM\"\r\n", lsn1), 200); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "201908100000", "201908110000", string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeOtherTable\",\"131\",\"1\",\"{0}\",\"10/08/2019 12:00:00 AM\"\r\n", lsn2), 131);// EMULATING API CALL

				using (var auditConnection = Db.NewAdminConnection(auditDbName))
				{
					BiMasterState.SetParameter(auditConnection, "ASP_BATCH_SIZE", "40");
				}

				AssertRetrieveCdcHistorySummaryDetails("JSON", "201907100000", "201907110000", string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeTable\",\"NumberOfRows\":200,\"Batches\":5,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-07-10T00:00:00\"}}]", lsn1)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("JSON", "201908100000", "201908110000", string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeOtherTable\",\"NumberOfRows\":131,\"Batches\":4,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-08-10T00:00:00\"}}]", lsn2)); // EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("CSV", "201907100000", "201907110000", string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeTable\",\"200\",\"5\",\"{0}\",\"10/07/2019 12:00:00 AM\"\r\n", lsn1)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "201908100000", "201908110000", string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeOtherTable\",\"131\",\"4\",\"{0}\",\"10/08/2019 12:00:00 AM\"\r\n", lsn2)); // EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("JSON", "0x0000E59C00021AE80001", string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeOtherTable\",\"NumberOfRows\":131,\"Batches\":4,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-08-10T00:00:00\"}}]", lsn2)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("JSON", "0x0000E59C00021AE80000", string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeTable\",\"NumberOfRows\":200,\"Batches\":5,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-07-10T00:00:00\"}}" + // EMULATING API CALL
					"]", lsn1));
				AssertRetrieveCdcHistorySummaryDetails("JSON", "0x0000E59C00021AE80002", "[]");

				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80001", string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n" + // EMULATING API CALL
					"\"DBO\",\"SomeOtherTable\",\"131\",\"4\",\"{0}\",\"10/08/2019 12:00:00 AM\"\r\n", lsn2));
				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80000", string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n" + // EMULATING API CALL
					"\"DBO\",\"SomeTable\",\"200\",\"5\",\"{0}\",\"10/07/2019 12:00:00 AM\"\r\n", lsn1));
				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80002", "SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n"); // EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("CSV", null, "20190710", "{\"Message\":\"Incorrect API Request: Either from_time and to_time is required or after_LSN is required\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "20190710", null, "{\"Message\":\"Incorrect API Request: Either from_time and to_time is required or after_LSN is required\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", null, null, "{\"Message\":\"Incorrect API Request: Either from_time and to_time is required or after_LSN is required\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", null, "{\"Message\":\"Incorrect API Request: Either from_time and to_time is required or after_LSN is required\"}"); // EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("CSV", "20190710", "20190710", "{\"Message\":\"Incorrect API Request: DateTime string is not in the correct format: yyyyMMddHHmm\"}"); // EMULATING API CALL

				BiServers.ClearBiServersCache();
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80002", "{\"Message\":\"Incorrect API Request: The Audit Web API has not been enabled on this system.\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "201907100000", "201907110000", "{\"Message\":\"Incorrect API Request: The Audit Web API has not been enabled on this system.\"}"); // EMULATING API CALL
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Get test date")]
		void InsertTestValuesIntoSomeTable(AdminConnection mainDbConnection, string auditDBName, string tableName, string lsn, int rows, int lsnperiod)
		{
			var seq = rows + 1;
			var year = 2000 + lsnperiod / 100;
			var month = lsnperiod % 100 > 10 ? $"{lsnperiod % 100}" : $"0{lsnperiod % 100}";
			var dateString = DateTime.Parse($"{year}-{month}-01 00:00:00.000").ToString("yyyy-MM-dd HH:mm:ss.fff");
			var script = $"INSERT INTO [{auditDBName}].biadmin.LsnTimeMapping VALUES ({lsn}, '{dateString}')";
			mainDbConnection.ExecuteNonQuery(script);
			for (var i = 0; i <= rows; i++)
			{
				var seqVal = seq--.ToString("X4");
				script = $"INSERT INTO [{auditDBName}].dbo.{tableName} VALUES ({lsn}, 0x{seqVal}, 2, 0x0, {lsnperiod}, 1, {i}, 'Test: {i}')";
				mainDbConnection.ExecuteNonQuery(script);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		[ExpectNoExceptions]
		public void TestCdcHistory()
		{
			var auditDbName = Db.AuditDatabaseName;
			var lsn1 = "0x0000E59C00021AE80001";
			var lsn2 = "0x0000E59C00021AE80002";

			var testSQL1 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn1}, 'DBO', 'SomeTable', 200, 1907, '10 JUL 2019')";
			var testSQL2 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn2}, 'DBO', 'SomeOtherTable', 200, 1909, '10 SEP 2019')";
			var testSQL3 = @$"
							CREATE TABLE [{auditDbName}].dbo.SomeTable
							(
								[__$start_lsn] [binary](10) NOT NULL,
								[__$seqval] [binary](10) NOT NULL,
								[__$operation] [int] NOT NULL,
								[__$update_mask] [varbinary](128) NOT NULL,
								[__$lsn_period] [smallint] NOT NULL,
								[__$command_id] [int] NOT NULL,
								[AA_Code] int,
								[AA_Description] [varchar](35) NULL
							);

							CREATE NONCLUSTERED INDEX [IX_SomeTable_StartLsn] ON [{auditDbName}].dbo.SomeTable
							(
								[__$start_lsn] ASC,
								[__$command_id] ASC,
								[__$seqval] ASC,
								[__$operation] ASC
							) WITH (DATA_COMPRESSION = PAGE);
";

			var testSQL4 = $@"
							CREATE TABLE [{auditDbName}].dbo.SomeOtherTable
							(
								[__$start_lsn] [binary](10) NOT NULL,
								[__$seqval] [binary](10) NOT NULL,
								[__$operation] [int] NOT NULL,
								[__$update_mask] [varbinary](128) NOT NULL,
								[__$lsn_period] [smallint] NOT NULL,
								[__$command_id] [int] NOT NULL,
								[BB_Code] int,
								[BB_Description] [varchar](35) NULL
							);

							CREATE NONCLUSTERED INDEX [IX_SomeOtherTable_StartLsn] ON [{auditDbName}].dbo.SomeOtherTable
							(
								[__$start_lsn] ASC,
								[__$command_id] ASC,
								[__$seqval] ASC,
								[__$operation] ASC
							) WITH (DATA_COMPRESSION = PAGE);
";

			using (var mainDbConnection = Db.NewAdminConnection())
			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.AuditServices.IsAllowed = true;
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				mainDbConnection.ExecuteNonQuery(testSQL1);
				mainDbConnection.ExecuteNonQuery(testSQL2);

				using (var auditConnection = Db.NewAdminConnection(auditDbName))
				{
					BiMasterState.SetParameter(auditConnection, "ASP_BATCH_SIZE", "40");
				}

				mainDbConnection.ExecuteNonQuery(testSQL3);
				mainDbConnection.ExecuteNonQuery(testSQL4);

				InsertTestValuesIntoSomeTable(mainDbConnection, auditDbName, "SomeTable", lsn1, 200, 1907);
				InsertTestValuesIntoSomeTable(mainDbConnection, auditDbName, "SomeOtherTable", lsn2, 131, 1909);

				var aaCsvHeader = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,AA_Code,AA_Description\r\n";
				var bbCsvHeader = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,BB_Code,BB_Description\r\n";

				AssertRetrieveCdcHistory("JSON", lsn1, 6, "dbo", "SomeTable", TestData.expected1); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn1, 1, "dbo", "SomeTable", TestData.expected2); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn1, 3, "dbo", "SomeTable", TestData.expected3); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn1, -1, "dbo", "SomeTable", "{\"Message\":\"Incorrect API Request: Batch number must start from zero\"}", throwsError: true); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn1, 7, "dbo", "SomeTable", "[]"); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn2, 1, "dbo", "SomeTable", "[]"); // EMULATING API CALL
				NUnit.Framework.Assert.That(delegate
					{
						AssertRetrieveCdcHistory("JSON", string.Empty, 1, "dbo", "SomeTable", "[]"); // EMULATING API CALL
					}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidDataException), "Found invalid data while decoding."), "Expected data not found");
				NUnit.Framework.Assert.That(delegate
					{
						AssertRetrieveCdcHistory("JSON", null, 1, "dbo", "SomeTable", "[]"); // EMULATING API CALL
					}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidDataException), "Found invalid data while decoding."), "Expected data not found");
				AssertRetrieveCdcHistory("CSV", lsn1, 6, "dbo", "SomeTable", TestData.csvExpected1); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn1, 1, "dbo", "SomeTable", TestData.csvExpected2); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn1, 3, "dbo", "SomeTable", TestData.csvExpected3); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn1, -1, "dbo", "SomeTable", "{\"Message\":\"Incorrect API Request: Batch number must start from zero\"}", throwsError: true); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn1, 7, "dbo", "SomeTable", aaCsvHeader); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn2, 1, "dbo", "SomeTable", aaCsvHeader); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", string.Empty, 1, "dbo", "SomeTable", "{\"Message\":\"Incorrect API Request: Table not found\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", null, 1, "dbo", "SomeTable", "{\"Message\":\"Incorrect API Request: Table not found\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistory("RUBBISH", null, 1, "dbo", "SomeTable", "{\"Message\":\"Incorrect API Request: You must specify the desired response format to be JSON or CSV\"}", throwsError: true); // EMULATING API CALL

				AssertRetrieveCdcHistory("JSON", lsn2, 4, "dbo", "SomeOtherTable", TestData.expected4); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn2, 1, "dbo", "SomeOtherTable", TestData.expected5); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn2, 3, "dbo", "SomeOtherTable", TestData.expected6); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn2, -1, "dbo", "SomeOtherTable", "{\"Message\":\"Incorrect API Request: Batch number must start from zero\"}", throwsError: true); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn2, 5, "dbo", "SomeOtherTable", "[]"); // EMULATING API CALL
				AssertRetrieveCdcHistory("JSON", lsn1, 1, "dbo", "SomeOtherTable", "[]"); // EMULATING API CALL
				NUnit.Framework.Assert.That(delegate
					{
						AssertRetrieveCdcHistory("JSON", string.Empty, 1, "dbo", "SomeOtherTable", "[]"); // EMULATING API CALL
					}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidDataException), "Found invalid data while decoding."), "Expected data not found");
				NUnit.Framework.Assert.That(delegate
					{
						AssertRetrieveCdcHistory("JSON", null, 1, "dbo", "SomeOtherTable", "[]"); // EMULATING API CALL
					}, CustomConstraints.InnermostExceptionThrown(typeof(InvalidDataException), "Found invalid data while decoding."), "Expected data not found");
				AssertRetrieveCdcHistory("CSV", lsn2, 4, "dbo", "SomeOtherTable", TestData.csvExpected4); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn2, 1, "dbo", "SomeOtherTable", TestData.csvExpected5); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn2, 3, "dbo", "SomeOtherTable", TestData.csvExpected6); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn2, -1, "dbo", "SomeOtherTable", "{\"Message\":\"Incorrect API Request: Batch number must start from zero\"}", throwsError: true); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn2, 5, "dbo", "SomeOtherTable", bbCsvHeader); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", lsn1, 1, "dbo", "SomeOtherTable", bbCsvHeader); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", string.Empty, 1, "dbo", "SomeOtherTable", "{\"Message\":\"Incorrect API Request: Table not found\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistory("CSV", null, 1, "dbo", "SomeOtherTable", "{\"Message\":\"Incorrect API Request: Table not found\"}"); // EMULATING API CALL
				AssertRetrieveCdcHistory("RUBBISH", null, 1, "dbo", "SomeOtherTable", "{\"Message\":\"Incorrect API Request: You must specify the desired response format to be JSON or CSV\"}", throwsError: true); // EMULATING API CALL

				AssertRetrieveCdcHistory("CSV", null, 1, "dbo", "JSON", "{\"Message\":\"Incorrect API Request: Table not found\"}"); // EMULATING API CALL

				BiServers.ClearBiServersCache();
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertRetrieveCdcHistory("CSV", null, 0, "dbo", "SomeOtherTable", "{\"Message\":\"Incorrect API Request: The Audit Web API has not been enabled on this system.\"}", throwsError: true); // EMULATING API CALL
			}
		}

		void AssertRetrieveCdcHistorySummaryDetails(string format, string startDate, string endDate, string expectedResult, int batchSize = 0)
		{
			var cancellationToken = new CancellationToken(false);
			var bc = new OldAuditApiController();
			var httpActionContext = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};
			bc.ControllerContext = httpActionContext.ControllerContext;
			bc.Request = new HttpRequestMessage();
			var actionResult = bc.GetSummaryData(format, startDate, endDate, batchSize, null);
			var response = actionResult.ExecuteAsync(cancellationToken);
			var byteStream = response.Result.Content.ReadAsStreamAsync();

			string deflatedString;
			if (format.Equals(BIAPIServiceConstants.CSV))
			{
				deflatedString = new StreamReader(byteStream.Result).ReadToEnd();
			}
			else
			{
				using (var m = new MemoryStream())
				{
					var deflatedStream = new GZipStream(byteStream.Result, CompressionMode.Decompress, leaveOpen: true);
					deflatedString = new StreamReader(deflatedStream).ReadToEnd();
				}
			}
			AssertEquals(expectedResult, deflatedString);
		}

		void AssertRetrieveCdcHistorySummaryDetails(string format, string lsn, string expectedResult, int batchSize = 0)
		{
			var cancellationToken = new CancellationToken(false);
			var bc = new OldAuditApiController();
			var httpActionContext = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};
			bc.ControllerContext = httpActionContext.ControllerContext;
			bc.Request = new HttpRequestMessage();
			var actionResult = bc.GetSummaryData(format, null, null, batchSize, lsn);
			var response = actionResult.ExecuteAsync(cancellationToken);
			var byteStream = response.Result.Content.ReadAsStreamAsync();

			if (format.Equals("CSV"))
			{
				AssertEquals(expectedResult, new StreamReader(byteStream.Result).ReadToEnd());
			}
			else
			{
				using (var m = new MemoryStream())
				{
					var deflatedStream = new GZipStream(byteStream.Result, CompressionMode.Decompress, leaveOpen: true);
					var deflatedString = new StreamReader(deflatedStream).ReadToEnd();
					AssertEquals(expectedResult, deflatedString);
				}
			}
		}

		void AssertRetrieveCdcHistory(string format, string lsn, int batch, string schema, string tableName, string expectedResult, bool throwsError = false, int batchSize = 0)
		{
			var cancellationToken = new CancellationToken(false);
			var apiController = new OldAuditApiController();
			var httpActionContext = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};
			apiController.ControllerContext = httpActionContext.ControllerContext;
			apiController.Request = new HttpRequestMessage();
			var actionResult = apiController.GetAuditData(schema, tableName, batch, batchSize, format, lsn);
			var response = actionResult.ExecuteAsync(cancellationToken);
			var byteStream = response.Result.Content.ReadAsStreamAsync();

			string deflatedString;
			if (format.Equals(BIAPIServiceConstants.CSV) || throwsError)
			{
				deflatedString = new StreamReader(byteStream.Result).ReadToEnd();
			}
			else
			{
				using (var m = new MemoryStream())
				{
					var deflatedStream = new GZipStream(byteStream.Result, CompressionMode.Decompress, leaveOpen: true);
					deflatedString = new StreamReader(deflatedStream).ReadToEnd();
				}
			}
			AssertEquals(expectedResult, deflatedString);
		}

		public void TestGetAuditDataWhenLsnIsNotValid()
		{
			var apiController = new OldAuditApiController();
			var httpActionContext = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};
			apiController.ControllerContext = httpActionContext.ControllerContext;
			apiController.Request = new HttpRequestMessage();

			var lsn = "0123";
			var actionResult = apiController.GetAuditData("schema", "tableName", 1, 0, "JSON", lsn);
			AssertEquals("{\"Message\":\"Incorrect API Request: LSN string is not in the correct format.\"}", actionResult.ExecuteAsync(new CancellationToken(false)).Result.Content.ReadAsStringAsync().Result);

			lsn = "0x0g";
			actionResult = apiController.GetAuditData("schema", "tableName", 1, 0, "JSON", lsn);
			AssertEquals("{\"Message\":\"Incorrect API Request: LSN string is not in the correct format.\"}", actionResult.ExecuteAsync(new CancellationToken(false)).Result.Content.ReadAsStringAsync().Result);

			lsn = "0x0G";
			actionResult = apiController.GetAuditData("schema", "tableName", 1, 0, "JSON", lsn);
			AssertEquals("{\"Message\":\"Incorrect API Request: LSN string is not in the correct format.\"}", actionResult.ExecuteAsync(new CancellationToken(false)).Result.Content.ReadAsStringAsync().Result);

			lsn = "0x";
			actionResult = apiController.GetAuditData("schema", "tableName", 1, 0, "JSON", lsn);
			AssertEquals("{\"Message\":\"Incorrect API Request: LSN string is not in the correct format.\"}", actionResult.ExecuteAsync(new CancellationToken(false)).Result.Content.ReadAsStringAsync().Result);

			lsn = "0x111111111111111111111";
			actionResult = apiController.GetAuditData("schema", "tableName", 1, 0, "JSON", lsn);
			AssertEquals("{\"Message\":\"Incorrect API Request: LSN string is not in the correct format.\"}", actionResult.ExecuteAsync(new CancellationToken(false)).Result.Content.ReadAsStringAsync().Result);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestCdcHistorySummaryPassingInBatchSize()
		{
			var auditDbName = Db.AuditDatabaseName;
			var lsn1 = "0x0000E59C00021AE80001";
			var lsn2 = "0x0000E59C00021AE80002";

			var testSQL1 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn1}, 'DBO', 'SomeTable', 200, 1907, '10 JUL 2019')";
			var testSQL2 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn2}, 'DBO', 'SomeOtherTable', 131, 1908, '10 AUG 2019')";

			using (var mainDbConnection = Db.NewAdminConnection())
			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.AuditServices.IsAllowed = true;
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				mainDbConnection.ExecuteNonQuery(testSQL1);
				mainDbConnection.ExecuteNonQuery(testSQL2);

				AssertRetrieveCdcHistorySummaryDetails("JSON", "201907100000", "201907110000", batchSize: 40, expectedResult: string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeTable\",\"NumberOfRows\":200,\"Batches\":5,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-07-10T00:00:00\"}}]", lsn1)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("JSON", "201908100000", "201908110000", batchSize: 40, expectedResult: string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeOtherTable\",\"NumberOfRows\":131,\"Batches\":4,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-08-10T00:00:00\"}}]", lsn2));// EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("CSV", "201907100000", "201907110000", batchSize: 40, expectedResult: string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeTable\",\"200\",\"5\",\"{0}\",\"10/07/2019 12:00:00 AM\"\r\n", lsn1)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "201908100000", "201908110000", batchSize: 40, expectedResult: string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeOtherTable\",\"131\",\"4\",\"{0}\",\"10/08/2019 12:00:00 AM\"\r\n", lsn2));// EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("JSON", "201907100000", "201907110000", batchSize: 20, expectedResult: string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeTable\",\"NumberOfRows\":200,\"Batches\":10,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-07-10T00:00:00\"}}]", lsn1)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("JSON", "201908100000", "201908110000", batchSize: 20, expectedResult: string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeOtherTable\",\"NumberOfRows\":131,\"Batches\":7,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-08-10T00:00:00\"}}]", lsn2)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "201907100000", "201907110000", batchSize: 20, expectedResult: string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeTable\",\"200\",\"10\",\"{0}\",\"10/07/2019 12:00:00 AM\"\r\n", lsn1)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("CSV", "201908100000", "201908110000", batchSize: 20, expectedResult: string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n\"DBO\",\"SomeOtherTable\",\"131\",\"7\",\"{0}\",\"10/08/2019 12:00:00 AM\"\r\n", lsn2)); // EMULATING API CALL

				AssertRetrieveCdcHistorySummaryDetails("JSON", "0x0000E59C00021AE80001", batchSize: 40, expectedResult: string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeOtherTable\",\"NumberOfRows\":131,\"Batches\":4,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-08-10T00:00:00\"}}]", lsn2)); // EMULATING API CALL
				AssertRetrieveCdcHistorySummaryDetails("JSON", "0x0000E59C00021AE80000", batchSize: 40, expectedResult: string.Format("[{{\"SchemaName\":\"DBO\",\"ChangedTableName\":\"SomeTable\",\"NumberOfRows\":200,\"Batches\":5,\"Lsn\":\"{0}\",\"TranEndTimeUTC\":\"2019-07-10T00:00:00\"}}]", lsn1));
				AssertRetrieveCdcHistorySummaryDetails("JSON", "0x0000E59C00021AE80002", "[]");

				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80001", batchSize: 40, expectedResult: string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n" + // EMULATING API CALL
					"\"DBO\",\"SomeOtherTable\",\"131\",\"4\",\"{0}\",\"10/08/2019 12:00:00 AM\"\r\n", lsn2));
				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80000", batchSize: 40, expectedResult: string.Format("SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n" + // EMULATING API CALL
					"\"DBO\",\"SomeTable\",\"200\",\"5\",\"{0}\",\"10/07/2019 12:00:00 AM\"\r\n", lsn1));
				AssertRetrieveCdcHistorySummaryDetails("CSV", "0x0000E59C00021AE80002", batchSize: 40, expectedResult: "SchemaName,ChangedTableName,NumberOfRows,Batches,Lsn,TranEndTimeUTC\r\n"); // EMULATING API CALL
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestCdcHistoryPassingInBatchSize()
		{
			var auditDbName = Db.AuditDatabaseName;
			var lsn1 = "0x0000E59C00021AE80001";
			var lsn2 = "0x0000E59C00021AE80002";

			var testSQL1 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn1}, 'DBO', 'SomeTable', 200, 1907, '10 JUL 2019')";
			var testSQL2 = $"INSERT INTO {auditDbName}.BIADMIN.CdcHistorySummary (Lsn,SchemaName,ChangedTableName,NumberOfRows,LsnPeriod,TranEndTimeUTC) VALUES ({lsn2}, 'DBO', 'SomeOtherTable', 200, 1909, '10 SEP 2019')";
			var testSQL3 = @$"
							CREATE TABLE [{auditDbName}].dbo.SomeTable
							(
								[__$start_lsn] [binary](10) NOT NULL,
								[__$seqval] [binary](10) NOT NULL,
								[__$operation] [int] NOT NULL,
								[__$update_mask] [varbinary](128) NOT NULL,
								[__$lsn_period] [smallint] NOT NULL,
								[__$command_id] [int] NOT NULL,
								[AA_Code] int,
								[AA_Description] [varchar](35) NULL
							);

							CREATE NONCLUSTERED INDEX [IX_SomeTable_StartLsn] ON [{auditDbName}].dbo.SomeTable
							(
								[__$start_lsn] ASC,
								[__$command_id] ASC,
								[__$seqval] ASC,
								[__$operation] ASC
							) WITH (DATA_COMPRESSION = PAGE);
";

			var testSQL4 = $@"
							CREATE TABLE [{auditDbName}].dbo.SomeOtherTable
							(
								[__$start_lsn] [binary](10) NOT NULL,
								[__$seqval] [binary](10) NOT NULL,
								[__$operation] [int] NOT NULL,
								[__$update_mask] [varbinary](128) NOT NULL,
								[__$lsn_period] [smallint] NOT NULL,
								[__$command_id] [int] NOT NULL,
								[BB_Code] int,
								[BB_Description] [varchar](35) NULL
							);

							CREATE NONCLUSTERED INDEX [IX_SomeOtherTable_StartLsn] ON [{auditDbName}].dbo.SomeOtherTable
							(
								[__$start_lsn] ASC,
								[__$command_id] ASC,
								[__$seqval] ASC,
								[__$operation] ASC
							) WITH (DATA_COMPRESSION = PAGE);
";

			using (var mainDbConnection = Db.NewAdminConnection())
			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.AuditServices.IsAllowed = true;
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				mainDbConnection.ExecuteNonQuery(testSQL1);
				mainDbConnection.ExecuteNonQuery(testSQL2);
				mainDbConnection.ExecuteNonQuery(testSQL3);
				mainDbConnection.ExecuteNonQuery(testSQL4);

				InsertTestValuesIntoSomeTable(mainDbConnection, auditDbName, "SomeTable", lsn1, 200, 1907);
				InsertTestValuesIntoSomeTable(mainDbConnection, auditDbName, "SomeOtherTable", lsn2, 131, 1909);

				CombineAssertions(() =>
				{
					AssertRetrieveCdcHistory("JSON", lsn1, 6, "dbo", "SomeTable", TestData.expected1, batchSize: 40); // EMULATING API CALL
					AssertRetrieveCdcHistory("JSON", lsn1, 1, "dbo", "SomeTable", TestData.expected2, batchSize: 40); // EMULATING API CALL
					AssertRetrieveCdcHistory("JSON", lsn1, 3, "dbo", "SomeTable", TestData.expected3, batchSize: 40); // EMULATING API CALL

					AssertRetrieveCdcHistory("CSV", lsn1, 6, "dbo", "SomeTable", TestData.csvExpected1, batchSize: 40); // EMULATING API CALL
					AssertRetrieveCdcHistory("CSV", lsn1, 1, "dbo", "SomeTable", TestData.csvExpected2, batchSize: 40); // EMULATING API CALL
					AssertRetrieveCdcHistory("CSV", lsn1, 3, "dbo", "SomeTable", TestData.csvExpected3, batchSize: 40); // EMULATING API CALL
				});
			}
		}

		public void TestRouteAttributeGetSummaryData()
		{
			var method = typeof(OldAuditApiController).GetMethod(nameof(OldAuditApiController.GetSummaryDataQuery));
			var attributes = method.GetCustomAttributes(typeof(RouteAttribute), inherit: true);
			Assert(attributes.Length == 1);
			var routeAttr = (RouteAttribute)attributes[0];
			Assert(routeAttr.Template.Equals("api/analytics/audit-data-summary"));
		}

		public void TestRouteAttributeGetAuditData()
		{
			var method = typeof(OldAuditApiController).GetMethod(nameof(OldAuditApiController.GetAuditDataQuery));
			var attributes = method.GetCustomAttributes(typeof(RouteAttribute), inherit: true);
			Assert(attributes.Length == 1);
			var routeAttr = (RouteAttribute)attributes[0];
			Assert(routeAttr.Template.Equals("api/analytics/audit-data"));
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestQueryStringParseSummaryData()
		{
			var cancellationToken = new CancellationToken(false);
			var apiControllerMock = new Mock<OldAuditApiController>() { CallBase = true };
			var apiController = apiControllerMock.Object;

			var httpActionContext = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};
			apiController.ControllerContext = httpActionContext.ControllerContext;
			using (var mainDbConnection = Db.NewAdminConnection())
			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.AuditServices.IsAllowed = true;
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				apiController.Request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/analytics/audit-data-summary?from_time=202112010000&to_time=202112110000&response_format=JSON");
				apiController.GetSummaryDataQuery();
				apiControllerMock.Verify(m => m.GetSummaryData("JSON", "202112010000", "202112110000", 0, null), Times.Once);
				apiController.Request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/analytics/audit-data-summary?after_lsn=0x0002D435000001D90001&page_size=40&response_format=CSV");
				apiController.GetSummaryDataQuery();
				apiControllerMock.Verify(m => m.GetSummaryData("CSV", null, null, 40, "0x0002D435000001D90001"), Times.Once);
				apiController.Request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/analytics/audit-data-summary?from_time=20211201&after_lsn=0x0002D435000001D90001&page_size=40&response_format=CSV");
				var actionResult = apiController.GetSummaryDataQuery();
				var response = actionResult.ExecuteAsync(cancellationToken);
				var byteStream = response.Result.Content.ReadAsStreamAsync();
				AssertEquals("{\"Message\":\"Incorrect API Request: Either from_time and to_time is required or after_LSN is required\"}", new StreamReader(byteStream.Result).ReadToEnd());
			}
		}

		public void TestQueryStringParseAuditData()
		{
			var cancellationToken = new CancellationToken(false);
			var apiControllerMock = new Mock<OldAuditApiController>() { CallBase = true };
			var apiController = apiControllerMock.Object;

			var httpActionContext = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};
			apiController.ControllerContext = httpActionContext.ControllerContext;
			apiController.Request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/analytics/audit-data?schema=SchemaName&table=TableName&lsn=0x0002D435000001D90001&response_format=JSON&page=10&page_size=40");
			apiController.GetAuditDataQuery();
			apiControllerMock.Verify(m => m.GetAuditData("SchemaName", "TableName", 10, 40, "JSON", "0x0002D435000001D90001"), Times.Once);
			apiController.Request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/analytics/audit-data?schema=SchemaName&table=TableName&lsn=0x0002D435000001D90001&response_format=JSON");
			apiController.GetAuditDataQuery();
			apiControllerMock.Verify(m => m.GetAuditData("SchemaName", "TableName", 1, 0, "JSON", "0x0002D435000001D90001"), Times.Once);
			Assert(true);
		}
	}
}
