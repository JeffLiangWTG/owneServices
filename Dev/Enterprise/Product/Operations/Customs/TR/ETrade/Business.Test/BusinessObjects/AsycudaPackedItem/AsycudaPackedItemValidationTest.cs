using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		internal string NegativeAmountNotAllowed = "Please enter a non-negative value.";

		public void TestCheckAPI_GoodsDescription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_GoodsDescription = "A123456";
			AssertNoNotifications(packedItem.API_GoodsDescriptionInfo);

			packedItem.API_GoodsDescription = ZString.Empty;
			AssertHasMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_RN_NKGoodsOrigin()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_RN_NKGoodsOrigin = "GR";
			AssertNoMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_RN_NKGoodsOrigin = "XX";
			AssertHasMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_RN_NKGoodsOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_CustomsQty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_CustomsQty = 10000.000;
			AssertNoNotifications(packedItem.API_CustomsQtyInfo);

			packedItem.API_CustomsQty = ZDecimal.Zero;
			AssertHasMessageErrorContaining(packedItem.API_CustomsQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_CustomsQty2()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_CustomsQty2 = ZDecimal.Zero;
			AssertNoErrors(packedItem.API_CustomsQty2Info);

			packedItem.API_CustomsQty2 = 10000.000;
			packedItem.API_CustomsUQ2 = "AYR";
			AssertNoErrorContaining(packedItem.API_CustomsQty2Info, NegativeAmountNotAllowed);

			packedItem.API_CustomsQty2 = -1234.00;
			AssertHasErrorContaining(packedItem.API_CustomsQty2Info, NegativeAmountNotAllowed);
		}

		public void TestCheckAPI_CustomsUQ2()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_CustomsQty2 = 1000.00;
			packedItem.API_CustomsUQ2 = ZString.Empty;
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQ2Info, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_CustomsUQ2 = "XXX";
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQ2Info, ListValidation.InvalidCodeMessageError);

			packedItem.API_CustomsUQ2 = "AYR";
			AssertNoNotifications(packedItem.API_CustomsUQ2Info);
		}

		public void TestCheckAPI_CustomsQty3()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_CustomsQty3 = 10000.000;
			AssertNoErrorContaining(packedItem.API_CustomsQty3Info, NegativeAmountNotAllowed);

			packedItem.API_CustomsQty3 = -1234.00;
			AssertHasErrorContaining(packedItem.API_CustomsQty3Info, NegativeAmountNotAllowed);
		}

		public void TestCheckAPI_CustomsUQ3()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_CustomsQty3 = 1000.00;
			packedItem.API_CustomsUQ3 = ZString.Empty;
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQ3Info, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_CustomsUQ3 = "XXX";
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQ3Info, ListValidation.InvalidCodeMessageError);

			packedItem.API_CustomsUQ3 = "AYR";
			AssertNoNotifications(packedItem.API_CustomsUQ3Info);
		}

		public void TestCheckAPI_GoodsValue()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_GoodsValue = 10000.000;
			AssertNoNotifications(packedItem.API_GoodsValueInfo);

			packedItem.API_GoodsValue = ZDecimal.Zero;
			AssertHasMessageErrorContaining(packedItem.API_GoodsValueInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_RX_NKGoodsValueCurrency()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.API_RX_NKGoodsValueCurrency = "USD";
			AssertNoNotifications(packedItem.API_RX_NKGoodsValueCurrencyInfo);

			packedItem.API_RX_NKGoodsValueCurrency = ZString.Empty;
			AssertHasMessageErrorContaining(packedItem.API_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_Tariff()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "0101", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "010129", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "38012090", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "010102038405", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			packedItem.Validation.ValidateAPI_Tariff();
			AssertHasMessageErrorContaining(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_Tariff = "0101";
			AssertNoMessageErrors(packedItem.API_TariffInfo);
			packedItem.API_Tariff = "010129";
			AssertNoMessageErrors(packedItem.API_TariffInfo);
			packedItem.API_Tariff = "38012090";
			AssertNoMessageErrors(packedItem.API_TariffInfo);
			packedItem.API_Tariff = "010102038405";
			AssertNoMessageErrors(packedItem.API_TariffInfo);

			packedItem.API_Tariff = "01010";
			AssertHasMessageErrorContaining(packedItem.API_TariffInfo, "Only 4, 6, 8, 12 digits are allowed");
		}
	}
}
