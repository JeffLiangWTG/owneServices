using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[CreateDatabase("4A6CBE267ADF49078DA396D5FE9AF3F9", DbSchema.RefDbRepoSafe, "929D732F277841929B49EE861B01358B", DbSchema.RefDbRepoStaging)]
	[CreateSafeDataUpdateService("4A6CBE267ADF49078DA396D5FE9AF3F9")]
	class XmlProcessIntegrationTestEngine
	{
		IDbConnection stagingDbConnection;
		IDbConnection safeDbConnection;

		string stagingDbConnectionString;
		string safeDbConnectionString;
		string jsonConfigFile;

		[TestCaseSource(typeof(XmlProcessIntegrationTestHelper), nameof(XmlProcessIntegrationTestHelper.GetTestedClasses))]
		public void RunIntegrationTest(Type type)
		{
			stagingDbConnection = new SqlConnection(stagingDbConnectionString);
			stagingDbConnection.Open();

			safeDbConnection = new SqlConnection(safeDbConnectionString);
			safeDbConnection.Open();
			try
			{
				var testedInstance = (IXmlProcessIntegrationTest)Activator.CreateInstance(type);
				Console.WriteLine($"Description: {testedInstance!.TestDescription}");
				Console.WriteLine("=============================");
				var fileNames = testedInstance.FileNames;
				var assertResults = testedInstance.AssertResults;
				Assert.That(fileNames.Length > 0);
				Assert.That(fileNames.Length == assertResults.Length);

				Assert.Multiple(async () =>
				{
					PrepareData(testedInstance);
					for (int i = 0; i < fileNames.Length; i++)
					{
						var fileName = fileNames[i];
						var assertResult = assertResults[i];

						ParseXml(fileName, XmlProcessIntegrationTestHelper.PathOfTypeDictionary[type]);

						await Merge();
						AssertResult(assertResult);
					}
				});
			}
			finally
			{
				try
				{
					CleanDatabase();
				}
				finally
				{
					stagingDbConnection?.Dispose();
					safeDbConnection?.Dispose();
				}
			}
		}

		[Test]
		public void TestSafeRepositoryGetLatestRecords()
		{
			var columnValue = "Reference Data";
			const string testUserAuthorizationPk = "0F550F63-ABB3-49CF-AA82-446F2CEE7C70";
			const string clientAuthorizationPk = "A0EF237A-8269-4723-AC49-42ABBD3630FA";

			safeDbConnection = new SqlConnection(safeDbConnectionString);
			safeDbConnection.Open();
			try
			{
				using (var safeCommand = safeDbConnection.CreateCommand())
				{
					safeCommand.CommandText = $@"
INSERT INTO [dbo].[UserAuthorization] (UA_PK, UA_User, UA_DataSetName, UA_TableName, UA_ColumnName, UA_ColumnValue)
VALUES
('{testUserAuthorizationPk}', 'testUser', 'Ref App', 'RefApplicationAttribute', 'RAA_JobGroup','{columnValue}'),
('{clientAuthorizationPk}', '{Constants.ClientId}', 'client', 'UserAuthorization', '','');";
					safeCommand.ExecuteNonQuery();
				}

				using (var accessTokenProvider = new AccessTokenProvider(Constants.TenantId, Constants.ClientId, Constants.ServiceId, Constants.PrivateKeyFileName, Constants.CertificateFileName))
				{
					var safeRepository = new SafeRepository(new Uri(Constants.SafeUpdateServiceUri), accessTokenProvider: accessTokenProvider);
					var userAuthorizations = safeRepository.GetLatest<UserAuthorization>()
						.Where(x => x.UA_User.Equals("testUser", StringComparison.OrdinalIgnoreCase)).ToArray();
					Assert.That(userAuthorizations, Has.Length.EqualTo(1));
					Assert.That(userAuthorizations[0].UA_ColumnValue, Is.EqualTo(columnValue));

					columnValue = "Air Team";
					using (var safeCommand = safeDbConnection.CreateCommand())
					{
						safeCommand.CommandText = $@"
UPDATE [dbo].[UserAuthorization] SET UA_ColumnValue = '{columnValue}' WHERE UA_User = 'testUser'";
						safeCommand.ExecuteNonQuery();
					}
					userAuthorizations = safeRepository.GetLatest<UserAuthorization>()
						.Where(x => x.UA_User.Equals("testUser", StringComparison.OrdinalIgnoreCase)).ToArray();
					Assert.That(userAuthorizations, Has.Length.EqualTo(1));
					Assert.That(userAuthorizations[0].UA_ColumnValue, Is.EqualTo(columnValue));
				}
			}
			finally
			{
				try
				{
					using (var safeCommand = safeDbConnection.CreateCommand())
					{
						safeCommand.CommandText = @"
DELETE FROM [dbo].[UserAuthorization] WHERE UA_PK = @testUserAuthorizationPk OR UA_PK = @clientAuthorizationPk";
						safeCommand.Parameters.Add(new SqlParameter("@testUserAuthorizationPk", testUserAuthorizationPk));
						safeCommand.Parameters.Add(new SqlParameter("@clientAuthorizationPk", clientAuthorizationPk));
						safeCommand.ExecuteNonQuery();
					}
				}
				finally
				{
					safeDbConnection?.Dispose();
				}
			}
		}

		void CleanDatabase()
		{
			var sql = @"
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'

DECLARE @TableName sysname
DECLARE @SqlStatement NVARCHAR(MAX)
DECLARE TableCursor CURSOR FOR
SELECT name
FROM sys.tables where name not like '%History'
OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @TableName
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @SqlStatement = 'delete from ' + QUOTENAME(@TableName)
	EXEC sp_executesql @SqlStatement
	FETCH NEXT FROM TableCursor INTO @TableName
END
CLOSE TableCursor
DEALLOCATE TableCursor

EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'
";
			using (var stagingCommand = stagingDbConnection.CreateCommand())
			{
				stagingCommand.CommandText = sql;
				stagingCommand.ExecuteNonQuery();
			}
			using (var safeCommand = safeDbConnection.CreateCommand())
			{
				safeCommand.CommandText = sql;
				safeCommand.ExecuteNonQuery();
			}
		}

		void PrepareData(IXmlProcessIntegrationTest testedInstance)
		{
			using (var stagingCommand = stagingDbConnection.CreateCommand())
			using (var safeCommand = safeDbConnection.CreateCommand())
			{
				PrepareCommonData(stagingCommand, safeCommand);
				testedInstance.PrepareData(stagingCommand, safeCommand);
			}
		}

		static void PrepareCommonData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			string safeSql = $@"
insert into [dbo].[UserAuthorization] (UA_PK, UA_User, UA_DataSetName, UA_TableName, UA_ColumnName, UA_ColumnValue)
values (NEWID(), '{Constants.ClientId}', 'AllDataSets', '*', '','');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void ParseXml(string fileName, string directory)
		{
			var filePath = Path.Combine(directory, fileName);
			Assert.True(File.Exists(filePath), $"{filePath} does not exist, please check filename and its folder.");
			UniversalXmlProcessor.Configuration.ApplicationConfig.SetConfigFileForTest(jsonConfigFile);
			Constants.SetConfigFileForTest(jsonConfigFile);
			DbConnectionStringManager.SetConfigFileForTest(jsonConfigFile);
			Cargowise.RefDbRepo.UniversalXmlProcessor.Program.Main(new[] { filePath, stagingDbConnectionString });
		}

		async Task Merge()
		{
			Constants.SetConfigFileForTest(jsonConfigFile);
			await Program.Main([]);
		}

		void AssertResult(XmlProcessIntegrationTestAssertResultHandler assertResult)
		{
			using (var stagingCommand = stagingDbConnection.CreateCommand())
			using (var safeCommand = safeDbConnection.CreateCommand())
			{
				assertResult(stagingCommand, safeCommand);
			}
		}

		[OneTimeSetUp]
		public void SetUp()
		{
			var stagingDbName = CreateDatabaseAttribute.GetDbName("929D732F277841929B49EE861B01358B");
			var safeDbName = CreateDatabaseAttribute.GetDbName("4A6CBE267ADF49078DA396D5FE9AF3F9");
			stagingDbConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			safeDbConnectionString = TestConnectionString.GetAdmin(safeDbName);
			jsonConfigFile = "CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test.config.json";
		}
	}
}
