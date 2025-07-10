using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	public class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheck_CustomsEntryNumberType()
		{
			AssertNoNotifications(bill.CustomsEntryNumberTypeInfo);

			bill.CustomsEntryNumber = "12345678901";
			AssertHasMessageError(bill.CustomsEntryNumberTypeInfo, MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(bill.CustomsEntryNumberTypeInfo, bill.CustomsEntryNumberInfo));

			bill.CustomsEntryNumberType = "86";
			AssertNoNotifications(bill.CustomsEntryNumberTypeInfo);
		}

		public void TestCheck_CustomsEntryNumber()
		{
			AssertNoNotifications(bill.CustomsEntryNumberInfo);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.Informal, true);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.Sec321a, true);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.HeadnotesHTS, false);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.Gifts, false);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.GoodsReturned, true);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.GiftsPossessions, false);
			AssertCustomsEntryNumber(bill, ACEManifestBillEntryNumberTypes.Codes.PersonalShipment, false);
		}

		void AssertCustomsEntryNumber(AsycudaBill bill, string customsEntryNumberType, bool expectedResult)
		{
			bill.CustomsEntryNumberType = customsEntryNumberType;
			if (expectedResult)
			{
				AssertHasMessageError(bill.CustomsEntryNumberInfo, MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(bill.CustomsEntryNumberInfo, bill.CustomsEntryNumberTypeInfo, (ZString)customsEntryNumberType));
			}
			else
			{
				AssertNoNotifications(bill.CustomsEntryNumberInfo);
			}
		}

		public void TestABL_ManifestUQValidation_IsNotReuiredForUS()
		{
			bill.ABL_ManifestUQ = ZString.Empty;
			AssertNoNotifications(bill.ABL_ManifestUQInfo);
			bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Bag;
			AssertNoNotifications(bill.ABL_ManifestUQInfo);
			bill.ABL_ManifestUQ = "XXX";
			AssertNoNotifications(bill.ABL_ManifestUQInfo);
		}

		public void TestABL_ConsigneePostcode_IsNotRequiredForUS()
		{
			bill.ABL_ConsigneePostcode = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneePostcodeInfo);
		}

		public void TestABL_ShipperPostcode_IsNotRequiredForUS()
		{
			bill.ABL_ShipperPostcode = ZString.Empty;
			AssertNoNotifications(bill.ABL_ShipperPostcodeInfo);
		}

		public void TestCheckABL_BillNumber()
		{
			header.AMA_MasterBill = "AIMMASTER1";
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill1.ABL_BillNumber = "123456789012THIRTEEN";
			AssertHasError(bill1.ABL_BillNumberInfo, ValidationConstants.BillNumberIsWrongLength);
			bill1.ABL_BillNumber = "123456789012";
			AssertNoError(bill1.ABL_BillNumberInfo, ValidationConstants.BillNumberIsWrongLength);
			header.Bills.RemoveAll();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill2.ABL_BillNumber = "12345678901234FIFTEEN";
			AssertHasError(bill2.ABL_BillNumberInfo, ValidationConstants.BillNumberIsWrongLength);
			bill2.ABL_BillNumber = "123456789012";
			AssertNoError(bill2.ABL_BillNumberInfo, ValidationConstants.BillNumberIsWrongLength);
			header.Bills.RemoveAll();
			var bill3 = header.Bills.AddNew();
			bill3.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill3.ABL_BillNumber = "H/B-59992";
			AssertNoError(bill3.ABL_BillNumberInfo, ValidationConstants.BillNumberIsWrongLength);
		}

		public void TestCheckABL_BillNumberIsUnique()
		{
			header.AMA_MasterBill = "AIMMASTER1";
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			bill.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.GoodsReturned;
			bill.CustomsEntryNumber = "LRN111";
			bill.ABL_BillNumber = "X";
			AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "You have not entered a Bill Number");
			bill.ABL_BillNumber = "";
			AssertHasMessageErrorContaining(bill.ABL_BillNumberInfo, "You have not entered a Bill Number");
			bill.ABL_BillNumber = "X";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "X";
			AssertHasErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
			bill2.ABL_BillNumber = "Y";
			AssertNoErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
		}

		public void TestCheckABL_GoodsValue()
		{
			header.AMA_MasterBill = "AIMMASTER1";
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			bill.ABL_BillNumber = "1234567890";
			AssertNoErrorContaining("Currency has no not entered error", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
			bill.ABL_GoodsValue = 100m;
			AssertHasErrorContaining("Currency is required as value is entered", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			AssertNoErrorContaining("Valid Currency no error", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining("Valid Currency no error", bill.ABL_RX_NKGoodsValueCurrencyInfo, ListValidation.InvalidCodeError);
			bill.ABL_RX_NKGoodsValueCurrency = "XXX";
			AssertNoErrorContaining("Invalid Currency no error", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(bill.ABL_RX_NKGoodsValueCurrencyInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckABL_GoodsDescription()
		{
			header.AMA_MasterBill = "AIMMASTER1";
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			bill.ABL_BillNumber = "1234567890";
			bill.ABL_GoodsDescription = "12345678901234567890123456789012345";
			AssertNoWarnings("No warning with length = 35", bill.ABL_GoodsDescriptionInfo);
			bill.ABL_GoodsDescription = "123456789012345678901234567890123456";
			AssertHasWarningContaining("Warning with length > 35", bill.ABL_GoodsDescriptionInfo, "35");
		}

		public void TestCheckABL_Tariff()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			tariffTypeHSN.ZZI_Description = "UnitedStates Harmonized Tariff";
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "AT");
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypeHSN.PK, "0101210001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypeHSN.PK, "0101210002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, atTariffType.PK, "0101210003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, atTariffType.PK, "0101210004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var message = ListValidation.InvalidCodeMessage.ToString();
			CombineAssertions(() =>
			{
				bill.ABL_Tariff = "0101210001";
				AssertNoMessageError(bill.ABL_TariffInfo, message);

				bill.ABL_Tariff = "0101210002";
				AssertNoMessageError(bill.ABL_TariffInfo, message);

				bill.ABL_Tariff = "0101210003";
				AssertHasMessageError(bill.ABL_TariffInfo, message);

				bill.ABL_Tariff = "0101210004";
				AssertHasMessageError(bill.ABL_TariffInfo, message);
			});
		}

		public void TestCheckABL_ManifestQtyMatchSumOfPacks()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.PackedItems.AddNew();
			bill.ABL_ManifestQty = 2;
			AssertEquals("Should not call base.CheckABL_ManifestQtyMatchSumOfPacks", false, bill.ABL_ManifestQtyInfo.Notifications.GetWarnings().ContainsNotificationContaining("Sum of packages' package counts (0) is not equal to manifest quantity"));
		}

		public void TestCheckGoodsOrigin()
		{
			bill.ABL_RL_NKOrigin = ZString.Empty;
			bill.Validation.ValidateAll();
			AssertHasMessageError(bill.GoodsOriginInfo, "You have not entered a Country/Region of Origin.");

			bill.GoodsOrigin = "XX";
			AssertNoMessageError(bill.GoodsOriginInfo, "You have not entered a Country/Region of Origin.");
			AssertHasMessageError(bill.GoodsOriginInfo, "The code you have selected is not in the list.");

			bill.GoodsOrigin = "US";
			AssertNoMessageError(bill.GoodsOriginInfo, "The code you have selected is not in the list.");

			bill.GoodsOrigin = ZString.Empty;
			AssertHasMessageError(bill.GoodsOriginInfo, "You have not entered a Country/Region of Origin.");

			bill.ABL_RL_NKOrigin = "AUSYD";
			AssertEquals("Default from ABL_RL_NKOrigin", "AU", bill.GoodsOrigin);
			AssertNoMessageError(bill.GoodsOriginInfo, "You have not entered a Country/Region of Origin.");
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
