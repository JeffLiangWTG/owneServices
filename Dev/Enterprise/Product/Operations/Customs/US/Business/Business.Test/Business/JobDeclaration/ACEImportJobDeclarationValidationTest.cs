using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEImportJobDeclarationValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckJE_ExportDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoMessageError("Export date is not required for ACE Cargo Release", declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestIsImporterOfRecordMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Validation.CheckIOROrgPK(declaration.IOROrgPKInfo);
			AssertHasMessageError(declaration.IOROrgPKInfo, "You have not entered an IOR.");
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stupid EIN";
			declaration.IOROrgPK = org1.PK;
			AssertNoMessageError(declaration.IOROrgPKInfo, "You have not entered an IOR.");
			AssertHasMessageError(declaration.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.IOROrgPK = Guid.Empty;
			AssertNoMessageError(declaration.IOROrgPKInfo, "You have not entered an IOR.");
			declaration.IOROrgPKInfo.ClearAllNotifications();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_TTBInd = "D";
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_PermitNumber = "dd";

			declaration.Validation.CheckIOROrgPK(declaration.IOROrgPKInfo);
			AssertHasMessageError(declaration.IOROrgPKInfo, "You have not entered an IOR.");
			declaration.IOROrgPK = org1.PK;
			declaration.Validation.CheckIOROrgPK(declaration.IOROrgPKInfo);
			AssertHasMessageError(declaration.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);

			org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-98732777", Core.Constants.CountryCodes.UnitedStates);
			declaration.IOROrgPKInfo.ClearAllNotifications();
			declaration.IOROrgPK = org1.PK;
			declaration.Validation.CheckIOROrgPK(declaration.IOROrgPKInfo);
			AssertNoMessageError(declaration.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.Validation.CheckIOROrgPK(declaration.IOROrgPKInfo);
			AssertNoMessageError(declaration.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);
		}

		public void TestCheckJE_VesselName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;

			declaration.US_EnableCRL = true;
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.US_EnableCRL = true;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ACEImportJobDeclarationValidation.VoyageFlightNoRequiredForNonAMS);

			declaration.JE_VoyageFlightNo = "A123";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ACEImportJobDeclarationValidation.VoyageFlightNoRequiredForNonAMS);

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;
			invoiceLine.US_FDAIndicator = "D";
			var fdaline = invoiceLine.ACE_FDALines.AddNew();
			fdaline.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ACEImportJobDeclarationValidation.VoyageFlightNoRequiredForPriorNotice);

			declaration.JE_VoyageFlightNo = "A123";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ACEImportJobDeclarationValidation.VoyageFlightNoRequiredForPriorNotice);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ACEImportJobDeclarationValidation.VoyageFlightNoRequiredForPriorNotice);
		}

		public void TestCheckJE_DateOfArrival()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, ACEImportJobDeclarationValidation.ArrivalDateRequiredForNonAMS);

			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, ACEImportJobDeclarationValidation.ArrivalDateRequiredForNonAMS);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, ACEImportJobDeclarationValidation.ArrivalDateNotRequiredForReWarehouse);

			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, ACEImportJobDeclarationValidation.ArrivalDateNotRequiredForReWarehouse);

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, ACEImportJobDeclarationValidation.ArrivalDateRequiredForEntryType86);

			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, ACEImportJobDeclarationValidation.ArrivalDateRequiredForEntryType86);
		}

		public void TestCheckJE_PrimaryITNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_PrimaryITNumber = "1234567890";
			AssertNoWarningContaining(declaration.JE_PrimaryITNumberInfo, ACEImportJobDeclarationValidation.ITNumberNotRequiredForNonAMSJob);

			declaration.US_NonAMS = true;
			declaration.AddInfoValidation.ValidateUS_ITDate();
			AssertHasWarningContaining(declaration.JE_PrimaryITNumberInfo, ACEImportJobDeclarationValidation.ITNumberNotRequiredForNonAMSJob);
		}

		public void TestCheckDuplicateMasterBillForHandCarried()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "11111111";
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.US_EnableENS = true;
			Factory.Save();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration2.JE_MasterBill = "11111111";
			declaration2.JE_MasterBillIssuerSCAC = "APPL";
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.JE_MasterBillIssuerSCAC = "APLU";
			AssertHasWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration2.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration2.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));
		}

		public void TestCheckDuplicateMasterBillForAutoPedestrianRoad()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "11111111";
			declaration.US_EnableENS = true;

			Factory.Save();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_MasterBill = "11111111";
			AssertHasWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration2.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration2.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration2.Validation.ValidateJE_MasterBill();
			declaration2.Validation.ValidateJE_MasterBill();
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			declaration2.Validation.ValidateJE_MasterBill();
			declaration2.Validation.ValidateJE_MasterBill();
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));

			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration2.Validation.ValidateJE_MasterBill();
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertNoWarningContaining(declaration2.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName));
		}

		public void TestJE_MasterBillForStandAlonePriorNotice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJE_TransportModeForReWarehouse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.JE_PrimaryITNumber = "437449633";
			declaration.JE_TransportMode = ZString.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertHasMessageError(declaration.JE_TransportModeInfo, ACEImportJobDeclarationValidation.TransportRequiredForITNotEmpty);

			declaration.JE_TransportMode = ZString.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoMessageError(declaration.JE_TransportModeInfo, ACEImportJobDeclarationValidation.TransportRequiredForITNotEmpty);
		}

		public void TestUS_JE_MasterBillIssuerSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;

			declaration.JE_MasterBillIssuerSCAC = "";
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError("JE_MasterBillIssuerSCAC can not be empty only applicable to BWB mode", declaration.JE_MasterBillIssuerSCACInfo, CommonImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_MasterBillIssuerSCAC = "UNKN";
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.JE_MasterBillIssuerSCAC = "UNKN";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.JE_MasterBillIssuerSCAC = "ZZZZ";
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.JE_MasterBillIssuerSCAC = "";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			AssertHasMessageError("JE_MasterBillIssuerSCAC can not be empty", declaration.JE_MasterBillIssuerSCACInfo, CommonImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_MasterBillIssuerSCAC = "UNKN";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError("JE_MasterBillIssuerSCAC is valid", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.JE_MasterBillIssuerSCAC = "ZZZZ";
			AssertNoMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.JE_MasterBillIssuerSCAC = "UNKN";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.JE_MasterBillIssuerSCAC = "ZZZZ";
			AssertNoMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.JE_MasterBillIssuerSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
		}

		public void TestValidateUnknownCarrierSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;

			declaration.US_UI_NKCarrierSCAC = "UNKN";
			AssertNoMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "ZZZZ";
			AssertNoMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.US_EnableCRL = true;
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			AssertHasMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "ZZZZ";
			AssertNoMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);

			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;

			declaration.US_UI_NKCarrierSCAC = "UNKN";
			AssertHasMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "ZZZZ";
			AssertNoMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
		}

		public void TestCheckJE_OA_ConsigneeAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(declaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var organization = Factory.New<OrgHeader>();
			declaration.JE_OA_ConsigneeAddress = organization.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			organization.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-98732879");
			declaration.JE_OA_ConsigneeAddress = organization.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(declaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OA_ConsigneeAddress = organization.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining(declaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableENS = false;
			declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(declaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OA_ConsigneeAddress = organization.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining(declaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_OH_FDASubmitter()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(5);

			var declaration = GetMergibleDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_FDAForcePN = true;

			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.JE_OH_FDASubmitterInfo, ACEImportJobDeclarationValidation.SubmitterRequireForPriorNotice);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SUBMIT";
			declaration.JE_OH_FDASubmitter = orgHeader.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_FDASubmitterInfo, ACEImportJobDeclarationValidation.SubmitterRequireForPriorNotice);
		}

		public void TestCheckJE_MessageType_DifferentFWSExporters()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";

			var header = declaration.Invoices.AddNew().InvoiceLines.AddNew().FWSHeaders.AddNew();
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "USLAX";
			header.US_OA_FWSExporterAddress = org.MainAddress.PK;

			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var header2 = invoiceLine2.FWSHeaders.AddNew();
			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "USCHI";
			header2.US_OA_FWSExporterAddress = ZGuid.Empty;

			declaration.Validation.ValidateJE_MessageType();
			AssertNoWarning(declaration.JE_MessageTypeInfo, ValidationConstants.FWS.FWSMultipleExporters);

			header2.US_OA_FWSExporterAddress = org.MainAddress.PK;
			declaration.Validation.ValidateJE_MessageType();
			AssertNoWarning(declaration.JE_MessageTypeInfo, ValidationConstants.FWS.FWSMultipleExporters);

			header2.US_OA_FWSExporterAddress = org2.MainAddress.PK;
			declaration.Validation.ValidateJE_MessageType();
			AssertHasWarning(declaration.JE_MessageTypeInfo, ValidationConstants.FWS.FWSMultipleExporters);
		}

		//CheckIOROrgPK is tested in ACEDeclarationValidationForPSC.TestCheckIOROrgPK

		//CheckIOROrgPK_LowValue is tested in ACEDeclarationValidationForPSC.TestCheckIOROrgPK_LowValue

		JobDeclaration GetMergibleDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			return declaration;
		}
	}
}
