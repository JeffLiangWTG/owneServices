using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(DatabaseScopedConfigurationViewModel))]
	class DatabaseScopedConfigurationViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor_DbListIsExpected()
		{
			// Arrange
			// Act
			var vm = new DatabaseScopedConfigurationViewModel();

			// Assert
			AssertEquals(
				true,
				vm.Containers
					.Select(x => x.Code)
					.All(x => x == Db.DatabaseName || x.StartsWith(Db.DatabaseName)));
		}

		public void TestConstructorWhenBiServerIsEmpty()
		{
			// Use new db to make sure there is no BI db around main db
			AssertNoExceptionThrown(() => new DatabaseScopedConfigurationViewModel(NewDbName1));
		}

		public void TestSaveChangesToMultipleDatabases_ValuesAreRefreshedThereafter()
		{
			// Arrange
			var tuples1 = new[]
			{
				new DatabaseScopedConfigurationTuple
				{
					Name = "MAXDOP",
					ProposedValue = "100",
				},
				new DatabaseScopedConfigurationTuple
				{
					Name = "ELEVATE_ONLINE",
					ProposedValue = "WHEN_SUPPORTED",
				},
			};
			MakeChangesToDatabaseConfigurations(container1, tuples1);

			var tuples2 = new[]
			{
				new DatabaseScopedConfigurationTuple
				{
					Name = "MAXDOP",
					ProposedValue = "1000",
				},
				new DatabaseScopedConfigurationTuple
				{
					Name = "ELEVATE_ONLINE",
					ProposedValue = "FAIL_UNSUPPORTED",
				},
			};
			MakeChangesToDatabaseConfigurations(container2, tuples2);

			// Act
			vm.Save();

			// Assert
			AssertEquals(100, tuples1[0].Configuration.CurrentValue);
			AssertEquals(false, tuples1[0].Configuration.IsValueDefault);
			AssertEquals(false, tuples1[0].Configuration.HasChanges);

			AssertEquals("WHEN_SUPPORTED", tuples1[1].Configuration.CurrentValue);
			AssertEquals(false, tuples1[1].Configuration.IsValueDefault);
			AssertEquals(false, tuples1[1].Configuration.HasChanges);

			AssertEquals(1000, tuples2[0].Configuration.CurrentValue);
			AssertEquals(false, tuples2[0].Configuration.IsValueDefault);
			AssertEquals(false, tuples2[0].Configuration.HasChanges);

			AssertEquals("FAIL_UNSUPPORTED", tuples2[1].Configuration.CurrentValue);
			AssertEquals(false, tuples2[1].Configuration.IsValueDefault);
			AssertEquals(false, tuples2[1].Configuration.HasChanges);
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		public void TestSaveChangesToMultipleDatabasesButSomeUpdatesFailed_ValuesAreRefreshedThereafter()
		{
			// Arrange
			var tuples1 = new[]
			{
				new DatabaseScopedConfigurationTuple
				{
					Name = "MAXDOP",
					ProposedValue = "100",
				},
				new DatabaseScopedConfigurationTuple
				{
					Name = "LEDGER_DIGEST_STORAGE_ENDPOINT",
					ProposedValue = "localhost",
				},
			};
			MakeChangesToDatabaseConfigurations(container1, tuples1);

			var tuples2 = new[]
			{
				new DatabaseScopedConfigurationTuple
				{
					Name = "ELEVATE_ONLINE",
					ProposedValue = "WHEN_SUPPORTED",
				},
				new DatabaseScopedConfigurationTuple
				{
					Name = "LEDGER_DIGEST_STORAGE_ENDPOINT",
					ProposedValue = "127.0.0.1",
				},
			};
			MakeChangesToDatabaseConfigurations(container2, tuples2);

			// Act
			var exception = AssertExceptionThrown<AggregateException>(() => vm.Save());

			// Assert
			AssertContains(NewDbName1, exception.InnerExceptions[0].Message);
			AssertContains(NewDbName2, exception.InnerExceptions[1].Message);

			AssertEquals(100, tuples1[0].Configuration.CurrentValue);
			AssertEquals(false, tuples1[0].Configuration.IsValueDefault);
			AssertEquals(false, tuples1[0].Configuration.HasChanges);

			AssertEquals("OFF", tuples1[1].Configuration.CurrentValue);
			AssertEquals(true, tuples1[1].Configuration.IsValueDefault);
			AssertEquals(true, tuples1[1].Configuration.HasChanges);

			AssertEquals("WHEN_SUPPORTED", tuples2[0].Configuration.CurrentValue);
			AssertEquals(false, tuples2[0].Configuration.IsValueDefault);
			AssertEquals(false, tuples2[0].Configuration.HasChanges);

			AssertEquals("OFF", tuples2[1].Configuration.CurrentValue);
			AssertEquals(true, tuples2[1].Configuration.IsValueDefault);
			AssertEquals(true, tuples2[1].Configuration.HasChanges);
		}

		const string PausedResumableIndexAbortDurationMinutes = "PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES";
		const string LedgerDigestStorageEndpoint = "LEDGER_DIGEST_STORAGE_ENDPOINT";
		const string MaxDop = "MAXDOP";
		static void MakeChangesToDatabaseConfigurations(DatabaseScopedConfigurationContainer container, IEnumerable<DatabaseScopedConfigurationTuple> tuples)
		{
			foreach (var tuple in tuples)
			{
				var config = DatabaseScopedConfigurationExtensions.FindByName(container.Configurations, tuple.Name);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(config, tuple.ProposedValue);
				tuple.Configuration = config;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DatabaseScopedConfigurationViewModel();
		}

		AdminConnection adminConnection;
		DatabaseScopedConfigurationViewModel vm;

		const string NewDbName1 = nameof(DatabaseScopedConfigurationViewModelTest) + "1";
		const string NewDbName2 = nameof(DatabaseScopedConfigurationViewModelTest) + "2";
		DatabaseScopedConfigurationContainer container1;
		DatabaseScopedConfigurationContainer container2;
		protected override void SetUp()
		{
			base.SetUp();

			adminConnection = Db.NewAdminConnection(Db.SqlMasterDb);

			CreateDb(NewDbName1);
			CreateDb(NewDbName2);

			container1 = new DatabaseScopedConfigurationContainer(adminConnection, NewDbName1);
			container2 = new DatabaseScopedConfigurationContainer(adminConnection, NewDbName2);
			vm = CreateViewModel();

			DatabaseScopedConfigurationViewModel CreateViewModel()
			{
				var containers = new DatabaseScopedConfigurationViewModel.DatabaseScopedConfigurationContainerCollection();
				containers.Add(container1);
				containers.Add(container2);
				return new DatabaseScopedConfigurationViewModel(containers);
			}

			void CreateDb(string dbName)
			{
				AdoTestUtils.CreateDbDropExisting(adminConnection, dbName);
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					adminConnection.ExecuteNonQuery(@"
CREATE TABLE [StmData] (
   [SD_PK] UNIQUEIDENTIFIER NOT NULL,
   [SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
   [SD_Owner] UNIQUEIDENTIFIER NULL,
   [SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
   [SD_Type] CHAR(3) NOT NULL DEFAULT '',
   [SD_IsLogged] BIT NOT NULL DEFAULT 0,
   [SD_BinaryValue] VARBINARY(MAX) NULL,
   [SD_GuidValue] UNIQUEIDENTIFIER NULL,
   [SD_IsCancelled] BIT NOT NULL DEFAULT 0,
   [SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
   [SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
");
				}
			}
		}

		protected override void TearDown()
		{
			AdoTestUtils.DropDbIfExists(adminConnection, NewDbName1);
			AdoTestUtils.DropDbIfExists(adminConnection, NewDbName2);

			adminConnection.Dispose();
			vm.Dispose();
			base.TearDown();
		}

		sealed class DatabaseScopedConfigurationTuple
		{
			public string Name { get; set; }
			public string ProposedValue { get; set; }
			public DatabaseScopedConfiguration Configuration { get; set; }
		}
	}
}
