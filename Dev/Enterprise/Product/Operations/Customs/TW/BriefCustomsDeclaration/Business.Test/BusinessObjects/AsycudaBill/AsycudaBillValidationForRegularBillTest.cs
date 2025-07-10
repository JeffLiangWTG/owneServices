using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForRegularBill))]
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_ManifestQtyMatchSumOfPacks()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.PackedItems.AddNew();
			bill.ABL_ManifestQty = 2;
			AssertEquals("Should not call base.CheckABL_ManifestQtyMatchSumOfPacks", false, bill.ABL_ManifestQtyInfo.Notifications.GetWarnings().ContainsNotificationContaining("Sum of packages' package counts (0) is not equal to manifest quantity"));
		}

		public void TestCheckABL_RX_NKGoodsValueCurrency()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_RX_NKGoodsValueCurrencyInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestABL_ManifestUQNotMappedMessageErrorIsNotRequired()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestUQ = PkgUnit.Bag;
			AssertEquals("Should not ShowMessageErrorIfUQUnknownForCountry", false, bill.ABL_ManifestUQInfo.Notifications.GetMessageErrors().ContainsNotificationContaining("does not map to a Customs package type for country"));
		}

		public void TestCheckABL_CustomsValue()
		{
			var targetInfo = bill.ABL_CustomsValueInfo;
			var message = "must equal to the sum of all item";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			bill.ABL_CustomsValue = 100m;
			AssertHasMessageErrorContaining("Check when set ABL_CustomsValue", targetInfo, message);

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_CustomsValue = 40m;
			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_CustomsValue = 60m;
			AssertNoMessageErrorContaining("Check when set API_CustomsValue", targetInfo, message);
		}

		public void TestCheckABL_GoodsValue()
		{
			var targetInfo = bill.ABL_GoodsValueInfo;
			var message = "Total goods value must equal to the sum of all item goods values.";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_UnitPrice = 33.333m;
			packedItem1.API_CustomsQty = 3m;

			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_CustomsQty = 1m;
			packedItem2.API_UnitPrice = 10m;
			bill.ABL_GoodsValue = 100m;
			AssertHasMessageErrorContaining("ABL_GoodsValue = 100m", targetInfo, message);

			bill.ABL_GoodsValue = 110m;
			AssertNoMessageErrorContaining("ABL_GoodsValue = 110m", targetInfo, message);
		}

		public void TestCheckABL_GoodsValueIsValidMoney()
		{
			var targetInfo = bill.ABL_GoodsValueInfo;
			var message = "the maximum value allowed for Total Invoice Amount is 99,999,999,999,999.99";
			bill.ABL_GoodsValue = 123456789012345.67m;
			AssertHasErrorContaining(targetInfo, message);

			bill.ABL_GoodsValue = 12345678901234.56m;
			AssertNoErrorContaining(targetInfo, message);
		}

		public void TestCheckABL_ShipperRegNoType()
		{
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			var targetInfo = bill.ABL_ShipperRegNoTypeInfo;
			var messageError = "You have not entered an ID Type.";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "VA1", "VAT");

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			bill.ABL_ShipperRegNoType = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);

			bill.ABL_ShipperRegNoType = "VA1";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckABL_ShipperRegNo()
		{
			var targetInfo = bill.ABL_ShipperRegNoInfo;
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Taiwan;
			bill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.VATCode;
			bill.ABL_ShipperRegNo = "1234567";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			bill.ABL_ShipperRegNo = "123456ab";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			bill.ABL_ShipperRegNo = "96944492";
			AssertNoMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			AssertHasMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");
			bill.ABL_ShipperRegNo = "96944490";
			AssertNoMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");

			bill.ABL_ShipperRegNoType = OrgCusCode.TaiwanCodeTypes.PID;
			bill.ABL_ShipperRegNo = "11111111111";
			AssertHasMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			bill.ABL_ShipperRegNo = "1111111111";
			AssertNoMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			var messageError = "You have not entered an ID.";
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			bill.ABL_ShipperRegNo = ZString.Empty;
			AssertHasMessageError(targetInfo, messageError);

			bill.ABL_ShipperRegNo = "1111111111";
			AssertNoMessageError(targetInfo, messageError);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			bill.ABL_ShipperRegNo = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);
		}

		public void TestCheckABL_ConsigneeName()
		{
			var targetInfo = bill.ABL_ConsigneeNameInfo;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			bill.ABL_ConsigneeName = "test";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeName = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			bill.ABL_ConsigneeName = "test";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeName = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_RN_NKConsigneeCountry()
		{
			var targetInfo = bill.ABL_RN_NKConsigneeCountryInfo;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			bill.ABL_RN_NKConsigneeCountry = "TW";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			bill.ABL_RN_NKConsigneeCountry = "TW";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckConsigneeMandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignee, "Consignee is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();
			var consigneeOrgInfo = bill.ABL_OA_ConsigneeInfo;
			var consigneeStreet1Info = bill.ABL_ConsigneeStreet1Info;
			var consigneeCityInfo = bill.ABL_ConsigneeCityInfo;
			var consigneePostcodeInfo = bill.ABL_ConsigneePostcodeInfo;
			var validation = bill.Validation;
			AssertCheckConsigneeMandatory(Universal.Helper.ShipmentTypeList.Codes.Import23, Core.Constants.TransportModes.Air);
			AssertCheckConsigneeMandatory(Universal.Helper.ShipmentTypeList.Codes.Import23, Core.Constants.TransportModes.Sea);
			AssertCheckConsigneeMandatory(Universal.Helper.ShipmentTypeList.Codes.Export22, Core.Constants.TransportModes.Air);
			AssertCheckConsigneeMandatory(Universal.Helper.ShipmentTypeList.Codes.Export22, Core.Constants.TransportModes.Sea);

			void AssertCheckConsigneeMandatory(ZString nature, ZString transportMode)
			{
				header.AMA_Nature = nature;
				header.AMA_TransportMode = transportMode;
				validation.ValidateABL_OA_Consignee();
				validation.ValidateABL_ConsigneeStreet1();
				validation.ValidateABL_ConsigneeCity();
				validation.ValidateABL_ConsigneePostcode();
				AssertNoMessageErrors(consigneeOrgInfo);
				AssertNoMessageErrors(consigneeStreet1Info);
				AssertNoMessageErrors(consigneeCityInfo);
				AssertNoMessageErrors(consigneePostcodeInfo);
			}
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			var targetInfo = bill.ABL_ConsigneeRegNoTypeInfo;
			bill.ABL_ConsigneeRegNo = "52889317";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.ABL_ConsigneeRegNoType = "VAT";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_ConsigneeRegNo()
		{
			var targetInfo = bill.ABL_ConsigneeRegNoInfo;
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Taiwan;
			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.VATCode;
			bill.ABL_ConsigneeRegNo = "1234567";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			bill.ABL_ConsigneeRegNo = "123456ab";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			bill.ABL_ConsigneeRegNo = "96944492";
			AssertNoMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			AssertHasMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");
			bill.ABL_ConsigneeRegNo = "96944490";
			AssertNoMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");

			bill.ABL_ConsigneeRegNoType = OrgCusCode.TaiwanCodeTypes.PID;
			bill.ABL_ConsigneeRegNo = "11111111111";
			AssertHasMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			bill.ABL_ConsigneeRegNo = "1111111111";
			AssertNoMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");
		}

		public void TestCheckABL_Procedure()
		{
			var targetInfo = bill.ABL_ProcedureInfo;
			bill.ABL_Procedure = ProcedureList.Codes.ShipSideCheckRelease;
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_Procedure = "1";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_Incoterm()
		{
			var targetInfo = bill.ABL_IncotermInfo;
			bill.ABL_Incoterm = IncoTerms.CostInsuranceAndFreight;
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_Incoterm = "FFF";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_TransportMode = "SEA";
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			bill.ABL_Incoterm = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = "AIR";
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			bill.Validation.ValidateABL_Incoterm();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			bill.Validation.ValidateABL_Incoterm();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_Incoterm = IncoTerms.ExWorks;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_RL_NKPortOfLoading()
		{
			var targetInfo = bill.ABL_RL_NKPortOfLoadingInfo;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "TWXXX", "TWTPE");
		}

		public void TestCheckABL_RL_NKPortOfDischarge()
		{
			var targetInfo = bill.ABL_RL_NKPortOfDischargeInfo;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "TWXXX", "TWTPE");
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			bill.Validation.ValidateABL_RL_NKFinalDestination();
			AssertEquals("Should not call base.CheckABL_RL_NKFinalDestination", false, bill.ABL_RL_NKFinalDestinationInfo.HasNotifications());
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			bill.Validation.ValidateABL_RL_NKOrigin();
			AssertEquals("Should not call base.CheckABL_RL_NKOrigin", false, bill.ABL_RL_NKOriginInfo.Notifications.HasNotifications());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
