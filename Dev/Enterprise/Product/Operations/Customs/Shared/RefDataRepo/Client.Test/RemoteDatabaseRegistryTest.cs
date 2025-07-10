using System;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[TestedType(typeof(RemoteDatabaseRegistry))]
	public class RemoteDatabaseRegistryTest : RegistryItemSetTestCaseWithFactory<RemoteDatabaseRegistry>
	{
		public void TestRefDbRepoServiceUri()
		{
			RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			TestRegistryItem(ItemSet.RemoteDatabaseServiceUri,
				"RemoteDatabaseServiceUri",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Service Uri",
				@"Remote Database Service Uri
Please save the record after changing",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
				TextEditorType.TextBox,
				@"https://refdbrepo.wisegrid.net/");
		}

		public void TestNextUpgrade()
		{
			TestGenericRegistryItem(ItemSet.NextUpgrade,
				"RemoteDatabaseServiceNextRun",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Next Upgrade",
				"Next Upgrade will run no sooner than (in UTC)",
				RegistryStorageFlags.System,
				RegistryOptions.NotCached);
		}

		public void TestUpgradeInterval()
		{
			TestGenericRegistryItem(ItemSet.UpgradeInterval,
				"RemoteDatabaseServiceUpgradeInterval",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Upgrade Interval",
				"The minimum time (in minutes) between two successful upgrades.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				60);
		}

		public void TestHttpClientTimeout()
		{
			TestGenericRegistryItem(ItemSet.HttpClientTimeout,
				"RemoteDatabaseServiceHttpClientTimeout",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Http Client Timeout",
				@"How long (in seconds) client should wait for server to respond before giving up. Only apply for data downloading.
Setting a non-positive integer means client will wait for-ever",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				1800);
		}

		public void TestCompressData()
		{
			TestGenericRegistryItem(ItemSet.CompressData,
				"RemoteDatabaseServiceRefDbRepoCompressData",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Compress data",
				"Set this to request server to send data in compressed form.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
				true);
		}

		public void TestVersionControlReset()
		{
			TestGenericRegistryItem(ItemSet.VersionControlReset,
				"RemoteDatabaseVersionControlReset",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Version Control Reset",
				"Un-manage all data managed so far, and re-download them from server.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
				false);
		}

		public void TestEnableIssueReport()
		{
			TestGenericRegistryItem(ItemSet.EnableIssueReport,
				"EnableIssueReport",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"RDU/REF Issue reporting",
				@"By deafult RDU/REF reports issues to the Issue Manager only when URI and DB name (for RDU) is 'default' to prevent various testing cases to be escalated as issue. When default setting is overriden, all issues raised by RDU/REF will be reported to the Issue Manager.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
				false);
		}

		public void TestFailureCount()
		{
			TestGenericRegistryItem(ItemSet.FailureCount,
				"RemoteDataBaseFailureCount",
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Failure Count",
				"Number of continuous failed attempts to get updates from the service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestSingleRefDatabaseName()
		{
			TestRegistryItem(ItemSet.SingleRefDatabaseName,
				DbRegistry.SingleRefDatabaseName.ItemName,
				RemoteDatabaseRegistry.System_RemoteDatabaseService,
				"Single RefDb Name",
				@"Use another single reference database name other than Single Reference Database's default name. This unique single reference database name will be used exclusively by this CW1 instance as opposed to shared Single Reference Database. 
Every time this registry item changed, please first save the value then run Help -> Database Administration -> Recreate Database Synonyms to create synonyms correctly pointing to the new db.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
				TextEditorType.TextBox,
				RefDbTableNameResolver.DefaultSingleRefDbName);
		}
	}
}
