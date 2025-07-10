using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	class CusUSLVConsignmentForMessagingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateReasonCode_ListValidation()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			consignmentForMessaging.RunPreSaveValidation();
			AssertListValidationInvalidCodeMessageError(consignmentForMessaging.ReasonCodeInfo, isExpectingMessageError: false);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedBy7512;
			consignmentForMessaging.RunPreSaveValidation();
			AssertListValidationInvalidCodeMessageError(consignmentForMessaging.ReasonCodeInfo, isExpectingMessageError: false);

			consignmentForMessaging.ReasonCode = "ZZ";
			consignmentForMessaging.RunPreSaveValidation();
			AssertListValidationInvalidCodeMessageError(consignmentForMessaging.ReasonCodeInfo, isExpectingMessageError: true);
		}

		public void TestCheckSendToCustoms()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(consignment);
			consignmentForMessaging.SendToCustoms = true;
			consignmentForMessaging.RunPreSaveValidation();

			AssertHasMessageErrorContaining(consignmentForMessaging.SendToCustomsInfo, "Message results still pending from last transmission.");

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			consignmentForMessaging.RunPreSaveValidation();
			AssertNoMessageErrors(consignmentForMessaging.SendToCustomsInfo);
		}

		public void TestCheckULB_SellerName()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_SellerName = "";
			AssertHasMessageErrorContaining(consignment.ULB_SellerNameInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_SellerName = "something";
			AssertNoMessageErrorContaining(consignment.ULB_SellerNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_SellerAddress1()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_SellerAddress1 = "";
			AssertHasMessageErrorContaining(consignment.ULB_SellerAddress1Info, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_SellerAddress1 = "something";
			AssertNoMessageErrorContaining(consignment.ULB_SellerAddress1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_SellerCity()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_SellerCity = "";
			AssertHasMessageErrorContaining(consignment.ULB_SellerCityInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_SellerCity = "something";
			AssertNoMessageErrorContaining(consignment.ULB_SellerCityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_RN_NKSellerCountry()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_RN_NKSellerCountry = "";
			AssertHasMessageErrorContaining(consignment.ULB_RN_NKSellerCountryInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_RN_NKSellerCountry = "AU";
			AssertNoMessageErrorContaining(consignment.ULB_RN_NKSellerCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_ConsigneeName()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_ConsigneeName = "";
			AssertHasMessageErrorContaining(consignment.ULB_ConsigneeNameInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_ConsigneeName = "something";
			AssertNoMessageErrorContaining(consignment.ULB_ConsigneeNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_ConsigneeAddress1()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_ConsigneeAddress1 = "";
			AssertHasMessageErrorContaining(consignment.ULB_ConsigneeAddress1Info, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_ConsigneeAddress1 = "something";
			AssertNoMessageErrorContaining(consignment.ULB_ConsigneeAddress1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_ConsigneeCity()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_ConsigneeCity = "";
			AssertHasMessageErrorContaining(consignment.ULB_ConsigneeCityInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_ConsigneeCity = "something";
			AssertNoMessageErrorContaining(consignment.ULB_ConsigneeCityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckULB_RN_NKConsigneeCountry()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			consignment.ULB_RN_NKConsigneeCountry = "";
			AssertHasMessageErrorContaining(consignment.ULB_RN_NKConsigneeCountryInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.ULB_RN_NKConsigneeCountry = "AU";
			AssertNoMessageErrorContaining(consignment.ULB_RN_NKConsigneeCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
