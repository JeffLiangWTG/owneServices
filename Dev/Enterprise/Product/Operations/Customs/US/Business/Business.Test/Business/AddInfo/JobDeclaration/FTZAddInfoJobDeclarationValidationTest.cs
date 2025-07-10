using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZAddInfoJobDeclarationValidationTest : CommonImportAddInfoJobDeclarationValidationTest
	{
		public void TestCheckUS_F_PNMode()
		{
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			AssertNoMessageErrorContaining(declaration.US_F_PNModeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_F_PNMode = "~";
			AssertHasMessageErrorContaining(declaration.US_F_PNModeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.O;
			AssertHasMessageErrorContaining(declaration.US_F_PNModeInfo, ValidationConstants.PriorNotice.ACSPriorNoticeTurnedOff);
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			AssertNoMessageErrorContaining(declaration.US_F_PNModeInfo, ValidationConstants.PriorNotice.ACSPriorNoticeTurnedOff);
		}

		public void TestCheckUS_SchDEntry()
		{
			declaration.US_SchDEntry = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "Test Port", startDate, endDate);
			newFactory.Save();

			declaration.US_SchDEntry = "2704";
			Assert(!declaration.US_SchDEntryInfo.HasMessageError(ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Sea)));
		}

		public void TestCheckUS_FDAContactName()
		{
			USCTariff importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			importTariff.UE_OGACodes = "FD4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			var fda = invoiceLine.FDAs.AddNew();
			declaration.US_FDAContactName = ZString.Empty;
			AssertHasMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			declaration.US_FDAContactName = "AAA";
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
		}

		public void TestCheckUS_FDAContactPhoneNo()
		{
			USCTariff importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			importTariff.UE_OGACodes = "FD4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			var fda = invoiceLine.FDAs.AddNew();
			declaration.US_FDAContactPhoneNo = ZString.Empty;
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			declaration.US_FDAContactPhoneNo = "8293845001";
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
		}

		public void TestCheckUS_FDAContactEmail()
		{
			USCTariff importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			importTariff.UE_OGACodes = "FD4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			var fda = invoiceLine.FDAs.AddNew();
			declaration.US_FDAContactEmail = ZString.Empty;
			AssertHasMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FDA.ContactEmail);
			declaration.US_FDAContactEmail = "BBB";
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FDA.ContactEmail);
		}

		public void TestCheckUS_FDAADTA()
		{
			USCTariff importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			importTariff.UE_OGACodes = "FD4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			var fda = invoiceLine.FDAs.AddNew();
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			declaration.US_FDAADTA = ZDateTime.Now;
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
		}

		public void TestCheckUS_SchDArrival()
		{
			AssertCheckUS_SchDArrivalAgainstTransportMode(declaration);
			declaration.US_SchDArrival = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_SchDArrivalInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_UI_NKCarrierSCAC()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertUS_UI_NKCarrierSCACRequired(declaration);
			AssertUS_UI_NKCarrierSCACValidity(declaration);
		}

		public void TestCheckUS_EntryDate()
		{
			declaration.US_EntryDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_EntryDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_F_AdmissionType()
		{
			declaration.US_F_AdmissionType = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_F_AdmissionTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_F_AdmissionType = "R";
			AssertHasMessageErrorContaining(declaration.US_F_AdmissionTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			AssertNoMessageErrorContaining(declaration.US_F_AdmissionTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestFIRMS()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "98-12345612");
			declaration.DeliveryOrPickupCartageCoPK = orgHeader.PK;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			var errorText = string.Format(ValidationConstants.FTZ.DataRequired, "FIRMS Code");
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, errorText);
			declaration.US_US_NKLocationOfGoods = "S002";
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, errorText);
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			errorText = string.Format(ValidationConstants.FTZ.DataRequired, "FIRMS Code");
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, errorText);
			declaration.US_US_NKLocationOfGoods = "S001";
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, errorText);
		}

		public void TestCheckDeliveryOrPickupCartageCoPK()
		{
			declaration.US_F_IncludePTT = true;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.ValidateOrgPK();
			var errorText = string.Format(ValidationConstants.FTZ.DataRequiredWhenPTTIncluded, "Carrier");
			AssertHasMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			declaration.DeliveryOrPickupCartageCoPK = organization.PK;
			AssertNoMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
			declaration.US_F_IncludePTT = false;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.DeliveryOrPickupCartageCoPKInfo, ValidationConstants.FTZ.CarrierWillNotBeSend);
			declaration.DeliveryOrPickupCartageCoPK = ZGuid.Empty;
			AssertNoWarning(declaration.DeliveryOrPickupCartageCoPKInfo, ValidationConstants.FTZ.CarrierWillNotBeSend);
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			errorText = string.Format(ValidationConstants.FTZ.DataRequired, "Carrier");
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
			declaration.DeliveryOrPickupCartageCoPK = organization.PK;
			AssertNoMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
		}

		public void TestCheckIOROrgPK()
		{
			declaration.IOROrgPK = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			string importerMessageError = string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Applicant");
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.IOROrgPKInfo, importerMessageError);
			var importer = Factory.New<OrgHeader>();
			declaration.IOROrgPK = importer.PK;
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(declaration.IOROrgPKInfo, importerMessageError);
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123");
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError("CBP Assigned Number is accepted", declaration.IOROrgPKInfo, importerMessageError);
			importer.CustomsCodes.RemoveAndDeleteAll();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456321");
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError("IOR (Applicant) has EIN", declaration.IOROrgPKInfo, importerMessageError);
		}

		public void TestCheckUS_F_RoutingDetails()
		{
			//test with valid numeric port and invalid alpha numeric filer
			declaration.US_F_RoutingDetails = "1234AA";
			AssertHasMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
			//test with invalid port and invalid alpha numeric filer
			declaration.US_F_RoutingDetails = "1A34AA";
			AssertHasMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
			//test with invalid filer
			declaration.US_F_RoutingDetails = "12341";
			AssertHasMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
			//test with invalid port with special character
			declaration.US_F_RoutingDetails = "~1";
			AssertHasMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
			// test with valid alpha numeric filer and valid numeric office
			declaration.US_F_RoutingDetails = "3311A2C11";
			AssertNoMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
			// test with valid alpha numeric filer no office
			declaration.US_F_RoutingDetails = "1234AA1";
			AssertNoMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
			// test with valid port, valid alpha numeric filer, and valid numeric office
			declaration.US_F_RoutingDetails = "3311ABC11";
			AssertNoMessageError(declaration.US_F_RoutingDetailsInfo, FTZAddInfoJobDeclarationValidation.RoutingDetailsInvalidFormat);
		}

		public void TestCheckUS_SPNIDType()
		{
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.US_SPNIDType = "~";
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.FTZAdmissionNumber = ZString.Empty;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.ControlNumberRequiredForFTZStandAlonePriorNotice);
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.ZoneIDRequiredForFTZStandAlonePriorNotice);
			declaration.FTZAdmissionNumber = "2140000|17|00000001";
			declaration.AddInfoValidation.ValidateUS_SPNIDType();
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.ControlNumberRequiredForFTZStandAlonePriorNotice);
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.ZoneIDRequiredForFTZStandAlonePriorNotice);
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.MasterBillNumberRequiredForBLNStandAlonePriorNotice);
			declaration.JE_MasterBill = "MB1111111111";
			declaration.AddInfoValidation.ValidateUS_SPNIDType();
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.MasterBillNumberRequiredForBLNStandAlonePriorNotice);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
		}

		JobDeclaration declaration;
	}
}
