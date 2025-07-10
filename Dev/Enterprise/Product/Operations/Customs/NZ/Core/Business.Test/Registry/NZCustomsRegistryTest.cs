using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Registry.Testing
{
	using System.Linq;
	using Enterprise.Registry.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCustomsDataRegistry))]
	class NZCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<NZCustomsDataRegistry>
	{
		public void TestPreferentialCountryGroupCodeDefaulting()
		{
			TestRegistryItem(ItemSet.PreferentialCountryGroupCodeDefaulting, "PreferentialCountryGroupCodeDefaulting", "Customs/Country or Region Specific/New Zealand", "Preferential Country/Region Group Code Defaulting", "When set to true, the preferential country/region group code will default to the code that will have the lowest duty rate.", RegistryStorageFlags.Company, expectedDefaultValue: false);
		}

		public void TestMAFeBACCaRegistryItems()
		{
			TestRegistryItem(ItemSet.MAFeBACCaTestMode, "NZMAFeBACCaTestMode", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Set to Test Mode", "Should MPI eBACCa messages be sent to the Test System?", RegistryStorageFlags.Company, expectedDefaultValue: false);

			TestRegistryItem(ItemSet.MAFeBACCaSendAcknowledgements, "NZMAFeBACCaSendAcknowledgements", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Send Message Acknowledgements", "Send message acknowledgements to staff member, nominated group or combination of both", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Constants.EmailTo.StaffMemberAndNominatedGroup);
			TestRegistryItem(ItemSet.MAFeBACCaSendAcknowledgementsToGroup, "NZMAFeBACCaSendAcknowledgementsToGroup", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Send Message Acknowledgements To Group", "Send message acknowledgements to selected group", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryFindBoxCollection.GlbGroup, Core.Constants.Groups.PostMastersGroupPK);

			TestRegistryItem(ItemSet.MAFeBACCaSendErrors, "NZMAFeBACCaSendErrors", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Send Message Errors", "Send message errors to staff member, nominated group or combination of both", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Constants.EmailTo.StaffMemberAndNominatedGroup);
			TestRegistryItem(ItemSet.MAFeBACCaSendErrorsToGroup, "NZMAFeBACCaSendErrorsToGroup", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Send Message Errors To Group", "Send message errors to selected group", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryFindBoxCollection.GlbGroup, Core.Constants.Groups.PostMastersGroupPK);

			TestRegistryItem(ItemSet.MAFeBACCaSendImpediments, "NZMAFeBACCaSendImpediments", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Send Message Impediments", "Send message Impediments to staff member, nominated group or combination of both", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Constants.EmailTo.StaffMemberAndNominatedGroup);
			TestRegistryItem(ItemSet.MAFeBACCaSendImpedimentsToGroup, "NZMAFeBACCaSendImpedimentsToGroup", "Customs/Country or Region Specific/New Zealand/MPI eBACCa", "Send Message Impediments To Group", "Send message Impediments to selected group", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryFindBoxCollection.GlbGroup, Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestAutoPrintOnClearanceResponseItems()
		{
			TestRegistryItem(ItemSet.CustomsCertificatePrinter, "NZCustomsCertificatePrinter", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Customs Certificate", "Printer", "The Printer you would like copies of the Customs Certificate Document to go to when a Delivery Order response is received.", RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue, RegistryFindBoxCollection.StmPrintQueue, Guid.Empty);
			TestRegistryItem(ItemSet.CustomsCertificateCopies, "NZCustomsCertificateCopies", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Customs Certificate", "Copies", "How many copies of the Customs Certificate Document you would like to automatically print when a Delivery Order response is received.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, 1);
			TestRegistryItem(ItemSet.CustomsCertificateCopyToEDocs, "NZCustomsCertificateCopyToEDocs", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Customs Certificate", "Copy to eDocs", "Would you like a copy of the Customs Certificate Document saved in eDocs when a Delivery Order response is received.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, expectedDefaultValue: true);
			TestRegistryItem(ItemSet.EntryPrintPrinter, "NZEntryPrintPrinter", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Entry Print", "Printer", "The Printer you would like copies of the Entry Print Document to go to when a Delivery Order response is received.", RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue, RegistryFindBoxCollection.StmPrintQueue, Guid.Empty);
			TestRegistryItem(ItemSet.EntryPrintCopies, "NZEntryPrintCopies", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Entry Print", "Copies", "How many copies of the Entry Print Document you would like to automatically print when a Delivery Order response is received.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, 1);
			TestRegistryItem(ItemSet.EntryPrintCopyToEDocs, "NZEntryPrintCopyToEDocs", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Entry Print", "Copy to eDocs", "Would you like a copy of the Entry Print Document saved in eDocs when a Delivery Order response is received.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, expectedDefaultValue: true);
			TestRegistryItem(ItemSet.DeliveryOrderPrinter, "NZDeliveryOrderPrinter", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Delivery Order", "Printer", "The Printer you would like copies of the Delivery Order Document to go to when a Delivery Order response is received.", RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue, RegistryFindBoxCollection.StmPrintQueue, Guid.Empty);
			TestRegistryItem(ItemSet.DeliveryOrderCopies, "NZDeliveryOrderCopies", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Delivery Order", "Copies", "How many copies of the Delivery Order Document you would like to automatically print when a Delivery Order response is received.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, 1);
			TestRegistryItem(ItemSet.DeliveryOrderCopyToEDocs, "NZDeliveryOrderCopyToEDocs", "Customs/Country or Region Specific/New Zealand/AutoPrint on Clearance Response/Delivery Order", "Copy to eDocs", "Would you like a copy of the Delivery Order Document saved in eDocs when a Delivery Order response is received.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, expectedDefaultValue: true);
		}

		public void TestBrokerDeferredCutoffDateItems()
		{
			TestRegistryItem(ItemSet.BrokerDeferredCutoffDateDaysBeforeWarning, "DaysBeforeCutoffWarning", "Customs/Country or Region Specific/New Zealand/Broker Deferred Cutoff Date", "Days before Cutoff to start Warning", "Days before Cutoff to start Warning", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, 3);
			TestRegistryItem(ItemSet.BrokerDeferredCutoffDateEnabled, "Enabled", "Customs/Country or Region Specific/New Zealand/Broker Deferred Cutoff Date", "Enabled", "Enable the Broker Deferred Cutoff Date. From 27 October 2020, NZ Customs moved their deferred broker payments to the 20th each month.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedDefaultValue: false);
			TestRegistryItem(ItemSet.BrokerDeferredCutoffDateMinimumDutyWarning, "MimimumDutyForWarning", "Customs/Country or Region Specific/New Zealand/Broker Deferred Cutoff Date", "Minimum Duty/Tax for Warning", "Minimum Duty/Tax for Warning", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, 0);
		}

		public void TestHideDeclarantCodeOnCustomsDocumentation()
		{
			TestRegistryItem(ItemSet.HideDeclarantCodeOnCustomsDocumentation, "NZCustomsHideDeclarantCode", "Customs/Country or Region Specific/New Zealand/Documentation Options", "Hide Declarant Code", "Do you want the Broker's Declarant Code to be hidden on Customs Documentation?", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedDefaultValue: false);
		}

		public void TestExportEntryFeeChargeCode()
		{
			TestRegistryItem(ItemSet.ExportEntryFeeChargeCode, "ExportEntryFeeChargeCode", "AutoRating/Charge Codes/Customs/New Zealand", "Export Entry Fee Charge Code", "This is the charge code that will be used when auto-rating an entry fee for export declarations. If this is not set, then system will not autorate entry fees for export declarations.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory, RegistryFindBoxCollection.AccChargeCode, Guid.Empty);
		}

		public void TestUpdateAttachedManifestedECIsWhenConsolDetailsChange()
		{
			TestRegistryItem(ItemSet.UpdateAttachedManifestedECIsWhenConsolDetailsChange, "NZCustomsUpdateECIManifestsFromConsol", "Customs/Country or Region Specific/New Zealand", "Update Write-off Manifests attached to Consols", @"Do you want to automatically update attached Manifested Write-offs when Consol Details Change?

Please keep in mind that this will make saving Consols slower when there are Manifest Write-offs attached to Shipments on a Consol you are trying to Save.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedDefaultValue: false);
		}

		public void TestDefaultResendingRemarks()
		{
			TestGenericRegistryItem(ItemSet.DefaultResendingRemarks, "NZDefaultResendingRemarks", "Customs/Country or Region Specific/New Zealand", "Default Resending Remarks", "Enter the default remarks for resending Declarations to Customs", RegistryStorageFlags.Company);
			AssertEquals("MaximumLength", 250, ItemSet.DefaultResendingRemarks.DataType.MaximumLength);
		}

		public void TestAllRegistryItemsHaveNZCountryFilter()
		{
			foreach (IRegistryItem registryItem in AllItems)
			{
				Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.NewZealand));
			}
		}

		public void TestDefaultOverseasInsurance()
		{
			TestRegistryItem(ItemSet.DefaultOverseasInsurance, "DefaultOverseasInsurance", "Customs/Country or Region Specific/New Zealand", "Default Insurance Percentage", "Default Insurance as Percentage of Invoice Line Price", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, 0m);
		}

		public void TestMaxNumberOfECIManifestLinesAccepted()
		{
			TestRegistryItem(ItemSet.MaxNumberOfECIManifestLinesAccepted, "MaxNumberOfECIManifestLinesAccepted", "Customs/Country or Region Specific/New Zealand", "Maximum number of entry lines accepted on ICR manifests", @"The Maximum number of entry lines accepted on Import ICR manifests. 
When creating an Air Cargo or Sea Cargo ICR using a Universal Shipment XML that exceeds the amount of Consignments permitted by this setting, the system will create multiple jobs up to this limit.", RegistryStorageFlags.Company, 9999);
		}

		public void TestMaxNumberOfECIManifestLinesAcceptedCRE()
		{
			TestRegistryItem(ItemSet.MaxNumberOfECIManifestLinesAcceptedCRE, "MaxNumberOfECIManifestLinesAcceptedCRE", "Customs/Country or Region Specific/New Zealand", "Maximum number of entry lines accepted on CRE manifests", @"The Maximum number of entry lines accepted on Export CRE manifests. 
When creating an Air Cargo or Sea Cargo CRE using a Universal Shipment XML that exceeds the amount of Consignments permitted by this setting, the system will create multiple jobs up to this limit.", RegistryStorageFlags.Company, 6000);
		}

		#region Registry items converted from deprecated Env.Registry classes

		public void TestLoadedEDITariffReferenceFilesNZDataVersion()
		{
			AssertEquals("LoadedEDITariffReferenceFilesNZDataVersion Default Value", 0, NZCustomsDataRegistry.Instance.LoadedEDITariffReferenceFilesNZDataVersion.Value);
			NZCustomsDataRegistry.Instance.LoadedEDITariffReferenceFilesNZDataVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1201);
			AssertEquals("LoadedEDITariffReferenceFilesNZDataVersion Should now be 1201", 1201, NZCustomsDataRegistry.Instance.LoadedEDITariffReferenceFilesNZDataVersion.Value);
			AssertEquals("LoadedEDITariffReferenceFilesNZDataVersion should be int", typeof(int), NZCustomsDataRegistry.Instance.LoadedEDITariffReferenceFilesNZDataVersion.Value.GetType());
		}

		public void TestNZBrokerageID()
		{
			AssertEquals("NZBrokerageID", "", NZCustomsDataRegistry.Instance.NZBrokerageID.Value);
			AssertEquals("NZBrokerageID should be string", typeof(string), NZCustomsDataRegistry.Instance.NZBrokerageID.Value.GetType());
			AssertType<UniqueBrokerageIDDataType>("The data type of NZ Brokerage ID should be UniqueBrokerageIDDataType.", NZCustomsDataRegistry.Instance.NZBrokerageID.Inner.DataType);
		}

		public void TestImportDeclarations()
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ImportDeclarationsTestMode", false, NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.Value);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ImportDeclarationsTestMode", true, NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.Value);
			AssertEquals("ImportDeclarationsSendErrorsToGroup should be guid", typeof(Guid), NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrorsToGroup.Value.GetType());
			AssertEquals("Default ImportDeclarationsSendErrors", Constants.EmailTo.StaffMemberAndNominatedGroup, NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrors.Value);
		}

		public void TestExportDeclarations()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ExportDeclarationsTestMode", false, NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.Value);
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ExportDeclarationsTestMode", true, NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.Value);
			AssertEquals("ExportDeclarationsSendErrorsToGroup should be guid", typeof(Guid), NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrorsToGroup.Value.GetType());
			AssertEquals("Default ExportDeclarationsSendErrors", Constants.EmailTo.StaffMemberAndNominatedGroup, NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrors.Value);
		}

		public void TestImportEci()
		{
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ImportEciTestMode", false, NZCustomsDataRegistry.Instance.ImportEciTestMode.Value);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ImportEciTestMode", true, NZCustomsDataRegistry.Instance.ImportEciTestMode.Value);
			AssertEquals("ImportEciSendErrorsToGroup should be guid", typeof(Guid), NZCustomsDataRegistry.Instance.ImportEciSendErrorsToGroup.Value.GetType());
			AssertEquals("Default ImportEciSendErrors", Constants.EmailTo.StaffMemberAndNominatedGroup, NZCustomsDataRegistry.Instance.ImportEciSendErrors.Value);
		}

		public void TestExportEci()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ExportEciTestMode", false, NZCustomsDataRegistry.Instance.ExportEciTestMode.Value);
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ExportEciTestMode", true, NZCustomsDataRegistry.Instance.ExportEciTestMode.Value);
			AssertEquals("ExportEciSendErrorsToGroup should be guid", typeof(Guid), NZCustomsDataRegistry.Instance.ExportEciSendErrorsToGroup.Value.GetType());
			AssertEquals("Default ExportEciSendErrors", Constants.EmailTo.StaffMemberAndNominatedGroup, NZCustomsDataRegistry.Instance.ExportEciSendErrors.Value);
		}

		public void TestExportOrn()
		{
			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ExportOrnTestMode", false, NZCustomsDataRegistry.Instance.ExportOrnTestMode.Value);
			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ExportOrnTestMode", true, NZCustomsDataRegistry.Instance.ExportOrnTestMode.Value);
			AssertEquals("ExportOrnSendErrorsToGroup should be guid", typeof(Guid), NZCustomsDataRegistry.Instance.ExportOrnSendErrorsToGroup.Value.GetType());
			AssertEquals("Default ExportOrnSendErrors", Constants.EmailTo.StaffMemberAndNominatedGroup, NZCustomsDataRegistry.Instance.ExportOrnSendErrors.Value);
		}

		public void TestDefaultCustomsProcessingPort()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.NZProcessingPort);
			Assert("Precondition: First element should not be empty", !string.IsNullOrEmpty(list[0].Code));

			NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list[0].Code);
			AssertEquals("DefaultCustomsProcessingPort should be a string", typeof(string), NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.Value.GetType());
			AssertEquals("DefaultCustomsProcessingPort value", list[0].Code, NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.Value);
		}

		#endregion

		public void TestEnableInwardCargoReportManifest()
		{
			TestRegistryItem(ItemSet.EnableInwardCargoReportManifest, "EnableInwardCargoReportManifest", NZCustomsDataRegistry.Categories.Customs_NewZealand, "Enable Inward Cargo Report Manifest", "Enable Inward Cargo Report Manifest.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, expectedDefaultValue: false);
		}

		public void TestEnableNZServiceTaskCheckForSendingMessage()
		{
			TestRegistryItem(
				ItemSet.EnableNZServiceTaskCheckForSendingMessage,
				"EnableNZServiceTaskCheckForSendingMessage",
				NZCustomsDataRegistry.Categories.Customs_NewZealand,
				"Enable NZ Service Task Check For Sending Message",
				"Enable NZ Service Task Check For Sending Message.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: true
			);
		}

		public void TestUseRefDatabaseData()
		{
			TestRegistryItem(
				ItemSet.UseRefDatabaseData,
				"UseRefDatabaseData",
				NZCustomsDataRegistry.Categories.Customs_NewZealand,
				"Use RefDatabase Data",
				"Setting this to True will force CW1 to use the reference data in CW-RefDatabase instead of RefDb_Trf_NZ.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false
			);
		}

		public void TestEnableSeaICRFields()
		{
			TestRegistryItem(
				ItemSet.EnableSeaICRFields,
				"EnableSeaICRFields",
				NZCustomsDataRegistry.Categories.Customs_NewZealand_Testing,
				"Enable new Sea ICR/CRE fields",
				"This will enable the functionality being created in WI00306302.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false
			);
		}

		public void TestEnableUpdatedMessagesubmissions()
		{
			TestRegistryItem(
				ItemSet.EnableUpdatedMessageSubmissions,
				"EnableUpdatedMessageSubmissions",
				NZCustomsDataRegistry.Categories.Customs_NewZealand_Testing,
				"Enable Updated Message Submissions",
				"Enables the latest message submission features that include the ability to manually amend a message before sending.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false
			);
		}

		protected override bool IsCountrySpecificRegistrySet
		{
			get { return true; }
		}
	}
}
