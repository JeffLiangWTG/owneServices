using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "0101", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "010129", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "38012090", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "010102038405", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "1A", "DRUM STEEL", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			Factory.Save();
			packedItem.Validation.ValidateAPI_Tariff();
			AssertHasMessageErrorContaining(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
			packedItem.Validation.ValidateAPI_Tariff();
			AssertNoMessageErrorContaining(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_Tariff = "0101";
			AssertNoMessageErrorContaining("After set value, there should not be message error on API_Tariff", packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_Tariff = "010129";
			AssertNoMessageErrorContaining("After set value, there should not be message error on API_Tariff", packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_Tariff = "38012090";
			AssertNoMessageErrorContaining("After set value, there should not be message error on API_Tariff", packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_Tariff = "010102038405";
			AssertNoMessageErrorContaining("After set value, there should not be message error on API_Tariff", packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_Tariff = "01010";
			AssertHasMessageErrorContaining(packedItem.API_TariffInfo, "Only 4, 6, 8, 12 digits are allowed");
		}

		public void TestCheckAPI_GoodsDescription()
		{
			packedItem.Validation.ValidateAPI_GoodsDescription();
			AssertHasMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_GoodsDescription = "Vert long descrption";
			AssertNoMessageErrorContaining("After set value, there should not be message error on API_GoodsDescription", packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_GrossWeight()
		{
			packedItem.API_GrossWeight = new ZDecimal("0");
			AssertHasMessageErrorContaining(packedItem.API_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_GrossWeight = new ZDecimal("1");
			AssertNoMessageErrorContaining("After set value, there should not be message error on API_GrossWeight", packedItem.API_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestAPI_CustomsUQ()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.PackageTypes, "BG", "BAG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			CombineAssertions("API_CustomsUQ", () =>
			{
				packedItem.API_CustomsUQ = ZString.Empty;
				AssertNoMessageErrors("Empty Code | No Mandotory", packedItem.API_CustomsUQInfo);
				packedItem.API_CustomsUQ = "XX";
				AssertHasMessageError("Invalid Code | Message error", packedItem.API_CustomsUQInfo, ListValidation.InvalidCodeMessageError);
				packedItem.API_CustomsUQ = "BG";
				AssertNoMessageError("Not Empty | No list message error", packedItem.API_CustomsUQInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = TRManifestTypes.Codes.TESLIM;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			packedItem = pack.PackedItems.AddNewPackedItem() as AsycudaPackedItem;
		}

		AsycudaManifestHeader header;
		AsycudaPackedItem packedItem;
	}
}
