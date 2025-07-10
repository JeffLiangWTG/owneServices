using System;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public sealed class RefDataRepoRegistry : RegistryItemSet, IClientConfiguration
	{
		public static RefDataRepoRegistry Instance
		{
			get { return instance ?? (instance = new RefDataRepoRegistry()); }
		}

		[ThreadStatic]
		static RefDataRepoRegistry instance;

		public override bool IsForProductivityWise => true;

		public static MultilingualString System_RefDataRepo { get { return CombineCategories(RawDataRegistry.Categories.System, ResString.GetMultilingualString("3BD2E195-AF1F-448A-B273-321182D59545", "Reference Data Repository")); } }

		public StringRegistryItem RefDbRepoServiceUri
		{
			get
			{
				return GetItem("RefDbRepoServiceUri", delegate
				{
					return new StringRegistryItem(
						"RefDbRepoServiceUri",
						System_RefDataRepo,
						ResString.GetMultilingualString("E21FE931-0D4C-4923-91ED-9BB22DC82BD7", "Service Uri"),
						ResString.GetMultilingualString("34CE9930-5123-4398-A269-A0C2EE290ABF", "Reference Data Repository Service Uri"),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						@"https://refdbrepo.wisegrid.net/"
					);
				});
			}
		}

		public DateTimeRegistryItem NextUpdate
		{
			get
			{
				return GetItem("RefDbUpdaterNextRun", delegate
				{
					var result = new DateTimeRegistryItem(
						"RefDbUpdaterNextRun",
						System_RefDataRepo,
						ResString.GetMultilingualString("770C8285-6D46-44DE-8858-4AF13438D5B3", "Next Update"),
						ResString.GetMultilingualString("846A027F-D581-4E54-9EA7-1052A7CD7BCC", "Next Update will run no sooner than (in UTC)"),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		public IntRegistryItem UpdateInterval
		{
			get
			{
				return GetItem("RefDbUpdateInterval", () =>
				{
					return new IntRegistryItem(
						"RefDbUpdateInterval",
						System_RefDataRepo,
						(NoResString)"Update Interval",
						(NoResString)"The minimum time (in minutes) between two successful updates.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						60);
				});
			}
		}

		public BooleanRegistryItem VersionControlReset
		{
			get
			{
				return GetItem("VersionControlReset", () =>
				{
					return new BooleanRegistryItem(
						"VersionControlReset",
						System_RefDataRepo,
						(NoResString)"Version Control Reset",
						(NoResString)"Un-manage all data managed so far, and re-download them from server.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
						false
					)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					};
				});
			}
		}

		public IntRegistryItem FailureCount
		{
			get
			{
				return GetItem("FailureCount", () =>
				{
					return new IntRegistryItem(
						"FailureCount",
						System_RefDataRepo,
						(NoResString)"Failure Count",
						(NoResString)"Number of continuous failed attempts to get updates from the service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
						);
				});
			}
		}

		public IntRegistryItem HttpClientTimeout
		{
			get
			{
				return GetItem("HttpClientTimeout", () =>
				{
					return new IntRegistryItem(
						"HttpClientTimeout",
						System_RefDataRepo,
						(NoResString)"Http Client Timeout",
						(NoResString)@"How long (in seconds) client should wait for server to respond before giving up. Only apply for data downloading.
Setting a non-positive integer means client will wait for-ever",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						1800
						);
				});
			}
		}

		public BooleanRegistryItem CompressData
		{
			get
			{
				return GetItem("RefDbRepoCompressData", () => new BooleanRegistryItem(
					"RefDbRepoCompressData",
					System_RefDataRepo,
					(NoResString)"Compress data",
					(NoResString)"Set this to request server to send data in compressed form.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
					true
				)
				{ IsExcludedFromCwOnlyNonCachedTest = true });
			}
		}

		public BooleanRegistryItem VerboseLogging
		{
			get
			{
				return GetItem("RefDbRepoVerboseLogging", () => new BooleanRegistryItem(
					"RefDbRepoVerboseLogging",
					System_RefDataRepo,
					(NoResString)"Verbose logging",
					(NoResString)"Verbose logging for REF service task.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
					false
				)
				{ IsExcludedFromCwOnlyNonCachedTest = true });
			}
		}

		public string ServerUri => RefDbRepoServiceUri.Value;

		public string ClientId
		{
			get
			{
				return fClientId ?? (fClientId = new ProductRegistrationHelper().GetClientId());
			}
#if DEBUG
			set
			{
				fClientId = value;
			}
#endif
		}
		string fClientId;

		public string ClientPassword => fClientPassword ?? (fClientPassword = new ProductRegistrationHelper().GetRegistrationKeyPassword());
		string fClientPassword;

		public string SystemType => fSystemType ?? (fSystemType = new ProductRegistrationHelper().GetSystemType());
		string fSystemType;

		bool IClientConfiguration.CompressData => CompressData.Value;

		public int ClientTimeout => HttpClientTimeout.Value;
	}
}
