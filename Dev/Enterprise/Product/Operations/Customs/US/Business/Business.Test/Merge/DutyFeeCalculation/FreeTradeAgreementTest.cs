using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FreeTradeAgreementTest : TestCaseWithFactory
	{
		public void TestOMANFTA()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			InvoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Oman;
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Oman;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.OM;
			AssertHasMessageError("some error on spi because tariff does not support oman FTA", InvoiceLine.US_SPIInfo, CodeNotInListMessage);

			InvoiceLine.US_SupTariff = "";
			var omanTariff = CreateTariffWithSPI("12345678", SpecialProgramList.Codes.OM);
			InvoiceLine.JI_Tariff = omanTariff.UE_Tariff;

			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.OM;
			AssertHasMessageError("some error on spi because country of origin does not match", InvoiceLine.US_SPIInfo, CodeNotInListMessage);

			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Oman;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.OM;
			AssertHasMessageError("some error on spi because country of export does not match", InvoiceLine.US_SPIInfo, "OMAN Free Trade Agreement requires that goods are exported and originated directly from Oman.");

			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Oman;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.OM;
			AssertNoMessageError("no country of export error on spi because country of export matches", InvoiceLine.US_SPIInfo, "OMAN Free Trade Agreement requires that goods are exported and originated directly from Oman.");
			AssertEquals("no notifications", false, InvoiceLine.US_SPIInfo.HasNotifications());

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No MPF", 0, InvoiceLine.FeeCusCodes.Count);
			AssertEquals("No DUTY", 0m, InvoiceLine.JI_Calc_DutyAmount);

			InvoiceLine.US_SPI = "";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("MPF", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, InvoiceLine.FeeCusCodes[0].CY_Code);
			Assert("DUTY", InvoiceLine.JI_Calc_DutyAmount > 0m);
		}

		public void TestPERUFTA()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			InvoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Peru;
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Peru;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.PE;
			AssertHasMessageError("some error on spi because tariff does not support Peru FTA", InvoiceLine.US_SPIInfo, CodeNotInListMessage);

			var peruTariff = CreateTariffWithSPI("12345678", SpecialProgramList.Codes.PE);
			InvoiceLine.JI_Tariff = peruTariff.UE_Tariff;

			InvoiceLine.US_SupTariff = "";
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.PE;
			AssertHasMessageError("some error on spi because country of origin does not match", InvoiceLine.US_SPIInfo, CodeNotInListMessage);

			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Peru;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.PE;
			AssertHasMessageError("some error on spi because country of export does not match", InvoiceLine.US_SPIInfo, "PERU Free Trade Agreement requires that goods are exported and originated directly from Peru.");

			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Peru;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.PE;
			AssertNoMessageError("no country of export error on spi because country of export matches", InvoiceLine.US_SPIInfo, "PERU Free Trade Agreement requires that goods are exported and originated directly from Peru.");
			AssertEquals("no notifications", false, InvoiceLine.US_SPIInfo.HasNotifications());

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No MPF", 0, InvoiceLine.FeeCusCodes.Count);
			AssertEquals("No DUTY", 0m, InvoiceLine.JI_Calc_DutyAmount);

			InvoiceLine.US_SPI = "";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("MPF", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, InvoiceLine.FeeCusCodes[0].CY_Code);
			Assert("DUTY", InvoiceLine.JI_Calc_DutyAmount > 0m);
		}

		public void TestJapanFTA()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			InvoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Japan;
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.JP;
			AssertHasMessageError("some error on spi because tariff does not support Japan FTA", InvoiceLine.US_SPIInfo, CodeNotInListMessage);

			var tariff = CreateTariffWithSPI("12345678", SpecialProgramList.Codes.JP);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.US_SupTariff = ZString.Empty;
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.JP;
			AssertHasMessageError("some error on spi because country of origin does not match", InvoiceLine.US_SPIInfo, CodeNotInListMessage);

			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.JP;
			AssertHasMessageError("some error on spi because country of export does not match", InvoiceLine.US_SPIInfo, "Japan Trade Agreement requires that goods are exported and originated directly from Japan.");

			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Japan;
			InvoiceLine.US_SPI = SpecialProgramList.Codes.JP;
			AssertNoMessageError("no country of export error on spi because country of export matches", InvoiceLine.US_SPIInfo, "Japan Trade Agreement requires that goods are exported and originated directly from Japan.");
			AssertEquals("no notifications", false, InvoiceLine.US_SPIInfo.HasNotifications());

			InvoiceLine.JI_LinePrice = 1000m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Has MPF", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, InvoiceLine.FeeCusCodes[0].CY_Code);
			AssertEquals("MPF Amount", 3.46m, InvoiceLine.FeeCusCodes[0].CY_FeeAmount);
			AssertEquals("No DUTY", 0m, InvoiceLine.JI_Calc_DutyAmount);

			InvoiceLine.US_SPI = "";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Has MPF", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, InvoiceLine.FeeCusCodes[0].CY_Code);
			AssertEquals("MPF Amount", 3.46m, InvoiceLine.FeeCusCodes[0].CY_FeeAmount);
			AssertEquals("DUTY Amount", 54.3m, InvoiceLine.JI_Calc_DutyAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		USCTariff CreateTariffWithSPI(ZString tariff, ZString sPI)
		{
			USCTariff result = Factory.New<USCTariff>();
			result.UE_Tariff = tariff;
			result.UE_SPICode = sPI;
			result.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			result.UE_Column1RateAdValorem = 0.0543m;
			result.UE_DateFrom = new ZDateTime(2008, 1, 1);
			result.UE_DateTo = ZDateTime.Today.AddDays(1);
			return result;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory));

		JobComInvoiceLine InvoiceLine => Declaration.Invoices[0].InvoiceLines[0];

		const string CodeNotInListMessage = "The code you have selected is not in the list.";
	}
}
