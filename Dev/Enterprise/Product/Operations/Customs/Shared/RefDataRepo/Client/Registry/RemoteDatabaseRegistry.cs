using System;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public sealed class RemoteDatabaseRegistry : RegistryItemSet, IClientConfiguration
	{
		public static RemoteDatabaseRegistry Instance
		{
			get { return instance ?? (instance = new RemoteDatabaseRegistry()); }
		}

		[ThreadStatic]
		static RemoteDatabaseRegistry instance;

		public override bool IsForProductivityWise => true;

		public static MultilingualString System_RemoteDatabaseService { get { return CombineCategories(RawDataRegistry.Categories.System, ResString.GetMultilingualString("D8AB9941-F15F-4D53-86ED-1ECF4B148807", "Remote Database Repository")); } }

		public RemoteDbStringRegistryItem RemoteDatabaseServiceUri
		{
			get
			{
				return GetItem("RemoteDatabaseServiceUri", delegate
				{
					return new RemoteDbStringRegistryItem(
						"RemoteDatabaseServiceUri",
						System_RemoteDatabaseService,
						(NoResString)"Service Uri",
						(NoResString)@"Remote Database Service Uri
Please save the record after changing",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
						@"https://refdbrepo.wisegrid.net/"
						);
				});
			}
		}

		public DateTimeRegistryItem NextUpgrade
		{
			get
			{
				return GetItem("RemoteDatabaseServiceNextRun", delegate
				{
					var result = new DateTimeRegistryItem(
						"RemoteDatabaseServiceNextRun",
						System_RemoteDatabaseService,
						ResString.GetMultilingualString("4FE7FA49-AEB0-41EC-AAB7-5F91CB13BBA3", "Next Upgrade"),
						ResString.GetMultilingualString("98FCDB5F-7483-40A3-BAAF-EB0DA2576D26", "Next Upgrade will run no sooner than (in UTC)"),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		public BooleanRegistryItem VersionControlReset
		{
			get
			{
				return GetItem("RemoteDatabaseVersionControlReset", () =>
				{
					return new BooleanRegistryItem(
						"RemoteDatabaseVersionControlReset",
						System_RemoteDatabaseService,
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

		public BooleanRegistryItem EnableIssueReport
		{
			get
			{
				return GetItem("EnableIssueReport", () =>
				{
					return new BooleanRegistryItem(
						"EnableIssueReport",
						System_RemoteDatabaseService,
						(NoResString)"RDU/REF Issue reporting",
						(NoResString)@"By deafult RDU/REF reports issues to the Issue Manager only when URI and DB name (for RDU) is 'default' to prevent various testing cases to be escalated as issue. When default setting is overriden, all issues raised by RDU/REF will be reported to the Issue Manager.",
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

		public IntRegistryItem UpgradeInterval
		{
			get
			{
				return GetItem("RemoteDatabaseServiceUpgradeInterval", () =>
				{
					return new IntRegistryItem(
						"RemoteDatabaseServiceUpgradeInterval",
						System_RemoteDatabaseService,
						(NoResString)"Upgrade Interval",
						(NoResString)"The minimum time (in minutes) between two successful upgrades.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						60);
				});
			}
		}

		public IntRegistryItem HttpClientTimeout
		{
			get
			{
				return GetItem("RemoteDatabaseServiceHttpClientTimeout", () =>
				{
					return new IntRegistryItem(
						"RemoteDatabaseServiceHttpClientTimeout",
						System_RemoteDatabaseService,
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
				return GetItem("RemoteDatabaseServiceRefDbRepoCompressData", () => new BooleanRegistryItem(
					"RemoteDatabaseServiceRefDbRepoCompressData",
					System_RemoteDatabaseService,
					(NoResString)"Compress data",
					(NoResString)"Set this to request server to send data in compressed form.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached,
					true
				)
				{ IsExcludedFromCwOnlyNonCachedTest = true });
			}
		}

		public IntRegistryItem FailureCount
		{
			get
			{
				return GetItem("RemoteDataBaseFailureCount", () =>
				{
					return new IntRegistryItem(
						"RemoteDataBaseFailureCount",
						System_RemoteDatabaseService,
						(NoResString)"Failure Count",
						(NoResString)"Number of continuous failed attempts to get updates from the service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
						);
				});
			}
		}

		public RemoteDbStringRegistryItem SingleRefDatabaseName
		{
			get
			{
				return GetItem("SingleRefDatabaseName", delegate
				{
					return new RemoteDbStringRegistryItem(
						DbRegistry.SingleRefDatabaseName.ItemName,
						System_RemoteDatabaseService,
						(NoResString)"Single RefDb Name",
						(NoResString)@"Use another single reference database name other than Single Reference Database's default name. This unique single reference database name will be used exclusively by this CW1 instance as opposed to shared Single Reference Database. 
Every time this registry item changed, please first save the value then run Help -> Database Administration -> Recreate Database Synonyms to create synonyms correctly pointing to the new db.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
						RefDbTableNameResolver.DefaultSingleRefDbName
					)
					{
						OnAllValuesSavedAction = () => { RefDbTableNameResolver.ResetSingleRefDatabaseName(); }
					};
				});
			}
		}

		public string ServerUri => RemoteDatabaseServiceUri.Value;

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
