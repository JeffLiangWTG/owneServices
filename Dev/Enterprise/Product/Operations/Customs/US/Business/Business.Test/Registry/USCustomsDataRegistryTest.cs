using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Data;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(USCustomsDataRegistry))]
	sealed class USCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<USCustomsDataRegistry>
	{
		public void TestBondStatusNotificationGroup()
		{
			var testItem = ItemSet.BondStatusNotificationGroup;
			testItem.Options = RegistryOptions.IsValueMandatory;
			TestRegistryItem(testItem,
				"BondStatusNotificationGroup",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_NotificationGroups,
				"Bond Status Notification Group",
				"The staff group that will be receiving bond status notifications for which system could not match to any jobs, if the value is blank, system don't send the notifications.",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
		}

		public void TestSuppressDutyWhenCensusWarningExist()
		{
			TestGenericRegistryItem(
				ItemSet.SuppressAutoBillingDisbursementWhenCensusWarningExists,
				"SuppressAutoBillingDisbursementWhenCensusWarningExists",
				USCustomsDataRegistry.Categories.Customs_Integration_UnitedStatesofAmerica,
				"Suppress auto-billing Customs Disbursement when Census Warning exists?",
				"If it is NO, system will auto-rate Customs Disbursement even when there is a census warning.",
				RegistryStorageFlags.Company,
				true);
		}

		public void TestReferenceFileRequestTimestamps()
		{
			TestGenericRegistryItem(ItemSet.RefFileRequestExchangeRateTimeStamp, "RefFileRequestExchangeRateTimeStamp", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import, "RefFileRequestExchangeRateTimeStamp", "RefFileRequestExchangeRateTimeStamp", RegistryStorageFlags.System, RegistryOptions.IsHidden);
		}

		public void TestBrokerAccounts()
		{
			TestGenericRegistryItem(ItemSet.BrokersAccounts, "ManagedAccounts", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement, USCustomsDataRegistry.BrokerAccountsCaption, USCustomsDataRegistry.BrokerAccountsHint, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestSuppressStatementUpdate()
		{
			TestGenericRegistryItem(ItemSet.SuppressStatementUpdate, "SuppressStatementUpdate", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement, USCustomsDataRegistry.SuppressStatementUpdateCaption, USCustomsDataRegistry.SuppressStatementUpdateDescription, RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		[ExpectNoExceptions()]
		public void TestBrokerAccountFormat1()
		{
			var commandText = string.Format(BrokerAccountTestCommandText, GlbCompany.CurrentCompany.PK, @"<ArrayOfManagedAccount xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><ManagedAccount><BankAccount>79d365eb-958c-4eb4-9e43-e453ca379627</BankAccount><PayerUnitNumber>4456</PayerUnitNumber></ManagedAccount></ArrayOfManagedAccount>");
			Db.Connection.ExecuteScalar(commandText); // Cannot use BusinessObjectFactory here
			var forceReadofRegistryData = USCustomsDataRegistry.Instance.BrokersAccounts.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		[ExpectNoExceptions()]
		public void TestBrokerAccountFormat2()
		{
			var commandText = string.Format(BrokerAccountTestCommandText, GlbCompany.CurrentCompany.PK, @"<ArrayOfManagedAccount xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><ManagedAccount><PayerUnitNumber>4456</PayerUnitNumber></ManagedAccount></ArrayOfManagedAccount>");
			Db.Connection.ExecuteScalar(commandText); // Cannot use BusinessObjectFactory here
			var forceReadofRegistryData = USCustomsDataRegistry.Instance.BrokersAccounts.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		[ExpectNoExceptions()]
		public void TestBrokerAccountFormat3()
		{
			var commandText = string.Format(BrokerAccountTestCommandText, GlbCompany.CurrentCompany.PK, @"<ArrayOfManagedAccount xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><ManagedAccount><BankAccount>79d365eb-958c-4eb4-9e43-e453ca379627</BankAccount></ManagedAccount></ArrayOfManagedAccount>");
			Db.Connection.ExecuteScalar(commandText); // Cannot use BusinessObjectFactory here
			USCustomsDataRegistry.Instance.BrokersAccounts.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		public void TestCustomsDeliveryOrderDisclaimer()
		{
			TestGenericRegistryItem(ItemSet.CustomsDeliveryOrderDisclaimer,
				"CustomsDeliveryOrderDisclaimer",
						USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import,
						"Delivery Order Disclaimer",
						"The disclaimer which will be printed at the bottom of the Delivery Order.",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"CARRIER MUST PRESENT DELIVERY ORDER TO TERMINAL AT TIME OF PICK UP. FAILURE OF CARRIER TO NOTIFY OUR OFFICE OF CARGO BEING DETAINED AT TERMINAL WILL RESULT IN CARRIER BEING RESPONSIBLE FOR ALL DEMURRAGE CHARGES BEING INCURRED."
				);
			StringRegistryDataType dataType = (StringRegistryDataType)ItemSet.CustomsDeliveryOrderDisclaimer.DataType;
			AssertEquals(CharacterCase.Upper, dataType.CharacterCase);
			TextRegistryEditorInfo editorInfo = (TextRegistryEditorInfo)ItemSet.CustomsDeliveryOrderDisclaimer.EditorInfo;
			AssertEquals(TextEditorType.Memo, editorInfo.EditorType);
		}

		public void TestDoDefaultCountriesOfOriginAndExport()
		{
			TestRegistryItem(ItemSet.DoDefaultCountriesOfOriginAndExport, "USDoDefaultCountriesOfOriginAndExport", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults, "Default Countries/Regions of Origin and Export?", "This registry setting controls how the country/region of origin field on the declaration defaults into invoice lines. If set to No, every line created for an invoice will default to the value on the invoice header. If set to Yes, the country/region of origin on invoice header will default based on the Manufacturer or Supplier Organization, the country/region of origin on invoice line will default based on the Manufacturer or FDA Shipper Organization. These values can be overridden at the individual line level if required.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter, true);
		}

		public void TestStatementProcessingPortEqualPortofEntryNonRLF()
		{
			TestRegistryItem(ItemSet.StatementProcessingPortEqualPortofEntryNonRLF,
				"USStatementProcessingPortEqualPortofEntry",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
				"Statement Processing Port equal to Port of Entry for non-RLF?",
				"If this is 'yes', then statement processing port (in the B record) will be set to the same value as the Port of Entry for non-RLF entries. If this is 'no' then the Statement Processing Port will be set to Processing->District Port as set up in the registry.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter,
				false);
		}

		public void TestDefaultDestState()
		{
			TestRegistryItem(ItemSet.DefaultDestState, "DefaultDestState", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults, "Default Destination State?", "Should the Destination State be defaulted from the state of the Ultimate Consignee?", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestUpdateDeclarationWithCargeReleaseResults()
		{
			TestGenericRegistryItem(ItemSet.UpdateDeclarationWithCargoReleaseResults,
				"UpdateDeclarationWithCargoReleaseResults",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				"Update Declaration with Cargo Release Results",
				"If set to Yes, the system will update the Port of Entry, Date of Arrival and Manifest details like quantity, carrier, voyage or flights based on the ACE Cargo Release results if no entry summary is on file at CBP yet and cargo is indicated as released in the ACE Cargo Release message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false
				);
		}

		public void TestEnableTwoStepProcess()
		{
			TestGenericRegistryItem(ItemSet.EnableTwoStepsProcess,
				"EnableTwoStepsProcess",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE,
				"Enable Two-Step Process",
				"If set to yes, system enables entry summary when users enable cargo release or vice versa. This is so that entry summary data is required and validated at the time of sending cargo release.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false
				);
		}

		public void TestEntryProcessingPortMappings()
		{
			TestGenericRegistryItem
				(ItemSet.EntryProcessingPortMappings,
				"EntryProcessingPortMappings",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
				"Non-RLF Entry Port/Processing Port Mapping",
				"The Processing Port for a particular Entry Port can be set up here. This mapping applies to non-RLF jobs.",
				RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestRequireApprovalPriorAuthorizingStatement()
		{
			TestGenericRegistryItem
				(ItemSet.RequireApprovalPriorAuthorizingStatement,
				"RequireApprovalPriorAuthorizingStatement",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
				USCustomsDataRegistry.RequireApprovalPriorAuthorizingStatementCaption,
				USCustomsDataRegistry.RequireApprovalPriorAuthorizingStatementHint,
				RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestBorderCargoReleasePorts()
		{
			TestGenericRegistryItem
				(ItemSet.BorderCargoReleasePorts,
				"BorderCargoReleasePorts",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				USCustomsDataRegistry.BorderCargoReleasePortsCaption,
				USCustomsDataRegistry.BorderCargoReleasePortsHint,
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestAutoCreateDeclarationFromLineRelease()
		{
			TestGenericRegistryItem
				(ItemSet.AutoCreateDeclarationFromLineRelease,
				"AutoCreateDeclarationFromLineRelease",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				USCustomsDataRegistry.AutoCreateDeclarationFromLineReleaseCaption,
				USCustomsDataRegistry.AutoCreateDeclarationFromLineReleaseHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestBranchDistrictPortRelationship()
		{
			TestGenericRegistryItem
				(ItemSet.BranchDistrictPortRelationship,
				"BranchDistrictPortRelationship",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				USCustomsDataRegistry.BranchPortsCaption,
				USCustomsDataRegistry.BranchPortsHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestDisableAbnormalityMessageReporting()
		{
			TestRegistryItem(ItemSet.DisableAbnormalityMessageReporting,
				"USDisableAbnormalityMessageReporting",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica,
				"Disable Abnormal Reporting?",
				"Should reporting of abnormal messaging be disabled?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestFDAContact()
		{
			AssertEquals("Registry value is per branch", RegistryStorageFlags.Branch, ItemSet.BranchFDAContact.Storage);
			AssertEquals("Default Branch PGA Contact", ItemSet.BranchFDAContact.Caption);
			AssertEquals("The contact name and phone number of the selected staff member will default to the declaration when Partner Government Agency reporting is required. \r\n\r\nIf the staff member does not have a work phone number entered, the branch phone number of the home branch of that staff member will be defaulted to the declaration.", ItemSet.BranchFDAContact.Hint);
			AssertEquals("Default value should be empty", Guid.Empty, ItemSet.BranchFDAContact.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			Guid newGuid = Guid.NewGuid();
			ItemSet.BranchFDAContact.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals(newGuid, ItemSet.BranchFDAContact.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCargoReleaseContact()
		{
			AssertEquals("Registry value is per branch", RegistryStorageFlags.Branch, ItemSet.CargoReleaseFTZContact.Storage);
			AssertEquals("Default Branch Cargo Release/FTZ Contact", ItemSet.CargoReleaseFTZContact.Caption);
			AssertEquals("The contact name and phone number of the selected staff member will default to the cargo release or FTZ when sending cargo release or FTZ messages. \r\n\r\nIf the staff member does not have a work phone number entered, the branch phone number of the home branch of that staff member will be defaulted to the cargo release or FTZ.", ItemSet.CargoReleaseFTZContact.Hint);
			AssertEquals("Default value should be empty", Guid.Empty, ItemSet.CargoReleaseFTZContact.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			Guid newGuid = Guid.NewGuid();
			ItemSet.CargoReleaseFTZContact.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals(newGuid, ItemSet.CargoReleaseFTZContact.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDefaultManufacturerFromSupplier()
		{
			TestRegistryItem(ItemSet.DefaultManufacturerFromSupplier, "DefaultManufacturerFromSupplier", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults, "Default Manufacturer from Supplier?", "Should the Manufacturer be automatically set to the Supplier?", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestDefaultSellerFromSupplier()
		{
			TestRegistryItem(ItemSet.DefaultSellerFromSupplier, "DefaultSellerFromSupplier", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults, "Default Seller from Supplier?", "Should the Seller be automatically set to the Supplier?", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestSPIValidation()
		{
			TestRegistryItem(ItemSet.SPIValidation, "SPIValidation", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				"SPI Validation",
				"If a tariff and country of origin is potentially eligible for a Free Trade Agreement, indicated by an SPI, the system will warn the user when no SPI is entered.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new SPIValidationTypeList(),
				SPIValidationTypeList.Codes.MER);
		}

		public void TestAESSendNotificationsToGroup()
		{
			AssertEquals(Core.Constants.Groups.PostMastersGroupPK, ItemSet.AESSendNotificationsToGroup);
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_FullName = "Bob Smith";
			staff.GS_EmailAddress = "dummy@what.com";
			Factory.Save();
			Guid checkGuid = group.PK.ToGuid();
			ItemSet.AESSendNotificationsToGroup = checkGuid;
			AssertEquals(checkGuid, ItemSet.AESSendNotificationsToGroup);
			TestGenericRegistryItem(ItemSet.AESSendNotificationsToGroupItem, "AESSendNotificationsToGroup", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export_AES, "Group To Send AES Notifications To", "The staff group that will be receiving notifications about AES Responses.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory, Core.Constants.Groups.PostMastersGroupPK);
			GuidFindBoxRegistryEditorInfo editorInfo = (GuidFindBoxRegistryEditorInfo)ItemSet.AESSendNotificationsToGroupItem.EditorInfo;
			AssertEquals("EditorInfo.FindBoxCollection", RegistryFindBoxCollection.GlbGroup, editorInfo.FindBoxCollection);
			AssertEquals("ItemSet.AESSendNotificationsToGroupItem.DataType", typeof(NotificationGroupGuidRegistryDataType), ItemSet.AESSendNotificationsToGroupItem.DataType.GetType());
		}

		public void TestUSExportAESRegistryItems()
		{
			var itemSet = GetNewItemSet();
			TestRegistryItem(itemSet.ExportDefaultTariffType, "ExportDefaultTariffType", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export_AES, "Default Tariff Type", "The default Tariff Type selected here (Schedule B or HTS) determines which Tariff type is defaulted to new Shippers Export Declarations.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, new TariffTypeList(), TariffTypeList.Codes.ScheduleB);

			GlbStaff.CurrentUser.GS_LoginName = "BOB";
			RegistryItemDictionary.Instance.PurgeAll();
			itemSet = GetNewItemSet();
			TestRegistryItem(itemSet.ExportDefaultTariffType, "ExportDefaultTariffType", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export_AES, "Default Tariff Type", "The default Tariff Type selected here (Schedule B or HTS) determines which Tariff type is defaulted to new Shippers Export Declarations.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, new TariffTypeList(), TariffTypeList.Codes.ScheduleB);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestDefaultOwnerRefOn7501()
		{
			TestRegistryItem(ItemSet.DefaultOwnerRefOn7501, "DefaultOwnerRefOn7501", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults, "Default Owner reference in 7501 Box 43?", $"Should the Owner Reference print in Box 43 on the 7501 Entry Summary?\r\n\r\nThe owner reference prints with the {BrandingFactory.Instance.ProductName} job number in Box 43 on the 7501 Entry Summary document.\r\n\r\nFor example: B00001042 / Ref: Order 7395-A", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, true);
		}

		public void TestDoDefaultShipTo()
		{
			TestRegistryItem(ItemSet.DoDefaultShipTo, "DoDefaultShipTo", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Defaults, "Default Ship To?", "Set to Yes to default the Ship To Party from the Consignee", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
		}

		public void TestSeverityLevelForMissingUSMCACertificate()
		{
			TestRegistryItem(ItemSet.SeverityLevelForMissingUSMCACertificate,
				"SeverityLevelForMissingUSMCACertificate",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica,
				"Severity Level for Missing USMCA Certificate",
				"Set the severity level for validating related to the use of claiming USMCA on a line. If validation is activated, the system will look for a valid Certificate of Origin. The COO may exist on the Product or on the Importer Organization.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				new ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
		}

		public void TestEnableETAValidation()
		{
			TestRegistryItem(ItemSet.EnableETAValidation, "EnableETAValidation", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI, "Enable ETA Validation", "If turned on, system will add a warning to the ETA field when no data is entered.", RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestEnableExportDeclarationDataUpdaterServiceTask()
		{
			TestRegistryItem(ItemSet.EnableExportDeclarationDataUpdaterServiceTask, "EnableExportDeclarationDataUpdaterServiceTask", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export, "Enable EDU Service Task", "If enabled, EDU (Export Declaration Data Updater) service task will run. To cause immediate effect, please restart the Process Controller in Service Task.", RegistryStorageFlags.System, false);
		}

		public void TestIsAnInBondCommodityMustHaveAValidProduct()
		{
			TestRegistryItem(ItemSet.AnInBondCommodityMustHaveAValidProduct, "USAnInBondCommodityMustHaveAValidProduct", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI, "In-Bond Commodity Product", "Ensure that all Bonded Warehousing commodities on an In-Bond job has a valid product.", RegistryStorageFlags.Company, false);
		}

		public void TestCreateMIDOrganizationOnUnmatchedImport()
		{
			TestGenericRegistryItem(ItemSet.CreateMIDOrganizationOnUnmatchedImport,
				"CreateMIDOrganizationOnUnmatchedImport",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import,
				"Create MID Organization on unmatched import",
				"When this item is enabled, during Native XML import of a Product or when using the Import Data function in a Customs Declaration, a new Manufacturer Identification (MID) Organization will automatically be created when the import contains a MID that does not match an existing Organization. Once created a Query Manufacturer message will be sent to US Customs and upon successful receipt of matching address details the newly created Organization record updated.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestAutoQueryEntrySummaries()
		{
			TestGenericRegistryItem
				(ItemSet.AutoQueryEntrySummaries,
				"AutoQueryEntrySummaries",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				USCustomsDataRegistry.AutoQueryEntrySummariesCaption,
				USCustomsDataRegistry.AutoQueryEntrySummariesHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestAutoQueryBillOfLadingCargoManifestStatus()
		{
			TestGenericRegistryItem
				(ItemSet.AutoQueryBillOfLadingCargoManifestStatus,
				"AutoQueryBillOfLadingCargoManifestStatus",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				USCustomsDataRegistry.AutoQueryBillOfLadingCargoManifestStatusCaption,
				USCustomsDataRegistry.AutoQueryBillOfLadingCargoManifestStatusHint,
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestDeferredTaxDueDateCalculation()
		{
			TestGenericRegistryItem(ItemSet.DefTaxDueDateCalculationOption,
				"DefTaxDueDateCalculationOption",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI,
				USCustomsDataRegistry.DeferredTaxDueDateCalculationCaption,
				USCustomsDataRegistry.DeferredTaxDueDateCalculationHint,
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				DefTaxDueDateCalculationOptionList.Codes.COL);
		}

		public void TestInformalImportWarningOrError()
		{
			TestGenericRegistryItem(ItemSet.InformalImportWarningOrError,
				"InformalImportWarningOrError",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE,
				USCustomsDataRegistry.InformalImportWarningOrErrorCaption,
				USCustomsDataRegistry.InformalImportWarningOrErrorHint,
				(RegistryStorageFlags.System | RegistryStorageFlags.Company),
				RegistryOptions.CannotCallParameterlessValueGetter,
				ErrorWarningInformalImport.Codes.WAR);
		}

		public void TestMonthlyStatementPaymentPosting()
		{
			TestGenericRegistryItem(ItemSet.MonthlyStatementPaymentPosting,
				"MonthlyStatementPaymentPosting",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Statement,
				USCustomsDataRegistry.MonthlyStatementPaymentPostingCaption,
				USCustomsDataRegistry.MonthlyStatementPaymentPostingHint,
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				MonthlyStatementPaymentPostingOptionList.Codes._1);
		}

		public void TestExportDefaultRoutedTransaction()
		{
			TestGenericRegistryItem
				(ItemSet.ExportDefaultRoutedTransaction,
				"ExportDefaultRoutedTransaction",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export_AES,
				(NoResString)"Default Routed Transaction",
				(NoResString)"If this is yes, the Routed Transaction is defaulted with N to new Shippers Export Declarations.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestAllowExportDefaultOriginIndicatorAtInvoice()
		{
			TestGenericRegistryItem
				(ItemSet.AllowExportDefaultOriginIndicatorAtInvoice,
				"AllowExportDefaultOriginIndicatorAtInvoice",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export_AES,
				(NoResString)"Allow Origin Indicator to be entered at Invoice Header level",
				(NoResString)"If this is no, each invoice line will require Origin Indicator to be entered.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestDefaultNLRLicenseType()
		{
			TestGenericRegistryItem
				(ItemSet.DefaultNLRLicenseType,
				"DefaultNLRLicenseType",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Export_AES,
				(NoResString)"Default NLR License Type",
				(NoResString)"If set to Yes, the License Type on a US Export Declaration will default to C33/NLR.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestEnableNCTAsDefaultContainerModeForAirOrTruckDeclaration()
		{
			TestGenericRegistryItem(ItemSet.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration,
				"EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica,
				"Enable NCT as default container mode for air or truck declaration",
				"This enables NCT to be defaulted to the container mode for stand-alone air or truck declarations.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestEnableDocAddressForFDA()
		{
			TestGenericRegistryItem(ItemSet.EnableDocAddressForFDA,
				"EnableDocAddressForFDA",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ACE_PGA,
				"Enable DocAddress for FDA",
				"If ticked, the DocAddress will be used instead of Organizations in FDA program.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableFDAForLowValueEntries()
		{
			TestGenericRegistryItem(ItemSet.EnableFDAForLowValueEntries,
				"EnableFDAForLowValueEntries",
				USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_LowValueEntries,
				"Enable FDA for Low Value Entries",
				"If ticked, FDA will be able to be declared in Low Value Entries.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestPrintSignatureOnEntryDocsBroker()
		{
			TestGenericRegistryItem(ItemSet.PrintSignatureOnEntryDocsBroker,
				"PrintSignatureOnEntryDocsBroker",
				ABIUSDocuments,
				"Declarant on Customs forms",
				"The staff whose signature and name will be printed on the Customs documents. If it is left blank, the signature and name of a declaration’s broker or login staff will be printed instead.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory);
		}

		public void TestAllRegistryItemsAvaialbeToUSandPR()
		{
			foreach (IRegistryItem item in AllItems)
			{
				Assert(item.Name + ".CountryFilterPKs available to US", item.CountryFilterPKs.Contains(Core.Constants.CountryGuids.UnitedStates));
				Assert(item.Name + ".CountryFilterPKs available to PR", item.CountryFilterPKs.Contains(Core.Constants.CountryGuids.PuertoRico));
			}
		}

		public void TestUSRegistryItems()
		{
			TestRegistryItem(ItemSet.IsAttorneyInFact, "IsAttorneyInFact", ABIUSDocuments, "Is Attorney-In-Fact?", "If ticked, indicates this Company is an 'Attorney In Fact' for documentation printing purposes.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter, true);
			TestRegistryItem(ItemSet.PrintBrokerSignatureOnEntryDocs, "PrintBrokerSignatureOn3461", ABIUSDocuments, "Print Broker Signature on Customs forms?", "Indicate whether the broker's electronic signature should be printed on the Customs documents. The signature will come from the brokers signature on their staff record.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
			TestRegistryItem(ItemSet.EntryDeclarant, "EntryDeclarant", ABIUSCustomsCategory, "Set the entry Declarant", "The default value will be the current User/Staff member logged in, you can override this to require a broker be entered as the declarant.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter, false);
			TestRegistryItem(ItemSet.ShipmentHTSMaximumValue, "ShipmentHTSMaximumValue", AESUSCustomsCategory, "Shipment HTS Maximum Value For Printing AWB/HBL", "If a HTS amount on a shipment exceeds this value then printing AWB/HBL will not continue until confirmed.", RegistryStorageFlags.System | RegistryStorageFlags.Company, 2500m);

			var shipmentAuditShouldCheckCompanyHint = "When a “Bill Of Lading” document is being printed a security check is performed if:" + System.Environment.NewLine + System.Environment.NewLine +
				"a)	Current Country is US" + System.Environment.NewLine +
				"b)	AND any total line price grouped by Harmonised Code is greater than $2500 (defined in the system registry)" + System.Environment.NewLine +
				"c)	AND there is no Internal Transaction Number (ITN) for the shipment" + System.Environment.NewLine + System.Environment.NewLine +
				"Do you also want to check if:" + System.Environment.NewLine + System.Environment.NewLine +
				"d) AND Current Branch or Companies Org proxy is the related broker on the Consignor" + System.Environment.NewLine + System.Environment.NewLine +
				"(Default: Yes)";
			TestRegistryItem(ItemSet.ShipmentAuditShouldCheckCompany, "ShipmentAuditShouldCheckCompany", AESUSCustomsCategory, "Shipment Audit Should Check Company", shipmentAuditShouldCheckCompanyHint, RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestDefaultPrelimStatementPrintDateRegistryItem()
		{
			AssertEquals(typeof(DefaultStatementPrintDateRegistryItem), ItemSet.DefaultPrelimStatementPrintDate.GetType());
			DefaultStatementPrintDateRegistryItem item = ItemSet.DefaultPrelimStatementPrintDate;
			AssertEquals("DefaultPrelimStatementPrintDate", item.Name);
			AssertEquals(ABIStatement, item.Category);
			AssertEquals("Default Statement Print Date/Month?", item.Caption);
			AssertEquals(USCustomsDataRegistry.DefaultPrelimStatementPrintDateHint, item.Hint);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
			AssertEquals(RegistryOptions.CannotCallParameterlessValueGetter, item.Options);
			AssertEquals("Default value should be false", false, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).DoDefaultPrelimStatementPrintDate);
		}

		public void TestAutoSendSDCRRegistryItem()
		{
			AssertEquals(typeof(AutoSendStatementDateChangeRequestRegistryItem), ItemSet.AutoSendSDCR.GetType());
			AutoSendStatementDateChangeRequestRegistryItem item = ItemSet.AutoSendSDCR;
			AssertEquals("AutoSendSDCR", item.Name);
			AssertEquals(ABIStatement, item.Category);
			AssertEquals("Auto send Statement Date Change Request?", item.Caption);
			AssertEquals(Core.Constants.ProductName + " has the ability to automatically generate and send a Statement Date Change Request message when a Release Date Update is received from ABI. \r\n\r\nThis registry setting, (and Default Statement Print Date registry, which sets the number of days to add when generating the date), controls the default behaviour for this. \r\n\r\nBy default, the Statement Date Change Request message is NOT automatically generated. Override this default setting here, for all cases, or based on individual organization settings - ('opt out' for individual organizations).", item.Hint);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
			AssertEquals("Default value should be blank", "", item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).OverrideAllOrByOrganisation);
		}

		public void TestDoDefaultImporterOfRecord()
		{
			TestRegistryItem(ItemSet.DoDefaultImporterOfRecord, "DoDefaultImporterOfRecord", ABIUSDefaults, "Default Importer Of Record?", "Set to Yes to default the Importer of Record organization from the Importer at declaration level.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestAutoSendEntrySummaryOnAcceptedAII()
		{
			TestRegistryItem(ItemSet.AutoSendEntrySummaryOnAcceptedAII, "AutoSendEntrySummaryOnAcceptedAII", ABIUSCustomsCategory, "Auto Send ENS on successful AII?", USCustomsDataRegistry.AutoSendEntrySummaryOnAcceptedAIIHint, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestClientBranchDesignation()
		{
			TestStringRegistryItem(ItemSet.ClientBranchDesignation, "ClientBranchDesignation", ABIStatement, "Client Branch Designation", "Client Branch Designation default value. The Client Branch Designation will be defaulted (where the entry is scheduled for a statement) to this value, based on the current branch and department. The Client Branch Designation will be defaulted at the time that the Payment Type is selected.", RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, TextEditorType.TextBox, RegistryOptions.CannotCallParameterlessValueGetter, "", CharacterCase.Upper);
		}

		public void TestReceiverDistrictPort()
		{
			var item = ItemSet.ReceiverDistrictPort;
			TestGenericRegistryItem(item, "ReceiverDistrictPort", ABIReceiver, "District Port", "What is the receiver district port?", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue, "");
			AssertEquals("EditorInfo.EditorType", TextEditorType.TextBox, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			RegistryTester.SetValue(item, "8888");
			AssertEquals("Value", "8888", item.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty));
			AssertEquals(4, ((StringRegistryDataType)item.DataType).MinLength);
			AssertEquals(4, ((StringRegistryDataType)item.DataType).MaxLength);
		}

		public void TestBoxNumbers()
		{
			AssertEquals(typeof(BoxNumberCollectionRegistryItem), ItemSet.BoxNumbers.GetType());
			TestGenericRegistryItem(ItemSet.BoxNumbers, "BoxNumbers", ABIUSDocuments, "Box Numbers", "Set up Box Numbers for Customs documentation.", RegistryStorageFlags.Branch, RegistryOptions.MustOverrideDefaultValue);
		}

		public void TestTIBStatement()
		{
			TestStringRegistryItem(ItemSet.TIBStatement, "TIBStatement", ABIUSDocuments, "TIB Statement", "Enter the required wording for the 7501 Entry Summary, Temporary Import Bond (TIB) Statement", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, TextEditorType.TextBox, RegistryOptions.Default, "IMPORTED FOR A PERIOD NOT TO EXCEED ONE YEAR. NOT TO BE PUT TO ANY OTHER USE AND NOT IMPORTED FOR SALE OR SALE ON APPROVAL.", CharacterCase.Normal);
		}

		public void TestTIBStatementForMV()
		{
			TestStringRegistryItem(ItemSet.TIBStatementForMV, "TIBStatementForMV", ABIUSDocuments, "TIB Statement for Motor Vehicles", "Enter the required wording for the 7501 Entry Summary, Temporary Import Bond (TIB) Statement in relation to Motor Vehicles", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, TextEditorType.TextBox, RegistryOptions.Default, "IMPORTED FOR A PERIOD NOT TO EXCEED SIX MONTHS. NOT TO BE PUT TO ANY OTHER USE AND NOT IMPORTED FOR SALE OR SALE ON APPROVAL.", CharacterCase.Normal);
		}

		public void TestEntryFilerRegistryItem()
		{
			AssertEquals(typeof(EntryFilerRegistryItem), ItemSet.EntryFiler.GetType());
			var item = ItemSet.EntryFiler;
			AssertEquals("EntryFiler", item.Name);
			AssertEquals(USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI, item.Category);
			AssertEquals("Entry Filer", item.Caption);
			AssertEquals("Entry Filer Information: A unique code assigned by CBP to all active entry document preparers, ABI Certified.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Default value for ABI Certified should be true", true, item.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).IsABICertified);
		}

		public void TestProcessingDistrictPortCode()
		{
			var item = ItemSet.ProcessingDistrictPortCode;
			TestGenericRegistryItem(item, "ProcessingDistrictPortCode", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Processing, "District Port", "What is the Processing District Port Code that will be reported to Customs for ABI and ISF messages?", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue, "");
			AssertEquals("EditorInfo.EditorType", TextEditorType.TextBox, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			Enterprise.ZArchitecture.Environment.Testing.RegistryTester.SetValue(item, "8888");
			AssertEquals("Value", "8888", item.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty));
			AssertEquals(4, ((StringRegistryDataType)item.DataType).MinLength);
			AssertEquals(4, ((StringRegistryDataType)item.DataType).MaxLength);
		}

		public void TestARecordOfficeCode()
		{
			TestStringRegistryItem(ItemSet.ARecordOfficeCode, "OfficeCode", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Office, "A-Record", "The Office Code which will be used in the A-Record when sending messages to CBP.", RegistryStorageFlags.Company, TextEditorType.TextBox, RegistryOptions.Default, "", CharacterCase.Upper, "AB");
		}

		public void TestBRecordOfficeCode()
		{
			TestStringRegistryItem(ItemSet.BRecordOfficeCode, "BRecordOfficeCode", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Office, "B-Record", "The Office Code which will be used in the B-Record when sending messages to CBP.  If it is not specified, then the A-Record Office Code will be used.", RegistryStorageFlags.Company, TextEditorType.TextBox, RegistryOptions.Default, "", CharacterCase.Upper, "AB");
			ItemSet.ARecordOfficeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "02");
			TestStringRegistryItem(ItemSet.BRecordOfficeCode, "BRecordOfficeCode", USCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_Office, "B-Record", "The Office Code which will be used in the B-Record when sending messages to CBP.  If it is not specified, then the A-Record Office Code will be used.", RegistryStorageFlags.Company, TextEditorType.TextBox, RegistryOptions.Default, "02", CharacterCase.Upper, "AB");
		}

		public void TestExportEntryFilerID()
		{
			AssertEquals(typeof(Registry.Business.Customs.US.ExportEntryFilerIDRegistryItem), ItemSet.ExportEntryFilerID.GetType());
			var item = ItemSet.ExportEntryFilerID;
			AssertEquals("ExportEntryFilerID", item.Name);
			AssertEquals(AESUSCustomsCategory, item.Category);
			AssertEquals(USCustomsDataRegistry.ExportEntryFilerIDCaption, item.Caption);
			AssertEquals(USCustomsDataRegistry.ExportEntryFilerIDHint, item.Hint);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
			AssertEquals(Enterprise.Core.CountryGuids.AllUSCountriesForEntryFilerID, item.CountryFilterPKs);
			AssertEquals("Default value for Filer ID Type", Registry.Business.Customs.US.AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber, item.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).EntryFilerIDType);
		}

		public void TestPreparerDistrictPort()
		{
			var item = ItemSet.PreparerDistrictPort;
			TestGenericRegistryItem(item, USCustomsDataRegistry.PreparerDistrictPortName,
				ABIUSRemoteLocationFilling,
				USCustomsDataRegistry.PreparerDistrictPortCaption,
				USCustomsDataRegistry.PreparerDistrictPortHint,
				RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter,
				"");

			AssertEquals("EditorInfo.EditorType", TextEditorType.TextBox, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			RegistryTester.SetValue(item, "8888");
			AssertEquals("Value", "8888", item.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty));
			AssertEquals(4, ((StringRegistryDataType)item.DataType).MinLength);
			AssertEquals(4, ((StringRegistryDataType)item.DataType).MaxLength);
		}

		public void TestPreparerOfficeCode()
		{
			TestStringRegistryItem(
				ItemSet.PreparerOfficeCode,
				USCustomsDataRegistry.PreparerOfficeCodeName,
				ABIUSRemoteLocationFilling,
				USCustomsDataRegistry.PreparerOfficeCodeCaption,
				USCustomsDataRegistry.PreparerOfficeCodeHint,
				RegistryStorageFlags.Branch,
				TextEditorType.TextBox,
				RegistryOptions.CannotCallParameterlessValueGetter,
				string.Empty,
				CharacterCase.Normal);
		}

		public void TestInBondNumberRegistryItems()
		{
			AssertEquals(typeof(InBondNumberRangeRegistryItem), ItemSet.CompanyOrBranchInBondNumberRange.GetType());
			TestGenericRegistryItem(ItemSet.CompanyOrBranchInBondNumberRange, "CompanyOrBranchInBondNumberRange", ABIInBondNumber, "Number Range", "Enter the range of In-Bond Numbers for this Company or for individual Branches.\r\nThese numbers are assigned by CBP and are eight digit numbers without a check digit.\r\ne.g. 10000000-19999999. Be sure to NOT enter a check digit below.\r\n\r\n\r\nEnter also the limit of available In-Bond numbers remaining in the range established for this Company or Branch before users are warned;\r\nonce reached, the system will start warning users of the potential for running out of In-Bond numbers.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
		}

		public void TestSendAllITNumbersInBOLMessage()
		{
			TestRegistryItem(ItemSet.SendAllITNumbersInBOLMessage,
				"SendAllITNumbersInBOLMessage",
				ABIUSCustomsCategory,
				"Send All IT Numbers Format in BOL",
				"Only 'V' IT Numbers will be send in Bill Of Lading Update message by default. If 'Yes', all types of IT Numbers will be sent in Bill Of Lading Update message.",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestTransportModeForInBondCreationFromConsol()
		{
			TestRegistryItem(ItemSet.TransportModeForInBondCreationFromConsol,
				"TransportModeForInBondCreationFromConsol",
				ABIUSCustomsCategory,
				"Permit transport mode for in-bond creation from consol",
				"The field below is utilized to verify the eligibility of the consolidation for creating in-bond jobs. If the provided value corresponds to the transport mode specified in the consolidation, the user will have the ability to create in-bond movements on the consolidation.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				new TransportModeForInBondCreationFromConsolList(),
				TransportModeForInBondCreationFromConsolList.Codes.AIR);
		}

		public void TestReconInterestRatesRegistryItem()
		{
			ReconInterestRatesRegistryItem item = ItemSet.ReconInterestRates;
			AssertEquals("USReconInterestRates", item.Name);
			AssertEquals("Recon. Interest Rates", item.Caption);
			AssertEquals(ABIUSCustomsCategory, item.Category);
			AssertEquals("The interest rates for additional duty payments are updated quarterly per published IRS interest rate.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			Assert("There are default items in the collection", item.DefaultValue.Count != 0);
		}

		public void TestUSPackageTypePairsRegistryItem()
		{
			var item = ItemSet.USPackageTypesMapping;
			AssertEquals(USCustomsDataRegistry.USPackageTypesMappingName, item.Name);
			AssertEquals(USCustomsDataRegistry.USPackageTypesMappingCaption, item.Caption);
			AssertEquals(USCustomsCategory, item.Category);
			AssertEquals(USCustomsDataRegistry.USPackageTypesMappingHint, item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			Assert("There are default items in the collection", item.DefaultValue.Count != 0);
		}

		public void TestSupervisorOverrideRegistryItem()
		{
			SupervisorOverrideRegistryItem item = ItemSet.SupervisorOverride;
			AssertEquals(USCustomsDataRegistry.SupervisorOverrideName, item.Name);
			AssertEquals(USCustomsDataRegistry.SupervisorOverrideCaption, item.Caption);
			AssertEquals(ABIUSCustomsCategory, item.Category);
			AssertEquals(USCustomsDataRegistry.SupervisorOverrideHint, item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
		}

		public void TestABIGroupNotificationRegistryItem()
		{
			TestGroupNotificationRegistryItem(ItemSet.ABIMessagesGroup,
				"ABIMessagesGroup",
				ABINotificationGroups,
				"ABI Messages",
				"Group to receive ABI messages sent by Customs if original senders were not found",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue,
				ManifestGroupNotification.Default);
		}

		public void TestLowValueEntriesReleaseMessagesRegistryItem()
		{
			TestGroupNotificationRegistryItem(ItemSet.LowValueEntriesReleaseMessages,
				"LowValueEntriesReleaseMessages",
				ABINotificationGroups,
				"Low Value Entries Release messages",
				"Group to receive Release notifications for Low Value Entries Consignments",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue,
				LowValueEntriesManifestGroupNotification.Default);
		}

		public void TestMessagesGroup()
		{
			TestGroupNotificationRegistryItem(ItemSet.DailyStatementsMessagesGroup,
				"DailyStatementsMessagesGroup",
				ABINotificationGroups,
				"Daily Statements Messages",
				"Group to receive Daily Statements Messages sent by Customs",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				GroupNotification.Default);

			TestGroupNotificationRegistryItem(ItemSet.PeriodicMonthlyStatementsMessagesGroup,
				"PeriodicMonthlyStatementsMessagesGroup",
				ABINotificationGroups,
				"Periodic Monthly Statements Messages",
				"Group to receive Periodic Monthly Statements Messages sent by Customs",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				GroupNotification.Default);

			TestGroupNotificationRegistryItem(ItemSet.AMSBrokerDownloadMessagesGroup,
				"AMSBrokerDownloadMessagesGroup",
				ABINotificationGroups,
				"AMS Broker Download Messages",
				"Group to receive AMS Broker Download Messages sent by Customs",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				GroupNotification.Default);

			TestRegistryItem(ItemSet.BorderLineReleaseMessagesGroup,
				"BorderLineReleaseMessagesGroup",
				ABINotificationGroups,
				"Border Line Release Messages",
				"Group to receive Border Line Release Messages sent by Customs",
				RegistryStorageFlags.Company,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);

			TestGroupNotificationRegistryItem(ItemSet.CourtesyNoticeMessagesGroup,
				"CourtesyNoticeMessagesGroup",
				ABINotificationGroups,
				"Courtesy Notice of Liquidation",
				"Group to receive Courtesy Notice of Liquidation Messages sent by Customs. You can also suppress email notifications when liquidation type is No Change.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				GroupNotification.Default);

			TestRegistryItem(ItemSet.DISMessagesGroup,
				"DISMessagesGroup",
				ABINotificationGroups,
				"DIS Messages",
				"Group to receive DIS Messages sent by Customs",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		public void TestAutoSendCargoReleaseMessageOnSuccessfulIJ()
		{
			TestRegistryItem(ItemSet.AutoSendCargoReleaseMessageOnSuccessfulIJ,
				"AutoSendCargoReleaseMessageOnSuccessfulIJ",
				ABIUSCustomsCategory,
				USCustomsDataRegistry.AutoSendCargoReleaseMessageOnSuccessfulIJCaption,
				USCustomsDataRegistry.AutoSendCargoReleaseMessageOnSuccessfulIJHint,
				RegistryStorageFlags.Branch,
				false);
		}

		public void TestDefaultFilerContactInformation()
		{
			TestGenericRegistryItem(ItemSet.DefaultFilerContactInformation,
				"DefaultFilerContactInformation",
				ABILowValueEntries,
				"Default Filer Contact Information",
				"The contact name and phone number of the Filer that will default to the Low Value Entries job under the Misc tab. If there are no details entered, the contact name and phone number of the logged in staff member will be used instead.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestDefaultEntryType()
		{
			TestRegistryItem(ItemSet.DefaultEntryType,
				"DefaultEntryType",
				ABILowValueEntries,
				"Default Entry Type",
				"The default value of Entry Type for all house bills on a Low Value Entries job.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				EntryTypeList.GetLowValueDeclarationEntryTypeList(),
				EntryTypeList.Codes.LowValue);
		}

		protected override bool IsCountrySpecificRegistrySet => true;

		const string USCustomsCategory = "Customs/Country or Region Specific/United States of America";
		const string USCustomsCategoryImport = USCustomsCategory + "/Import";
		const string ABIUSCustomsCategory = USCustomsCategoryImport + "/ABI";
		const string ABIUSDefaults = ABIUSCustomsCategory + "/Defaults";
		const string ABIUSDocuments = ABIUSCustomsCategory + "/Documents";
		const string ABIInBondNumber = ABIUSCustomsCategory + "/In-Bond Number";
		const string ABINotificationGroups = ABIUSCustomsCategory + "/Notification Groups";
		const string ABIReceiver = ABIUSCustomsCategory + "/Receiver";
		const string ABIStatement = ABIUSCustomsCategory + "/Statement";
		const string USCustomsCategoryExport = USCustomsCategory + "/Export";
		const string AESUSCustomsCategory = USCustomsCategoryExport + "/AES";
		const string ABIUSRemoteLocationFilling = ABIUSCustomsCategory + "/Remote Location Filling";
		const string ABILowValueEntries = ABIUSCustomsCategory + "/Low Value Entries";
		const string BrokerAccountTestCommandText = @"
insert into dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue)
	values(newid(), 'ManagedAccounts',  '{0}', 'BIN', convert(varbinary(max), N'{1}'))";
	}
}
