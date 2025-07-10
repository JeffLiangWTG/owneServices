using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class SGPackQuantityAndUnitConverterTest : TestCaseWithFactory
	{
		public void TestCustomsPackDetailsWhenTariffHasInvalidUnitOfQuantity()
		{
			var tariff = SetupTariff("-");
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, tariff, 5, UnitOfQuantityCodeList.Codes.BRL);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 5m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.BRL, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestCustomsPackDetailsWhenTariffHasValidUnitOfQuantity()
		{
			var tariff = SetupTariff(UnitOfQuantityCodeList.Codes.TNE);
			Factory.Save();
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, tariff, 1000, Core.Constants.Weight.Kilograms);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 1m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.TNE, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestCustomsPackDetailWithNonStandardConversionsWhenTariffHasValidUnitOfQuantity()
		{
			SetupRefPack(UnitOfQuantityCodeList.Codes.BOX, UnitOfQuantityCodeList.Codes.CEN, 0.01);
			var tariff = SetupTariff(UnitOfQuantityCodeList.Codes.CEN);
			Factory.Save();
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, tariff, 1000, UnitOfQuantityCodeList.Codes.BOX);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 10m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.CEN, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestCustomsPackDetailsWhenRefPackHasInvalidUnitOfQuantity()
		{
			SetupRefPack(UnitOfQuantityCodeList.Codes.CAN, "AAA", 1);
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, null, 10, UnitOfQuantityCodeList.Codes.CAN);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 10m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.CAN, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestCustomsPackDetailsWhenRefPackHasValidUnitOfQuantity()
		{
			SetupRefPack(UnitOfQuantityCodeList.Codes.CAN, UnitOfQuantityCodeList.Codes.CTM, 0.1);
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, null, 20, UnitOfQuantityCodeList.Codes.CAN);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 20m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.CAN, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestCustomsPackTypeConversionFallbackToDefaultWithInvalidTariffUQRefPackUQAndPackLineUQ()
		{
			SetupRefPack(UnitOfQuantityCodeList.Codes.CAN, "AAA", 1);
			var tariff = SetupTariff("-");
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, tariff, 15, UnitOfQuantityCodeList.Codes.CAN);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 15m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.CAN, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestPackTypeIsInvalidAndRefPacksHasMultiplePackTypesSetupAndBothInvalid()
		{
			SetupRefPack("ABL", "AAA", 1);
			SetupRefPack("ABL", "BBB", 1);
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, null, 10, "ABL");
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 10m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.NMB, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestPackTypeIsInvalidAndRefPacksHasMultiplePackTypesSetupAndOneIsValid()
		{
			SetupRefPack("DEF", "AAA", 1);
			SetupRefPack("DEF", UnitOfQuantityCodeList.Codes.CAN, 1);
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, null, 10, "DEF");
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 10m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.CAN, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestPackTypeIsInvalidAndRefPacksHasMultiplePackTypesSetupAndBothValid()
		{
			SetupRefPack("GHI", UnitOfQuantityCodeList.Codes.POU, 1);
			SetupRefPack("GHI", UnitOfQuantityCodeList.Codes.STK, 1);
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, null, 10, "GHI");
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 10m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.POU, customsPackDetails.UnitOfQuantity);
			});
		}

		public void TestValidTariffUnitOfQuantityIsUsedAndQtyIsZeroIfConversionCannotBeDone()
		{
			var tariff = SetupTariff(UnitOfQuantityCodeList.Codes.KEG);
			Factory.Save();
			var customsPackDetails = sgPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, sgPackLine.UnitConverter, tariff, 10, "PCE");
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 0m, customsPackDetails.Quantity);
				AssertEquals("Unit of Quantity", UnitOfQuantityCodeList.Codes.KEG, customsPackDetails.UnitOfQuantity);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			sgPackQuantityAndUnitConverter = new SGPackQuantityAndUnitConverter();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			var bill = header.Bills.AddNew();
			var packLine = bill.Packs.AddNew();
			sgPackLine = packLine.PackedItem;
		}

		AsycudaPackedItem sgPackLine;
		SGPackQuantityAndUnitConverter sgPackQuantityAndUnitConverter;

		TariffView SetupTariff(ZString unitOfQty)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(tariffType, "1111.11.11");
			helper.LoadOrCreateCreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, unitOfQty);
			return tariff;
		}

		CusRefPacks SetupRefPack(ZString commercialPackType, ZString customsPackType, ZDecimal conversionFactor)
		{
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = commercialPackType;
			refPack.RP_CustomsPack = customsPackType;
			refPack.RP_CustomsCountry = Core.Constants.CountryCodes.Singapore;
			refPack.RP_ConversionFactor = conversionFactor;
			Factory.Save();
			return refPack;
		}
	}
}
