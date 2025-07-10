using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class CommonImportAddInfoJobDeclarationValidationTest : AddInfoJobDeclarationValidationAbstractTest
	{
		public void TestShouldCheckFDAADTA_PSCTicked()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			line.RefreshInvoiceLinesWithPGAIndicators();
			line.ACE_FDALines.AddNew();
			declaration.US_PSC = false;
			declaration.US_FDAADTA = ZDateTime.Now.AddDays(-30);
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-9);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			declaration.US_PSC = true;
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-11);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
		}

		public void TestRestrictedEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			declaration.IOROrgPK = importer.PK;
			declaration.IORWrapper.RestrictedEntryTypes.AddNew(RestrictedCodeTypeList.Codes.RestrictedEntryType, EntryTypeList.Codes.BargeMovement);
			var message = "is not allowed for Importer of Record. For more details please refer to Importer of Record > Details > Config > US Defaults";
			declaration.US_EntryType = EntryTypeList.Codes.BargeMovement;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
		}

		public void TestCheckUS_IsFeeLikely()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EnableENS = false;
			AssertNoWarning(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeLikelyForSea);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			Assert("Pre-condition", !declaration.IsEntrySummaryValidationMode);
			AssertNoWarning(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeLikelyForSea);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertNoWarning(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeLikelyForSea);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			AssertNoWarning(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeLikelyForSea);
			declaration.US_EnableENS = true;
			Assert("Pre-condition", declaration.IsEntrySummaryValidationMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			AssertHasWarning(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeLikelyForSea);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			AssertNoWarning(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeLikelyForSea);
		}

		public void TestCheckUS_SchDArrivalPortOfUnlading()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code7777 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "7777", "TEST NAME", startDate, endDate);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code7777.PK, attributeNameUnlading.ZXE_Name, "N");
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.US_EnableCRL = true;
			declaration.US_SchDArrival = "7777";

			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError(declaration.US_SchDArrivalInfo, CommonImportAddInfoJobDeclarationValidation.SelectedCodeNotPortOfDischarge);

			var code7778 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "7778", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code7778.PK, attributeNameUnlading.ZXE_Name, "Y");
			newFactory.Save();

			declaration.US_SchDArrival = "7778";
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertNoMessageError(declaration.US_SchDArrivalInfo, CommonImportAddInfoJobDeclarationValidation.SelectedCodeNotPortOfDischarge);
		}

		public void TestCheckUS_FDAAPCForIMP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;
			declaration.US_FDAAPC = "!";
			AssertHasMessageError(declaration.US_FDAAPCInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_FDAAPCForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;
			declaration.US_FDAAPC = "!";
			AssertHasMessageError(declaration.US_FDAAPCInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_FDAContacts_EDS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			declaration.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			AssertBrokerContactDetailsForEDS(declaration);
			invoiceLine.US_FWSInd = "";
			declaration.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
		}

		protected void AssertCheckUS_SchDArrivalAgainstTransportMode(JobDeclaration declaration)
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "###3", "TEST NAME", startDate, endDate);
			newFactory.Save();

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Bill bill = declaration.Bills.AddNew();
			bill.ITAndSplitDetails.AddNew().US_ITNumber = "890342";
			AssertNotEquals("PreCondition", ZString.Empty, declaration.JE_PrimaryITNumber);

			declaration.US_SchDArrival = "###3";
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Sea));
		}

		protected void AssertUS_UI_NKCarrierSCACRequired(JobDeclaration declaration)
		{
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, "10"));
			declaration.US_UI_NKCarrierSCAC = usCarrier.UI_Code;
			AssertNoMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_UI_NKCarrierSCAC = usCarrier.UI_Code;
			AssertNoMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected void AssertUS_UI_NKCarrierSCACValidity(JobDeclaration declaration)
		{
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, "10"));
			declaration.US_UI_NKCarrierSCAC = "~";
			AssertHasMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", declaration.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			declaration.US_UI_NKCarrierSCAC = "~FRT";
			AssertNoMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", declaration.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			AssertHasMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, "Carrier" + IssuerCarrierSCACValidator.InvalidSCAC);
			declaration.US_UI_NKCarrierSCAC = "";
			AssertNoMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, "Carrier" + IssuerCarrierSCACValidator.InvalidSCAC);
			declaration.JE_TransportMode = TransportTypeList.ConvertFromTransportCode(usCarrier.UI_ModeOfTransportation);
			declaration.US_UI_NKCarrierSCAC = usCarrier.UI_Code;
			AssertNoMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, "Carrier" + IssuerCarrierSCACValidator.InvalidSCAC);
		}

		void AssertBrokerContactDetailsForEDS(JobDeclaration declaration)
		{
			declaration.US_FDAContactName = "";
			AssertHasMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			declaration.US_FDAContactName = "John";
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			declaration.US_FDAContactPhoneNo = "";
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			declaration.US_FDAContactPhoneNo = "+1 82 9384";
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			declaration.US_FDAContactPhoneNo = "1829384";
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			declaration.US_FDAContactPhoneNo = "8293845001";
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			declaration.US_FDAContactEmail = "";
			AssertHasMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			declaration.US_FDAContactEmail = "r";
			AssertHasWarning(declaration.US_FDAContactEmailInfo, "Invalid email format");
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			declaration.US_FDAContactEmail = "johan.anderson@tpg.com";
			AssertNoWarning(declaration.US_FDAContactEmailInfo, "Invalid email format");
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
		}
	}
}
