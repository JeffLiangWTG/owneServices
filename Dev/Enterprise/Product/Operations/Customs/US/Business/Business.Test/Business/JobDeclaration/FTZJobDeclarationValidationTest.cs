using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FTZJobDeclarationValidationTest : CommonImportJobDeclarationValidationTest
	{
		public void TestJE_VesselName()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TestTestTest";

			declaration.JE_VesselName = "TestTestTest";
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			var errorText = string.Format(ValidationConstants.Declaration.VesselNameLength, 23);
			AssertNoWarningContaining(declaration.JE_VesselNameInfo, errorText);

			declaration.JE_VesselName = "WELLTHEYCALLHIMYOSEMITESAM";
			AssertHasWarningContaining(declaration.JE_VesselNameInfo, errorText);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.ZoneToZone;
			declaration.Validation.ValidateJE_VesselName();
			AssertHasWarningContaining(declaration.JE_VesselNameInfo, errorText);
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageError(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);
		}

		public void TestIsVesselVoyageNumberForFTZ()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			dec.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;

			dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			dec.JE_VoyageFlightNo = "";
			AssertNoMessageErrorContaining(dec.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			dec.JE_TransportMode = TransportTypeList.Codes.Road;
			dec.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(dec.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);
		}

		public void TestJE_VesselNameOnFDA()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "ADMIRALENGRACHT6666688888888";
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			var fda = invLine.ACE_FDALines.AddNew();
			AssertEquals("FTZ Vesselname length - 23", JobDeclaration.Schema.FTZVesselNameLength, ((FTZJobDeclarationValidation)declaration.Validation).VesselNameLength);
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_IntendedUseDescr = "THIS IS VERY LONG DESC";
			fda.US_FDAForcePN = true;

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000550066";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			fda.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;

			var iFDADetails = (IFDAData)fda;
			AssertEquals("Vessel Name length is 23 on FTZ Job.", "ADMIRALENGRACHT66666888", iFDADetails.AffirmationOfCompliance.FirstOrDefault(x => x.Key == ACE_AffirmationOfComplianceList.Codes.VES).Value);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;

			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_VoyageFlightNo = "121231";
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "AA001456";
			AssertNoWarnings(declaration.JE_VoyageFlightNoInfo);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.StatusChange;
			declaration.JE_VoyageFlightNo = "US1231";
			AssertNoWarnings(declaration.JE_VoyageFlightNoInfo);
		}

		public void TestCheckJE_ExportDate()
		{
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.ZoneToZone;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestFTZCheckDatesOfArrival()
		{
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.StatusChange;
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.Domestic;
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_MessageTypeForEntryFilerCode()
		{
			DeclarationTestHelper.SetEntryFilerCode("");
			var ftzDec = Factory.New<JobDeclaration>();
			ftzDec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals("PreCondition", ZString.Empty, ftzDec.US_EntryFilerCode);
			var messageError = string.Format(CommonImportJobDeclarationValidation.EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated, CommonImportJobDeclarationValidation.WhyNeedEntryFilerCodeFTZ) + ((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location;
			AssertHasError(ftzDec.JE_MessageTypeInfo, messageError);

			ftzDec.JE_MessageType = ZString.Empty;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			ftzDec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDec.Validation.ValidateJE_MessageType();
			AssertNoError(ftzDec.JE_MessageTypeInfo, messageError);
		}

		public void TestCheckJE_ContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertHasMessageError(declaration.JE_ContainerModeInfo, FTZJobDeclarationValidation.NoPackageEnteredWhenContainerModeIsSelected);
			declaration.CusContainers.AddNew();
			AssertHasMessageError(declaration.JE_ContainerModeInfo, FTZJobDeclarationValidation.NoPackageEnteredWhenContainerModeIsSelected);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertNoMessageError(declaration.JE_ContainerModeInfo, FTZJobDeclarationValidation.NoPackageEnteredWhenContainerModeIsSelected);
			declaration.Packages.AddNew();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertNoMessageError(declaration.JE_ContainerModeInfo, FTZJobDeclarationValidation.NoPackageEnteredWhenContainerModeIsSelected);
		}

		public void TestJE_MasterBill()
		{
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MasterBill = "W004ADJ290312001";
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, FTZJobDeclarationValidation.BillNumFormat);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.OverageAdmission;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, FTZJobDeclarationValidation.BillNumFormat);
		}

		public void TestCheckFTZAdmissionNumberWithEmptyZoneidAndControlNumber()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "SD3234@";
			org.CompanyData.OB_IMUsedBondedWhs = true;

			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.Validation.ValidateFTZAdmissionNumber();
			declaration.FTZYear = "17";
			declaration.FTZAdmissionNumber = "1530115|17|00000112";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OH_Importer = org.PK;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration2.Validation.ValidateFTZAdmissionNumber();
			declaration2.FTZZoneID = ZString.Empty;
			declaration.FTZYear = "17";
			declaration2.FTZAdmissionNumber = "|17|";
			declaration2.FTZControlNumber = ZString.Empty;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.Save();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration3.FTZAdmissionNumber = "1530115|17|00000112";
			declaration3.Validation.ValidateAll();
			AssertHasError(declaration3.FTZControlNumberInfo, FTZJobDeclarationValidation.FTZAdmissionNumberAlreadyExists);

			declaration2.FTZZoneID = ZString.Empty;
			declaration2.FTZControlNumber = ZString.Empty;
			declaration3.FTZAdmissionNumber = "|17|";
			declaration3.Validation.ValidateAll();
			AssertNoError(declaration3.FTZControlNumberInfo, FTZJobDeclarationValidation.FTZAdmissionNumberAlreadyExists);
		}

		public void TestCheckFTZAdmissionNumber()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "SD3234@";
			org.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.Validation.ValidateFTZAdmissionNumber();
			declaration.FTZYear = ZString.Empty;
			declaration.FTZAdmissionNumber = "1530115|14|00000112";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declaration2 = newFactory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration2.FTZAdmissionNumber = "1530115|14|00000112";
			declaration2.Validation.ValidateAll();
			AssertHasError(declaration2.FTZControlNumberInfo, FTZJobDeclarationValidation.FTZAdmissionNumberAlreadyExists);

			declaration2.FTZAdmissionNumber = "1530115|14|00000113";
			declaration2.Validation.ValidateAll();
			AssertNoError(declaration2.FTZControlNumberInfo, FTZJobDeclarationValidation.FTZAdmissionNumberAlreadyExists);

			newFactory.Save();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone;
			entry2.CH_WarehouseTransactionStatus = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			AssertNoError(declaration2.FTZAdmissionNumberInfo, FTZJobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			declaration2.FTZAdmissionNumber = "A0000013";
			AssertHasError(declaration2.FTZAdmissionNumberInfo, FTZJobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			declaration2.FTZAdmissionNumber = "";
			AssertHasError(declaration2.FTZAdmissionNumberInfo, FTZJobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			declaration2.FTZAdmissionNumber = "1530115|14|00000113";
			AssertNoError(declaration2.FTZAdmissionNumberInfo, FTZJobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);
		}

		public void TestCheckFTZZoneID()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.Validation.ValidateFTZZoneID();
			AssertHasMessageErrorContaining(declaration.FTZZoneIDInfo, MandatoryValidation.YouHaveNotEntered);

			var errorMsg = "Zone ID must be 3 digits + 3 alpha numeric + 3 alpha numeric or  3 digits + 2 alpha numeric + 2 digits.";
			declaration.FTZZoneID = "12345678";
			AssertNoMessageErrorContaining(declaration.FTZZoneIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "1234567";
			AssertNoMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "123456789";
			AssertNoMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "1A23456";
			AssertHasMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "122B458";
			AssertNoMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "1A2345678";
			AssertHasMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "123ABCDEF";
			AssertNoMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.FTZZoneID = "122B45890";
			AssertNoMessageError(declaration.FTZZoneIDInfo, errorMsg);

			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			declaration.US_F_DirectDelivery = true;
			declaration.FTZZoneID = ZString.Empty;
			declaration.Validation.ValidateFTZZoneID();
			AssertNoMessageErrorContaining(declaration.FTZZoneIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_F_DirectDelivery = false;
			declaration.Validation.ValidateFTZZoneID();
			AssertHasMessageErrorContaining(declaration.FTZZoneIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckYearAndFTZControlNumber()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.Validation.ValidateFTZControlNumber();
			AssertHasMessageErrorContaining(declaration.FTZControlNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.FTZZoneID = "1530100";
			declaration.FTZControlNumber = "1U454";
			AssertNoMessageErrorContaining(declaration.FTZControlNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.FTZControlNumber = "1U4549I0";
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			declaration.US_F_DirectDelivery = true;
			declaration.FTZControlNumber = ZString.Empty;
			declaration.Validation.ValidateFTZControlNumber();
			AssertNoMessageErrorContaining(declaration.FTZControlNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_F_DirectDelivery = false;
			declaration.Validation.ValidateFTZControlNumber();
			AssertHasMessageErrorContaining(declaration.FTZControlNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_RL_NKPortOfArrival()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_RL_NKPortOfArrival = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RL_NKPortOfArrival = "USCHI";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckWHSInvLineFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OP = part.PK;
			invoiceLine.JI_InvoiceQuantity = 1m;
			var whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSInvLineFilter = invoiceLine.PK;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.WHSInvLineFilter = ZGuid.Invalid;
			AssertHasWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_OH_Importer = org2.PK;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			invoiceLine.JI_OP = ZGuid.Empty;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSInvLineFilter = ZGuid.Invalid;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckWHSProductFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			var whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSProductFilter = part.OP_PartNum;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.WHSProductFilter = "#DS#@";
			AssertHasWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_OH_Importer = org2.PK;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSProductFilter = "#DS#@";
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckWHSPackageFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			var whsPack = declaration.WHSPacks.AddNew();
			var whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
			declaration.WHSPackageFilter = whsPack.PK;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.WHSPackageFilter = ZGuid.Invalid;
			AssertHasWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_OH_Importer = org2.PK;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			invoiceLine.JI_OP = ZGuid.Empty;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			whsPack = declaration.WHSPacks.AddNew();
			whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
			declaration.WHSPackageFilter = ZGuid.Invalid;
			AssertHasWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckJE_OH_FDASubmitterForPriorNotice()
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

			declaration.JE_OH_FDASubmitter = ZGuid.Empty;
			AssertHasMessageError(declaration.JE_OH_FDASubmitterInfo, string.Format(ValidationConstants.PriorNotice.Organisation, "FDA Submitter"));

			var submitter = Factory.New<OrgHeader>();
			declaration.JE_OH_FDASubmitter = submitter.PK;
			AssertNoMessageError(declaration.JE_OH_FDASubmitterInfo, string.Format(ValidationConstants.PriorNotice.Organisation, "FDA Submitter"));
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
