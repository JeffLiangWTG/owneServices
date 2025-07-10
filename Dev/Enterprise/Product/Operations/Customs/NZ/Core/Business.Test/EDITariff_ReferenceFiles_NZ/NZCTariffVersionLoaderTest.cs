using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	public class NZCTariffVersionLoaderTest : TestCaseWithFactory
	{
		public void TestStmDataExists()
		{
			DropStmDataTable();

			AssertEquals(GetMinimumDataVersionMessage(NZCTariffVersionLoader.MinimumDataVersionRequired, 0), new NZCTariffVersionLoader(Factory).ErrorMessage);
		}

		public static void DropStmDataTable()
		{
			string sqlText = string.Format("IF EXISTS (SELECT null FROM [{0}].sys.objects WHERE name = 'StmData') DROP TABLE [{0}]..StmData", DbName);

			using (var command = Connection.Command(sqlText))
			{
				command.ExecuteNonQuery();
			}
		}

		public void TestIsMinimumDataVersionMet()
		{
			SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			AssertNullOrEmpty("No Errors No Problem", new NZCTariffVersionLoader(Factory).ErrorMessage);

			SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired - 1);
			AssertEquals(GetMinimumDataVersionMessage(NZCTariffVersionLoader.MinimumDataVersionRequired, NZCTariffVersionLoader.MinimumDataVersionRequired - 1),
							new NZCTariffVersionLoader(Factory).ErrorMessage);

			AssertEquals("Invalid object name 'ABCDE'.", new NZCTariffVersionLoaderForExceptionThrown(Factory).ErrorMessage);
		}

		[ExpectNoExceptions]
		public void TestGetMinimumDataVersionMissingDatabase()
		{
			((IPhysicalRefDbLocation)Connection).ClearRefDbNameBuffers();
			string synonymPrefix = RefDbTableNameResolver.GetRefDbSynonymPrefix(RefDbTypeEnum.Tariff, "NZ");
			string sqlText = string.Format(@"
				DECLARE @DropSynonymCommand nvarchar(max)='';
				SELECT @DropSynonymCommand = @DropSynonymCommand + dropCommand
				FROM
					(	SELECT DISTINCT 'DROP SYNONYM ' + SYN.name + '; ' COLLATE {2} AS dropCommand
						FROM [{0}].sys.synonyms SYN
						WHERE  SYN.name like '{1}%'
					) dropCommands;
				EXEC (@DropSynonymCommand);", Connection.CurrentDatabase, synonymPrefix, "SQL_Latin1_General_CP1_CI_AS");

			using (var command = Connection.Command(sqlText))
			{
				command.ExecuteNonQuery();
			}
			AssertEquals(GetMinimumDataVersionMessage(NZCTariffVersionLoader.MinimumDataVersionRequired, 0),
							new NZCTariffVersionLoader(Factory).ErrorMessage);
		}

		public static void SetDataVersion(int version)
		{
			string sqlText = string.Format(@"
UPDATE [{0}]..[StmData]
SET SD_BinaryValue = CONVERT(image, CONVERT(varbinary(8000), N'{1}'))
	WHERE [SD_Name] = 'DATABASE_DATA_VERSION'
IF @@ROWCOUNT < 1
	INSERT INTO [{0}]..[StmData] ([SD_Name], [SD_BinaryValue])
	VALUES ('DATABASE_DATA_VERSION', CONVERT(image, CONVERT(varbinary(8000), N'{1}')))
", DbName, version);

			using (var command = Connection.Command(sqlText))
			{
				command.ExecuteNonQuery();
			}
		}

		static DbConnection Connection
		{
			get { return ((IDbConnected)(new BusinessObjectFactory())).Connection; }
		}

		static string DbName
		{
			get { return ((IPhysicalRefDbLocation)Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ"); }
		}

		public static string GetMinimumDataVersionMessage(int required, int actual)
		{
			return string.Format(@"NZ Tariff minimum data version [{0}] requirement not met. The current Tariff data version is [{1}].
Please update your Tariff data by running ediTariff -> " + Core.Constants.ProductName + " -> Export NZ data to " + Core.Constants.ProductName + ".", required, actual);
		}

		class NZCTariffVersionLoaderForExceptionThrown : NZCTariffVersionLoader
		{
			public NZCTariffVersionLoaderForExceptionThrown(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override string SelectVersionSql
			{
				get { return "SELECT TOP 1 {0} FROM ABCDE"; }
			}
		}
	}

	public class NZCTariffVersionLoaderNonTransactionalTest : TestCase
	{
		public void TestNoRightsToRefDatabase()
		{
			string testDb = NZCTariffVersionLoaderWithTestDatabase.TestDb;

			using (var testAdminConn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testDb);
					testAdminConn.ExecuteNonQuery(String.Format("CREATE DATABASE [{0}]", testDb));

					using (((ICurrentDbControl)testAdminConn).UseDatabase(testDb))
					{
						testAdminConn.ExecuteNonQuery("CREATE TABLE T1 (Col1 int)");
						testAdminConn.ExecuteNonQuery("INSERT T1 VALUES (" + Int32.MaxValue + ")");
						CopyApplicationUserToTestDatabase(testAdminConn);
					}

					var versionLoader = new NZCTariffVersionLoaderWithTestDatabase(new BusinessObjectFactory());
					AssertEquals("ErrorMessage", "", versionLoader.ErrorMessage);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testDb);
				}
			}
		}

		void CopyApplicationUserToTestDatabase(AdminConnection testAdminConn)
		{
			string generateCreateUserSql = String.Format(CultureInfo.InvariantCulture, @"
				SELECT '
				IF NOT EXISTS(SELECT NAME FROM sys.server_principals WHERE NAME = ' + '''' + dp.name + '''' +')
				BEGIN
				CREATE USER [' + dp.name + '] FOR LOGIN [' + dp.name + ']
				END'
				FROM [{0}].sys.database_principals dp
				INNER JOIN sys.server_principals sp ON sp.sid = dp.sid
				WHERE dp.type = 'S'
				AND dp.name like '{0}[_]%'
				AND dp.name not like '%reader%'",
				Db.DatabaseName);

			foreach (string createUserSql in DataUtils.GetListOfValuesFromQuery(testAdminConn, generateCreateUserSql))
			{
				testAdminConn.ExecuteNonQuery(createUserSql);
			}
		}

		class NZCTariffVersionLoaderWithTestDatabase : NZCTariffVersionLoader
		{
			public NZCTariffVersionLoaderWithTestDatabase(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override string SelectVersionSql
			{
				get { return String.Format(CultureInfo.InvariantCulture, "SELECT Col1 FROM [{0}]..T1", TestDb); }
			}

			public static readonly string TestDb = Db.DatabaseName + "_SD000";
		}
	}
}
