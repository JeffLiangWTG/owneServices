using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AppendixFCalculatorTest : TestCaseWithFactory
	{
		public void TestRoundForSugarSpecificRates()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 3406.00m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.JI_Tariff = "1701.11.0500";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_CustomsSecondQuantity = 515m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Duty amount should have been rounded to two decimals", 150.57m, invoiceLine.CusEntryLine.DutyAmount);
			AssertEquals("Sugar specific rate should be returned in DutyResult", "10.03782c/KG", invoiceLine.CusEntryLine.CL_DutyPercentAsString);
		}

		public void TestClearWhen99999DutyRateIsApplied()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98220105";
			AssertEquals("Precondition", true, InvalidDutyRate.IsInvalid(invoiceLine.ImportSupTariff.UE_Column1RateAdValorem));
			invoiceLine.JI_Tariff = "1701115000";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		public void TestErrorMessageWhenOverFlowInCalculation()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98220105";
			invoiceLine.JI_LinePrice = 9999999999999999999999999999m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertNoExceptionThrown(() => declaration.DoMerge());

			invoiceLine.Validation.ValidateAll();
			var calculationErrorMessage = "Computation error because data is too large.(Tariff:'98220105',Rate Specific:'0.00000000',Rate Advalorem:'9999.99999999',Rate Other:'0.00000000',Quantity1:'0',Quantity2:'0',Quantity3:'0',LinePrice:'9999999999999999999999999999')";
			AssertHasErrorContaining(invoiceLine.JI_TariffInfo, calculationErrorMessage);

			invoiceLine.JI_LinePrice = 999999m;
			AssertNoExceptionThrown(() => declaration.DoMerge());

			invoiceLine.Validation.ValidateAll();
			AssertNoErrorContaining(invoiceLine.JI_TariffInfo, calculationErrorMessage);
		}

		public void TestDoesNotBlowWhenUsersHaveNotSelectedDutyRateType()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_Tariff = "2204.21.80 60";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertNoExceptionThrown(() => declaration.DoMerge());
		}

		public void TestDutyFormulaAndDescription()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.5m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 18m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("50c/", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the first unit of measure is multiplied by the specific or primary rate.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "10100000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("50c/", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the second unit of measure is multiplied by the specific or primary rate.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "10101000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.MultipleSpecific;
			tariff.UE_Column1RateSpecific = 0.5m;
			tariff.UE_Column1RateOther = 0.314m;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 18m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsSecondQuantity = 15m;
			invoiceLine.JI_CustomsSecondUnitQty = "PFL";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("50c/KG + 31.4c/PFL", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the first unit of measure is multiplied by the specific or primary rate. The quantity of the second unit of measure is multiplied by the other rate usually minimum or other ad valorem. The results are added.", invoiceLine.DutyFormulaDescription);
			AssertEquals("MultipleSpecific should return complex rate string", "50c/KG + 31.4c/PFL", invoiceLine.CusEntryLine.CL_DutyPercentAsString);

			tariff.UE_Tariff = "10101000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			tariff.UE_Column1RateSpecific = 0.3m;
			tariff.UE_Column1RateAdValorem = 0.5m;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 18m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("30c/KG + 50%", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the first unit of measure is multiplied by the specific or primary rate. The value is multiplied by the ad valorem or secondary rate. The results are added.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "10101010";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity;
			tariff.UE_Column1RateSpecific = 0.4m;
			tariff.UE_Column1RateAdValorem = 0.6m;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsSecondQuantity = 15m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("40c/ + 60%", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the second unit of measure is multiplied by the specific or primary rate. The value is multiplied by the ad valorem or secondary rate. The results are added.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "12101010";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificPlusCompound;
			tariff.UE_Column1RateOther = 5.75m;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsSecondQuantity = 23m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("40c/NO + $5.75/KG + 60%", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the first unit of measure is multiplied by the specific or primary rate. The quantity of the second unit of measure is multiplied by the other rate usually minimum or other ad valorem and the value is multiplied by the ad valorem or secondary rate. The results are added.", invoiceLine.DutyFormulaDescription);
			AssertEquals("SpecificPlusCompound should return complex rate string", "40c/NO + $5.75/KG + 60%", invoiceLine.CusEntryLine.CL_DutyPercentAsString);

			tariff.UE_Tariff = "12121010";
			tariff.UE_Column1RateOther = 0.8m;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("60%", invoiceLine.DutyFormula);
			AssertEquals("The value is multiplied by the ad valorem or secondary rate.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "12121210";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("40c/", invoiceLine.DutyFormula);
			AssertEquals("The quantity of the first unit of measure is multiplied by the primary rate or the secondary rate. The user must determine which of the two rates is correct.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "12121212";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSugarJ;
			tariff.UE_Column1RateSpecific = 0.5m;
			tariff.UE_Column1RateAdValorem = 0.3m;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsSecondQuantity = 1230m;
			invoiceLine.JI_CustomsThirdQuantity = 25m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("80c/", invoiceLine.DutyFormula);
			AssertEquals("The greater of two separate computations is used: 1) The difference of 100 minus the quantity of the third unit of measure multiplied by the ad valorem or secondary rate and the result is subtracted from the primary or specific rate and this result is multiplied by the quantity of the second unit of measure. 2) The quantity of the second unit of measure is multiplied by the other rate usually minimum or other ad valorem.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Column1RateSpecific = 0.8m;
			tariff.UE_Column1RateOther = 0.1m;
			invoiceLine.JI_CustomsThirdQuantity = 98m;
			invoiceLine.JI_CustomsSecondQuantity = 250m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("20c/", invoiceLine.DutyFormula);
			AssertEquals("The greater of two separate computations is used: 1) The difference of 100 minus the quantity of the third unit of measure multiplied by the ad valorem or secondary rate and the result is subtracted from the primary or specific rate and this result is multiplied by the quantity of the second unit of measure. 2) The quantity of the second unit of measure is multiplied by the other rate usually minimum or other ad valorem.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Tariff = "12321212";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSugarK;
			tariff.UE_Column1RateSpecific = 0.5m;
			tariff.UE_Column1RateAdValorem = 0.3m;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 1230m;
			invoiceLine.JI_CustomsSecondQuantity = 25m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("10c/", invoiceLine.DutyFormula);
			AssertEquals("The greater of two separate computations is used: 1) R1) The difference of 100 minus the quantity of the second unit of measure multiplied by the ad valorem or secondary rate and the result is subtracted from the primary or specific rate and this result is multiplied by the quantity of the first unit of measure. 2) The quantity of the first unit of measure is multiplied by the other rate usually minimum or other ad valorem.", invoiceLine.DutyFormulaDescription);

			tariff.UE_Column1RateSpecific = 0.8m;
			tariff.UE_Column1RateOther = 0.1m;
			invoiceLine.JI_CustomsSecondQuantity = 98m;
			invoiceLine.JI_CustomsQuantity = 250m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("20c/", invoiceLine.DutyFormula);
			AssertEquals("The greater of two separate computations is used: 1) R1) The difference of 100 minus the quantity of the second unit of measure multiplied by the ad valorem or secondary rate and the result is subtracted from the primary or specific rate and this result is multiplied by the quantity of the first unit of measure. 2) The quantity of the first unit of measure is multiplied by the other rate usually minimum or other ad valorem.", invoiceLine.DutyFormulaDescription);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZString.Empty, invoiceLine.DutyFormula);
			AssertEquals("No Computation Formula Available", invoiceLine.DutyFormulaDescription);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZString.Empty, invoiceLine.DutyFormula);
			AssertEquals("Free", invoiceLine.DutyFormulaDescription);
		}

		public void TestCalculateAmountBySupCustomsValueIfProvided()
		{
			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1, "KG");

			var dutyData = new DutyDataTest();
			dutyData.Tariff = "99030120";
			dutyData.ImportTariff = tariff99030120;
			dutyData.CustomsValue = 100m;

			var rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
			var dutyResult = new AppendixFDutyCalculator(dutyData, Factory).DutyResult;
			AssertEquals("Duty calculate by customs value * advalorem rate = 100 * 0.1", 10m, dutyResult.TotalAmount.Amount);

			dutyData.SupCustomsValue = 1000m;
			rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
			dutyResult = new AppendixFDutyCalculator(dutyData, Factory).DutyResult;
			AssertEquals("Duty calculate by sup customs value * advalorem rate = 1000 * 0.1", 100m, dutyResult.TotalAmount.Amount);

			var tariff99030121 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030121", "4", 0.2, "KG");
			tariff99030121.UE_Column1RateSpecific = 0.3m;

			dutyData = new DutyDataTest();
			dutyData.Tariff = "99030121";
			dutyData.ImportTariff = tariff99030121;
			dutyData.CustomsValue = 200m;
			dutyData.Quantity1 = 150m;

			rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
			dutyResult = new AppendixFDutyCalculator(dutyData, Factory).DutyResult;
			AssertEquals("Duty calculate by quantity * specific rate + customs value * advalorem rate = 150 * 0.3 + 200 * 0.2", 85m, dutyResult.TotalAmount.Amount);

			dutyData.SupCustomsValue = 2000m;
			rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
			dutyResult = new AppendixFDutyCalculator(dutyData, Factory).DutyResult;
			AssertEquals("Duty calculate by quantity * specific rate + sup customs value * advalorem rate = 150 * 0.3 + 2000 * 0.2", 445m, dutyResult.TotalAmount.Amount);
		}
	}
}
