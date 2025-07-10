using System;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class USCustomsDataRegistry : RegistryItemSet, Integration.Customs.US.IUSCustomsDataRegistry
	{
		#region Construction
		public static USCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new USCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static USCustomsDataRegistry instance;

		USCustomsDataRegistry()
		{
		}

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Integration_UnitedStatesofAmerica { get { return CombineCategories(Customs_Integration, (NoResString)"United States of America"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica { get { return CombineCategories(Customs_CountryOrRegion, (NoResString)"United States of America"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_CBP { get { return CombineCategories(Customs_UnitedStatesofAmerica, (NoResString)"CBP"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_CBP_TestOnlySettings { get { return CombineCategories(Customs_UnitedStatesofAmerica_CBP, (NoResString)"Test Only Settings"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import { get { return CombineCategories(Customs_UnitedStatesofAmerica, (NoResString)"Import"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import, (NoResString)"ABI"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_ACE { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"ACE"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_ACE_PGA { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI_ACE, (NoResString)"PGA"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_LowValueEntries { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"Low Value Entries"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_Statement { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"Statement"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_Defaults { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"Defaults"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_InBondNumber { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"In-Bond Number"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups { get { return CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, (NoResString)"Notification Groups"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Export { get { return CombineCategories(Customs_UnitedStatesofAmerica, (NoResString)"Export"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Export_AES { get { return CombineCategories(Customs_UnitedStatesofAmerica_Export, (NoResString)"AES"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_RemoteLocationFilling => CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, US.Business.ResString.GetMultilingualString("8D7688A0-3062-4B57-BA7D-D12EEEF75DA4", "Remote Location Filling"));
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_Receiver => CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, US.Business.ResString.GetMultilingualString("9ABEC46B-8B98-4F06-982B-B69079002E23", "Receiver"));
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_Documents => CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, US.Business.ResString.GetMultilingualString("6CA1EC43-4E56-48AC-B84C-4FC5DDF33E72", "Documents"));
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_Processing => CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, US.Business.ResString.GetMultilingualString("E344BD20-7E15-4F7E-80E9-980CFA081F1C", "Processing"));
			public static MultilingualString Customs_UnitedStatesofAmerica_Import_ABI_Office => CombineCategories(Customs_UnitedStatesofAmerica_Import_ABI, US.Business.ResString.GetMultilingualString("Office", "Office"));
		}

		#endregion

		#region Intergration

		public BooleanRegistryItem SuppressAutoBillingDisbursementWhenCensusWarningExists
		{
			get
			{
				return GetItem("SuppressAutoBillingDisbursementWhenCensusWarningExists", delegate
				{
					var result = new BooleanRegistryItem(
						"SuppressAutoBillingDisbursementWhenCensusWarningExists",
						Categories.Customs_Integration_UnitedStatesofAmerica,
						(NoResString)"Suppress auto-billing Customs Disbursement when Census Warning exists?",
						(NoResString)"If it is NO, system will auto-rate Customs Disbursement even when there is a census warning.",
						RegistryStorageFlags.Company,
						true);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
					return result;
				});
			}
		}

		#endregion

		public BooleanRegistryItem SuppressStatementUpdate
		{
			get
			{
				return GetItem("SuppressStatementUpdate", delegate
				{
					return new BooleanRegistryItem(
						"SuppressStatementUpdate",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)SuppressStatementUpdateCaption,
						(NoResString)SuppressStatementUpdateDescription,
						RegistryStorageFlags.Company,
						false);
				});
			}
		}
		internal const string SuppressStatementUpdateCaption = "Suppress Statement Update if the entry is attached to a Preliminary Statement";
		internal const string SuppressStatementUpdateDescription = "By default, if an entry is on a Preliminary Statement but that statement has not yet been authorized for payment, the statement date update message will be sent when applicable. By overriding the setting below, if an entry exists on the preliminary statement, the statement update message will not be sent when otherwise applicable, regardless if the statement has been authorized for payment or not.";

		public BrokersAccountRegistryItem BrokersAccounts
		{
			get
			{
				return GetItem("ManagedAccounts", delegate
				{
					var result = new BrokersAccountRegistryItem(
						"ManagedAccounts",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)BrokerAccountsCaption,
						(NoResString)BrokerAccountsHint
						);

					return result;
				});
			}
		}
		internal const string BrokerAccountsCaption = "Broker's Bank Accounts";
		internal const string BrokerAccountsHint = "The system uses this to determine whether a statement is be paid by the broker or the Importer. If one of the debit accounts is used for ACH payment authorization, the system records the statement as 'Paid by Broker'. In the case that a credit account is used for ACH payment authorization, then the bank account matching the Client Branch Designation will be used (where the Payers Unit Number is blank).";

		public BooleanRegistryItem DoDefaultCountriesOfOriginAndExport
		{
			get
			{
				return GetItem("USDoDefaultCountriesOfOriginAndExport", delegate
				{
					return new BooleanRegistryItem(
						"USDoDefaultCountriesOfOriginAndExport",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Countries/Regions of Origin and Export?",
						(NoResString)"This registry setting controls how the country/region of origin field on the declaration defaults into invoice lines. If set to No, every line created for an invoice will default to the value on the invoice header. If set to Yes, the country/region of origin on invoice header will default based on the Manufacturer or Supplier Organization, the country/region of origin on invoice line will default based on the Manufacturer or FDA Shipper Organization. These values can be overridden at the individual line level if required.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						true);
				});
			}
		}

		public BooleanRegistryItem CreateMIDOrganizationOnUnmatchedImport
		{
			get
			{
				return GetItem("CreateMIDOrganizationOnUnmatchedImport", () =>
				{
					return new BooleanRegistryItem(
						"CreateMIDOrganizationOnUnmatchedImport",
						Categories.Customs_UnitedStatesofAmerica_Import,
						(NoResString)"Create MID Organization on unmatched import",
						(NoResString)"When this item is enabled, during Native XML import of a Product or when using the Import Data function in a Customs Declaration, a new Manufacturer Identification (MID) Organization will automatically be created when the import contains a MID that does not match an existing Organization. Once created a Query Manufacturer message will be sent to US Customs and upon successful receipt of matching address details the newly created Organization record updated.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#region Statement

		public BooleanRegistryItem StatementProcessingPortEqualPortofEntryNonRLF
		{
			get
			{
				return GetItem("USStatementProcessingPortEqualPortofEntry", delegate
				{
					return new BooleanRegistryItem(
						"USStatementProcessingPortEqualPortofEntry",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)"Statement Processing Port equal to Port of Entry for non-RLF?",
						(NoResString)"If this is 'yes', then statement processing port (in the B record) will be set to the same value as the Port of Entry for non-RLF entries. If this is 'no' then the Statement Processing Port will be set to Processing->District Port as set up in the registry.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						false);
				});
			}
		}

		public CodePairRegistryItem MonthlyStatementPaymentPosting
		{
			get
			{
				return GetItem("MonthlyStatementPaymentPosting", delegate
				{
					var result = new CodePairRegistryItem(
						"MonthlyStatementPaymentPosting",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)MonthlyStatementPaymentPostingCaption,
						(NoResString)MonthlyStatementPaymentPostingHint,
						new CodeDescriptionPairListProvider(() => new MonthlyStatementPaymentPostingOptionList()),
						RegistryStorageFlags.Company,
						MonthlyStatementPaymentPostingOptionList.Codes._1);
					result.Options = RegistryOptions.CannotCallParameterlessValueGetter;
					return result;
				});
			}
		}
		internal const string MonthlyStatementPaymentPostingCaption = "Monthly Statement Payment Posting";
		internal const string MonthlyStatementPaymentPostingHint = "How to post Monthly Statement Payment - one payment per periodic daily statement or payment per monthly statement.";

		#endregion

		public BooleanRegistryItem DefaultDestState
		{
			get
			{
				return GetItem("DefaultDestState", delegate
				{
					return new BooleanRegistryItem(
						"DefaultDestState",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Destination State?",
						(NoResString)"Should the Destination State be defaulted from the state of the Ultimate Consignee?",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem DisableAbnormalityMessageReporting
		{
			get
			{
				return GetItem("USDisableAbnormalityMessageReporting", delegate
				{
					return new BooleanRegistryItem(
						"USDisableAbnormalityMessageReporting",
						Categories.Customs_UnitedStatesofAmerica,
						(NoResString)"Disable Abnormal Reporting?",
						(NoResString)"Should reporting of abnormal messaging be disabled?",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem UseViewForDrawbackEntryLine
		{
			get
			{
				return GetItem("USUseViewForDrawbackEntryLine", delegate
				{
					return new BooleanRegistryItem(
						"USUseViewForDrawbackEntryLine",
						Categories.Customs_UnitedStatesofAmerica,
						(NoResString)"Use View for Drawback Entry Line?",
						(NoResString)"Use View Data for Drawback Entry Line instead of Declaration business objects",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		public CodePairRegistryItem SeverityLevelForMissingUSMCACertificate
		{
			get
			{
				return GetItem("SeverityLevelForMissingUSMCACertificate", delegate
				{
					return new CodePairRegistryItem(
						"SeverityLevelForMissingUSMCACertificate",
						Categories.Customs_UnitedStatesofAmerica,
						(NoResString)"Severity Level for Missing USMCA Certificate",
						(NoResString)"Set the severity level for validating related to the use of claiming USMCA on a line. If validation is activated, the system will look for a valid Certificate of Origin. The COO may exist on the Product or on the Importer Organization.",
						new CodeDescriptionPairListProvider(() => new ProductAuditActions()),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ProductAuditActions.Codes.NoAction);
				});
			}
		}

		public EntryProcessingPortsMappingRegistryItem EntryProcessingPortMappings
		{
			get
			{
				return GetItem("EntryProcessingPortMappings", delegate
				{
					return new EntryProcessingPortsMappingRegistryItem(
						"EntryProcessingPortMappings",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)"Non-RLF Entry Port/Processing Port Mapping",
						(NoResString)"The Processing Port for a particular Entry Port can be set up here. This mapping applies to non-RLF jobs.",
						RegistryStorageFlags.Company);
				});
			}
		}

		public BooleanRegistryItem RequireApprovalPriorAuthorizingStatement
		{
			get
			{
				return GetItem("RequireApprovalPriorAuthorizingStatement", delegate
				{
					var result = new BooleanRegistryItem(
						"RequireApprovalPriorAuthorizingStatement",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)RequireApprovalPriorAuthorizingStatementCaption,
						(NoResString)RequireApprovalPriorAuthorizingStatementHint,
						RegistryStorageFlags.Company,
						false);

					return result;
				});
			}
		}
		internal const string RequireApprovalPriorAuthorizingStatementCaption = "Require Statement Authorization Approval?";
		internal const string RequireApprovalPriorAuthorizingStatementHint = "If this setting is 'Yes', then permission to send the payment authorization message will need to be granted by a user, who has the appropriate rights, prior to the sending of this authorization message.";

		public BorderCargoPortRegistryItem BorderCargoReleasePorts
		{
			get
			{
				return GetItem("BorderCargoReleasePorts", delegate
				{
					var result = new BorderCargoPortRegistryItem(
						"BorderCargoReleasePorts",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)BorderCargoReleasePortsCaption,
						(NoResString)BorderCargoReleasePortsHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

					return result;
				});
			}
		}
		internal const string BorderCargoReleasePortsCaption = "Border Cargo Release Ports";
		internal const string BorderCargoReleasePortsHint = "List of land border ports that is used to set default values when MOT is Truck.";

		public BooleanRegistryItem AutoCreateDeclarationFromLineRelease
		{
			get
			{
				return GetItem("AutoCreateDeclarationFromLineRelease", delegate
				{
					return new BooleanRegistryItem(
						"AutoCreateDeclarationFromLineRelease",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)AutoCreateDeclarationFromLineReleaseCaption,
						(NoResString)AutoCreateDeclarationFromLineReleaseHint,
						RegistryStorageFlags.Company,
						false);
				});
			}
		}
		internal const string AutoCreateDeclarationFromLineReleaseCaption = "Auto Create Declaration From Line Release";
		internal const string AutoCreateDeclarationFromLineReleaseHint = "If turned on, a new declaration will be created when a Line Release message is received.";

		public CodePairRegistryItem SPIValidation
		{
			get
			{
				return GetItem("SPIValidation", delegate
				{
					return new CodePairRegistryItem(
						"SPIValidation",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						US.Business.ResString.GetMultilingualString("EB412B9C-97D6-4BA8-8C65-D8A651E0AB9B", "SPI Validation"),
						US.Business.ResString.GetMultilingualString("489252AB-AC01-41ED-9BD6-2B621900E0FE", "If a tariff and country of origin is potentially eligible for a Free Trade Agreement, indicated by an SPI, the system will warn the user when no SPI is entered."),
						new CodeDescriptionPairListProvider(() => new SPIValidationTypeList()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						SPIValidationTypeList.Codes.MER);
				});
			}
		}

		public BranchDistrictPortRegistryItem BranchDistrictPortRelationship
		{
			get
			{
				return GetItem("BranchDistrictPortRelationship", delegate
				{
					var result = new BranchDistrictPortRegistryItem(
						"BranchDistrictPortRelationship",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)BranchPortsCaption,
						(NoResString)BranchPortsHint,
						RegistryStorageFlags.Company);

					return result;
				});
			}
		}
		internal const string BranchPortsCaption = "Branch to District/Port Relationship";
		internal const string BranchPortsHint = "Relationship between Branch and Districts and Ports.";

		public GuidRegistryItem BranchFDAContact
		{
			get
			{
				return GetItem("BranchFDAContact", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BranchFDAContact",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Branch PGA Contact",
						(NoResString)"The contact name and phone number of the selected staff member will default to the declaration when Partner Government Agency reporting is required. \r\n\r\nIf the staff member does not have a work phone number entered, the branch phone number of the home branch of that staff member will be defaulted to the declaration.",
						RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		public GuidRegistryItem CargoReleaseFTZContact
		{
			get
			{
				return GetItem("CargoReleaseFTZContact", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CargoReleaseFTZContact",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Branch Cargo Release/FTZ Contact",
						(NoResString)"The contact name and phone number of the selected staff member will default to the cargo release or FTZ when sending cargo release or FTZ messages. \r\n\r\nIf the staff member does not have a work phone number entered, the branch phone number of the home branch of that staff member will be defaulted to the cargo release or FTZ.",
						RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		public BooleanRegistryItem DefaultManufacturerFromSupplier
		{
			get
			{
				return GetItem("DefaultManufacturerFromSupplier", delegate
				{
					return new BooleanRegistryItem(
						"DefaultManufacturerFromSupplier",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Manufacturer from Supplier?",
						(NoResString)"Should the Manufacturer be automatically set to the Supplier?",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem DefaultSellerFromSupplier
		{
			get
			{
				return GetItem("DefaultSellerFromSupplier", delegate
				{
					return new BooleanRegistryItem(
						"DefaultSellerFromSupplier",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Seller from Supplier?",
						(NoResString)"Should the Seller be automatically set to the Supplier?",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem DefaultOwnerRefOn7501
		{
			get
			{
				return GetItem("DefaultOwnerRefOn7501", delegate
				{
					return new BooleanRegistryItem(
						"DefaultOwnerRefOn7501",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						US.Business.ResString.GetMultilingualString("5f30847e-5894-4c4b-8037-f1e1fae97db2", "Default Owner reference in 7501 Box 43?"),
						US.Business.ResString.GetMultilingualString("cb549e3b-cc2a-4d8d-8ac6-f09dd3834429", "Should the Owner Reference print in Box 43 on the 7501 Entry Summary?\r\n\r\nThe owner reference prints with the {0} job number in Box 43 on the 7501 Entry Summary document.\r\n\r\nFor example: B00001042 / Ref: Order 7395-A", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem DoDefaultSoldToParty
		{
			get
			{
				return GetItem("DoDefaultSoldToParty", delegate
				{
					var result = new BooleanRegistryItem(
						"DoDefaultSoldToParty",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Sold To Party?",
						(NoResString)"Set to Yes to default the Sold To Party organization from the Importer at declaration level for ACE declarations.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);

					return result;
				});
			}
		}

		public BooleanRegistryItem DoDefaultShipTo
		{
			get
			{
				return GetItem("DoDefaultShipTo", delegate
				{
					var result = new BooleanRegistryItem(
						"DoDefaultShipTo",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Ship To?",
						(NoResString)"Set to Yes to default the Ship To Party from the Consignee",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);

					return result;
				});
			}
		}

		#region AES

		public ZString AESSendNotifications
		{
			get { return (ZString)AESSendNotificationsItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { AESSendNotificationsItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public CodePairRegistryItem AESSendNotificationsItem
		{
			get
			{
				return GetItem("AESSendNotifications", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"AESSendNotifications",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Send AES Notifications",
						(NoResString)"Enter the mode to send AES notifications",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company,
						Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
					return result;
				});
			}
		}

		public Guid AESSendNotificationsToGroup
		{
			get { return AESSendNotificationsToGroupItem.Value; }
#if DEBUG
			set { AESSendNotificationsToGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public GuidRegistryItem AESSendNotificationsToGroupItem
		{
			get
			{
				return GetItem("AESSendNotificationsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"AESSendNotificationsToGroup",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Group To Send AES Notifications To",
						(NoResString)"The staff group that will be receiving notifications about AES Responses.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK);
					result.DataType = new NotificationGroupGuidRegistryDataType();
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportDefaultTariffType
		{
			get
			{
				return GetItem("ExportDefaultTariffType", delegate
				{
					return new CodePairRegistryItem(
						"ExportDefaultTariffType",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Default Tariff Type",
						(NoResString)"The default Tariff Type selected here (Schedule B or HTS) determines which Tariff type is defaulted to new Shippers Export Declarations.",
						new CodeDescriptionPairListProvider(() => new TariffTypeList()),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						TariffTypeList.Codes.ScheduleB);
				});
			}
		}

		public BooleanRegistryItem AllowExportDefaultOriginIndicatorAtInvoice
		{
			get
			{
				return GetItem("AllowExportDefaultOriginIndicatorAtInvoice", delegate
				{
					return new BooleanRegistryItem(
						"AllowExportDefaultOriginIndicatorAtInvoice",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Allow Origin Indicator to be entered at Invoice Header level",
						(NoResString)"If this is no, each invoice line will require Origin Indicator to be entered.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem DefaultNLRLicenseType
		{
			get
			{
				return GetItem("DefaultNLRLicenseType", delegate
				{
					return new BooleanRegistryItem(
						"DefaultNLRLicenseType",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Default NLR License Type",
						(NoResString)"If set to Yes, the License Type on a US Export Declaration will default to C33/NLR.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		public StringRegistryItem DestinationControlStatement
		{
			get
			{
				return GetItem("DestinationControlStatement", delegate
				{
					return new StringRegistryItem("DestinationControlStatement",
						Categories.Customs_UnitedStatesofAmerica_Export,
						(NoResString)"Destination Control Statement",
						(NoResString)"This Destination Control Statement will display on the Commercial Invoice for goods controlled by EAR (Export Administration Regulation) or ITAR (International Traffic in Arms) regulation for exports from U.S. when the Export Declaration Inv. Lines meet certain conditions.",
						new StringRegistryDataType(CharacterCase.Normal),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"These items are controlled by the U.S. Government and authorized for export only to the country of ultimate destination for use by the ultimate consignee or end-user(s) herein identified.  They may not be resold, transferred, or otherwise disposed of, to any other country or to any person other than the authorized ultimate consignee or end-user(s), either in their original form or after being incorporated into other items, without first obtaining approval from the U.S. government or as otherwise authorized by U.S. law and regulations."
					);
				});
			}
		}

		public BooleanRegistryItem EnableExportDeclarationDataUpdaterServiceTask
		{
			get
			{
				return GetItem("EnableExportDeclarationDataUpdaterServiceTask", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableExportDeclarationDataUpdaterServiceTask",
						Categories.Customs_UnitedStatesofAmerica_Export,
						(NoResString)"Enable EDU Service Task",
						(NoResString)"If enabled, EDU (Export Declaration Data Updater) service task will run. To cause immediate effect, please restart the Process Controller in Service Task.",
						RegistryStorageFlags.System,
						false);

					return result;
				});
			}
		}

		#region Sending To Customs Settings
#if DEBUG
		public GuidRegistryItem TestCaseWithSetupImporterPK
		{
			get
			{
				return GetItem("TestCaseWithSetupImporterPK", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"TestCaseWithSetupImporterPK",
						Categories.Customs_UnitedStatesofAmerica_CBP_TestOnlySettings,
						(NoResString)"Importer",
						(NoResString)"The importer to use in sending test messages to customs like the 100 point test.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		public GuidRegistryItem TestCaseWithSetupShippingLinePK
		{
			get
			{
				return GetItem("TestCaseWithSetupShippingLinePK", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"TestCaseWithSetupShippingLinePK",
						Categories.Customs_UnitedStatesofAmerica_CBP_TestOnlySettings,
						(NoResString)"Shipping Line",
						(NoResString)"The shipping line to use in sending test messages to customs like the 100 point test.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		public GuidRegistryItem TestCaseWithSetupManufacturerPK
		{
			get
			{
				return GetItem("TestCaseWithSetupManufacturerPK", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"TestCaseWithSetupManufacturerPK",
						Categories.Customs_UnitedStatesofAmerica_CBP_TestOnlySettings,
						(NoResString)"Manufacturer",
						(NoResString)"The manufacturer to use in sending test messages to customs like the 100 point test.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

#endif
		#endregion

		public StringRegistryItem CustomsDeliveryOrderDisclaimer
		{
			get
			{
				return GetItem("CustomsDeliveryOrderDisclaimer", delegate
				{
					return new StringRegistryItem("CustomsDeliveryOrderDisclaimer",
						Categories.Customs_UnitedStatesofAmerica_Import,
						(NoResString)"Delivery Order Disclaimer",
						(NoResString)"The disclaimer which will be printed at the bottom of the Delivery Order.",
						new StringRegistryDataType(CharacterCase.Upper),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"CARRIER MUST PRESENT DELIVERY ORDER TO TERMINAL AT TIME OF PICK UP. FAILURE OF CARRIER TO NOTIFY OUR OFFICE OF CARGO BEING DETAINED AT TERMINAL WILL RESULT IN CARRIER BEING RESPONSIBLE FOR ALL DEMURRAGE CHARGES BEING INCURRED."
					);
				});
			}
		}

		public BooleanRegistryItem EnableETAValidation
		{
			get
			{
				return GetItem("EnableETAValidation", delegate
				{
					return new BooleanRegistryItem(
						"EnableETAValidation",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"Enable ETA Validation",
						(NoResString)"If turned on, system will add a warning to the ETA field when no data is entered.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public CodePairRegistryItem DefTaxDueDateCalculationOption
		{
			get
			{
				return GetItem("DefTaxDueDateCalculationOption", delegate
				{
					var result = new CodePairRegistryItem(
						"DefTaxDueDateCalculationOption",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)DeferredTaxDueDateCalculationCaption,
						(NoResString)DeferredTaxDueDateCalculationHint,
						new CodeDescriptionPairListProvider(() => new DefTaxDueDateCalculationOptionList()),
						RegistryStorageFlags.Company,
						DefTaxDueDateCalculationOptionList.Codes.COL);
					result.Options = RegistryOptions.CannotCallParameterlessValueGetter;
					return result;
				});
			}
		}
		internal const string DeferredTaxDueDateCalculationCaption = "How to calculate Deferred Tax Due Date";
		internal const string DeferredTaxDueDateCalculationHint = "CBP uses the Collection Date to calculate the Deferred Tax Due Date of an entry. Checking the override box here and selecting Release Date ('REL') will cause the system to calculate the Deferred Tax Due Date based on the Release Date of all non-warehouse entries, instead of the Collection Date.";

		public CodePairRegistryItem InformalImportWarningOrError
		{
			get
			{
				return GetItem("InformalImportWarningOrError", delegate
				{
					var result = new CodePairRegistryItem(
						"InformalImportWarningOrError",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE,
						(NoResString)InformalImportWarningOrErrorCaption,
						(NoResString)InformalImportWarningOrErrorHint,
						new CodeDescriptionPairListProvider(() => new ErrorWarningInformalImport()),
						(RegistryStorageFlags.System | RegistryStorageFlags.Company),
						ErrorWarningInformalImport.Codes.WAR);
					result.Options = RegistryOptions.CannotCallParameterlessValueGetter;
					return result;
				});
			}
		}
		internal const string InformalImportWarningOrErrorCaption = "Notification Type for Eligible Informal Entry";
		internal const string InformalImportWarningOrErrorHint = "Show warning or error message when an entry is eligible to file as an informal entry";

		#region ACE Items

		public BooleanRegistryItem IseBondAutoSendACEMessage
		{
			get
			{
				return GetItem("IseBondAutoSendACEMessage", delegate
				{
					return new BooleanRegistryItem(
						"IseBondAutoSendACEMessage",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE,
						(NoResString)eBondAutoSendACEMessageCaption,
						(NoResString)eBondAutoSendACEMessageHint,
						RegistryStorageFlags.Company,
						false);
				});
			}
		}
		internal const string eBondAutoSendACEMessageCaption = "Send ACE Entry Summary/Cargo Release automatically";
		internal const string eBondAutoSendACEMessageHint = "Send ACE Entry Summary/Cargo Release automatically when eBond Status Notification indicates a bond has added.";

		public GuidRegistryItem BondStatusNotificationGroup
		{
			get
			{
				return GetItem("BondStatusNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"BondStatusNotificationGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)BondStatusNotificationGroupCaption,
						(NoResString)BondStatusNotificationGroupHint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.Options = RegistryOptions.IsValueOptional;
					return result;
				});
			}
		}
		internal const string BondStatusNotificationGroupCaption = "Bond Status Notification Group";
		internal const string BondStatusNotificationGroupHint = "The staff group that will be receiving bond status notifications for which system could not match to any jobs, if the value is blank, system don't send the notifications.";

		public BooleanRegistryItem RequestForBillAndEntryData
		{
			get
			{
				return GetItem("RequestForBillAndEntryData", delegate
				{
					return new BooleanRegistryItem(
						"RequestForBillAndEntryData",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE,
						(NoResString)RequestForBillAndEntryDataCaption,
						(NoResString)RequestForBillAndEntryDataHint,
						RegistryStorageFlags.Company,
						false);
				});
			}
		}
		internal const string RequestForBillAndEntryDataCaption = "Request For Bill And Entry Details";
		internal const string RequestForBillAndEntryDataHint = "If this registry setting is set to Yes, system will make a request on bill of lading and in-bond status when entry status is queried and Customs will return status for any master bills, house bills and/or in-bond numbers found in an entry.";

		public BooleanRegistryItem UpdateDeclarationWithCargoReleaseResults
		{
			get
			{
				return GetItem("UpdateDeclarationWithCargoReleaseResults", delegate
				{
					return new BooleanRegistryItem(
						"UpdateDeclarationWithCargoReleaseResults",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"Update Declaration with Cargo Release Results",
						(NoResString)"If set to Yes, the system will update the Port of Entry, Date of Arrival and Manifest details like quantity, carrier, voyage or flights based on the ACE Cargo Release results if no entry summary is on file at CBP yet and cargo is indicated as released in the ACE Cargo Release message.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false
							);
				});
			}
		}

		public BooleanRegistryItem EnableTwoStepsProcess
		{
			get
			{
				return GetItem("EnableTwoStepsProcess", delegate
				{
					return new BooleanRegistryItem(
						"EnableTwoStepsProcess",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE,
						(NoResString)"Enable Two-Step Process",
						(NoResString)"If set to yes, system enables entry summary when users enable cargo release or vice versa. This is so that entry summary data is required and validated at the time of sending cargo release.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false
						);
				});
			}
		}
		#endregion

		public BooleanRegistryItem EnableDocAddressForFDA
		{
			get
			{
				return GetItem("EnableDocAddressForFDA", delegate
				{
					return new BooleanRegistryItem(
						"EnableDocAddressForFDA",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE_PGA,
						(NoResString)EnableDocAddressForFDACaption,
						(NoResString)EnableDocAddressForFDACaptionHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		internal const string EnableDocAddressForFDACaption = "Enable DocAddress for FDA";
		internal const string EnableDocAddressForFDACaptionHint = "If ticked, the DocAddress will be used instead of Organizations in FDA program.";

		public BooleanRegistryItem EnableFDAForLowValueEntries
		{
			get
			{
				return GetItem("EnableFDAForLowValueEntries", delegate
				{
					return new BooleanRegistryItem(
						"EnableFDAForLowValueEntries",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_LowValueEntries,
						(NoResString)EnableFDAForLowValueEntriesCaption,
						(NoResString)EnableFDAForLowValueEntriesCaptionHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		internal const string EnableFDAForLowValueEntriesCaption = "Enable FDA for Low Value Entries";
		internal const string EnableFDAForLowValueEntriesCaptionHint = "If ticked, FDA will be able to be declared in Low Value Entries.";

		public DateTimeRegistryItem NextInBondNumberLimitWarningReportRun
		{
			get
			{
				return GetItem("NextInBondNumberLimitWarningReportRun", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
						"NextInBondNumberLimitWarningReportRun",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_InBondNumber,
						(NoResString)"Next In-Bond Number Limit Warning Report Run",
						(NoResString)"The time the system should next run a In-Bond Number Warning Limit Report.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.IsOnlyForSupport);
					result.EditorInfo = new DateTimeRegistryEditorInfo(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		public BooleanRegistryItem AnInBondCommodityMustHaveAValidProduct
		{
			get
			{
				return GetItem("USAnInBondCommodityMustHaveAValidProduct", delegate
				{
					return new BooleanRegistryItem(
						"USAnInBondCommodityMustHaveAValidProduct",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"In-Bond Commodity Product",
						(NoResString)"Ensure that all Bonded Warehousing commodities on an In-Bond job has a valid product.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public DateTimeRegistryItem RefFileRequestExchangeRateTimeStamp
		{
			get
			{
				return GetItem("RefFileRequestExchangeRateTimeStamp", delegate
				{
					return new DateTimeRegistryItem("RefFileRequestExchangeRateTimeStamp",
						Categories.Customs_UnitedStatesofAmerica_Import,
						(NoResString)"RefFileRequestExchangeRateTimeStamp",
						(NoResString)"RefFileRequestExchangeRateTimeStamp",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden);
				});
			}
		}

		internal const string TurnedOffRequestExchangeRateHint = "The date when CBP stops responding to the exchange rate query messages";

		public BooleanRegistryItem AutoQueryEntrySummaries
		{
			get
			{
				return GetItem("AutoQueryEntrySummaries", delegate
				{
					return new BooleanRegistryItem(
						"AutoQueryEntrySummaries",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)AutoQueryEntrySummariesCaption,
						(NoResString)AutoQueryEntrySummariesHint,
						RegistryStorageFlags.Company,
						false);
				});
			}
		}
		internal const string AutoQueryEntrySummariesCaption = "Auto Query Entry Summaries";
		internal const string AutoQueryEntrySummariesHint = "The automatic Entry Summary query is used to obtain the Collection Date and Anticipated Liquidation Date of each entry summary. By default this function is NOT activated. Override this default here to activate the automatic query.";

		public AutoQueryBillOfLadingCargoManifestStatusItem AutoQueryBillOfLadingCargoManifestStatus
		{
			get
			{
				return GetItem("AutoQueryBillOfLadingCargoManifestStatus", delegate
				{
					return new AutoQueryBillOfLadingCargoManifestStatusItem(
						"AutoQueryBillOfLadingCargoManifestStatus",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)AutoQueryBillOfLadingCargoManifestStatusCaption,
						(NoResString)AutoQueryBillOfLadingCargoManifestStatusHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
				});
			}
		}
		internal const string AutoQueryBillOfLadingCargoManifestStatusCaption = "Auto Query Bill of Lading Cargo Manifest Status";
		internal const string AutoQueryBillOfLadingCargoManifestStatusHint = "The Auto Query Bill of Lading Manifest Status is used to query the bill of lading status of each bill on a declaration. The default setting is set to No. If you override the registry, you can select one or both options listed. The Send on ETA option will send the cargo manifest query prior to the ETA of the shipment.  The Send on First Save option will send the query immediately after the declaration is first saved. This setting also has an additional option to update the entry with the query results, which will update manifest quantity, ETA date, etc. based on the query results.";

		public BooleanRegistryItem ExportDefaultRoutedTransaction
		{
			get
			{
				return GetItem("ExportDefaultRoutedTransaction", delegate
				{
					return new BooleanRegistryItem(
						"ExportDefaultRoutedTransaction",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Default Routed Transaction",
						(NoResString)"If this is yes, the Routed Transaction is defaulted with N to new Shippers Export Declarations.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration
		{
			get
			{
				return GetItem("EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration", delegate
				{
					return new BooleanRegistryItem(
						"EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration",
						Categories.Customs_UnitedStatesofAmerica,
						(NoResString)EnableNCTAsDefaultContainerModeForAirOrTruckDeclarationCaption,
						(NoResString)EnableNCTAsDefaultContainerModeForAirOrTruckDeclarationHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		internal const string EnableNCTAsDefaultContainerModeForAirOrTruckDeclarationCaption = "Enable NCT as default container mode for air or truck declaration";
		internal const string EnableNCTAsDefaultContainerModeForAirOrTruckDeclarationHint = "This enables NCT to be defaulted to the container mode for stand-alone air or truck declarations.";

		public BooleanRegistryItem EnableExportManifest
		{
			get
			{
				return GetItem("EnableExportManifest", delegate
				{
					return new BooleanRegistryItem(
						"EnableExportManifest",
						Categories.Customs_UnitedStatesofAmerica,
						(NoResString)"Enable Export Manifest",
						(NoResString)"Enable Export Manifest",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public const int DefaultInBondNumberWarningLimit = 300;

		public InBondNumberRangeRegistryItem CompanyOrBranchInBondNumberRange
		{
			get
			{
				return GetItem("CompanyOrBranchInBondNumberRange", delegate
				{
					return new InBondNumberRangeRegistryItem("CompanyOrBranchInBondNumberRange",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_InBondNumber,
						(NoResString)"Number Range",
						(NoResString)"Enter the range of In-Bond Numbers for this Company or for individual Branches.\r\nThese numbers are assigned by CBP and are eight digit numbers without a check digit.\r\ne.g. 10000000-19999999. Be sure to NOT enter a check digit below.\r\n\r\n\r\nEnter also the limit of available In-Bond numbers remaining in the range established for this Company or Branch before users are warned;\r\nonce reached, the system will start warning users of the potential for running out of In-Bond numbers.", new InBondNumberRange());
				});
			}
		}

		public BooleanRegistryItem SendAllITNumbersInBOLMessage
		{
			get
			{
				return GetItem("SendAllITNumbersInBOLMessage", delegate
				{
					return new BooleanRegistryItem(
						"SendAllITNumbersInBOLMessage",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"Send All IT Numbers Format in BOL",
						(NoResString)"Only 'V' IT Numbers will be send in Bill Of Lading Update message by default. If 'Yes', all types of IT Numbers will be sent in Bill Of Lading Update message.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public CodePairRegistryItem TransportModeForInBondCreationFromConsol
		{
			get
			{
				return GetItem("TransportModeForInBondCreationFromConsol", delegate
				{
					return new CodePairRegistryItem(
						"TransportModeForInBondCreationFromConsol",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)TransportModeForInBondCreationFromConsolCaption,
						(NoResString)TransportModeForInBondCreationFromConsolHint,
						new CodeDescriptionPairListProvider(() => new TransportModeForInBondCreationFromConsolList()),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						TransportModeForInBondCreationFromConsolList.Codes.AIR);
				});
			}
		}
		internal const string TransportModeForInBondCreationFromConsolCaption = "Permit transport mode for in-bond creation from consol";
		internal const string TransportModeForInBondCreationFromConsolHint = "The field below is utilized to verify the eligibility of the consolidation for creating in-bond jobs. If the provided value corresponds to the transport mode specified in the consolidation, the user will have the ability to create in-bond movements on the consolidation.";

		IRegistryItem Integration.Customs.US.IUSCustomsDataRegistry.TransportModeForInBondCreationFromConsol => TransportModeForInBondCreationFromConsol;

		public StringRegistryItem PreparerDistrictPort
		{
			get
			{
				return GetItem("PreparerDistrictPort", delegate
				{
					return new StringRegistryItem(
						PreparerDistrictPortName,
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_RemoteLocationFilling,
						(NoResString)PreparerDistrictPortCaption,
						(NoResString)PreparerDistrictPortHint,
						new StringRegistryDataType(4, 4),
						RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter);
				});
			}
		}
		internal const string PreparerDistrictPortName = "PreparerDistrictPort";
		internal const string PreparerDistrictPortCaption = "Preparer District Port";
		internal const string PreparerDistrictPortHint = "Please enter the Preparer District Port. Preparer District Port is used as defaults for 'Declaration->Misc->Preparer District Port' when 'RLF' (Remote Location Filling) is enabled";

		public StringRegistryItem PreparerOfficeCode
		{
			get
			{
				return GetItem("PreparerOfficeCode", delegate
				{
					return new StringRegistryItem(
						PreparerOfficeCodeName,
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_RemoteLocationFilling,
						(NoResString)PreparerOfficeCodeCaption,
						(NoResString)PreparerOfficeCodeHint,
						RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter);
				});
			}
		}
		internal const string PreparerOfficeCodeName = "PreparerOfficeCode";
		internal const string PreparerOfficeCodeCaption = "Preparer Office Code";
		internal const string PreparerOfficeCodeHint = "Please enter the Preparer Office Code. Preparer Office Code is used as defaults for 'Declaration->Misc->Preparer Office Code' when 'RLF' (Remote Location Filling) is enabled";

		#region Receiver

		public StringRegistryItem ReceiverDistrictPort
		{
			get
			{
				return GetItem("ReceiverDistrictPort", delegate
				{
					return new StringRegistryItem(
						"ReceiverDistrictPort",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Receiver,
						(NoResString)"District Port",
						(NoResString)"What is the receiver district port?",
						new StringRegistryDataType(CharacterCase.Upper, 4, 4),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue);
				});
			}
		}

		public BooleanRegistryItem NationalImporterLiquidationIndicator
		{
			get
			{
				return GetItem("CourtesyLiquidationNotice", delegate
				{
					return new BooleanRegistryItem(
						"CourtesyLiquidationNotice",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Receiver,
						(NoResString)"Courtesy Liquidation Notice",
						(NoResString)"Are you eligible to receive liquidation notices?",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		public EntryFilerRegistryItem EntryFiler
		{
			get
			{
				return GetItem("EntryFiler", delegate
				{
					return new EntryFilerRegistryItem(
						"EntryFiler",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						US.Business.ResString.GetMultilingualString("B09700B5-9094-4E9B-9FCD-AA19E243C3BE", "Entry Filer"),
						US.Business.ResString.GetMultilingualString("56263A0C-4F15-45A1-872B-5FFBF2BCF118", "Entry Filer Information: A unique code assigned by CBP to all active entry document preparers, ABI Certified."));
				});
			}
		}

		public StringRegistryItem ProcessingDistrictPortCode
		{
			get
			{
				return GetItem("ProcessingDistrictPortCode", delegate
				{
					return new StringRegistryItem(
						"ProcessingDistrictPortCode",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Processing,
						US.Business.ResString.GetMultilingualString("FB7AA64F-4F24-43F7-97D8-922278271229", "District Port"),
						US.Business.ResString.GetMultilingualString("3A1785B2-1C87-4903-BA20-4424D39042FB", "What is the Processing District Port Code that will be reported to Customs for ABI and ISF messages?"),
						new StringRegistryDataType(CharacterCase.Upper, 4, 4),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue,
						"");
				});
			}
		}

		public StringRegistryItem ARecordOfficeCode
		{
			get
			{
				return GetItem("OfficeCode", delegate
				{
					return new StringRegistryItem(
						"OfficeCode",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Office,
						(NoResString)"A-Record",
						(NoResString)"The Office Code which will be used in the A-Record when sending messages to CBP.",
						new StringRegistryDataType(CharacterCase.Upper, 0, 2),
						RegistryStorageFlags.Company);
				});
			}
		}

		public StringRegistryItem BRecordOfficeCode
		{
			get
			{
				return GetItem("BRecordOfficeCode", delegate
				{
					return new StringRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("BRecordOfficeCode",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Office,
						(NoResString)"B-Record",
						(NoResString)"The Office Code which will be used in the B-Record when sending messages to CBP.  If it is not specified, then the A-Record Office Code will be used.",
						new StringRegistryDataType(CharacterCase.Upper, 0, 2),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						(Guid companyPK, Guid branchPK, Guid departmentPK) => ARecordOfficeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty))
					);
				});
			}
		}

		public ExportEntryFilerIDRegistryItem ExportEntryFilerID
		{
			get
			{
				var exportEntryFilerID = GetItem("ExportEntryFilerID", delegate
				{
					return new ExportEntryFilerIDRegistryItem(
						"ExportEntryFilerID",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)ExportEntryFilerIDCaption,
						(NoResString)ExportEntryFilerIDHint);
				});
				exportEntryFilerID.CountryFilterPKs = Core.CountryGuids.AllUSCountriesForEntryFilerID;
				return exportEntryFilerID;
			}
		}
		internal const string ExportEntryFilerIDCaption = "Entry Filer ID";
		internal const string ExportEntryFilerIDHint = "Agent/Broker Reference ID in the form of EIN or SSN or DUNS.\r\nValid formats are:\r\n'NN-NNNNNNN' Employer Identification Number (EIN)\r\n'NNN-NN-NNNN' Social Security Number (SSN)\r\nor\r\n'NNNNNNNNN', where N is a number.";

		IRegistryItem Integration.Customs.US.IUSCustomsDataRegistry.ExportEntryFilerID => ExportEntryFilerID;

		string Integration.Customs.US.IUSCustomsDataRegistry.ExportEntryFilerIDValue => ExportEntryFilerID.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty).EntryFilerID;

		public BooleanRegistryItem EntryDeclarant
		{
			get
			{
				return GetItem("EntryDeclarant", delegate
				{
					return new BooleanRegistryItem(
						"EntryDeclarant",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"Set the entry Declarant",
						(NoResString)"The default value will be the current User/Staff member logged in, you can override this to require a broker be entered as the declarant.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						false);
				});
			}
		}

		public StringRegistryItem ClientBranchDesignation
		{
			get
			{
				return GetItem("ClientBranchDesignation", delegate
				{
					return new StringRegistryItem(
						"ClientBranchDesignation",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)"Client Branch Designation",
						(NoResString)"Client Branch Designation default value. The Client Branch Designation will be defaulted (where the entry is scheduled for a statement) to this value, based on the current branch and department. The Client Branch Designation will be defaulted at the time that the Payment Type is selected.",
						new StringRegistryDataType(CharacterCase.Upper),
						RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						RegistryOptions.CannotCallParameterlessValueGetter);
				});
			}
		}

		public GroupNotificationRegistryItem<GroupNotification> AMSBrokerDownloadMessagesGroup
		{
			get
			{
				return GetItem("AMSBrokerDownloadMessagesGroup", delegate
				{
					return new GroupNotificationRegistryItem<GroupNotification>(
						"AMSBrokerDownloadMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"AMS Broker Download Messages",
						(NoResString)"Group to receive AMS Broker Download Messages sent by Customs",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						GroupNotification.Default);
				});
			}
		}

		public GuidRegistryItem BorderLineReleaseMessagesGroup
		{
			get
			{
				return GetItem("BorderLineReleaseMessagesGroup", delegate
				{
					return new GuidRegistryItem(
						"BorderLineReleaseMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"Border Line Release Messages",
						(NoResString)"Group to receive Border Line Release Messages sent by Customs",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK);
				});
			}
		}

		public GuidRegistryItem DISMessagesGroup
		{
			get
			{
				return GetItem("DISMessagesGroup", delegate
				{
					return new GuidRegistryItem(
						"DISMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"DIS Messages",
						(NoResString)"Group to receive DIS Messages sent by Customs",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK);
				});
			}
		}

		public ManifestGroupNotificationRegistryItem ABIMessagesGroup
		{
			get
			{
				return GetItem("ABIMessagesGroup", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"ABIMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"ABI Messages",
						(NoResString)"Group to receive ABI messages sent by Customs if original senders were not found",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue,
						ManifestGroupNotification.Default);
				});
			}
		}

		IRegistryItem Integration.Customs.US.IUSCustomsDataRegistry.ABIMessagesGroup => ABIMessagesGroup;

		public ManifestGroupNotificationRegistryItem LowValueEntriesReleaseMessages
		{
			get
			{
				return GetItem("LowValueEntriesReleaseMessages", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"LowValueEntriesReleaseMessages",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"Low Value Entries Release messages",
						(NoResString)"Group to receive Release notifications for Low Value Entries Consignments",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue,
						LowValueEntriesManifestGroupNotification.Default);
				});
			}
		}

		public ReconInterestRatesRegistryItem ReconInterestRates
		{
			get
			{
				return GetItem("USReconInterestRates", delegate
				{
					ReconInterestRateCollection defaultValue = new ReconInterestRateCollection();
					defaultValue.AddDefaultValues();
					return new ReconInterestRatesRegistryItem(
						"USReconInterestRates",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"Recon. Interest Rates",
						(NoResString)"The interest rates for additional duty payments are updated quarterly per published IRS interest rate.",
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		public SupervisorOverrideRegistryItem SupervisorOverride
		{
			get
			{
				return GetItem("SupervisorOverride", delegate
				{
					SupervisorOverrideData defaultValue = new SupervisorOverrideData();
					return new SupervisorOverrideRegistryItem(
						SupervisorOverrideName,
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)SupervisorOverrideCaption,
						(NoResString)SupervisorOverrideHint,
						RegistryStorageFlags.Company,
						defaultValue);
				});
			}
		}
		internal const string SupervisorOverrideName = "SupervisorOverride";
		internal const string SupervisorOverrideCaption = "Supervisor Override";
		internal const string SupervisorOverrideHint = "Select the additional selection fields for the ‘Allow Message Errors’ supervisor override function as activated in user/group security.";

		public USPackageTypePairsRegistryItem USPackageTypesMapping
		{
			get
			{
				return GetItem("USPackageTypesMapping", delegate
				{
					USPackageTypePairCollection defaultValue = new USPackageTypePairCollection();
					defaultValue.AddDefaultValues();
					return new USPackageTypePairsRegistryItem(
						USPackageTypesMappingName,
						Categories.Customs_UnitedStatesofAmerica,
						(NoResString)USPackageTypesMappingCaption,
						(NoResString)USPackageTypesMappingHint,
						RegistryStorageFlags.Company,
						defaultValue);
				});
			}
		}
		internal const string USPackageTypesMappingName = "USPackageTypesMapping";
		internal const string USPackageTypesMappingCaption = "Package Type Mappings";
		internal const string USPackageTypesMappingHint = "The mappings from Freight package types to Customs package type. If a brokerage job is embedded in a shipment, this mapping is used to convert the package types Freight shipments use to those Customs jobs use.";

		public GroupNotificationRegistryItem<GroupNotification> DailyStatementsMessagesGroup
		{
			get
			{
				return GetItem("DailyStatementsMessagesGroup", delegate
				{
					return new GroupNotificationRegistryItem<GroupNotification>(
						"DailyStatementsMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"Daily Statements Messages",
						(NoResString)"Group to receive Daily Statements Messages sent by Customs",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						GroupNotification.Default);
				});
			}
		}

		public GroupNotificationRegistryItem<GroupNotification> PeriodicMonthlyStatementsMessagesGroup
		{
			get
			{
				return GetItem("PeriodicMonthlyStatementsMessagesGroup", delegate
				{
					return new GroupNotificationRegistryItem<GroupNotification>(
						"PeriodicMonthlyStatementsMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"Periodic Monthly Statements Messages",
						(NoResString)"Group to receive Periodic Monthly Statements Messages sent by Customs",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						GroupNotification.Default);
				});
			}
		}

		public LiquidationGroupNotificationRegistryItem CourtesyNoticeMessagesGroup
		{
			get
			{
				return GetItem("CourtesyNoticeMessagesGroup", delegate
				{
					return new LiquidationGroupNotificationRegistryItem(
						"CourtesyNoticeMessagesGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"Courtesy Notice of Liquidation",
						(NoResString)"Group to receive Courtesy Notice of Liquidation Messages sent by Customs. You can also suppress email notifications when liquidation type is No Change.",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						LiquidationGroupNotification.Default);
				});
			}
		}

		public GroupNotificationRegistryItem<GroupNotification> NumberRangeLimitWarningGroup
		{
			get
			{
				return GetItem("NumberRangeLimitWarningGroup", delegate
				{
					return new GroupNotificationRegistryItem<GroupNotification>(
						"NumberRangeLimitWarningGroup",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
						(NoResString)"Number Range Limit Warning Group",
						(NoResString)"Group to receive warning when the number range limit reached.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						GroupNotification.Default);
				});
			}
		}

		public BoxNumberCollectionRegistryItem BoxNumbers
		{
			get
			{
				return GetItem("BoxNumbers", delegate
				{
					BoxNumberCollection defaultCollection = new BoxNumberCollection();
					return new BoxNumberCollectionRegistryItem("BoxNumbers",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Documents,
						(NoResString)"Box Numbers",
						(NoResString)"Set up Box Numbers for Customs documentation.",
						defaultCollection);
				});
			}
		}

		public StringRegistryItem TIBStatement
		{
			get
			{
				return GetItem("TIBStatement", delegate
				{
					return new StringRegistryItem(
						"TIBStatement",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Documents,
						(NoResString)"TIB Statement",
						(NoResString)"Enter the required wording for the 7501 Entry Summary, Temporary Import Bond (TIB) Statement",
						new StringRegistryDataType(CharacterCase.Normal),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						"IMPORTED FOR A PERIOD NOT TO EXCEED ONE YEAR. NOT TO BE PUT TO ANY OTHER USE AND NOT IMPORTED FOR SALE OR SALE ON APPROVAL.");
				});
			}
		}

		public StringRegistryItem TIBStatementForMV
		{
			get
			{
				return GetItem("TIBStatementForMV", delegate
				{
					return new StringRegistryItem(
						"TIBStatementForMV",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Documents,
						(NoResString)"TIB Statement for Motor Vehicles",
						(NoResString)"Enter the required wording for the 7501 Entry Summary, Temporary Import Bond (TIB) Statement in relation to Motor Vehicles",
						new StringRegistryDataType(CharacterCase.Normal),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						"IMPORTED FOR A PERIOD NOT TO EXCEED SIX MONTHS. NOT TO BE PUT TO ANY OTHER USE AND NOT IMPORTED FOR SALE OR SALE ON APPROVAL.");
				});
			}
		}

		public BooleanRegistryItem IsAttorneyInFact
		{
			get
			{
				return GetItem("IsAttorneyInFact", delegate
				{
					return new BooleanRegistryItem(
						"IsAttorneyInFact",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Documents,
						(NoResString)"Is Attorney-In-Fact?",
						(NoResString)"If ticked, indicates this Company is an 'Attorney In Fact' for documentation printing purposes.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						true);
				});
			}
		}

		public BooleanRegistryItem DoDefaultImporterOfRecord
		{
			get
			{
				return GetItem("DoDefaultImporterOfRecord", delegate
				{
					return new BooleanRegistryItem(
						"DoDefaultImporterOfRecord",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults,
						(NoResString)"Default Importer Of Record?",
						(NoResString)"Set to Yes to default the Importer of Record organization from the Importer at declaration level.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem PrintBrokerSignatureOnEntryDocs
		{
			get
			{
				return GetItem("PrintBrokerSignatureOn3461", delegate
				{
					return new BooleanRegistryItem(
						"PrintBrokerSignatureOn3461",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Documents,
						(NoResString)"Print Broker Signature on Customs forms?",
						(NoResString)"Indicate whether the broker's electronic signature should be printed on the Customs documents. The signature will come from the brokers signature on their staff record.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public GuidRegistryItem PrintSignatureOnEntryDocsBroker
		{
			get
			{
				return GetItem("PrintSignatureOnEntryDocsBroker", delegate
				{
					return new GuidRegistryItem(
						"PrintSignatureOnEntryDocsBroker",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Documents,
						(NoResString)"Declarant on Customs forms",
						(NoResString)"The staff whose signature and name will be printed on the Customs documents. If it is left blank, the signature and name of a declaration’s broker or login staff will be printed instead.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Guid.Empty);
				});
			}
		}

		public BooleanRegistryItem AutoSendEntrySummaryOnAcceptedAII
		{
			get
			{
				return GetItem("AutoSendEntrySummaryOnAcceptedAII", delegate
				{
					return new BooleanRegistryItem(
						"AutoSendEntrySummaryOnAcceptedAII",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)"Auto Send ENS on successful AII?",
						(NoResString)AutoSendEntrySummaryOnAcceptedAIIHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}
		internal const string AutoSendEntrySummaryOnAcceptedAIIHint = "When an invoice is sent as an AII transaction, the Entry Summary will not be transmitted until the invoice is acknowledged as accepted. \r\nOnce the invoice is accepted the Entry Summary is automatically sent.";

		public DefaultStatementPrintDateRegistryItem DefaultPrelimStatementPrintDate
		{
			get
			{
				return GetItem("DefaultPrelimStatementPrintDate", delegate
				{
					return new DefaultStatementPrintDateRegistryItem(
						"DefaultPrelimStatementPrintDate",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)"Default Statement Print Date/Month?",
						(NoResString)DefaultPrelimStatementPrintDateHint
						);
				});
			}
		}
		internal const string DefaultPrelimStatementPrintDateHint = "Should the Preliminary Statement Print Date (PSD) and Periodic Statement Month be defaulted. \r\n\r\nIf automatically defaulting, please enter the number of WORKING days you wish to be added when generating the PSD. \r\n\r\nThe 'base date' for calculating the default values will be determined from the following fallback values: Presentation Date or Estimated Entry Date, (if entered, in precedence), or the later of Estimated Date of Arrival (ETA) and Current Date.";

		public AutoSendStatementDateChangeRequestRegistryItem AutoSendSDCR
		{
			get
			{
				return GetItem("AutoSendSDCR", delegate
				{
					return new AutoSendStatementDateChangeRequestRegistryItem(
						"AutoSendSDCR",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
						(NoResString)"Auto send Statement Date Change Request?",
						(NoResString)(Core.Constants.ProductName + " has the ability to automatically generate and send a Statement Date Change Request message when a Release Date Update is received from ABI. \r\n\r\nThis registry setting, (and Default Statement Print Date registry, which sets the number of days to add when generating the date), controls the default behaviour for this. \r\n\r\nBy default, the Statement Date Change Request message is NOT automatically generated. Override this default setting here, for all cases, or based on individual organization settings - ('opt out' for individual organizations).")
						);
				});
			}
		}

		#region ShipmentAuditMaximumValue

		public DecimalRegistryItem ShipmentHTSMaximumValue
		{
			get
			{
				return GetItem("ShipmentHTSMaximumValue", delegate
				{
					return new DecimalRegistryItem(
						"ShipmentHTSMaximumValue",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Shipment HTS Maximum Value For Printing AWB/HBL",
						(NoResString)"If a HTS amount on a shipment exceeds this value then printing AWB/HBL will not continue until confirmed.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company, 2500m);
				});
			}
		}

		IRegistryItem Integration.Customs.US.IUSCustomsDataRegistry.ShipmentHTSMaximumValue => ShipmentHTSMaximumValue;

		#endregion

		#region ShipmentAuditShouldCheckCompany

		public BooleanRegistryItem ShipmentAuditShouldCheckCompany
		{
			get
			{
				return GetItem("ShipmentAuditShouldCheckCompany", delegate
				{
					return new BooleanRegistryItem(
						"ShipmentAuditShouldCheckCompany",
						Categories.Customs_UnitedStatesofAmerica_Export_AES,
						(NoResString)"Shipment Audit Should Check Company",
						(NoResString)("When a “Bill Of Lading” document is being printed a security check is performed if:" + System.Environment.NewLine + System.Environment.NewLine +
						"a)	Current Country is US" + System.Environment.NewLine +
						"b)	AND any total line price grouped by Harmonised Code is greater than $2500 (defined in the system registry)" + System.Environment.NewLine +
						"c)	AND there is no Internal Transaction Number (ITN) for the shipment" + System.Environment.NewLine + System.Environment.NewLine +
						"Do you also want to check if:" + System.Environment.NewLine + System.Environment.NewLine +
						"d) AND Current Branch or Companies Org proxy is the related broker on the Consignor" + System.Environment.NewLine + System.Environment.NewLine +
						"(Default: Yes)"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
				});
			}
		}

		IRegistryItem Integration.Customs.US.IUSCustomsDataRegistry.ShipmentAuditShouldCheckCompany => ShipmentAuditShouldCheckCompany;

		#endregion

		public BooleanRegistryItem AutoSendCargoReleaseMessageOnSuccessfulIJ
		{
			get
			{
				return GetItem("AutoSendCargoReleaseMessageOnSuccessfulIJ", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"AutoSendCargoReleaseMessageOnSuccessfulIJ",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI,
						(NoResString)AutoSendCargoReleaseMessageOnSuccessfulIJCaption,
						(NoResString)AutoSendCargoReleaseMessageOnSuccessfulIJHint,
						RegistryStorageFlags.Branch,
						false);
					return result;
				});
			}
		}
		internal const string AutoSendCargoReleaseMessageOnSuccessfulIJCaption = "Auto Send Cargo Release Message on successful Consignee Name/Address Add Response?";
		internal const string AutoSendCargoReleaseMessageOnSuccessfulIJHint = "If 'Yes', Cargo Release Message will be sent when successful Consignee Name/Address Add Response coming, if Declaration has no errors.";

		public DefaultFilerContactInformationRegistryItem DefaultFilerContactInformation
		{
			get
			{
				return GetItem("DefaultFilerContactInformation", delegate
				{
					return new DefaultFilerContactInformationRegistryItem(
						"DefaultFilerContactInformation",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_LowValueEntries,
						(NoResString)"Default Filer Contact Information",
						(NoResString)"The contact name and phone number of the Filer that will default to the Low Value Entries job under the Misc tab. If there are no details entered, the contact name and phone number of the logged in staff member will be used instead.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default);
				});
			}
		}

		public CodePairRegistryItem DefaultEntryType
		{
			get
			{
				return GetItem("DefaultEntryType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultEntryType",
						Categories.Customs_UnitedStatesofAmerica_Import_ABI_LowValueEntries,
						(NoResString)"Default Entry Type",
						(NoResString)"The default value of Entry Type for all house bills on a Low Value Entries job.",
						new CodeDescriptionPairListProvider(EntryTypeList.GetLowValueDeclarationEntryTypeList),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						EntryTypeList.Codes.LowValue);
				});
			}
		}
	}

	public static class JobDeclarationExtensionMethods
	{
		public static ZDateTime GetEffectiveDateForECR(this JobDeclaration declaration)
		{
			var result = ZDateTime.Empty;

			if (declaration != null && declaration.US_DateOfExport.IsValid)
			{
				result = declaration.US_DateOfExport;
			}

			if (result.IsEmpty)
			{
				result = ZDateTime.Today;
			}

			return result;
		}
	}
}
