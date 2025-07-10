using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo_CPT()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TerminalControlledPremisesID;
			cusCode.OK_CustomsRegNo = "1234567890123456!";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "A valid Customs Controlled Premises Code – Terminal is composed of between 1 and 17 alpha numeric characters.");
			cusCode.OK_CustomsRegNo = "1234567A";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "123456A";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNo_CPD()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
			cusCode.OK_CustomsRegNo = "1234567890123456!";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "A valid Customs Controlled Premises Code – Depot is composed of between 1 and 17 alpha numeric characters.");
			cusCode.OK_CustomsRegNo = "1234567A";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "123456A";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNo_BGV()
		{
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.BGV;
			cusCode.OK_CustomsRegNo = "ABC123";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "Bond Guarantee Amount must be numeric.");
			cusCode.OK_CustomsRegNo = "-1";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "Bond Guarantee Value cannot be less than zero.");
			cusCode.OK_CustomsRegNo = "0";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "1111";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CodeType()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			var cusCodeREM = organisation.CustomsCodes.AddNew(ZString.Empty, "REM", Core.Constants.CountryCodes.SouthAfrica);
			var cusCodeBHR = organisation.CustomsCodes.AddNew(ZString.Empty, "REM", Core.Constants.CountryCodes.SouthAfrica);
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.BGV;
			cusCode.Validation.ValidateOK_CodeType();
			AssertHasErrorContaining(cusCode.OK_CodeTypeInfo, "Bond Guarantee Value only applicable if a Bond Holder (BHR) or Remover (REM) Code has been captured.");
			cusCodeREM.OK_CodeType = ZString.Empty;
			cusCodeBHR.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.BGV;
			cusCode.Validation.ValidateOK_CodeType();
			AssertNoErrorContaining(cusCode.OK_CodeTypeInfo, "Bond Guarantee Value only applicable if a Bond Holder (BHR) or Remover (REM) Code has been captured.");
			cusCodeREM.OK_CodeType = OrgCusCode.CodeTypes.BondHolderCode;
			cusCodeBHR.OK_CodeType = ZString.Empty;
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.BGV;
			cusCode.Validation.ValidateOK_CodeType();
			AssertNoErrorContaining(cusCode.OK_CodeTypeInfo, "Bond Guarantee Value only applicable if a Bond Holder (BHR) or Remover (REM) Code has been captured.");
			cusCodeREM.OK_CodeType = OrgCusCode.CodeTypes.BondHolderCode;
			cusCodeBHR.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.BGV;
			cusCode.Validation.ValidateOK_CodeType();
			AssertNoErrorContaining(cusCode.OK_CodeTypeInfo, "Bond Guarantee Value only applicable if a Bond Holder (BHR) or Remover (REM) Code has been captured.");
		}

		public void TestCheckOK_CustomsRegNo_ShouldBeEntered()
		{
			cusCode.OK_CustomsRegNo = "";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
			cusCode.OK_CustomsRegNo = "blabla";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
		}

		public void TestCheckOK_CustomsRegNo_CCD()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234567A";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			cusCode.OK_CustomsRegNo = "12345678";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			TestIncorrectCheckDigit(OrgCusCode.CodeTypes.CustomsClientCode);
		}

		public void TestCheckOK_CustomsRegNo_CSC()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234567A";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			cusCode.OK_CustomsRegNo = "12345678";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			TestIncorrectCheckDigit(OrgCusCode.CodeTypes.SupplierCode);
		}

		void TestIncorrectCheckDigit(ZString customsRegNo)
		{
			var factory = new BusinessObjectFactory();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTOR2";
			org2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var cusCode2 = org2.CustomsCodes.AddNew(customsRegNo, "10000096", Core.Constants.CountryCodes.SouthAfrica);
			cusCode2.Validation.ValidateOK_CodeType();
			org2.MainAddress.OA_RN_NKCountryCode = "ZA";
			CombineAssertions(() =>
			{
				AssertNoMessageErrors(cusCode2.OK_CustomsRegNoInfo);
				cusCode2.OK_CustomsRegNo = "10000097";
				AssertHasMessageError("10000097", cusCode2.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectCheckDigit("6"));
				cusCode2.OK_CustomsRegNo = "10000096";
				AssertNoNotifications(cusCode2.OK_CustomsRegNoInfo);
				cusCode2.OK_CustomsRegNo = "10000245";
				AssertHasMessageError("10000245", cusCode2.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectCheckDigit("7"));
				cusCode2.OK_CustomsRegNo = "10000247";
				AssertNoNotifications(cusCode2.OK_CustomsRegNoInfo);
				cusCode2.OK_CustomsRegNo = "92000102";
				AssertHasMessageError("92000102", cusCode2.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectCheckDigit("0"));
				cusCode2.OK_CustomsRegNo = "93102006";
				AssertHasMessageError("93102006", cusCode2.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectCheckDigit("1"));
				cusCode2.OK_CustomsRegNo = "10000029";
				AssertNoNotifications(cusCode2.OK_CustomsRegNoInfo);
			});
		}

		public void TestCheckOK_CustomsRegNo_VAT()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "4770181941";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", false, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			cusCode.OK_CustomsRegNo = "4A70181941";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "The VAT must be 10 digits only");
			cusCode.OK_CustomsRegNo = "5770181941";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "The VAT must start with a '4'");
			cusCode.OK_CustomsRegNo = "4770181942";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "The VAT is invalid");
			cusCode.OK_CustomsRegNo = "477018194";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "The VAT must be 10 digits long");
			cusCode.OK_CustomsRegNo = "NA";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", false, cusCode.OK_CustomsRegNoInfo.HasNotifications());
		}

		public void TestCheckOK_CustomsRegNo_AGT()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.AgentCode;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234567A";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZAAgentCodeFormatError);
			cusCode.OK_CustomsRegNo = "12345678";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
			var factoryForDuplicate = new BusinessObjectFactory();
			var org = factoryForDuplicate.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTORG";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "11112222", Core.Constants.CountryCodes.SouthAfrica);
			factoryForDuplicate.Save();
			var org2 = factoryForDuplicate.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTOR2";
			var cusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "11113333", Core.Constants.CountryCodes.SouthAfrica);
			cusCode2.Validation.ValidateOK_CodeType();
			AssertNoWarningContaining(cusCode2.OK_CustomsRegNoInfo, "can be found on the following Organization: TSTORG");
			cusCode2.OK_CustomsRegNo = "11112222";
			cusCode2.Validation.ValidateOK_CodeType();
			AssertHasWarningContaining(cusCode2.OK_CustomsRegNoInfo, "can be found on the following Organization: TSTORG");
		}

		public void TestCheckOK_CustomsRegNo_APE()
		{
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter;
			cusCode.OK_CustomsRegNo = "1234!678";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ZACustomsApprovedExporterCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234567A";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "123456A";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNo_IDO()
		{
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.IDNumber;
			cusCode.OK_CustomsRegNo = "1234567890128";
			AssertEquals("There is no error in OK_CustomsRegNoInfo.", false, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			cusCode.OK_CustomsRegNo = "1234567890123";
			AssertEquals("There is an error in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The ID Number does not have a valid check-digit. The check-digit should be 8.");
			cusCode.OK_CustomsRegNo = "123456789012a";
			AssertEquals("There is an error in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The ID Number must be 12 digits and a 13th check-digit.");
			cusCode.OK_CustomsRegNo = "123456789012";
			AssertEquals("There is an error in OK_CustomsRegNoInfo.", true, cusCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The ID Number must be 12 digits and a 13th check-digit.");
		}

		public void TestCheckOK_CustomsRegNo_VGM()
		{
			const string VGMInvalidFormatMessage = "Approved Method 2 weighing parties for this country/region do not have an approval number. Enter \"Y\" or \"Yes\" to show this company’s address has an approved status.";
			const string noPremisesAddressMessage = "An address is required for code type 'VGM'.";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			cusCode.OK_RN_NKCodeCountry = "ZA";
			cusCode.OK_CustomsRegNo = "1234";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, VGMInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "Y";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "Yes";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "yes";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, noPremisesAddressMessage);
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestCheckOK_CustomsRegNo_CPW()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "The length of Customs Controlled Premises Code – Warehouse must be 11");
			cusCode.OK_CustomsRegNo = "1DBN4567890";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "Characters 1-3 of Customs Controlled Premises Code – Warehouse must be a valid customs office code.");
			cusCode.OK_CustomsRegNo = "DBN34567890";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "Characters 4-6 of Customs Controlled Premises Code – Warehouse can only be one of the following 'OS', 'SOS', 'VM', 'SVM'，'AM', 'VS', 'VMP', 'VMS', 'SWE'.Note that for ‘VM’, ‘OS’ & ‘VS’ the 3rd character must be a space.");
			cusCode.OK_CustomsRegNo = "DBNSOSA7890";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, "Characters 7-11 of Customs Controlled Premises Code – Warehouse must be numeric.");
			cusCode.OK_CustomsRegNo = "DBNSOS78901";
			AssertNoWarnings(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNo_GTX()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			cusCode.OK_CustomsRegNo = "16CI470091";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "There should be a Customs Importer code of 70707070.");
			organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, ValidationConstants.Declaration.UnregisteredTraderCustomsCode);
			cusCode.OK_CustomsRegNo = "16CI470091";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, "There should be a Customs Importer code of 70707070.");
		}

		OrgCusCode cusCode;
		OrgHeader organisation;
		protected override void SetUp()
		{
			base.SetUp();
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("DBN");
			Factory.Save();
			organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
		}
	}
}
