using System;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataRegistry.Business
{
	public sealed class ISFRegistry : RegistryItemSet
	{
		#region Construction
		public static ISFRegistry Instance
		{
			get { return instance ?? (instance = new ISFRegistry()); }
		}
		[ThreadStatic]
		static ISFRegistry instance;

		ISFRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_UnitedStatesofAmerica { get { return CombineCategories(Customs_CountryOrRegion, (NoResString)"United States of America"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import { get { return CombineCategories(Customs_UnitedStatesofAmerica, (NoResString)"Import"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import, (NoResString)"ABI"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"Importer Security Filing"); } }
			public static MultilingualString System_DataImportSettings_ImporterSecurityFiling { get { return CombineCategories(System_DataImportSettings, ISF.Business.ResString.GetMultilingualString("C9B25D6F-1201-4749-A2BE-742E3F3290AB", "Importer Security Filing")); } }
		}

		#endregion

		#region ImporterSecurityFilingNumberCustomisation

		public BillCustomisationRegistryItem ImporterSecurityFilingNumberCustomisation
		{
			get
			{
				return GetItem("ImporterSecurityFilingNumberCustomisation", delegate
				{
					return new BillCustomisationRegistryItem(
						"ImporterSecurityFilingNumberCustomisation",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("EB09B473-8DAA-4043-93A6-1FE5215DF750", "Importer Security Filing Number Customization"),
						ISF.Business.ResString.GetMultilingualString("8C758522-E757-4B95-9692-45D2FBE293E6", "Override this value to customize how Importer Security Filing numbers are formatted"),
						RegistryStorageFlags.All,
						new ISFCustomisationRegistryDataType());
				});
			}
		}

		#endregion

		public CodePairRegistryItem ImporterSecurityFilingNoOfHTSDigits
		{
			get
			{
				return GetItem("ImporterSecurityFilingNoOfHTSDigits", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"ImporterSecurityFilingNoOfHTSDigits",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("64B654ED-39EB-4C0B-BEED-1FB8C4A45323", "No. Of HTS Digits"),
						ISF.Business.ResString.GetMultilingualString("A680FE1B-400B-436C-BD70-4FCA5C218A35", "The default number of HTS digits to be reported to Customs."),
						new CodeDescriptionPairListProvider(() => new NumberOfHarmonizedDigitsList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						NumberOfHarmonizedDigitsList.Codes.Ten);
					result.Options = RegistryOptions.CannotCallParameterlessValueGetter;
					return result;
				});
			}
		}

		public BooleanRegistryItem ImporterSecurityFilingShouldMergeLine
		{
			get
			{
				return GetItem("ImporterSecurityFilingShouldMergeLine", delegate
				{
					return new BooleanRegistryItem(
						"ImporterSecurityFilingShouldMergeLine",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("C592C973-E747-4E0A-A391-B7D2F4217C1B", "Merge Lines?"),
						ISF.Business.ResString.GetMultilingualString("506B6596-0BAD-45EC-8572-1F7BC91D70FD", "If ticked, the system will merge the lines by default."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						false);
				});
			}
		}

		public BooleanRegistryItem ImporterSecurityFilingShouldReportContainerToCustoms
		{
			get
			{
				return GetItem("ImporterSecurityFilingShouldReportContainerToCustoms", delegate
				{
					return new BooleanRegistryItem(
						"ImporterSecurityFilingShouldReportContainerToCustoms",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("B530E933-B0A3-4373-A5E7-CBB79860771F", "Report Containers?"),
						ISF.Business.ResString.GetMultilingualString("6094411E-F1DE-4DA5-B5EC-B1E3FB18FB32", "If ticked, the system will report the container details to Customs by default."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						true);
				});
			}
		}

		public GuidRegistryItem ImporterSecurityFilingXMLImportNotificationGroup
		{
			get
			{
				return GetItem("ImporterSecurityFilingXMLImportNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"ImporterSecurityFilingXMLImportNotificationGroup",
						Categories.Notification,
						ISF.Business.ResString.GetMultilingualString("50051B63-07CB-4D8C-A263-C8E7FBAF8788", "ISF XML Import Notification Group"),
						ISF.Business.ResString.GetMultilingualString("665BD28B-DD59-461C-91AA-39825826AE29", "The staff group that will be notified about the Result of Importer Security Filing XML Import."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Enterprise.ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.CannotCallParameterlessValueGetter,
						Core.Constants.Groups.PostMastersGroupPK);
				});
			}
		}

		public StringRegistryItem ImporterSecurityFilingDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ImporterSecurityFilingDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
							"ImporterSecurityFilingDataImportDirectory",
							Categories.System_DataImportSettings_ImporterSecurityFiling,
							ISF.Business.ResString.GetMultilingualString("67DA51D0-56A0-4707-8F4D-AAD8C8EA5FF9", "Folder to scan for ISF XML files"),
							ISF.Business.ResString.GetMultilingualString("7A585C1E-6B9F-4971-B784-19CA73784BB2", "The folder specified here should contain any Importer Security Filing XML files that are to be automatically imported."),
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public DateTimeRegistryItem ImporterSecurityFilingMessageUsageReportDate
		{
			get
			{
				return GetItem("ImporterSecurityFilingMessageUsageReportDate", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
							"ImporterSecurityFilingMessageUsageReportDate",
							Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
							ISF.Business.ResString.GetMultilingualString("947DE69D-1731-4EC6-BA92-1FE2BF927EB2", "Next Message Usage Report Date"),
							ISF.Business.ResString.GetMultilingualString("AEDED43E-A74B-43F5-8163-DF01FEC345C6", "The date that the next Message Usage Report should be run."),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short);
					return result;
				});
			}
		}

		public GuidRegistryItem ImporterSecurityFilingMessagesGroup
		{
			get
			{
				return GetItem("ImporterSecurityFilingMessagesGroup", delegate
				{
					return new GuidRegistryItem(
						"ImporterSecurityFilingMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("A59D5855-3033-479F-AD43-FEFFF080373A", "Messaging Group"),
						ISF.Business.ResString.GetMultilingualString("03EE08B4-6D69-441C-9539-AECF303611BB", "Group to receive Importer Security Filing messaging notifications"),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						Core.Constants.Groups.PostMastersGroupPK);
				});
			}
		}

		public BooleanRegistryItem ISFUSRestrictISFJobs
		{
			get
			{
				return GetItem("ISFUSRestrictISFJobs", delegate
				{
					return new BooleanRegistryItem(
						"ISFUSRestrictISFJobs",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("839472A7-BCE8-4D55-8FAD-E7B859EFB0AE", "Restrict ISF jobs to the Home Branch"),
						ISF.Business.ResString.GetMultilingualString("10943C3A-F20A-4DEC-BBA2-4197F02F7B41", "This registry setting will restrict ISF jobs from being saved if the branch on the ISF job is in a different company than the user’s home branch company."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter,
						false);
				});
			}
		}

		public ManifestGroupNotificationRegistryItem HVLVImporterSecurityFilingMessagesGroup
		{
			get
			{
				return GetItem("HVLVImporterSecurityFilingMessagesGroup", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"HVLVImporterSecurityFilingMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
						ISF.Business.ResString.GetMultilingualString("4efca5b5-dd63-4122-a24f-8c9e4774f814", "HVLV Messaging Group"),
						ISF.Business.ResString.GetMultilingualString("655ad621-d6e6-41d4-b03e-58a67ca11160", "Group to receive Importer Security Filing messaging notifications for filings that originate from HVL shipments. If 'Send Error Only' ticked, only when an error occurs then send the message notification."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter,
						new ManifestGroupNotification(Core.Constants.EmailTo.StaffMember, Guid.Empty, true));
				});
			}
		}
	}
}
