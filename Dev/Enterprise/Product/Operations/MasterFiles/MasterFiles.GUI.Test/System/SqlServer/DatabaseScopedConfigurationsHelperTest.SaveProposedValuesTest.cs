using System;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed partial class DatabaseScopedConfigurationsHelperTest
	{
		sealed class SaveProposedValuesTest : TestCase
		{
			public void TestInt()
			{
				var proposedValues = new[]
				{
					"10",
					" 10 ",
					@"
10
"
				};
				foreach (var value in proposedValues)
				{
					Test(value);
				}

				void Test(string value)
				{
					// Arrange
					const string CfgName = "MAXDOP";
					var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
					var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
					DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, value);

					// Act
					DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

					// Assert
					AssertEquals(false, cfg.HasRowNotifications);
					AssertEquals(10, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, CfgName));
				}
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			public void TestInt_Error()
			{
				Test(
					"MAXDOP",
					"-1",
					"Writing value [-1] failed due to ['-1' is out of range for the database scoped configuration option 'MAXDOP'. See sp_configure option 'max degree of parallelism' for valid values.]. SQL error number = 12108.");
				Test(
					"PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES",
					"-1",
					"Writing value [-1] failed due to [Time value -1 used with PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES is not a valid value; PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES wait time must be greater or equal to 0 and less or equal to 71582.]. SQL error number = 12121.");

				void Test(string cfgName, string proposedValue, string error)
				{
					// Arrange
					var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
					var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, cfgName);
					DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, proposedValue);

					// Act
					AssertExceptionThrown<AggregateException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));

					// Assert
					AssertEquals(
						$"Actual error:\r\n{string.Join("\r\n", cfg.RowErrors.Select(x => x.Message))}",
						true,
						cfg.RowErrors.Any(x => x.Message == error));
					AssertEquals(cfg.CurrentValue, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, cfgName));
				}
			}

			public void TestReservedKeyword()
			{
				// Arrange
				const string CfgName = "ELEVATE_ONLINE";
				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
				var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, "WHEN_SUPPORTED");

				// Act
				DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

				// Assert
				AssertEquals(false, cfg.HasRowNotifications);
				AssertEquals(cfg.ProposedValue, DatabaseScopedConfigurationExtensions.ReadConfiguration<string>(adminConnection, CfgName));
			}

			public void TestReservedKeyword_Error()
			{
				// Arrange
				const string CfgName = "ELEVATE_ONLINE";
				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
				var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, "WHE_SUPPORTED");

				// Act
				AssertExceptionThrown<InvalidOperationException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));

				// Assert
				AssertEquals(cfg.CurrentValue, DatabaseScopedConfigurationExtensions.ReadConfiguration<string>(adminConnection, CfgName));
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			public void TestString()
			{
				const string CfgName = "LEDGER_DIGEST_STORAGE_ENDPOINT";

				TestGoodValue("OFF");
				TestValueNotPreparedWell("https://mystorage.blob.core.windows.net");
				TestValueNotPreparedWell("'https://mystorage.blob.core.windows.net'");

				void TestGoodValue(string value)
				{
					// Arrange
					var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
					var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
					DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, value);

					// Act
					DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

					// Assert
					AssertEquals(false, cfg.HasRowNotifications);
					AssertEquals(cfg.ProposedValue, DatabaseScopedConfigurationExtensions.ReadConfiguration<string>(adminConnection, CfgName));
				}

				void TestValueNotPreparedWell(string value)
				{
					// Arrange
					var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
					var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
					DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, value);

					// Act
					// Assert
					var exception = AssertExceptionThrown<AggregateException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));
					AssertEquals(
						@"A specific error of number 37531 means setting this endpoint fails due to lack of access,
and it also indicates that the value provided to SQL Server meets syntax requirement.",
						37531,
						((SqlException)exception.InnerExceptions[0].InnerException).Number);
					AssertEquals(true, cfg.HasRowNotifications);
				}
			}

			public void TestSqlInjection()
			{
				// Arrange
				const string NewTableName = "NewTable";
				const string CfgName = "MAXDOP";
				var cfgProposedValue = $"10; CREATE TABLE {NewTableName} (Id INT PRIMARY KEY CLUSTERED);";

				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
				var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, cfgProposedValue);

				// Act
				AssertExceptionThrown<InvalidOperationException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));

				// Assert
				AssertNotEquals(10, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, CfgName));
				AssertEquals(
					"Table wasn't created",
					0,
					adminConnection.ExecuteScalar<int>($"SELECT IIF(OBJECT_ID('{NewTableName}', 'U') IS NOT NULL, 1, 0);"));
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			public void TestSqlInjection_String()
			{
				const string NewTableName = "NewTable";
				var proposedValues = new[]
				{
					$"'LOCALHOST'; CREATE TABLE {NewTableName} (Id INT PRIMARY KEY CLUSTERED);",
					$"OFF; CREATE TABLE {NewTableName} (Id INT PRIMARY KEY CLUSTERED);",
					$"LOCALHOST'; CREATE TABLE {NewTableName} (Id INT PRIMARY KEY CLUSTERED, Code NVARCHAR(4)); SELECT * FROM {NewTableName} WHERE Code = 'ABCD",
				};
				foreach (var value in proposedValues)
				{
					Test(value);
				}

				void Test(string value)
				{
					// Arrange
					const string CfgName = "LEDGER_DIGEST_STORAGE_ENDPOINT";
					var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
					var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
					DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, value);

					// Act
					AssertExceptionThrown<Exception>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));

					// Assert
					AssertEquals(cfg.CurrentValue, DatabaseScopedConfigurationExtensions.ReadConfiguration<string>(adminConnection, CfgName));
					AssertEquals(
						"Table wasn't created",
						0,
						adminConnection.ExecuteScalar<int>($"SELECT IIF(OBJECT_ID('{NewTableName}', 'U') IS NOT NULL, 1, 0);"));
				}
			}

			public void TestMultipleItems()
			{
				// Arrange
				const string MaxDop = "MAXDOP";
				const string ElevateOnline = "ELEVATE_ONLINE";

				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(DatabaseScopedConfigurationExtensions.FindByName(configurations, MaxDop), "10");
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(DatabaseScopedConfigurationExtensions.FindByName(configurations, ElevateOnline), "WHEN_SUPPORTED");

				// Act
				DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

				// Assert
				AssertEquals(10, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, MaxDop));
				AssertEquals("WHEN_SUPPORTED", DatabaseScopedConfigurationExtensions.ReadConfiguration<string>(adminConnection, ElevateOnline));
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			public void TestMultipleItems_AllIsProcessedIfError()
			{
				// Arrange
				const string PausedResumableIndexAbortDurationMinutes = "PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES";
				const string LedgerDigestStorageEndpoint = "LEDGER_DIGEST_STORAGE_ENDPOINT";
				const string MaxDop = "MAXDOP";

				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

				var pausedResumableIndexAbortDurationMinutes = DatabaseScopedConfigurationExtensions.FindByName(configurations, PausedResumableIndexAbortDurationMinutes);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(pausedResumableIndexAbortDurationMinutes, "-1");
				var ledgerDigestStorageEndpoint = DatabaseScopedConfigurationExtensions.FindByName(configurations, LedgerDigestStorageEndpoint);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(ledgerDigestStorageEndpoint, "ABC");
				var maxDop = DatabaseScopedConfigurationExtensions.FindByName(configurations, MaxDop);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(maxDop, "100");

				// Act
				var aggregateException = AssertExceptionThrown<AggregateException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));

				// Assert
				AssertEquals(2, aggregateException.InnerExceptions.Count);
				foreach (var exception in aggregateException.InnerExceptions)
				{
					AssertType<InvalidOperationException>(exception);
					AssertType<SqlException>(exception.InnerException);
				}

				AssertEquals(true, ledgerDigestStorageEndpoint.HasRowNotifications);
				AssertEquals(true, pausedResumableIndexAbortDurationMinutes.HasRowNotifications);
				AssertEquals(100, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, MaxDop));
			}

			public void TestExceptionWhenAllHasNoChange()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

				// Act
				DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName).ProposedValue = "10";

				// Assert
				AssertExceptionThrown<InvalidOperationException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));
			}

			public void TestExceptionIfAnyConfigHasErrors()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
				var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, "1'0");

				// Act
				// Assert
				AssertExceptionThrown<InvalidOperationException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));
			}

			public void TestConfigWontBeSavedIfHasChangesIsFalse()
			{
				// Arrange
				const string MaxDop = "MAXDOP";
				const string ElevateOnline = "ELEVATE_ONLINE";

				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(DatabaseScopedConfigurationExtensions.FindByName(configurations, MaxDop), "10");

				const string newElevateOnlineValue = "FAIL_UNSUPPORTED";
				var elevateOnline = DatabaseScopedConfigurationExtensions.FindByName(configurations, ElevateOnline);
				AssertNotEquals(elevateOnline.CurrentValue, newElevateOnlineValue);
				elevateOnline.ProposedValue = newElevateOnlineValue;

				// Act
				DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

				// Assert
				AssertEquals(10, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, MaxDop));
				AssertEquals(elevateOnline.CurrentValue, DatabaseScopedConfigurationExtensions.ReadConfiguration<string>(adminConnection, ElevateOnline));
			}

			public void TestReSaveAfterFixingError()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
				var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, "-1");

				AssertExceptionThrown<AggregateException>(() => DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations));
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, "10");

				// Act
				DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

				// Assert
				AssertEquals(false, cfg.HasRowNotifications);
				AssertEquals(10, DatabaseScopedConfigurationExtensions.ReadConfiguration<int>(adminConnection, CfgName));
			}

			public void TestConfigHasChangesIsResetAfterSave()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
				var cfg = DatabaseScopedConfigurationExtensions.FindByName(configurations, CfgName);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(cfg, "10");

				// Act
				DatabaseScopedConfigurationsHelper.SaveProposedValues(adminConnection, configurations);

				// Assert
				AssertEquals(false, cfg.HasChanges);
				AssertEquals(false, cfg.CanBeSavedPotentially);
				AssertEquals(false, configurations.CanBeSavedPotentially);
			}

			AdminConnection adminConnection;
			const string NewDbName = nameof(SaveProposedValuesTest);

			protected override void SetUp()
			{
				base.SetUp();

				adminConnection = Db.NewAdminConnection(Db.SqlMasterDb);

				AdoTestUtils.CreateDbDropExisting(adminConnection, NewDbName);
				((ICurrentDbControl)adminConnection).UseDatabase(NewDbName);
			}

			protected override void TearDown()
			{
				((ICurrentDbControl)adminConnection).UseDatabase(Db.SqlMasterDb);
				AdoTestUtils.DropDbIfExists(adminConnection, NewDbName);

				adminConnection.Dispose();
				base.TearDown();
			}
		}
	}
}
