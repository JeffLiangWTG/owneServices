using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemValidation))]
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "11081990009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var targetInfo = packedItem.API_TariffInfo;
			var message = $"The entered {targetInfo.HumanReadableName} is invalid.";
			packedItem.Validation.ValidateAPI_Tariff();
			AssertNoMessageError("Empty", targetInfo, message);

			packedItem.API_Tariff = "01012100006";
			AssertHasMessageError("UniversalTariff is null", targetInfo, message);

			packedItem.API_Tariff = "11081990009";
			AssertNoMessageError("UniversalTariff is not null", targetInfo, message);
		}

		public void TestCheckAPI_GoodsDescription()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_GoodsDescriptionInfo);
		}

		public void TestCheckAPI_Brand()
		{
			var targetInfo = packedItem.API_BrandInfo;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			packedItem.API_Brand = ZString.Empty;
			packedItem.Validation.ValidateAPI_Brand();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_CustomsValue()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_CustomsValueInfo);
		}

		public void TestCheckAPI_CustomsQty()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_CustomsQtyInfo);
		}

		public void TestCheckAPI_CustomsQtyIsValidZDecimal()
		{
			var targetInfo = packedItem.API_CustomsQtyInfo;
			var errorMessage = "the maximum value allowed for Quantity is 99,999,999,999.99999.";
			packedItem.API_CustomsQty = 100000000000m;
			AssertHasErrorContaining(targetInfo, errorMessage);

			packedItem.API_CustomsQty = 99999999999m;
			AssertNoErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckAPI_CustomsUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Packing Units of Measurement");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "CTN", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(packedItem.API_CustomsUQInfo, "XXX", "CTN");
		}

		public void TestCheckAPI_UnitPrice()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_UnitPriceInfo);
		}

		public void TestCheckAPI_UnitPriceIsValidZDecimal()
		{
			var targetInfo = packedItem.API_UnitPriceInfo;
			var errorMessage = "the maximum value allowed for Unit Price is 9,999,999,999,999.999999.";
			packedItem.API_UnitPrice = 10000000000000m;
			AssertHasErrorContaining(targetInfo, errorMessage);

			packedItem.API_UnitPrice = 9999999999999m;
			AssertNoErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckAPI_GoodsValue()
		{
			var targetInfo = packedItem.API_GoodsValueInfo;
			var errorMessage = "Goods Value must be the product of Unit Price and Quantity.";
			packedItem.API_UnitPrice = 1.111111m;
			packedItem.Validation.ValidateAPI_GoodsValue();
			AssertNoMessageError(targetInfo, errorMessage);

			packedItem.API_CustomsQty = 2.22222m;
			packedItem.Validation.ValidateAPI_GoodsValue();
			AssertNoMessageError(targetInfo, errorMessage);

			packedItem.API_GoodsValue = 2.46m;
			AssertHasMessageError(targetInfo, errorMessage);

			packedItem.API_GoodsValue = 2.47m;
			AssertNoMessageError(targetInfo, errorMessage);

			packedItem.API_UnitPrice = 1.107001m;
			packedItem.API_GoodsValue = 2.46m;
			AssertNoMessageError(targetInfo, errorMessage);

			packedItem.API_UnitPrice = 1.111111m;
			packedItem.API_CustomsQty = 2.214m;
			packedItem.Validation.ValidateAPI_GoodsValue();
			AssertNoMessageError(targetInfo, errorMessage);
		}

		public void TestCheckAPI_GoodsValueIsValidMoney()
		{
			var targetInfo = packedItem.API_GoodsValueInfo;
			var errorMessage = "the maximum value allowed for Goods Value is 99,999,999,999,999,999.99.";
			packedItem.API_GoodsValue = 100000000000000000m;
			AssertHasErrorContaining(targetInfo, errorMessage);

			packedItem.API_GoodsValue = 99999999999999999m;
			AssertNoErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckAPI_NetWeight()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_NetWeightInfo);
		}

		public void TestCheckAPI_NetWeightUQ()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(packedItem.API_NetWeightUQInfo, "XX", "KG");
		}

		public void TestCheckAPI_Preference()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestDataHelper.CreatePreferenceForCountry("PR1", "Preference1", "TW");
			universalReferenceTestDataHelper.CreatePreferenceForCountry("PR2", "Preference2", "TW");
			universalReferenceTestDataHelper.CreatePreferenceForCountry("STD", "Standard", "TW");
			Factory.Save();

			var targetInfo = packedItem.API_PreferenceInfo;
			header.AMA_Nature = "IMP";
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(targetInfo, new ZString[] { "REF" }, new ZString[] { "PR1", "PR2", "STD" });

			header.AMA_Nature = "EXP";
			packedItem.API_Preference = "ABC";
			AssertNoMessageErrors(targetInfo);

			packedItem.API_Preference = "";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckAPI_PreviousEntryNo()
		{
			var targetInfo = packedItem.API_PreviousEntryNoInfo;
			var errorMessage = "Previous Bonded Entry Number should be 14 characters long.";
			packedItem.API_PreviousEntryNo = new ZString('A', 13);
			AssertHasMessageError(targetInfo, errorMessage);

			packedItem.API_PreviousEntryNo = new ZString('A', 14);
			AssertNoMessageError(targetInfo, errorMessage);

			packedItem.API_PreviousEntryNo = ZString.Empty;
			AssertNoMessageError(targetInfo, errorMessage);
		}

		public void TestCheckAPI_PreviousEntryLineNo()
		{
			var targetInfo = packedItem.API_PreviousEntryLineNoInfo;
			packedItem.API_PreviousEntryNo = ZString.Empty;
			packedItem.API_PreviousEntryLineNo = ZShort.Zero;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			packedItem.API_PreviousEntryNo = new ZString('A', 14);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			packedItem = header.Bills.AddNew().PackedItems.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaPackedItem packedItem;
	}
}
