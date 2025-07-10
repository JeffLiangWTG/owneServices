using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[TestedType(typeof(RefDataRepoRegistry))]
	public class RefDataRepoRegistryTest : RegistryItemSetTestCaseWithFactory<RefDataRepoRegistry>
	{
		public void TestRefDbRepoServiceUri()
		{
			TestRegistryItem(ItemSet.RefDbRepoServiceUri,
				"RefDbRepoServiceUri",
				RefDataRepoRegistry.System_RefDataRepo,
				"Service Uri",
				"Reference Data Repository Service Uri",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				TextEditorType.TextBox,
				@"https://refdbrepo.wisegrid.net/");
		}

		public void TestNextUpdate()
		{
			TestGenericRegistryItem(ItemSet.NextUpdate,
				"RefDbUpdaterNextRun",
				RefDataRepoRegistry.System_RefDataRepo,
				"Next Update",
				"Next Update will run no sooner than (in UTC)",
				RegistryStorageFlags.System,
				RegistryOptions.NotCached);
		}

		public void TestUpdateInterval()
		{
			TestGenericRegistryItem(ItemSet.UpdateInterval,
				"RefDbUpdateInterval",
				RefDataRepoRegistry.System_RefDataRepo,
				"Update Interval",
				"The minimum time (in minutes) between two successful updates.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				60);
		}

		public void TestVersionControlReset()
		{
			TestGenericRegistryItem(ItemSet.VersionControlReset,
				"VersionControlReset",
				RefDataRepoRegistry.System_RefDataRepo,
				"Version Control Reset",
				"Un-manage all data managed so far, and re-download them from server.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
				false);
		}

		public void TestFailureCount()
		{
			TestGenericRegistryItem(ItemSet.FailureCount,
				"FailureCount",
				RefDataRepoRegistry.System_RefDataRepo,
				"Failure Count",
				"Number of continuous failed attempts to get updates from the service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				0);
		}

		public void TestHttpClientTimeout()
		{
			TestGenericRegistryItem(ItemSet.HttpClientTimeout,
				"HttpClientTimeout",
				RefDataRepoRegistry.System_RefDataRepo,
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
				"RefDbRepoCompressData",
				RefDataRepoRegistry.System_RefDataRepo,
				"Compress data",
				"Set this to request server to send data in compressed form.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
				true);
		}

		public void TestVerboseLogging()
		{
			TestGenericRegistryItem(ItemSet.VerboseLogging,
				"RefDbRepoVerboseLogging",
				RefDataRepoRegistry.System_RefDataRepo,
				"Verbose logging",
				"Verbose logging for REF service task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
				false);
		}
	}
}
