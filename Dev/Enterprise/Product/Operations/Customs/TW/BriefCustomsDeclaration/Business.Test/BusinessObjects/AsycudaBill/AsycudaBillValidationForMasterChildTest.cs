using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_GoodsLocation()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(masterBill.ABL_GoodsLocationInfo, new ZString[] { "XXXXXX" }, masterBill.Lookups.Locations.ToList<ZString>().ToArray());
		}

		public void TestCheckABL_BillNumber()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_BillNumberInfo);

			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_BillNumberInfo);
		}

		public void TestCheckABL_ConsigneeName()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ConsigneeNameInfo);
		}

		public void TestCheckABL_ConsigneeStreet1()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ConsigneeStreet1Info);
		}

		public void TestCheckABL_ConsigneeCity()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ConsigneeCityInfo);
		}

		public void TestCheckABL_ConsigneePostcode()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ConsigneePostcodeInfo);
		}

		public void TestCheckABL_RN_NKConsigneeCountry()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_RN_NKConsigneeCountryInfo);
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;

			var targetInfo = masterBill.ABL_ConsigneeRegNoTypeInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			masterBill.ABL_ConsigneeRegNoType = "XXX";
			var messageError = "is invalid";
			AssertHasMessageErrorContaining(targetInfo, messageError);

			masterBill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.VATCode;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, messageError);
		}

		public void TestCheckABL_ConsigneeRegNo()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var targetInfo = masterBill.ABL_ConsigneeRegNoInfo;
			masterBill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Taiwan;
			masterBill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.VATCode;
			masterBill.ABL_ConsigneeRegNo = "1234567";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			masterBill.ABL_ConsigneeRegNo = "123456ab";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			masterBill.ABL_ConsigneeRegNo = "96944492";
			AssertNoMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			AssertHasMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");
			masterBill.ABL_ConsigneeRegNo = "96944490";
			AssertNoMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");

			masterBill.ABL_ConsigneeRegNoType = OrgCusCode.TaiwanCodeTypes.PID;
			masterBill.ABL_ConsigneeRegNo = "11111111111";
			AssertHasMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			masterBill.ABL_ConsigneeRegNo = "1111111111";
			AssertNoMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");
		}

		public void TestCheckABL_ShipperName()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ShipperNameInfo);
		}

		public void TestCheckABL_ShipperStreet1()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ShipperStreet1Info);
		}

		public void TestCheckABL_ShipperPostcode()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_ShipperPostcodeInfo);
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_RN_NKShipperCountryInfo);
		}

		public void TestCheckABL_ShipperRegNoType()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;

			var targetInfo = masterBill.ABL_ShipperRegNoTypeInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			masterBill.ABL_ShipperRegNoType = "XXX";
			var messageError = "is invalid";
			AssertHasMessageErrorContaining(targetInfo, messageError);

			masterBill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.VATCode;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, messageError);
		}

		public void TestCheckABL_ShipperRegNo()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			var targetInfo = masterBill.ABL_ShipperRegNoInfo;
			masterBill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Taiwan;
			masterBill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.VATCode;
			masterBill.ABL_ShipperRegNo = "1234567";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			masterBill.ABL_ShipperRegNo = "123456ab";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			masterBill.ABL_ShipperRegNo = "96944492";
			AssertNoMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			AssertHasMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");
			masterBill.ABL_ShipperRegNo = "96944490";
			AssertNoMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");

			masterBill.ABL_ShipperRegNoType = OrgCusCode.TaiwanCodeTypes.PID;
			masterBill.ABL_ShipperRegNo = "11111111111";
			AssertHasMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			masterBill.ABL_ShipperRegNo = "1111111111";
			AssertNoMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");
		}

		public void TestCheckABL_CarrierReference()
		{
			masterBill.ABL_CarrierReference = ZString.Empty;
			var messageError = "must be exactly 4 characters long";
			AssertNoMessageErrorContaining(masterBill.ABL_CarrierReferenceInfo, messageError);
			masterBill.ABL_CarrierReference = "XXXX";
			AssertNoMessageErrorContaining(masterBill.ABL_CarrierReferenceInfo, messageError);
			masterBill.ABL_CarrierReference = "XXX";
			AssertHasMessageErrorContaining(masterBill.ABL_CarrierReferenceInfo, messageError);
		}

		public void TestCheckMandatoryABL_E_ARV()
		{
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(masterBill.ABL_E_ARVInfo);
		}

		public void TestCheckHasAsycudaCountry()
		{
			masterBill.Validation.ValidateABL_RL_NKPortOfLoading();
			var expectedErrorMessage = ASYCUDA.Business.ValidationConstants.ManifestMustGoThruSupportedCountries;
			Assert(!masterBill.ABL_RL_NKPortOfLoadingInfo.Notifications.Any(x => x.Message.Contains(expectedErrorMessage)));
		}

		public void TestCheckMandatoryABL_RL_NKPortOfDischarge()
		{
			masterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
			AssertNoMessageErrors(masterBill.ABL_RL_NKPortOfDischargeInfo);
		}

		public void TestCheckMandatoryABL_RL_NKPortOfLoading()
		{
			masterBill.Validation.ValidateABL_RL_NKPortOfLoading();
			AssertNoMessageErrors(masterBill.ABL_RL_NKPortOfLoadingInfo);
		}

		public void TestCheckMandatoryABL_E_DEP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "An Estimated Departure Time is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList("TW", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedDepartureTime, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();
			masterBill.Validation.ValidateABL_E_DEP();
			AssertNoMessageErrors(masterBill.ABL_E_DEPInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			masterBill = manifestHeader.MasterBill;
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaBill masterBill;
	}
}
