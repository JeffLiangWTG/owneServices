using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderValidation))]
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Voyage()
		{
			var propertyInfo = header.AMA_VoyageInfo;
			var messageError = "Flight Number must start with a valid 2-letter IATA Airline code.";
			var messageWaring = "System will automatically declare two-letter airline code and the four-digit flight number, separated by a space character. For example, CI 0008, where the two-letter airline code is CI and the four-digit flight number is 0008.";
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			header.AMA_Voyage = "00 0001";
			AssertHasMessageError(propertyInfo, messageError);
			header.AMA_Voyage = "CI0001";
			AssertNoMessageError(propertyInfo, messageError);
			AssertHasWarning(propertyInfo, messageWaring);
			header.AMA_Voyage = "CI 0001";
			AssertNoWarning(propertyInfo, messageWaring);
			header.AMA_Voyage = "CI001";
			AssertHasWarning(propertyInfo, messageWaring);
			header.AMA_Voyage = "CI 001";
			AssertHasWarning(propertyInfo, messageWaring);
			header.AMA_Voyage = "CIC001";
			AssertHasWarning(propertyInfo, messageWaring);
			header.AMA_Voyage = "CI12001";
			AssertHasWarning(propertyInfo, messageWaring);
			header.AMA_Voyage = "5X 0061";
			AssertNoWarning(propertyInfo, messageWaring);

			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_Voyage = "00 0001";
			AssertNoMessageError(propertyInfo, messageError);
			header.AMA_Voyage = "CI12001";
			AssertNoWarning(propertyInfo, messageWaring);
		}

		public void TestCheckAMA_OA_DeconsolidateAddress()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "Z1";
			var orgAddress1 = testOrg1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "line 1";
			orgAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = "Z2";
			var orgAddress2 = testOrg2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "line 2";

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_Code = "Z3";
			var orgAddress3 = testOrg3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "line 3";
			testOrg3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "85291444", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();

			var messageText = "The selected Deconsolidator does not have a TW-VAT number.";
			header.AMA_OA_DeconsolidateAddress = orgAddress3.PK;
			var targetInfo = header.AMA_OA_DeconsolidateAddressInfo;
			AssertNoMessageError(targetInfo, messageText);

			header.AMA_OA_DeconsolidateAddress = orgAddress2.PK;
			AssertHasMessageError(targetInfo, messageText);

			header.AMA_OA_DeconsolidateAddress = orgAddress3.PK;
			AssertNoMessageError(targetInfo, messageText);

			header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
			AssertNoMessageError(targetInfo, messageText);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_CustomsOfficeInfo);
		}

		public void TestCheckAMA_MasterBill()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_MasterBillInfo);
		}

		public void TestValidateMasterBillAirlinePrefix()
		{
			var messageWarning = "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.";
			header.AMA_Voyage = "QF123";
			header.AMA_MasterBill = "081";
			AssertNoWarning(header.AMA_MasterBillInfo, messageWarning);

			header.AMA_MasterBill = "083";
			AssertHasWarning(header.AMA_MasterBillInfo, messageWarning);

			header.AMA_Voyage = "Q";
			header.Validation.ValidateAMA_MasterBill();
			AssertNoWarning(header.AMA_MasterBillInfo, messageWarning);

			header.AMA_Voyage = "ZZ";
			header.Validation.ValidateAMA_MasterBill();
			AssertNoWarning(header.AMA_MasterBillInfo, messageWarning);
		}

		public void TestCheckAMA_GoodsLocationFromMasterBill()
		{
			var targetInfo = header.AMA_GoodsLocationFromMasterBillInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			TestCheckAMA_GoodsLocationFromMasterBillInList(targetInfo);
		}

		void TestCheckAMA_GoodsLocationFromMasterBillInList(ZPropertyInfo targetInfo)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			Factory.Save();

			header.AMA_GoodsLocationFromMasterBill = "AA1234";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_GoodsLocationFromMasterBill = "ANP0060D";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_GoodsLocationFromMasterBill = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckAMA_E_ARV()
		{
			var targetInfo = header.AMA_E_ARVInfo;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_E_ARV = ZDateTime.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckAMA_VehicleRegistration()
		{
			var messageError = "Vehicle Registration is exactly 6 characters long.";
			var targetInfo = header.AMA_VehicleRegistrationInfo;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_VehicleRegistration = "123456";
			AssertNoMessageErrorContaining(targetInfo, messageError);

			header.AMA_VehicleRegistration = "1234";
			AssertHasMessageErrorContaining(targetInfo, messageError);

			header.AMA_VehicleRegistration = "12345";
			AssertHasMessageErrorContaining(targetInfo, messageError);

			header.AMA_VehicleRegistration = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, messageError);

			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			header.AMA_VehicleRegistration = "12345";
			AssertNoMessageErrorContaining(targetInfo, messageError);
		}

		public void TestCheckAMA_CarrierCodeWhenIsSea()
		{
			var error = "The entered value is not a valid Taiwan Carrier or Shipping Agency Code. To create one, visit Maintain > Customs > Global Carriers.";
			var invalidValue = "INVALID";
			var propertyInfo = header.AMA_CarrierCodeInfo;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;

			var carrier1 = helper.CreateZZRefCarrierCombined("TWTW1", "TWTW1", Core.Constants.CountryCodes.Taiwan);
			var carrier2 = helper.CreateZZRefCarrierCombined("USUS1", "USUS1", Core.Constants.CountryCodes.UnitedStates);
			var carrier3 = helper.CreateZZRefCarrierCombined("TWTW2", "TWTW2", Core.Constants.CountryCodes.Taiwan);

			CombineAssertions(() =>
			{
				header.AMA_CarrierCode = invalidValue;
				AssertHasMessageError(propertyInfo, error);

				header.AMA_CarrierCode = carrier1.ZZ4_Code;
				AssertNoMessageError(propertyInfo, error);

				header.AMA_CarrierCode = carrier2.ZZ4_Code;
				AssertHasMessageError(propertyInfo, error);

				header.AMA_CarrierCode = carrier3.ZZ4_Code;
				AssertNoMessageError(propertyInfo, error);
			});
		}

		public void TestCheckAMA_CarrierCodeWhenIsAir()
		{
			var error1 = "The entered Airline Code does not exist. To create one, visit Maintain > Reference Files > Airlines.";
			var error2 = "The entered Airline Code exists but is not an IATA Airline Code. To mark the Airline Code as an IATA Airline Code, visit Maintain > Reference Files > Airline, select the airline record, and check the IATA Member checkbox.";
			var invalidValue = "INVALID";
			var propertyInfo = header.AMA_CarrierCodeInfo;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;

			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "100";
			airline1.RM_MembershipFlagIATA = false;

			var airline2 = Factory.NewWithValidTestData<RefAirline>();
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "200";
			airline2.RM_MembershipFlagIATA = true;

			var airline3 = Factory.NewWithValidTestData<RefAirline>();
			airline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "300";
			airline3.RM_MembershipFlagIATA = true;

			CombineAssertions(() =>
			{
				header.AMA_CarrierCode = invalidValue;
				AssertHasMessageError(propertyInfo, error1);

				header.AMA_CarrierCode = airline1.RM_EagleAddedAirlinePrefixOrAccountingCode;
				AssertHasMessageError(propertyInfo, error2);

				header.AMA_CarrierCode = airline2.RM_EagleAddedAirlinePrefixOrAccountingCode;
				AssertNoMessageError(propertyInfo, error1);
				AssertNoMessageError(propertyInfo, error2);

				header.AMA_CarrierCode = airline3.RM_EagleAddedAirlinePrefixOrAccountingCode;
				AssertNoMessageError(propertyInfo, error1);
				AssertNoMessageError(propertyInfo, error2);
			});
		}

		public void TestShouldValidateConveyanceCountry()
		{
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			header.Validation.ValidateAMA_RN_NKConveyanceNationality();
			AssertEquals(false, header.AMA_RN_NKConveyanceNationalityInfo.HasWarning(string.Format("You have not entered a {0}.", header.AMA_RN_NKConveyanceNationalityInfo.HumanReadableName)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			helper = new UniversalReferenceTestDataHelper(Factory);
		}

		AsycudaManifestHeader header;
		UniversalReferenceTestDataHelper helper;
	}
}
