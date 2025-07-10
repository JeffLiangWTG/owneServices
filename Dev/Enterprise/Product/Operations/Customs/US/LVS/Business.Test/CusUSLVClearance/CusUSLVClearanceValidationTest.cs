using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVClearanceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckULH_IORTypeNotInTheList()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_IORType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			AssertNoErrors(clearance.ULH_IORTypeInfo);

			clearance.ULH_IORType = ">_<";
			AssertHasError(clearance.ULH_IORTypeInfo, "Enter a valid Import of Record Type.");
		}

		public void TestCheckULH_IORReferenceAndULH_IORType()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_ContainerMode = "BBK";
			clearance.ULH_IORType = "EIN";
			clearance.ULH_IORReference = "";
			AssertHasMessageErrorContaining(clearance.ULH_IORReferenceInfo, "Registration Number is mandatory when Registration Type has been entered");

			clearance.ULH_IORReference = "AAAAA";
			clearance.ULH_IORType = "";
			AssertHasMessageErrorContaining(clearance.ULH_IORTypeInfo, "Registration Type is mandatory when Registration Number has been entered");

			var pga = clearance.CusUSLVConsignments.AddNew().CusUSLVItems.AddNew().CusUSLVItemPGAs.AddNew();
			pga.ULP_Agency = "EPA";
			pga.ULP_AgencyProgram = GovernmentAgencyProgramCodeList.Codes.ODS;
			pga.ULP_DisclaimReason = "C";
			pga.ULP_Indicator = "C";
			clearance.ULH_IORReference = "";
			clearance.ULH_IORType = "";
			Factory.Save();

			AssertHasMessageErrorContaining(clearance.ULH_IORReferenceInfo, "Mandatory when PGA data exists");
			AssertHasMessageErrorContaining(clearance.ULH_IORTypeInfo, "Mandatory when PGA data exists");
		}

		public void TestCheckULH_ContactName()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_ContactName = "";
			AssertHasMessageErrorContaining(clearance.ULH_ContactNameInfo, "Please enter a Contact Name.");

			clearance.ULH_ContactName = "Bob";
			AssertNoMessageError(clearance.ULH_ContactNameInfo, "Please enter a Contact Name.");
		}

		public void TestCheckULH_ContactPhone()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_ContactPhone = "";
			AssertHasMessageErrorContaining(clearance.ULH_ContactPhoneInfo, "Please enter a Contact Phone.");

			clearance.ULH_ContactPhone = "000";
			AssertNoMessageError(clearance.ULH_ContactPhoneInfo, "Please enter a Contact Phone.");
		}

		public void TestCheckULH_TransportMode()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "AA";
			clearance.ULH_TransportMode = string.Empty;
			AssertHasMessageErrorContaining(clearance.ULH_TransportModeInfo, "You have not entered a Transport Mode.");

			clearance.ULH_TransportMode = "XXX";
			AssertHasError(clearance.ULH_TransportModeInfo, "Enter a valid Transport Mode.");

			clearance.ULH_TransportMode = "AIR";
			AssertNoErrors(clearance.ULH_TransportModeInfo);
		}

		public void TestCheckULH_ContainerMode()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_ContainerMode = string.Empty;
			AssertHasMessageErrorContaining(clearance.ULH_ContainerModeInfo, "You have not entered a Container Mode.");

			clearance.ULH_ContainerMode = "XXX";
			AssertHasError(clearance.ULH_ContainerModeInfo, "Enter a valid Container Mode.");

			clearance.ULH_ContainerMode = "CNT";
			AssertNoErrors(clearance.ULH_ContainerModeInfo);
		}

		public void TestCheckULH_US_NKCentralizedExamSite()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_US_NKCentralizedExamSite = ">_<";
			AssertHasMessageErrorContaining(clearance.ULH_US_NKCentralizedExamSiteInfo, "The code you have selected is not in the list.");

			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "AAB1", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			clearance.ULH_US_NKCentralizedExamSite = "AAB1";
			AssertNoMessageError(clearance.ULH_US_NKCentralizedExamSiteInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckULH_US_NKLocationOfGoods()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_US_NKLocationOfGoods = ">_<";
			AssertHasMessageErrorContaining(clearance.ULH_US_NKLocationOfGoodsInfo, "The code you have selected is not in the list.");

			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "AAB1", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			clearance.ULH_US_NKLocationOfGoods = "AAB1";
			AssertNoMessageError(clearance.ULH_US_NKLocationOfGoodsInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckULH_PortOfLoading()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_PortOfLoading = "12";
			AssertHasMessageErrorContaining(clearance.ULH_PortOfLoadingInfo, "The code you have selected is not in the list.");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "00011", "TORONTO", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			clearance.ULH_PortOfLoading = "00011";
			AssertNoMessageError(clearance.ULH_PortOfLoadingInfo, "The code you have selected is not in the list.");

			var clearance2 = Factory.New<CusUSLVClearance>();
			clearance2.ULH_PortOfLoading = "123";
			clearance2.ULH_RL_NKPortOfLoading = "USCHI";

			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "USCHI";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			match.RY_LocalPortCode = "1111";
			var match2 = loco.RefLocoMaps.AddNew();
			match2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			match2.RY_LocalPortCode = "2222";

			clearance2.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance2.ULH_PortOfLoading = ZString.Empty;
			AssertEquals(2, USScheduleResolver.GetMatchesForSchedule(Schedule.K, clearance2.ULH_RL_NKPortOfLoading, clearance2.ULH_TransportMode, Factory).Count);
			AssertHasMessageError(clearance2.ULH_PortOfLoadingInfo, "Multiple Schedule K port code matches have been found for this UNLoco. Please select the appropriate code from list.");

			var clearance3 = Factory.New<CusUSLVClearance>();
			clearance3.ULH_TransportMode = TransportTypeList.Codes.Air;
			clearance3.ULH_PortOfLoading = "123";
			clearance3.ULH_RL_NKPortOfLoading = "USCHI";
			clearance3.ULH_PortOfLoading = ZString.Empty;
			AssertEquals(2, USScheduleResolver.GetMatchesForSchedule(Schedule.K, clearance3.ULH_RL_NKPortOfLoading, clearance3.ULH_TransportMode, Factory).Count);
			AssertNoMessageError(clearance3.ULH_PortOfLoadingInfo, "Multiple Schedule K port code matches have been found for this UNLoco. Please select the appropriate code from list.");

			loco.RefLocoMaps.Delete(match2);
			var clearance4 = Factory.New<CusUSLVClearance>();
			clearance4.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance4.ULH_PortOfLoading = "123";
			clearance4.ULH_RL_NKPortOfLoading = "USCHI";
			clearance4.ULH_PortOfLoading = ZString.Empty;
			AssertEquals(1, USScheduleResolver.GetMatchesForSchedule(Schedule.K, clearance4.ULH_RL_NKPortOfLoading, clearance4.ULH_TransportMode, Factory).Count);
			AssertNoMessageError(clearance4.ULH_PortOfLoadingInfo, "Multiple Schedule K port code matches have been found for this UNLoco. Please select the appropriate code from list.");
		}

		public void TestCheckULH_PortOfDischarge()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "MAI";
			clearance.ULH_PortOfDischarge = "AA";
			clearance.ULH_PortOfDischarge = string.Empty;
			AssertHasMessageErrorContaining(clearance.ULH_PortOfDischargeInfo, "Discharge is mandatory when Mode of Transport is MAI.");

			clearance.ULH_PortOfDischarge = "12";
			AssertHasMessageErrorContaining(clearance.ULH_PortOfDischargeInfo, "The code you have selected is not in the list.");

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
			Factory.Save();

			clearance.ULH_PortOfDischarge = "0011";
			AssertNoMessageError(clearance.ULH_PortOfDischargeInfo, "The code you have selected is not in the list.");

			var clearance2 = Factory.New<CusUSLVClearance>();
			clearance2.ULH_RL_NKPortOfDischarge = "USBBB";

			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "USBBB";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			match.RY_LocalPortCode = "1111";
			var match2 = loco.RefLocoMaps.AddNew();
			match2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			match2.RY_LocalPortCode = "2222";
			var match3 = loco.RefLocoMaps.AddNew();
			match3.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match3.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			match3.RY_LocalPortCode = "3333";
			var match4 = loco.RefLocoMaps.AddNew();
			match4.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match4.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			match4.RY_LocalPortCode = "4444";

			clearance2.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance2.ULH_PortOfDischarge = "AA";
			clearance2.ULH_RL_NKPortOfDischarge = "USBBB";
			clearance2.ULH_PortOfDischarge = ZString.Empty;
			AssertEquals(2, USScheduleResolver.GetMatchesForSchedule(Schedule.D, clearance2.ULH_RL_NKPortOfDischarge, clearance2.ULH_TransportMode, Factory).Count);
			AssertHasMessageError(clearance2.ULH_PortOfDischargeInfo, "Multiple Schedule D port code matches have been found for this UNLoco. Please select the appropriate code from list.");

			var clearance3 = Factory.New<CusUSLVClearance>();
			clearance3.ULH_PortOfDischarge = "AA";
			clearance3.ULH_RL_NKPortOfDischarge = "USBBB";
			clearance3.ULH_TransportMode = TransportTypeList.Codes.Air;
			clearance3.ULH_PortOfDischarge = ZString.Empty;
			AssertEquals(2, USScheduleResolver.GetMatchesForSchedule(Schedule.D, clearance3.ULH_RL_NKPortOfDischarge, clearance3.ULH_TransportMode, Factory).Count);
			AssertNoMessageError(clearance3.ULH_PortOfDischargeInfo, "Multiple Schedule D port code matches have been found for this UNLoco. Please select the appropriate code from list.");

			loco.RefLocoMaps.Delete(match2);
			var clearance4 = Factory.New<CusUSLVClearance>();
			clearance4.ULH_PortOfDischarge = "AA";
			clearance4.ULH_RL_NKPortOfDischarge = "USBBB";
			clearance4.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance4.ULH_PortOfDischarge = ZString.Empty;
			AssertEquals(1, USScheduleResolver.GetMatchesForSchedule(Schedule.D, clearance4.ULH_RL_NKPortOfDischarge, clearance4.ULH_TransportMode, Factory).Count);
			AssertNoMessageError(clearance4.ULH_PortOfDischargeInfo, "Multiple Schedule D port code matches have been found for this UNLoco. Please select the appropriate code from list.");
		}

		public void TestCheckULH_DischargeDate()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.Validation.ValidateULH_DischargeDate();
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(clearance.ULH_DischargeDateInfo.Description);

			clearance.ULH_DischargeDate = ZDate.Empty;
			AssertHasMessageError(clearance.ULH_DischargeDateInfo, messageError);

			clearance.ULH_DischargeDate = ZDate.Today;
			AssertNoMessageError(clearance.ULH_DischargeDateInfo, messageError);
		}

		public void TestCheckULH_PortOfEntry_Mandatory()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.RunPreSaveValidation();
			AssertHasMessageError(clearance.ULH_PortOfEntryInfo, MandatoryValidation.YouHaveNotEnteredMessage(clearance.ULH_PortOfEntryInfo.Description));

			clearance.ULH_PortOfEntry = "0011";
			AssertNoErrors(clearance.ULH_PortOfEntryInfo);
		}

		public void TestCheckULH_PortOfEntry_ValidCode()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_PortOfEntry = "11";
			AssertHasMessageErrorContaining(clearance.ULH_PortOfEntryInfo, "The code you have selected is not in the list.");

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
			Factory.Save();

			clearance.ULH_PortOfEntry = "0011";
			AssertNoMessageError(clearance.ULH_PortOfEntryInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckULH_PortOfEntry_PreparerDistrictPort()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0012", "US22 Port of Discharge", startDate, endDate);
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_RemoteLocationFiling = false;
			clearance.ULH_PreparerDistrictPort = "0011";
			clearance.ULH_PortOfEntry = "0011";

			AssertNoWarnings(clearance.ULH_PortOfEntryInfo);

			clearance.ULH_RemoteLocationFiling = true;
			clearance.ULH_PreparerDistrictPort = "0022";
			clearance.ULH_PortOfEntry = "0022";

			AssertHasWarningContaining(clearance.ULH_PortOfEntryInfo, "Port of Entry and Preparer Port should normally differ on an RLF entry");

			clearance.ULH_PortOfEntry = "0011";
			AssertNoWarnings(clearance.ULH_PortOfEntryInfo);
		}

		public void TestCheckULH_MasterBillIssuerSCAC()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "AIR";

			var messageError = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 characters in length for Air and 4 characters in length for other modes of transport.";

			clearance.ULH_MasterBillIssuerSCAC = "AA12";
			AssertHasMessageError("Issuer SCAC should be 2 char long for AIR", clearance.ULH_MasterBillIssuerSCACInfo, messageError);

			clearance.ULH_MasterBillIssuerSCAC = "AA";
			AssertNoMessageErrors("Issuer SCAC should be 2 char long for AIR", clearance.ULH_MasterBillIssuerSCACInfo);

			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_MasterBillIssuerSCAC = string.Empty;
			clearance.ULH_MasterBillIssuerSCAC = "AA";
			AssertHasMessageError("Issuer SCAC should be 4 char long for other than AIR", clearance.ULH_MasterBillIssuerSCACInfo, messageError);

			clearance.ULH_MasterBillIssuerSCAC = "AA12";
			AssertNoMessageErrorContaining("Issuer SCAC should be 4 char long for other than AIR", clearance.ULH_MasterBillIssuerSCACInfo, messageError);
		}

		public void TestMasterBillIssuerSCAC_WhenTransportModeIsAirSeaRailOrTruck_IsRequired()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var messageError = "Standard Carrier Alpha Code (SCAC) required for Master Bill (when Mode of Transport is Sea, Rail, Air, or Truck.)";

			clearance.ULH_TransportMode = "AIR";
			clearance.ULH_MasterBillIssuerSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, messageError);

			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_MasterBillIssuerSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, messageError);

			clearance.ULH_TransportMode = "RAI";
			clearance.ULH_MasterBillIssuerSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, messageError);

			clearance.ULH_TransportMode = "TRK";
			clearance.ULH_MasterBillIssuerSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, messageError);
		}

		public void TestMasterBillIssuerSCAC_WhenSCACIsInValidFormatAndNotInDatabase_HasErrorMessage()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var messageError = $"Issuer Standard Carrier Alpha Code (SCAC) unable to be found.When the Declaration is saved, {BrandingFactory.Instance.ProductName} will automatically submit a request for the latest information relating to the Issuer SCAC that has been entered. A response should be available in a few minutes. Note that SCAC Requests can also be sent manually, at any time, by selecting Customs Declarations > Actions > Reference Files Request > Carrier Codes.";

			var sCACNotInDBForAir = "XX";
			clearance.ULH_TransportMode = "AIR";
			clearance.ULH_MasterBillIssuerSCAC = sCACNotInDBForAir;
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, messageError);

			var sCACNotInDBForOthers = "XX12";
			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_CarrierSCAC = string.Empty;
			clearance.ULH_MasterBillIssuerSCAC = sCACNotInDBForOthers;
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, messageError);
		}

		public void TestMasterBillIssuerSCAC_WhenSCACIsUNKN_HasErrorMessage()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_MasterBillIssuerSCAC = "UNKN";
			AssertHasMessageError(clearance.ULH_MasterBillIssuerSCACInfo, "SCAC Code \"UNKN\" cannot be used for ACE Cargo Release. Use a valid carrier or \"ZZZZ\" if unknown.");
		}

		public void TestCheckULH_MasterBill()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "";

			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			clearance.Validation.ValidateULH_MasterBill();
			AssertHasMessageError("Master bill is mandatory for all transport modes", clearance.ULH_MasterBillInfo, "You have not entered a Master Bill.");

			clearance.ULH_TransportMode = TransportTypeList.Codes.Road;
			clearance.Validation.ValidateULH_MasterBill();
			AssertHasMessageError("Master bill is mandatory for all transport modes", clearance.ULH_MasterBillInfo, "You have not entered a Master Bill.");

			clearance.ULH_TransportMode = TransportTypeList.Codes.Rail;
			clearance.Validation.ValidateULH_MasterBill();
			AssertHasMessageError("Master bill is mandatory for all transport modes", clearance.ULH_MasterBillInfo, "You have not entered a Master Bill.");

			clearance.ULH_TransportMode = TransportTypeList.Codes.Mail;
			clearance.Validation.ValidateULH_MasterBill();
			AssertHasMessageError("Master bill is mandatory for all transport modes", clearance.ULH_MasterBillInfo, "You have not entered a Master Bill.");

			clearance.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance.Validation.ValidateULH_MasterBill();
			AssertHasMessageError("Master bill is mandatory for all transport modes", clearance.ULH_MasterBillInfo, "You have not entered a Master Bill.");

			clearance.ULH_TransportMode = TransportTypeList.Codes.Air;
			clearance.Validation.ValidateULH_MasterBill();
			AssertHasMessageError("Master bill is mandatory for all transport modes", clearance.ULH_MasterBillInfo, "You have not entered a Master Bill.");

			clearance.ULH_MasterBill = "12345678901";
			AssertHasWarningContaining(clearance.ULH_MasterBillInfo, "Invalid check digit. The last digit should be");

			clearance.ULH_MasterBill = "1234567890A";
			AssertHasWarningContaining(clearance.ULH_MasterBillInfo, "The MAWB must contain numeric characters in positions 4-11.");

			clearance.ULH_MasterBill = "123456789012";
			AssertHasWarningContaining(clearance.ULH_MasterBillInfo, "Check the MAWB format. The first 3 characters should be the Airline Code, followed by 8 numeric characters (including check digit).");

			clearance.ULH_MasterBill = "12212121211";
			AssertNoWarnings(clearance.ULH_MasterBillInfo);

			clearance.ULH_TransportMode = "SEA";

			clearance.ULH_MasterBill = "@#$%";
			AssertHasWarningContaining(clearance.ULH_MasterBillInfo, CusUSLVClearanceValidation.OceanBillInvalidCharacters);

			clearance.ULH_MasterBill = "1234567890123";
			AssertHasWarningContaining(clearance.ULH_MasterBillInfo, string.Format(CusUSLVClearanceValidation.OceanBillTooLong, 12));

			clearance.ULH_MasterBill = "123456789";
			AssertNoWarnings("No check digit validation for other than Air", clearance.ULH_MasterBillInfo);
		}

		public void TestCheckULH_EntryDate()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_EntryDate = ZDate.Today.AddDays(-91);
			AssertHasMessageError(clearance.ULH_EntryDateInfo, "The Arrival Date is older than 90 days and no GO Number is entered. Please Verify");

			clearance.ULH_EntryDate = ZDate.Today;
			AssertNoMessageErrors(clearance.ULH_EntryDateInfo);

			clearance.ULH_EntryDate = ZDate.Today.AddDays(61);
			AssertHasMessageError(clearance.ULH_EntryDateInfo, "If the Arrival Date is more than 60 days in the future, measured from the submission date, Cargo Certification is not permitted");
		}

		public void TestCheckULH_DepartureDate()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var item = clearance.CusUSLVConsignments.AddNew().CusUSLVItems.AddNew();
			item.ULI_GoodsValue = 100m;
			item.ULI_RX_NKCurrency = "AUD";

			clearance.ULH_DepartureDate = ZDate.Empty;
			AssertHasMessageError("If there is a non US value in a house bill, we need the Departure Date to determine the exchange rate", clearance.ULH_DepartureDateInfo, "Departure Date must be entered where an exchange rate is required.");

			clearance.ULH_DepartureDate = ZDateTime.BrettsBirthday.Date;
			AssertNoMessageErrors(clearance.ULH_DepartureDateInfo);
		}

		public void TestCheckULH_EntryFilerCode()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_EntryFilerCode = ZString.Empty;
			AssertHasErrorContaining(clearance.ULH_EntryFilerCodeInfo, "An entry filer code has not been set up for this branch or company. Please set up one in Admin->System->Registry");

			clearance.ULH_EntryFilerCode = "SV9";
			AssertNoError(clearance.ULH_EntryFilerCodeInfo, "An entry filer code has not been set up for this branch or company. Please set up one in Admin->System->Registry");
		}

		public void TestCheckULH_VoyageFlightNo()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance.ULH_VoyageFlightNo = "123456789";
			AssertHasWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncated);

			clearance.ULH_VoyageFlightNo = "12345";
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncated);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Air;
			clearance.ULH_VoyageFlightNo = "AA001456";
			AssertHasWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncatedAir);

			clearance.ULH_VoyageFlightNo = "AA0016";
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncatedAir);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			clearance.ULH_VoyageFlightNo = "123456789";
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncated);
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncatedAir);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Mail;
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncated);
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncatedAir);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Rail;
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncated);
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncatedAir);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Road;
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncated);
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoTruncatedAir);
		}

		public void TestCheckULH_VoyageFlightNoFormatForAir()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportTypeList.Codes.Air;
			clearance.ULH_VoyageFlightNo = "123456789";

			AssertHasMessageError(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoInvalidFormat);
			AssertHasWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.AirlineCodeNotRecognized);

			clearance.ULH_VoyageFlightNo = "QF001";

			AssertNoMessageError(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.VoyageFlightNoInvalidFormat);
			AssertNoWarningContaining(clearance.ULH_VoyageFlightNoInfo, CusUSLVClearanceValidation.AirlineCodeNotRecognized);
		}

		public void TestCheckULH_PreparerDistrictPort_MandatoryIfRemoteLocationFiling()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_RemoteLocationFiling = false;
			clearance.ULH_PreparerDistrictPort = string.Empty;
			AssertNoErrors(clearance.ULH_PreparerDistrictPortInfo);

			clearance.ULH_RemoteLocationFiling = true;
			AssertHasMessageError(clearance.ULH_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEnteredMessage(clearance.ULH_PreparerDistrictPortInfo.Description));

			clearance.ULH_PreparerDistrictPort = "0011";
			AssertNoErrors(clearance.ULH_PreparerDistrictPortInfo);
		}

		public void TestCheckULH_PreparerDistrictPort_ValidCode()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_PreparerDistrictPort = "11";

			AssertListValidationInvalidCodeMessageError(clearance.ULH_PreparerDistrictPortInfo, isExpectingMessageError: true);

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
			Factory.Save();

			clearance.ULH_PreparerDistrictPort = "0011";
			AssertListValidationInvalidCodeMessageError(clearance.ULH_PreparerDistrictPortInfo, isExpectingMessageError: false);
		}

		public void TestCheckULH_GB_Mandatory()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_GB = ZGuid.Empty;
			clearance.RunPreSaveValidation();
			AssertMandatoryValidationError(clearance.ULH_GBInfo, isExpectingError: true);

			clearance.ULH_GB = Env.CurrentBranch.PK;
			clearance.RunPreSaveValidation();
			AssertMandatoryValidationError(clearance.ULH_GBInfo, isExpectingError: false);
		}

		public void TestCheckULH_OH_Importer()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			clearance.ULH_OH_Importer = ZGuid.Empty;
			clearance.RunPreSaveValidation();
			AssertNoMessageError("No message error when no consignment entry type is 11", clearance.ULH_OH_ImporterInfo, MandatoryValidation.YouHaveNotEnteredMessage(clearance.ULH_OH_ImporterInfo.Description));

			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			clearance.RunPreSaveValidation();
			AssertHasMessageError("Add message error when any consignment entry type is 11", clearance.ULH_OH_ImporterInfo, MandatoryValidation.YouHaveNotEnteredMessage(clearance.ULH_OH_ImporterInfo.Description));

			var org = Factory.New<OrgHeader>();
			clearance.ULH_OH_Importer = org.PK;

			consignment.ULB_EntryType = EntryTypeList.Codes.LowValue;
			clearance.RunPreSaveValidation();
			AssertNoMessageErrors(clearance.ULH_OH_ImporterInfo);

			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			clearance.RunPreSaveValidation();
			AssertHasMessageError(clearance.ULH_OH_ImporterInfo, "The Importer of Record should have EIN, SSN or CBP assigned number.");
			AssertHasWarning(clearance.ULH_OH_ImporterInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
		}

		public void TestULH_CarrierSCAC_WhenWithCertainCalculatedModeOfTransport_IsMandatory()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Air;
			clearance.ULH_ContainerMode = Core.Constants.ContainerModes.Containerised;

			clearance.ULH_CarrierSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageErrorContaining(clearance.ULH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage("Carrier SCAC"));

			clearance.ULH_TransportMode = Core.Constants.TransportModes.Rail;
			clearance.ULH_ContainerMode = Core.Constants.ContainerModes.Containerised;

			clearance.ULH_CarrierSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageErrorContaining(clearance.ULH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage("Carrier SCAC"));

			clearance.ULH_TransportMode = Core.Constants.TransportModes.Sea;
			clearance.ULH_ContainerMode = Core.Constants.ContainerModes.Containerised;

			clearance.ULH_CarrierSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertHasMessageErrorContaining(clearance.ULH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage("Carrier SCAC"));

			clearance.ULH_TransportMode = Core.Constants.TransportModes.Road;
			clearance.ULH_ContainerMode = Core.Constants.ContainerModes.Containerised;

			clearance.ULH_CarrierSCAC = string.Empty;
			clearance.RunPreSaveValidation();
			AssertNoMessageError(clearance.ULH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage("Carrier SCAC"));
		}

		public void TestULH_CarrierSCAC_WhenSCACIsInValidFormatAndNotInDatabase_HasErrorMessage()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var messageError = $"Carrier Standard Carrier Alpha Code (SCAC) unable to be found.When the Declaration is saved, {BrandingFactory.Instance.ProductName} will automatically submit a request for the latest information relating to the Carrier SCAC that has been entered. A response should be available in a few minutes. Note that SCAC Requests can also be sent manually, at any time, by selecting Customs Declarations > Actions > Reference Files Request > Carrier Codes.";

			var sCACNotInDBForAir = "XX";
			clearance.ULH_TransportMode = "AIR";
			clearance.ULH_CarrierSCAC = sCACNotInDBForAir;
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			var sCACNotInDBForOthers = "XX12";
			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_CarrierSCAC = string.Empty;
			clearance.ULH_CarrierSCAC = sCACNotInDBForOthers;
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);
		}

		public void TestULH_CarrierSCAC_WhenSCACIsNot2CharLongForAir_HasErrorMessage()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "AIR";

			var messageError = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 characters in length for Air and 4 characters in length for other modes of transport.";

			clearance.ULH_CarrierSCAC = "AA12";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			clearance.ULH_CarrierSCAC = "A";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);
		}

		public void TestULH_CarrierSCAC_WhenSCACIsNot4CharLongForOtherTransportModes_HasErrorMessage()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var messageError = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 characters in length for Air and 4 characters in length for other modes of transport.";

			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_CarrierSCAC = "AA1";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			clearance.ULH_CarrierSCAC = "A";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			clearance.ULH_TransportMode = "RAI";
			clearance.ULH_CarrierSCAC = "AA1";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			clearance.ULH_CarrierSCAC = "A";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			clearance.ULH_TransportMode = "TRK";
			clearance.ULH_CarrierSCAC = "AA1";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);

			clearance.ULH_CarrierSCAC = "A";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, messageError);
		}

		public void TestULH_CarrierSCAC_WhenSCACIsUNKN_HasErrorMessage()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_CarrierSCAC = "UNKN";
			AssertHasMessageError(clearance.ULH_CarrierSCACInfo, "SCAC Code \"UNKN\" cannot be used for ACE Cargo Release. Use a valid carrier or \"ZZZZ\" if unknown.");
		}
	}
}
