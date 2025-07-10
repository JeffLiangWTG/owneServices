using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUS_TSCAODSCertIndividualList()
		{
			Assert(lookups.US_TSCAODSCertIndividualList.Count == 2);
			Assert(lookups.US_TSCAODSCertIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(lookups.US_TSCAODSCertIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
		}

		public void TestFIRMSList()
		{
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), lookups.FIRMSList.GetType());
		}

		public void TestEntryTypeListDoNotHave26()
		{
			Assert(!lookups.US_EntryTypeList.ContainsCode(EntryTypeList.Codes.WarehouseFTZ));
		}

		public void TestTaxApplyList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CodeDescriptionPairList list = declaration.AddInfoLookups.TaxApplyList;
			// In this order
			AssertEquals(TaxApplyList.Codes.Yes, list[0].Code);
			AssertEquals(TaxApplyList.Codes.No, list[1].Code);
			AssertEquals(TaxApplyList.Codes.Override, list[2].Code);
		}

		public void TestDDTCExemptionCodes()
		{
			var list = lookups.DDTCExemptionCodes;
			var loadListFromCachedValue = Factory.GetCachedValue<ACEDDTCExemptionCodes>();
			AssertEquals("DDTCExemptionCodes", loadListFromCachedValue, list);
			var expectedMsg = "22 CFR 126.4(a)(1) Export, Re-export, Re-Transfer or Temporary Import of defense articles, technical data, or defense services by an agency or employee of U.S. Government when acting in an official capacity; or by persons in a contractual relationship with an agency of U.S. Government to conduct contracted-for activities within the scope of the contractual relationship.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4A1", expectedMsg, list.GetDescriptionFromCode("126.4A1"));
			expectedMsg = "22 CFR 126.4(a)(2) Export, Re-export, Re-Transfer or Temporary Import of defense articles, technical data, or defense services by an agency of U.S. Government for carrying out a cooperative project, program, or other activity in furtherance of an agreement or arrangement.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4A2", expectedMsg, list.GetDescriptionFromCode("126.4A2"));
			expectedMsg = "22 CFR 126.4(a)(3) Export, Re-export, Re-Transfer or Temporary Import of defense articles, technical data, or defense services by an agency of U.S. Government for carrying out any foreign assistance or sales program authorized by law and subject to control by the President by other means.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4A3", expectedMsg, list.GetDescriptionFromCode("126.4A3"));
			expectedMsg = "22 CFR 126.4(a)(4) Export, Re-export, Re-Transfer or Temporary Import of defense articles, technical data, or defense services by agency of U.S. Government for any other security cooperation programs and activities of the Department of Defense authorized by law and subject to control by the President by other means.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4A4", expectedMsg, list.GetDescriptionFromCode("126.4A4"));
			expectedMsg = "22 CFR 126.4(b)(1) Export, Re-export, Re-Transfer or Temporary Import of defense article, technical data, or defense service when made by another person for an agency of the U.S. Government to an agency of the U.S. Government at its request.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4B1", expectedMsg, list.GetDescriptionFromCode("126.4B1"));
			expectedMsg = "22 CFR 126.4(b)(2) Export, Re-export, Re-Transfer or Temporary Import of defense articles, technical data, or defense services when made by another person for an agency of the U.S. Government to an entity other than the U.S. Government at the written direction of the U.S. Government for an activity authorized for that agency in paragraphs (a)(1) through (a)(4) of this section.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4B2", expectedMsg, list.GetDescriptionFromCode("126.4B2"));
			expectedMsg = "22 CFR 126.4(c) For the return to the United States of defense articles, technical data, or defense services exported pursuant to 126.4(a)(1)-(4) or 126.4(b)(1)-(2) and to the U.S. Government.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4C1", expectedMsg, list.GetDescriptionFromCode("126.4C1"));
			expectedMsg = "22 CFR 126.4(c) For the return to the United States of defense articles, technical data, or defense services exported pursuant to 126.4(a)(1)-(4) or 126.4(b)(1)-(2) and to the person who exported the item.";
			AssertEquals("DDTC ExemptionCode Message Changes 126.4C2", expectedMsg, list.GetDescriptionFromCode("126.4C2"));
		}

		public void TestDDTCLicenseTypeCodes()
		{
			AssertEquals("DDTCLicenseTypeCodes", Factory.GetCachedValue<DDTCLicenseTypeCodes>(), lookups.DDTCLicenseTypeCodes);
		}

		public void TestReconOriginalEntries()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			AssertNotNull(invoice.AddInfoLookups.ReconOriginalEntries);
			AssertNotNull(invoiceLine.AddInfoLookups.ReconOriginalEntries);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			AssertNull(invoice.AddInfoLookups.ReconOriginalEntries);
			AssertNull(invoiceLine.AddInfoLookups.ReconOriginalEntries);
		}

		public void TestUS_OGAIndicatorList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(OGAIndicatorList), declaration.AddInfoLookups.US_OGAIndicatorList.GetType());
			AssertEquals(2, declaration.AddInfoLookups.US_OGAIndicatorList.Count);
			Assert(declaration.AddInfoLookups.US_OGAIndicatorList.ContainsCode(OGAIndicatorList.Codes.Declared));
			Assert(declaration.AddInfoLookups.US_OGAIndicatorList.ContainsCode(OGAIndicatorList.Codes.Disclaimed));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(OGAIndicatorList), declaration.AddInfoLookups.US_OGAIndicatorList.GetType());
			AssertEquals(1, declaration.AddInfoLookups.US_OGAIndicatorList.Count);
			Assert(declaration.AddInfoLookups.US_OGAIndicatorList.ContainsCode(OGAIndicatorList.Codes.Declared));
			Assert(!declaration.AddInfoLookups.US_OGAIndicatorList.ContainsCode(OGAIndicatorList.Codes.Disclaimed));
		}

		public void TestUS_OGAIndicatorWithoutDisclaimerList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var list = declaration.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList;
			AssertEquals("Cached", OGAIndicatorList.GetWithoutDisclaim(Factory), list);
			AssertEquals("Should be cached", list, declaration.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);
			AssertEquals(1, list.Count);
			AssertEquals(OGAIndicatorList.Descriptions.Declared, list.GetDescriptionFromCode(OGAIndicatorList.Codes.Declared));
		}

		public void TestUS_CylindricalRectangularList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(CylindricalRectangularList), declaration.AddInfoLookups.US_CylindricalRectangularList.GetType());
		}

		public void TestUS_DDTCUnitOfMeasureList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(DDTCUnitOfMeasureList), declaration.AddInfoLookups.US_DDTCUnitOfMeasureList.GetType());
		}

		public void TestUS_DDTCITARExemptionCodes()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, "126.4B", "22 CFR 126.4 (b)", startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(CodeDescriptionPairList), declaration.AddInfoLookups.US_DDTCITARExemptionCodes.GetType());
			Assert(declaration.AddInfoLookups.US_DDTCITARExemptionCodes.ContainsCode("126.4B"));
		}

		public void TestUS_USMLCategoryCodes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(USMLCategoryCodes), declaration.AddInfoLookups.US_USMLCategoryCodes.GetType());
		}

		public void TestUS_TaxDeferIndicatorList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(TaxDeferIndicatorList), declaration.AddInfoLookups.US_TaxDeferIndicatorList.GetType());
		}

		public void TestEntryTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "64", "TFTEA 5062(C) - TFTEA Distilled spirits, wines, or beer which are unmerchantable or do not conform to sample or specifications", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			CodeDescriptionPairList result = declaration.AddInfoLookups.US_EntryTypeList;
			AssertEquals("ExWarehouse type entry type should be there", true, result.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals("Other type is available", true, result.ContainsCode(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals("Drawback Summary entry types should be removed", false, result.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			result = declaration.AddInfoLookups.US_EntryTypeList;
			AssertEquals("6 Entry Types for Drawback", 6, result.Count);
			AssertEquals("Drawback Summary entry types should be there", true, result.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			result = declaration.AddInfoLookups.US_EntryTypeList;
			AssertEquals("3 entry types for Delivery Certificate for Drawback Purpose", 3, result.Count);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			result = declaration.AddInfoLookups.US_EntryTypeList;
			AssertEquals("2 entry types for ACE Drawback", 2, result.Count);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon; //for recon, normal entry type should be shown
			result = declaration.AddInfoLookups.US_EntryTypeList;
			AssertEquals("ExWarehouse type entry type should stay", true, result.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals("Other type is available", true, result.ContainsCode(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals("Drawback Summary entry types should be removed", false, result.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = false;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			result = declaration.AddInfoLookups.US_EntryTypeList;
			AssertEquals(true, result.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(true, result.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));
		}

		public void TestUS_SelectedRateTypeList()
		{
			AssertEquals(typeof(RateTypeList), lookups.US_SelectedRateTypeList.GetType());
		}

		public void TestUS_InbondType_List()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(InbondCommonTypeList), declaration.AddInfoLookups.US_InbondType_List.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(InbondTypeList), declaration.AddInfoLookups.US_InbondType_List.GetType());
		}

		public void TestUS_ExportCode_List()
		{
			AssertEquals(typeof(ExportInformationCodeList), lookups.US_ExportCode_List.GetType());
		}

		public void TestTransferorsAndTransferees()
		{
			AssertEquals(typeof(OrganisationsFindBoxCollection), lookups.TransferorsAndTransferees.GetType());
		}

		public void TestUS_YesOnlyList()
		{
			AssertEquals(typeof(YesNoDefaultList), lookups.US_YesOnlyList.GetType());
			Assert("Should not contain No", !lookups.US_YesOnlyList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", lookups.US_YesOnlyList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
		}

		public void TestUS_YesNoList()
		{
			AssertEquals(typeof(YesNoDefaultList), lookups.US_YesNoList.GetType());
			Assert("Should contain No", lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !lookups.US_YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
		}

		public void TestUS_YesNoDefaultList()
		{
			AssertEquals(typeof(YesNoDefaultList), lookups.US_YesNoDefaultList.GetType());
		}

		public void TestUS_ADDCVDNonReimbursementList()
		{
			AssertEquals(typeof(ADDCVDNonReimbursementList), lookups.US_ADDCVDNonReimbursementList.GetType());
			Assert("Should contain Declared", lookups.US_ADDCVDNonReimbursementList.ContainsCode(ADDCVDNonReimbursementList.Codes.Declared));
			Assert("Should contain Once-off", lookups.US_ADDCVDNonReimbursementList.ContainsCode(ADDCVDNonReimbursementList.Codes.OnceOff));
		}

		public void TestUS_RelatedOrgList()
		{
			AssertNotNull(lookups.US_RelatedOrgList);
			Assert("Should contain No", lookups.US_RelatedOrgList.ContainsCode("N"));
			Assert("Should contain Yes", lookups.US_RelatedOrgList.ContainsCode("Y"));
		}

		public void TestUS_VehicleIDType_List()
		{
			AssertEquals(typeof(VehicleIDTypeList), lookups.US_VehicleIDType_List.GetType());
		}

		public void TestUS_AESOriginIndicator_List()
		{
			AssertEquals(typeof(AESOriginIndicatorList), lookups.US_AESOriginIndicator_List.GetType());
		}

		public void TestFDAStatusList()
		{
			AssertEquals(typeof(FDAStatusList), lookups.FDAStatusList.GetType());
		}

		public void TestUSStateList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), lookups.USStateList.GetType());
			RefCountry uSCountry = GlbCompany.CurrentCompany.Country;
			AssertEquals("States count", uSCountry.States.Count, lookups.USStateList.Count);
			foreach (RefCountryStates state in uSCountry.States)
			{
				AssertEquals("Must contain code " + state.RW_Code, true, lookups.USStateList.ContainsCode(state.RW_Code));
			}
		}

		public void TestUSStatesForVehiclesList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), lookups.USStatesForVehiclesList.GetType());
			AssertEquals("Must contain code 'US' for diplomatic vehicle use." + "US", true, lookups.USStatesForVehiclesList.ContainsCode("US"));
		}

		public void TestUS_MissingDocumentList()
		{
			AssertEquals(typeof(MissingDocumentList), lookups.US_MissingDocumentList.GetType());
		}

		public void TestProductClaimList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(!invoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.S));
			Assert(!invoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.X));
			Assert(!invoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.V));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert(invoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.S));
			Assert(invoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.X));
			Assert(invoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.V));
			Factory.ClearCachedValue<CodeDescriptionPairList>("ProductClaimListACS");
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			var invoice = reconDeclaration.OriginalEntries[0].Invoice;
			var reconInvoiceLine = invoice.InvoiceLines.AddNew();
			Assert(!reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.C));
			Assert(!reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.F));
			Assert(!reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.G));
			Assert(!reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.H));
			Assert(!reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.M));
			Assert(!reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.S));
			Assert(reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.X));
			Assert(reconInvoiceLine.AddInfoLookups.ProductClaimList.ContainsCode(SecondarySpecProgIndicatorList.Codes.V));
		}

		public void TestZoneStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.Domestic));
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.NonPrivilegedForeign));
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.PrivilegedForeign));
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.ZoneRestricted));
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.Domestic));
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.NonPrivilegedForeign));
			Assert(invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.PrivilegedForeign));
			Assert(!invoiceLine.AddInfoLookups.US_ZoneStatusList.ContainsCode(ZoneStatusList.Codes.ZoneRestricted));
		}

		public void TestUS_OGAIndicatorListWithDisclaimerList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(OGAIndicatorList), declaration.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList.GetType());
			AssertEquals(2, declaration.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList.Count);
			Assert(declaration.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList.ContainsCode(OGAIndicatorList.Codes.Declared));
			Assert(declaration.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList.ContainsCode(OGAIndicatorList.Codes.Disclaimed));
		}

		public void TestAMSDisclaimProgramList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var list = declaration.AddInfoLookups.AMSDisclaimProgramList;
			AssertEquals(1, list.Count);
			AssertEquals(AMSProgramList.Codes.MO8, list[0].Code);
		}

		public void TestPSTDisclaimProgramList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(PSTProductTypeList), declaration.AddInfoLookups.PSTDisclaimProgramList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			addInfoDummy = new AddInfoForTest(classification.CC_AddInfoInfo);
			lookups = new USAddInfoLookups(addInfoDummy);
		}

		CusClassification classification;
		AddInfoForTest addInfoDummy;
		USAddInfoLookups lookups;
	}
}
