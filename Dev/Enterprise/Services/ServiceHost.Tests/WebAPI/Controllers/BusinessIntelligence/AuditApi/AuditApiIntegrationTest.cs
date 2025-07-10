using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CargoWise.Async;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.Test;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit;
using Enterprise.ZArchitecture.Core;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test
{
	[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
	class AuditApiIntegrationTest : TestCase
	{
		protected override void SetUp()
		{
			initialUserContext = Env.CurrentUserContext;
			auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName);
			factory = new BusinessObjectFactory();
			staff = SetupUser(factory);
			server = TestServer.Create<TestServerStartup>();
		}

		protected override void TearDown()
		{
			Env.Instance.CleanupUserContextOnCurrentThread();
			Env.SetUserContext(initialUserContext);
			server.Dispose();
			auditConnection.Dispose();
		}

		IUserContext initialUserContext;
		BusinessObjectFactory factory;
		DbConnection auditConnection;
		GlbStaff staff;
		TestServer server;

		// Request without authentication
		public void TestAuthenticationIsChecked()
		{
			var request = server.CreateRequest("http://testserver/api/replication/change-summary?afterLsn=0x000000000000000ff000&format=JSON");

			var response = ExecuteRequest(request);
			var contents = GetResponseString(response);

			AssertEquals("Unauthorized", contents);
		}

		// Request with invalid parameters
		public void TestParametersAreChecked()
		{
			using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				GiveUserPermissions();
				EnableApi();
				var expectedVersionNumber = new EnterpriseInformationRetriever().VersionNumber;
				var request = server.CreateRequest("http://testserver/api/replication/change-summary?after_lsn=0xasdfasdf&format=anInvalidFormatType");
				request.AddHeader("Authorization", AuthHeader.ToString());

				var response = ExecuteRequest(request);
				var contents = GetResponseStringGzip(response);

				var expectedMessage = $"{{\"apiVersion\":\"{expectedVersionNumber}\",\"error\":{{\"errors\":[{{\"message\":\"Parameter is invalid: The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x').\"}},{{\"message\":\"Parameter is invalid: The value 'anInvalidFormatType' is not valid for Format.\"}}]}}}}";
				AssertEquals(expectedMessage, contents);
			}
		}

		public void TestParametersAreCheckedWhenItsNull()
		{
			using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				GiveUserPermissions();
				EnableApi();
				var expectedVersionNumber = new EnterpriseInformationRetriever().VersionNumber;
				var request = server.CreateRequest("http://testserver/api/replication/change-summary");
				request.AddHeader("Authorization", AuthHeader.ToString());

				var response = ExecuteRequest(request);
				var contents = GetResponseStringGzip(response);

				var expectedMessage = $"{{\"apiVersion\":\"{expectedVersionNumber}\",\"error\":{{\"errors\":[{{\"message\":\"Missing parameters. Please check documentation for correct usage.\"}}]}}}}";
				AssertEquals(expectedMessage, contents);
			}
		}

		public void TestSummaryResultsAreCorrect()
		{
			// CW1IdentityBasicAuthentication sets the user context to the authenticated user, which then causes an error after the snapshot cleans up that user
			// so use a disposable to restore the user context after the test finishes
			using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				auditConnection.ExecuteNonQuery(@"
TRUNCATE TABLE biadmin.TableState;
INSERT INTO biadmin.TableState (SourceSchemaName, SourceTableName, AetHWMHistorySummaryLsn) VALUES
('dbo', 'table1', 0x01), -- last change before lsn
('dbo', 'table2', 0x02), -- last change was their hwm
('dbo', 'table3', 0x03),
('dbo', 'table4', 0x04),
('dbo', 'table5', NULL); -- never had a change
;");

				BiMasterState.SetParameter(auditConnection, BiConstants.LastMaxLsnProcessed, "0x04000000000000000000");
				GiveUserPermissions();
				EnableApi();
				var expectedVersionNumber = new EnterpriseInformationRetriever().VersionNumber;

				var request = server.CreateRequest("http://testserver/api/replication/change-summary?after_lsn=0x02000000000000000000&format=JSON");
				request.AddHeader("Authorization", AuthHeader.ToString());

				var response = ExecuteRequest(request);
				var contents = GetResponseStringGzip(response);

				var expected = $"{{\"apiVersion\":\"{expectedVersionNumber}\",\"data\":{{\"afterLsn\":\"0x02000000000000000000\",\"maxLsn\":\"0x04000000000000000000\",\"totalItems\":2,\"items\":[{{\"schemaName\":\"dbo\",\"tableName\":\"table3\"}},{{\"schemaName\":\"dbo\",\"tableName\":\"table4\"}}]}}}}";
				AssertEquals(expected, contents);
			}
		}

		public void TestChangeDetail()
		{
			using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				auditConnection.ExecuteNonQuery(@"
TRUNCATE TABLE biadmin.SchemaVersionHistory
TRUNCATE TABLE biadmin.SchemaMappingHistory

SET IDENTITY_INSERT biadmin.SchemaVersionHistory ON;

INSERT INTO biadmin.SchemaVersionHistory (SchemaVersionId, SchemaVersion)
VALUES (1, '8000.0')

INSERT INTO biadmin.SchemaMappingHistory (SchemaVersionId, SchemaName, TableName, ColumnName, ColumnOrdinal, DataType, MaxLength, Precision, Scale)
VALUES
	(1, 'dbo', 'StmData', 'SD_PK', 1, 'uniqueidentifier', 16, 0, 0),
	(1, 'dbo', 'StmData', 'SD_Name', 2, 'varchar', 300, 0, 0)

INSERT INTO dbo.StmData (__$start_lsn, __$seqval, __$operation, __$update_mask, __$lsn_period, __$command_id, SD_PK, SD_Name)
SELECT 0xFFFFFFFFFFFFFFFFFFFF, 0x0, 3, 0xFFFFFFFFFFFFFFFFFFFF, 0, 0, newid(), 'Test'");

				BiMasterState.SetParameter(auditConnection, BiConstants.LastMaxLsnProcessed, "0xFFFFFFFFFFFFFFFFFFFF");
				GiveUserPermissions();
				EnableApi();
				var expectedVersionNumber = new EnterpriseInformationRetriever().VersionNumber;

				var request = server.CreateRequest("http://testserver/api/replication/change-detail?schemaName=dbo&tableName=StmData&after_lsn=0x00000000000000000000");
				request.AddHeader("Authorization", AuthHeader.ToString());

				var response = ExecuteRequest(request);
				var contents = GetResponseStringGzip(response);

				CombineAssertions(() =>
				{
					AssertContains(@"""schemaName"":""dbo""", contents);
					AssertContains(@"""tableName"":""StmData""", contents);
					AssertContains(@"""totalItems"":1", contents);
				});
			}
		}

		public void TestChangeDetail_NoChanges()
		{
			BiMasterState.SetParameter(auditConnection, BiConstants.LastMaxLsnProcessed, "0xFFFFFFFFFFFFFFFFFFFF");
			GiveUserPermissions();
			EnableApi();
			var expectedVersionNumber = new EnterpriseInformationRetriever().VersionNumber;

			var request = server.CreateRequest("http://testserver/api/replication/change-detail?schemaName=dbo&tableName=StmData&after_lsn=0x00000000000000000000");
			request.AddHeader("Authorization", AuthHeader.ToString());

			var response = ExecuteRequest(request);
			var contents = GetResponseStringGzip(response);

			var expected =
@$"
{{
	""apiVersion"":""{expectedVersionNumber}"",
	""data"":
	{{
		""schemaName"":""dbo"",
		""tableName"":""StmData"",
		""totalItems"":0,
		""pageSize"":1000,
		""lastItem"":
		{{
			""__$start_lsn"":""0xFFFFFFFFFFFFFFFFFFFF"",
			""__$seqval"":""0xFFFFFFFFFFFFFFFFFFFF"",
			""__$command_id"":2147483647,
			""__$operation"":4
		}},
		""items"":[]
	}}
}}";
			expected = expected.Replace("\r\n", "").Replace("\n", "").Replace("\t", "");
			AssertEquals(expected, contents);
		}

		public void TestBearerAuthenticationFails_WithInvalidToken()
		{
			var request = server.CreateRequest("http://testserver/api/replication/change-summary?after_lsn=0x02000000000000000000&format=JSON");
			request.AddHeader("Authorization", "Bearer invalid-token");

			var response = ExecuteRequest(request);

			AssertNotNull(response);
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			var contents = GetResponseString(response);
			AssertEquals("Unauthorized", contents);
		}

		const string testUsername = "oliver";
		const string testPassword = "thePassword";

		GlbStaff SetupUser(BusinessObjectFactory factory, string username = testUsername, string password = testPassword)
		{
			var helper = new BasicAuthTestHelper(factory);
			var staff = helper.CreateStaff(username, password, "tst", active: true, resource: false, isController: false);
			factory.Save();
			return staff;
		}

		void GiveUserPermissions(bool isAdmin = true)
		{
			staff.GS_IsController = isAdmin;
			factory.Save();
		}

		void EnableApi(bool enabled = true)
		{
			SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);
		}

		AuthenticationHeaderValue AuthHeader => new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes($"{testUsername}:{testPassword}")));

		HttpResponseMessage ExecuteRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.GetAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}

		string GetResponseString(HttpResponseMessage response)
		{
			return response.Content.ReadAsStringAsync().Result;
		}

		string GetResponseStringGzip(HttpResponseMessage response)
		{
			var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
			var deflated = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
			return new StreamReader(deflated).ReadToEnd();
		}
	}

	class MyAssemblyResolver : IAssembliesResolver
	{
		public ICollection<Assembly> GetAssemblies()
		{
			return new List<Assembly> { Assembly.GetAssembly(typeof(AuditApiController)) };
		}
	}

	class TestServerStartup
	{
		public void Configuration(IAppBuilder app)
		{
			HttpConfiguration config = new HttpConfiguration();
			config.Services.Replace(typeof(IAssembliesResolver), new MyAssemblyResolver());
			config.MapHttpAttributeRoutes();
			app.UseWebApi(config);
		}
	}
}
