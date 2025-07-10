using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	public class EXPJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckJE_OA_ConsigneeAddressCheckJE_OH_ConsigneeForMM()
		{
			var org1 = CreateNewOrg("ORG1", "ORG1 ADDRESS", "MARY1", "AU");
			var org2 = CreateNewOrg("ORG2", "ORG2 ADDRESS", "MARY2", "MM");
			var org3 = CreateNewOrg("ORG3", "ORG3 ADDRESS", "MARY3", "BU");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = new ZDateTime(2019, 09, 21);
			declaration.JE_OH_Consignee = org1.PK;
			declaration.JE_OA_ConsigneeAddress = org1.PK;
			AssertNoWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.JE_OH_Consignee = org2.PK;
			declaration.JE_OA_ConsigneeAddress = org2.MainAddress.PK;
			AssertNoWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.US_DateOfExport = new ZDateTime(2019, 09, 01);
			declaration.Validation.ValidateJE_OH_Consignee();
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertHasWarning(declaration.JE_OA_ConsigneeAddressInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.JE_OH_Consignee = org3.PK;
			declaration.JE_OA_ConsigneeAddress = org3.MainAddress.PK;
			AssertNoWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);

			declaration.US_DateOfExport = new ZDateTime(2019, 09, 21);
			declaration.Validation.ValidateJE_OH_Consignee();
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			AssertHasWarning(declaration.JE_OA_ConsigneeAddressInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
		}

		protected OrgHeader CreateNewOrg(ZString companyName, ZString address, ZString contactName, ZString countryCode)
		{
			return CreateNewOrg(companyName, address, contactName, "", countryCode);
		}
		protected virtual OrgHeader CreateNewOrg(ZString companyName, ZString address, ZString contactName, ZString contactPhone, ZString countryCode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = companyName;
			result.MainAddress.OA_Address1 = address;
			result.MainAddress.OA_RN_NKCountryCode = countryCode;
			result.OH_RL_NKClosestPort = countryCode + "PT";

			var contact = result.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = contactPhone;
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			document.OD_DefaultContact = true;

			return result;
		}

		public void TestCheckOrganisationCountryForMyanmar()
		{
			var org1 = CreateNewOrg("ORG2", "ORG2 ADDRESS", "MARY2", "MM");
			var org2 = CreateNewOrg("ORG3", "ORG3 ADDRESS", "MARY3", "BU");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = new ZDateTime(2019, 09, 01);
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OH_Consignee = org1.PK;
			declaration.JE_OH_Forwarder = org1.PK;

			AssertHasWarning(declaration.JE_OH_ImporterInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertHasWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertHasWarning(declaration.JE_OH_ForwarderInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.US_DateOfExport = new ZDateTime(2019, 09, 21);
			declaration.Validation.ValidateJE_OH_Importer();
			declaration.Validation.ValidateJE_OH_Consignee();
			declaration.Validation.ValidateJE_OH_Forwarder();
			declaration.AddInfoValidation.ValidateUS_RN_NKCountryOfDestination();
			AssertNoWarning(declaration.JE_OH_ImporterInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertNoWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			AssertNoWarning(declaration.JE_OH_ForwarderInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.JE_OH_Importer = org2.PK;
			declaration.JE_OH_Consignee = org2.PK;
			declaration.JE_OH_Forwarder = org2.PK;
			declaration.US_RN_NKCountryOfDestination = USCCountry.Burma;
			AssertHasWarning(declaration.JE_OH_ImporterInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			AssertHasWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			AssertHasWarning(declaration.JE_OH_ForwarderInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);

			declaration.US_DateOfExport = new ZDateTime(2019, 09, 01);
			declaration.Validation.ValidateJE_OH_Importer();
			declaration.Validation.ValidateJE_OH_Consignee();
			declaration.Validation.ValidateJE_OH_Forwarder();
			declaration.AddInfoValidation.ValidateUS_RN_NKCountryOfDestination();
			AssertNoWarning(declaration.JE_OH_ImporterInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			AssertNoWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			AssertNoWarning(declaration.JE_OH_ForwarderInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
		}

		public void TestCheckJE_ExportDate()
		{
			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-6);
			AssertHasWarning(declaration.JE_ExportDateInfo, string.Format(EXPJobDeclarationValidation.PostdepartureReportTimeMessage, "5"));
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-5);
			AssertNoWarning(declaration.JE_ExportDateInfo, string.Format(EXPJobDeclarationValidation.PostdepartureReportTimeMessage, "5"));
			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertHasWarning(declaration.JE_ExportDateInfo, EXPJobDeclarationValidation.PredepartureReportTimeMessage);
			declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoWarning(declaration.JE_ExportDateInfo, EXPJobDeclarationValidation.PredepartureReportTimeMessage);

			declaration.JE_ExportDate = ZDateTime.Empty;
			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			declaration.Validation.ValidateJE_ExportDate();
			AssertNoWarning(declaration.JE_ExportDateInfo, string.Format(EXPJobDeclarationValidation.PostdepartureReportTimeMessage, "5"));
		}

		public void TestCheckJE_MessageSubType()
		{
			declaration.JE_MessageSubType = "FRM";
			AssertNoMessageErrors("JE_MessageSubType is not used for Export", declaration.JE_MessageSubTypeInfo);
		}

		public void TestVoyageFlightNo()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("IsSea", true, declaration.IsSea);
			AssertEquals("IsAir", false, declaration.IsAir);
			declaration.JE_VoyageFlightNo = "";
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_VoyageFlightNo = "V234";
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("IsSea", false, declaration.IsSea);
			AssertEquals("IsAir", true, declaration.IsAir);
			declaration.JE_VoyageFlightNo = "";
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			AssertEquals("IsSea", false, declaration.IsSea);
			AssertEquals("IsAir", false, declaration.IsAir);
			declaration.JE_VoyageFlightNo = "";
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);
		}

		public void TestCheckJE_OH_Supplier()
		{
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;

			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			OrgCusCode cusCode = supplier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "A");
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			cusCode.OK_RN_NKCodeCountry = ZString.Empty;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);
		}

		public void TestCheckJE_OH_Forwarder()
		{
			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_IsForwarder = true;

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			var cusCode = forwarder.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "A");
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var forwarderContact = forwarder.Contacts.AddNew();
			forwarderContact.OC_ContactName = "!";
			forwarderContact.OC_Phone = "02 2342 2342";
			var document = forwarderContact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.ExportAirFreightAgent.ToString();
			document.OD_DefaultContact = true;

			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			forwarderContact.OC_ContactName = "Forwarder Name!";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			forwarderContact.OC_ContactName = "Forwarder Name";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			forwarderContact.OC_Phone = "";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			forwarder.MainAddress.OA_Phone = "02 2342 2342";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			forwarder.CustomsCodes.RemoveAndDeleteAll();
			cusCode = forwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			forwarder.MainAddress.OA_Phone = "02 2342 2342";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			cusCode.OK_RN_NKCodeCountry = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactNameMissing);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderContactPhoneMissing);

			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderRequired);

			forwarder.MainAddress.OA_City = "!";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, AESAddressValidator.CityRequired);

			forwarder.MainAddress.OA_City = "SYDNEY!";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, AESAddressValidator.CityRequired);

			forwarder.MainAddress.OA_City = "";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, AESAddressValidator.CityRequired);

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_CustomsRegNo = "A";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, EXPJobDeclarationValidation.ForwarderEIN_DUNSCodeRequired);
		}

		public void TestJE_RL_NKFinalDestination()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "FINPR";
			declaration.Lookups.FinalDestinations.AdditionalFilter = new ZQuery(RefUNLOCOSchema.RL_Code, "FINPR");
			Assert(declaration.Lookups.FinalDestinations.Contains(unloco));

			declaration.JE_RL_NKFinalDestination = unloco.RL_Code;
			AssertNoMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);

			declaration.JE_RL_NKFinalDestination = "";
			AssertHasMessageError(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered + " a " + declaration.JE_RL_NKFinalDestinationInfo.Description + ".");

			declaration.JE_RL_NKFinalDestination = "~";
			AssertHasMessageError(declaration.JE_RL_NKFinalDestinationInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);

			declaration.JE_RL_NKFinalDestination = unloco.RL_Code;
			AssertNoMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);

			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);

			var validUNLOCO = Factory.New<RefUNLOCO>();
			validUNLOCO.RL_Code = "VALID";
			validUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			var invalidUNLOCO = Factory.New<RefUNLOCO>();
			invalidUNLOCO.RL_Code = "INVAL";
			invalidUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			declaration.JE_RL_NKFinalDestination = validUNLOCO.RL_Code;
			AssertNoMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);
			declaration.JE_RL_NKFinalDestination = invalidUNLOCO.RL_Code;
			AssertNoMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestJE_VesselNameHasCountryOfRegistration()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("IsSea", true, declaration.IsSea);

			RefCountry country = Factory.New<RefCountry>();
			RefVessel vesselWithoutCountryOfRegistration = Factory.New<RefVessel>();
			vesselWithoutCountryOfRegistration.RV_Code = "VesselWithoutRego";
			RefVessel vesselWithCountryOfRegistration = Factory.New<RefVessel>();
			vesselWithCountryOfRegistration.RV_Code = "VesselWithRego";
			vesselWithCountryOfRegistration.RV_RN_NKCountryOfReg = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			declaration.JE_VesselName = "VesselWithoutRego";
			AssertNoMessageErrors(declaration.JE_VesselNameInfo);

			declaration.JE_VesselName = "VesselWithRego";
			AssertNoMessageErrors(declaration.JE_VesselNameInfo);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("IsSea", false, declaration.IsSea);
			declaration.JE_VesselName = "VesselWithoutRego";
			AssertNoMessageErrors(declaration.JE_VesselNameInfo);
		}

		public void TestJE_TotalWeightWhenPHC()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 100;
			declaration.JE_TotalWeightUnit = "KG";

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalWeight = 90;
			AssertHasWarning(declaration.JE_TotalWeightInfo, declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.JE_TotalWeight = 0;
			AssertNoWarning(declaration.JE_TotalWeightInfo, declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
			AssertNoWarning(declaration.JE_TotalWeightInfo, JobDeclaration.Constants.MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC);

			declaration.JE_TotalWeight = 10;
			AssertHasWarning(declaration.JE_TotalWeightInfo, JobDeclaration.Constants.MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC);
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}
	}
}
