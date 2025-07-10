using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RoundedQuantityTest : TestCaseWithFactory
	{
		public void TestGetRoundedForChapter22()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "22000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.44m;

			var invoiceLine = GetInvoiceLine();
			IDutyData dutyData = invoiceLine;

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.JI_LinePrice = 10000.20m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 2.523m;

			var rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity1 rounded", 2.52m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));
		}

		public void TestGetRoundedQuantity1()
		{
			var invoiceLine = GetInvoiceLine();
			IDutyData dutyData = invoiceLine;

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.JI_LinePrice = 10000.20m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 2.523m;

			var rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity1 rounded", 3m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.44m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 2.523m;
			rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity1 rounded", 3m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			tariff.UE_Column1RateSpecific = 1.44m;
			rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity1 rounded to two decimals", 2.52m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			invoiceLine.US_TextileCategoryNo = "GT5";
			AssertEquals("Quantity1 rounded to whole number - textile category Number exists", 3m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));
		}

		public void TestGetRoundedQuantity2()
		{
			var invoiceLine = GetInvoiceLine();
			IDutyData dutyData = invoiceLine;

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.JI_LinePrice = 10000.20m;
			invoiceLine.JI_CustomsQuantity = 2.523m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsSecondUnitQty = "LT";
			invoiceLine.JI_CustomsSecondQuantity = 4.625m;
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 8.867m;

			var rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity2", 5m, RoundedQuantity.GetRoundedQuantity2(invoiceLine, invoiceLine.JI_CustomsSecondQuantity));

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_Column2RateSpecific = 1.44m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 2.523m;
			invoiceLine.JI_CustomsSecondQuantity = 4.625m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";
			rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity2 rounded to two decimals", 4.63m, RoundedQuantity.GetRoundedQuantity2(invoiceLine, invoiceLine.JI_CustomsSecondQuantity));

			invoiceLine.US_TextileCategoryNo = "GT5";
			AssertEquals("Quantity2 rounded to whole number - textile category Number exists", 5m, RoundedQuantity.GetRoundedQuantity2(invoiceLine, invoiceLine.JI_CustomsSecondQuantity));
		}

		public void TestGetRoundedQuantity3()
		{
			var invoiceLine = GetInvoiceLine();
			IDutyData dutyData = invoiceLine;

			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.JI_LinePrice = 10000.20m;
			invoiceLine.JI_CustomsQuantity = 2.523m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsSecondUnitQty = "LT";
			invoiceLine.JI_CustomsSecondQuantity = 4.625m;
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 8.867m;

			var rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity3", 9m, RoundedQuantity.GetRoundedQuantity3(invoiceLine, invoiceLine.JI_CustomsThirdQuantity));

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_SPICode = "A E J CAILMX";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_ISOCountryCode = "SG";
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			dutyRate.UD_AdValoremSpecialRate = 1.4m;
			dutyRate.UD_SpecificSpecialRate = 1.6m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SPI = "E";
			invoiceLine.JI_CustomsQuantity = 2.523m;
			invoiceLine.JI_CustomsSecondQuantity = 4.625m;
			invoiceLine.JI_CustomsThirdQuantity = 8.867m;
			rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity3 rounded to two decimals", 8.87m, RoundedQuantity.GetRoundedQuantity3(invoiceLine, invoiceLine.JI_CustomsThirdQuantity));

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificFunctionalAdValorem;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificFunctionalAdValorem;

			dutyRate.UD_OtherSpecialRate = 1.14m;
			rates = DutyRateWrapper.GetWrapper(invoiceLine);
			AssertEquals("Quantity3 rounded to two decimals", 8.87m, RoundedQuantity.GetRoundedQuantity3(invoiceLine, invoiceLine.JI_CustomsThirdQuantity));

			invoiceLine.US_TextileCategoryNo = "GT5";
			AssertEquals("Quantity3 rounded to whole number - textile category Number exists", 9m, RoundedQuantity.GetRoundedQuantity3(invoiceLine, invoiceLine.JI_CustomsThirdQuantity));
		}

		public void TestDutyComputationCodeK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1701991010";
			invoiceLine.JI_CustomsQuantity = 192000m;
			invoiceLine.JI_CustomsSecondQuantity = 99.81m;
			invoiceLine.JI_LinePrice = 1728000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ientryLine = (ICusEntryLine)invoiceLine.CusEntryLine;
			AssertEquals(99.81m, ientryLine.Quantity2);
			AssertEquals(7020.81m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		public void TestRoundQuantityForTaxPurpose_CS00145781()
		{
			var invoiceLine = GetInvoiceLine();

			invoiceLine.JI_Tariff = "2402.20.9000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);

			invoiceLine.US_UC_NKCountryOfOrigin = "HU";
			invoiceLine.JI_LinePrice = 2m;
			invoiceLine.JI_CustomsQuantity = 0.05m;
			invoiceLine.JI_CustomsUnitQty = "K";
			invoiceLine.JI_CustomsSecondQuantity = 2m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Tobacco_1;

			AssertEquals("Quantity 1 should be rounded to two decimals", 0.05m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Tax calculated", 2.52m, invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEstimatedTax);
		}

		public void TestGetRoundedQuantity1WithVisaNo()
		{
			var invoiceLine = GetInvoiceLine();
			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.JI_Tariff = "2402.20.9000";
			invoiceLine.ImportTariff.UE_DutyComputationCode = "7";
			invoiceLine.JI_LinePrice = 10000.20m;
			invoiceLine.JI_CustomsUnitQty = "DOZ";
			invoiceLine.JI_CustomsQuantity = 192.50m;
			invoiceLine.US_VisaNo = "";
			invoiceLine.US_TextileCategoryNo = "111";
			AssertEquals("Quantity1 should be rounded to two decimals", 193m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));
			invoiceLine.US_VisaNo = "111";
			AssertEquals("Quantity1 should be rounded without decimal", 192.50m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));
		}

		public void TestGetRoundedQuntity1WithValueFrom0To1()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "2710190630";
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff1.UE_SPICode = "D R P A+AUBHCACLCOILJ+JOKRMAMXOMPAPESG";
			tariff1.UE_Column1RateSpecific = 2.19m;
			Factory.Save();

			var invoiceLine = GetInvoiceLine();
			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.JI_Tariff = "2710.19.0630";
			invoiceLine.JI_LinePrice = 214.50m;
			invoiceLine.JI_CustomsUnitQty = "DOZ";
			invoiceLine.JI_CustomsQuantity = 10088.22m;
			invoiceLine.US_VisaNo = "";
			invoiceLine.US_TextileCategoryNo = "";

			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.US_UC_NKCountryOfExport = "MX";
			invoiceLine.US_SPI = "MX";
			var rate = RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Quantity1 should get round because there is no matched DutyRate record with SPI MX", 10088m, rate);

			invoiceLine.US_SPI = "";
			rate = RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Quantity1 should not round to two decimals because Specific Rate is greater than 1", 10088.22m, rate);
		}

		public void TestGetRoundedQuantity1WithoutTextileNo()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1111111111";
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff1.UE_Column1RateSpecific = 0m;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "3333333333";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff2.UE_Column1RateSpecific = 0.199m;

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "5555555555";
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff3.UE_Column1RateSpecific = 1.8m;
			Factory.Save();

			var invoiceLine = GetInvoiceLine();
			invoiceLine.JI_Tariff = "1111111111";
			invoiceLine.JI_CustomsQuantity = 4999.6666m;
			AssertEquals("Quantity1 should be rounded to two decimals", 4999.67m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			invoiceLine.JI_Tariff = "3333333333";
			invoiceLine.JI_CustomsQuantity = 4999.6666m;
			AssertEquals("Quantity1 should be rounded without decimal", 5000m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			invoiceLine.JI_Tariff = "5555555555";
			invoiceLine.JI_CustomsQuantity = 4999.6666m;
			AssertEquals("Quantity1 should be rounded to two decimals", 4999.67m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.MultipleSpecific;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.MultipleSpecific;
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.MultipleSpecific;
			Factory.Save();

			invoiceLine.JI_Tariff = "1111111111";
			invoiceLine.JI_CustomsQuantity = 4999.6666m;
			AssertEquals("Quantity1 should be rounded without decimals", 5000m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			invoiceLine.JI_Tariff = "3333333333";
			invoiceLine.JI_CustomsQuantity = 4999.6666m;
			AssertEquals("Quantity1 should be rounded without decimal", 5000m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));

			invoiceLine.JI_Tariff = "5555555555";
			invoiceLine.JI_CustomsQuantity = 4999.6666m;
			AssertEquals("Quantity1 should be rounded to two decimals", 4999.67m, RoundedQuantity.GetRoundedQuantity1(invoiceLine, invoiceLine.JI_CustomsQuantity));
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		JobComInvoiceLine GetInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			return invoiceLine;
		}
	}
}
