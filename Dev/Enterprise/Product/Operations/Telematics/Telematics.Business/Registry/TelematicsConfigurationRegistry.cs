using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Telematics.Business.Registry
{
	public sealed class TelematicsConfigurationRegistry : RegistryItemSet
	{
		#region Construction

		public static TelematicsConfigurationRegistry Instance
		{
			get { return instance ?? (instance = new TelematicsConfigurationRegistry()); }
		}
		[ThreadStatic]
		static TelematicsConfigurationRegistry instance;

		TelematicsConfigurationRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public static MultilingualString Telematics_VehicleSpeedSettings { get { return CombineCategories(LocalCartageDataRegistry.Categories.Telematics, ResString.GetMultilingualString("4E18AEA9-88D3-4CD9-AFA2-BE03A3B26188", "Vehicle Speed Settings")); } }
		public static MultilingualString Telematics_DataRetentionSettings { get { return CombineCategories(LocalCartageDataRegistry.Categories.Telematics, ResString.GetMultilingualString("3a3e073f-157d-4b6f-a2a5-a5f8dce01f2d", "Data Retention Settings")); } }
		public static MultilingualString Telematics_TcaSettings { get { return CombineCategories(LocalCartageDataRegistry.Categories.Telematics, ResString.GetMultilingualString("c87fa360-2255-4c2f-b74a-0b1306bdc56c", "Transport Certification Authority Data Settings")); } }
		public static MultilingualString Telematics_TcaLoginDetails { get { return CombineCategories(Telematics_TcaSettings, ResString.GetMultilingualString("190514fe-d31d-4ac6-892c-9e7cb8ad793c", "Login Details")); } }
		public static MultilingualString Telematics_EmailNotificationGroup { get { return CombineCategories(LocalCartageDataRegistry.Categories.Telematics, ResString.GetMultilingualString("2f87edd0-b253-4eb6-a94a-fedfc60f8b3c", "Email Notification Group")); } }
		public static MultilingualString Telematics_LandParcelSettings { get { return CombineCategories(LocalCartageDataRegistry.Categories.Telematics, ResString.GetMultilingualString("85056708-bdda-4139-9922-d7def076ed1d", "Land Parcel Settings")); } }

		public OverspeedAlertRegistryItem OverspeedAlert
		{
			get
			{
				return GetItem("OverspeedAlert", () => new OverspeedAlertRegistryItem(
					nameof(OverspeedAlert),
					Telematics_VehicleSpeedSettings,
					ResString.GetMultilingualString("168e7066-8d04-4884-84ad-1361801682d4", "Overspeed Alert"),
					ResString.GetMultilingualString("060a5e2c-78c6-4560-b4b9-541f9cd04bab", "By default, the alert will be displayed for 60 minutes. Minutes can be changed to any value greater than 0. There are two other options available, never show the alert or always show the alert."),
					RegistryStorageFlags.System,
					RegistryOptions.Default
				));
			}
		}

		public IntRegistryItem OverspeedThresholdValue
		{
			get
			{
				return GetItem("OverspeedThresholdValue", () => new IntRegistryItem(
					nameof(OverspeedThresholdValue),
					Telematics_VehicleSpeedSettings,
					ResString.GetMultilingualString("38573bb3-8ff4-440e-9aec-1d8ba1a81c9d", "Threshold for Overspeeding"),
					ResString.GetMultilingualString("d1012f7a-8529-4b9d-a669-3d2e9f981b09", "If the difference between current speed and speed limit data exceeds the threshold value set in the registry, then an overspeed condition has occurred. The value will be in km/h."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					5,
					0,
					int.MaxValue));
			}
		}

		public IntRegistryItem NumberOfYearsToKeepData
		{
			get
			{
				return GetItem("NumberOfYearsToKeepData", () => new IntRegistryItem(
					nameof(NumberOfYearsToKeepData),
					Telematics_DataRetentionSettings,
					ResString.GetMultilingualString("71e7efab-f48b-44d1-9c69-b0742b7acf88", "Number of Years of Device Data to Keep"),
					ResString.GetMultilingualString("627981e4-d759-478a-94ed-a0e24e5add19", "Obsolete device data is periodically removed from the database. This setting allows you to configure the number of years of data you wish to keep in the database. " +
													"This always excludes the current calendar year i.e. if the value chosen is 1, then all data from before January 1st of the previous calendar year will be marked for deletion."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					5,
					1,
					100));
			}
		}

		public IntRegistryItem BatchSize
		{
			get
			{
				return GetItem("BatchSize", () => new IntRegistryItem(
					nameof(BatchSize),
					Telematics_DataRetentionSettings,
					ResString.GetMultilingualString("087d6cf1-2642-448b-92cd-9c58a809185a", "Batch Size for Obsolete Data Removal"),
					ResString.GetMultilingualString("66c0d9ac-84ff-4b86-a8a3-3b9cfcf6d47d", "In order to maintain efficient operation while deleting obsolete device data from the database, only a small number of items will be deleted at once. Here you can configure the number of items to be deleted each time the service task is run"),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					10000,
					100,
					100000
					));
			}
		}

		public IntRegistryItem NumberOfBatchesDeletedPerRun
		{
			get
			{
				return GetItem("NumberOfBatchesDeletedPerRun", () => new IntRegistryItem(
					nameof(NumberOfBatchesDeletedPerRun),
					Telematics_DataRetentionSettings,
					ResString.GetMultilingualString("f2e5f3bc-3768-40d7-a096-5912fba4cc88", "Number of Batches of Obsolete Data Deleted Per Service Task Run"),
					ResString.GetMultilingualString("a85e73f9-a924-4850-bb3d-52ad25fb526c", "The number of batches of obsolete data to remove each time the obsolete service task runs. Setting this value too high may result in the obsolete data removal service task taking extended periods of time to run."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					1000,
					1,
					10000
					));
			}
		}

		public IntRegistryItem RimMaximumDataBatchSize
		{
			get
			{
				return GetItem("RimMaximumDataBatchSize", () => new IntRegistryItem(
					nameof(RimMaximumDataBatchSize),
					Telematics_TcaSettings,
					ResString.GetMultilingualString("86C0E9C5-78B1-4686-9835-1F8584EA263F", "Number of RIM records provided to TCA Per batch"),
					ResString.GetMultilingualString("01A448C3-2616-4190-B392-174437DF497A", "The number of data records to send to the TCA (Transport Certification Authority) REST endpoint per batch"),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					10000,
					1,
					1000000));
			}
		}

		public StringRegistryItem TcaRimUsername
		{
			get
			{
				return GetItem(nameof(TcaRimUsername), () => new StringRegistryItem(
					nameof(TcaRimUsername),
					Telematics_TcaLoginDetails,
					ResString.GetMultilingualString("AE27763B-8B7A-457E-9302-2CE51968E3A6", "Telematics RIM Authentication Details - Username"),
					ResString.GetMultilingualString("9C1CC3DD-E182-436B-ABC2-CE6341ED43CA", "The username used for accessing the Transport Certification Authorities (TCA) Remote Infrastructure Management (RIM) REST API"),
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.TextBox),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					"tdewtg2"));
			}
		}

		public StringRegistryItem TcaRimPassword
		{
			get
			{
				return GetItem(nameof(TcaRimPassword), () => new StringRegistryItem(
					nameof(TcaRimPassword),
					Telematics_TcaLoginDetails,
					ResString.GetMultilingualString("D55385D8-EF45-4E5A-9A64-FC80758ECBD2", "Telematics RIM Authentication Details - Password"),
					ResString.GetMultilingualString("CE738CAB-B0C3-419C-A8B2-1BB002353893", "The password used for accessing the Transport Certification Authorities (TCA) Remote Infrastructure Management (RIM) REST API"),
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.Password),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					(NoResString)"5K#uR7rsT%mFV^T"));
			}
		}

		public StringRegistryItem TcaRimUrl
		{
			get
			{
				return GetItem(nameof(TcaRimUrl), () => new StringRegistryItem(
					nameof(TcaRimUrl),
					Telematics_TcaLoginDetails,
					ResString.GetMultilingualString("3A038DB4-9429-44F2-A651-22B9F809FC4C", "Telematics RIM Authentication Details - URL"),
					ResString.GetMultilingualString("FF082FA4-19F7-4DE5-8DD4-DF08F3A0EB0A", "The URL of the Transport Certification Authorities (TCA) Remote Infrastructure Management (RIM) REST API"),
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.Url),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					"https://livetde.tca.gov.au/rest/"));
			}
		}

		public StringRegistryItem XtRecipient
		{
			get
			{
				return GetItem(nameof(XtRecipient), () => new StringRegistryItem(
					nameof(XtRecipient),
					Telematics_TcaSettings,
					ResString.GetMultilingualString("B98C2BEB-2160-4131-8B4B-DE09EF715BA4", "XT Recipient for TCA Rim messages"),
					ResString.GetMultilingualString("AF2E922B-EF95-4BBF-95D8-4019F5B08752", "The identifier used to route TCA Rim messages through eHub to the XT service"),
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.TextBox),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					"XHUB_Telematics_RIM"));
			}
		}

		public GuidRegistryItem TelematicsChecklistEmailNotificationGroup
		{
			get
			{
				return GetItem("TelematicsChecklistEmailNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"TelematicsChecklistEmailNotificationGroup",
						Telematics_EmailNotificationGroup,
						ResString.GetMultilingualString("A969A20C-8AF4-4090-A152-AC45865F9FB6", "Pre-Drive Checklist Email Notification Group"),
						ResString.GetMultilingualString("E0237162-7C42-48AA-83CD-748C902AE256", "The staff group that will be receiving the exception report after a failed Pre-Drive Checklist."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Guid.Empty);
					return result;
				});
			}
		}

		public IntRegistryItem PublicPrivateRoadEndpointRecordBatchSize
		{
			get
			{
				return GetItem("PublicPrivateRoadEndpointRecordBatchSize", () => new IntRegistryItem(
					nameof(PublicPrivateRoadEndpointRecordBatchSize),
					Telematics_LandParcelSettings,
					ResString.GetMultilingualString("F6E57C58-1AFD-4866-A690-CD404C11C0BD", "Public Private Processor Batch Size"),
					ResString.GetMultilingualString("D72207C6-760E-41CB-8C8D-7F14BC5317BB", "The number of records processed per batch for public/private land status."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					1000,
					1,
					1000000));
			}
		}

		public IntRegistryItem PublicPrivateRoadEndpointTimeoutInSeconds
		{
			get
			{
				return GetItem("PublicPrivateRoadEndpointTimeoutInSeconds", () => new IntRegistryItem(
					nameof(PublicPrivateRoadEndpointTimeoutInSeconds),
					Telematics_LandParcelSettings,
					ResString.GetMultilingualString("8506B2A6-78F2-43EA-8849-D0C38E0E1ADC", "Public Private Processor Endpoint Timeout"),
					ResString.GetMultilingualString("60999169-D4F0-47C8-8044-F4FFFCB1D399", "The timeout for REST queries made to the Land Parcel endpoint."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					30,
					1,
					300
				));
			}
		}

		public BooleanRegistryItem ProcessGlobalPositionDataOnRoadType
		{
			get
			{
				return GetItem("ProcessGlobalPositionDataOnRoadType", () => new BooleanRegistryItem(
					nameof(ProcessGlobalPositionDataOnRoadType),
					Telematics_LandParcelSettings,
					ResString.GetMultilingualString("61023FA7-1A1E-4813-99A9-A9CAE48CB187", "Process Global Positioning Data On Road Type"),
					ResString.GetMultilingualString("17A4C100-E811-4100-B4A7-828EAFD9BEEB", "GPS locations will be marked to calculate if position is on public or private roads."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false
				));
			}
		}

		public TimeSpan PublicPrivateRoadEndpointTimeout => TimeSpan.FromSeconds(PublicPrivateRoadEndpointTimeoutInSeconds.Value);
	}
}
