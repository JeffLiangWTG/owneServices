using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class HarborMaintenanceFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestCS00253108NonTariffSpecificFeesAreCalculatedOnSumOfParentAndSecondaryLines()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("HMF should be calculated on the sum", 2.09m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void Test99TariffsHMFExemptions()
		{
			#region Setup Tariffs
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "98040030";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var uscTariff2 = Factory.New<USCTariff>();
			uscTariff2.UE_Tariff = "2222222222";
			uscTariff2.UE_DateFrom = startDate;
			uscTariff2.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98040030", startDate, endDate);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "2222222222", startDate, endDate);
			var attribute = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "HMF", tariff1);

			Factory.Save();

			#endregion

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2222222222";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var calc = new HarborMaintenanceFeeCalculator(Factory);
			AssertEquals("HMF should be calculated on the sum", 1.70m, calc.CalculateFee(invoiceLine).Amount.Round(2));

			invoiceLine.JI_Tariff = "98040030";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("HMF should be empty", 0m, calc.CalculateFee(invoiceLine).Amount);

			invoiceLine.JI_Tariff = "2222222222";
			invoiceLine.US_SupTariff = "98040030";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("HMF should be empty", 0m, calc.CalculateFee(invoiceLine).Amount);

			invoiceLine.JI_Tariff = "98040030";
			invoiceLine.US_SupTariff = "";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2222222222";
			invoiceLine2.US_IsParent = true;

			invoiceLine.JI_ParentID = invoiceLine2.PK;
			invoiceLine.JI_ParentLine = invoiceLine2.JI_LineNo;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("HMF should be empty", 0m, calc.CalculateFee(invoiceLine).Amount);
		}

		public void TestFee()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var mock = Factory.NewMoq<JobComInvoiceLine>();
			mock.Setup(m => m.JI_CustomsValue).Returns(10000m);
			AssertEquals(12.5m, new HarborMaintenanceFeeCalculator(Factory).CalculateFee(mock.Object).Amount);
		}

		public void TestCalculateHFMOnInvoiceLineWithSupAdditionalTariffs()
		{
			#region Setup Tariffs

			var tariff99038501 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038501", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff99038821 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038821", "7", 0.25m, ZString.Empty);
			var tariff7601103000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "7601103000", "7", 0.026m, "KG");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99038501 = helper.CreateTariff("US", hsnTariffType.PK, "99038501", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038501 = helper.CreateRate(tariffView99038501, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038501, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038501);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			var tariffView99038821 = helper.CreateTariff("US", hsnTariffType.PK, "99038821", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038821 = helper.CreateRate(tariffView99038821, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038821, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038821);

			#endregion

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_FormattedTariff = "7601.10.3000";
			invoiceLine.SupTariffFormatted = "9903.85.01";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.21";
			invoiceLine.JI_LinePrice = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("HMF should be calculated", 1.25m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestCalculateHFMOnXVVLinesWithSupAdditionalTariffsOnXLine()
		{
			#region Setup Tariffs

			var tariff99038869 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038869", "X", 0m, ZString.Empty);
			var tariff99030123 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030123", "0", 0m, ZString.Empty);
			var tariff8424201000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");
			var tariff9503000013 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9503000013", "7", 0m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99038869 = helper.CreateTariff("US", hsnTariffType.PK, "99038869", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038869 = helper.CreateRate(tariffView99038869, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038869, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038869);

			var tariffView99030123 = helper.CreateTariff("US", hsnTariffType.PK, "99030123", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030123 = helper.CreateRate(tariffView99030123, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030123, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030123);

			#endregion

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			var xInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			xInvoiceLine.US_SetInd = "X";
			xInvoiceLine.JI_FormattedTariff = "9018.39.0020";
			xInvoiceLine.SupTariffFormatted = "9903.01.23";
			xInvoiceLine.SupFormattedAdditionalTariff1 = "9903.88.69";
			var vParentLine = invoice.JobComInvoiceLines.AddNew();
			vParentLine.US_SetInd = "V";
			vParentLine.JI_FormattedTariff = "9018.39.0020";
			vParentLine.SupTariffFormatted = "9903.01.23";
			vParentLine.SupFormattedAdditionalTariff1 = "9903.88.69";
			vParentLine.JI_LinePrice = 179.3m;
			var vChildLine1 = invoice.JobComInvoiceLines.AddNew();
			vChildLine1.US_SetInd = "V";
			vChildLine1.JI_FormattedTariff = "3926.90.9910";
			vChildLine1.SupTariffFormatted = "9903.01.23";
			vChildLine1.JI_LinePrice = 690.91m;
			var vChildLine2 = invoice.JobComInvoiceLines.AddNew();
			vChildLine2.US_SetInd = "V";
			vChildLine2.JI_FormattedTariff = "3006.70.0000";
			vChildLine2.JI_LinePrice = 26.90m;
			var vChildLine3 = invoice.JobComInvoiceLines.AddNew();
			vChildLine3.US_SetInd = "V";
			vChildLine3.JI_FormattedTariff = "3004.90.9214";
			vChildLine3.SupTariffFormatted = "9903.01.23";
			vChildLine3.JI_LinePrice = 89.63m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("HMF should be calculated", 1.23m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestCalculateHFMOnXVVLinesWithSupAdditionalTariffsOnVLine()
		{
			#region Setup Tariffs

			var tariff99038869 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038869", "X", 0m, ZString.Empty);
			var tariff99030123 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030123", "0", 0m, ZString.Empty);
			var tariff8424201000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");
			var tariff9503000013 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9503000013", "7", 0m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99038869 = helper.CreateTariff("US", hsnTariffType.PK, "99038869", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038869 = helper.CreateRate(tariffView99038869, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038869, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038869);

			var tariffView99030123 = helper.CreateTariff("US", hsnTariffType.PK, "99030123", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030123 = helper.CreateRate(tariffView99030123, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030123, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030123);

			#endregion

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			var xInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			xInvoiceLine.US_SetInd = "X";
			xInvoiceLine.JI_FormattedTariff = "9018.39.0020";
			xInvoiceLine.SupTariffFormatted = "9903.01.23";
			var vParentLine = invoice.JobComInvoiceLines.AddNew();
			vParentLine.US_SetInd = "V";
			vParentLine.JI_FormattedTariff = "9018.39.0020";
			vParentLine.SupTariffFormatted = "9903.01.23";
			vParentLine.JI_LinePrice = 179.3m;
			var vChildLine1 = invoice.JobComInvoiceLines.AddNew();
			vChildLine1.US_SetInd = "V";
			vChildLine1.JI_FormattedTariff = "3926.90.9910";
			vChildLine1.SupTariffFormatted = "9903.88.69";
			vChildLine1.SupFormattedAdditionalTariff1 = "9903.01.23";
			vChildLine1.JI_LinePrice = 690.91m;
			var vChildLine2 = invoice.JobComInvoiceLines.AddNew();
			vChildLine2.US_SetInd = "V";
			vChildLine2.JI_FormattedTariff = "3006.70.0000";
			vChildLine2.JI_LinePrice = 26.90m;
			var vChildLine3 = invoice.JobComInvoiceLines.AddNew();
			vChildLine3.US_SetInd = "V";
			vChildLine3.JI_FormattedTariff = "3004.90.9214";
			vChildLine3.SupTariffFormatted = "9903.01.23";
			vChildLine3.JI_LinePrice = 89.63m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("HMF should be calculated", 1.23m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestPartsOfSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SecondarySPI = "V";

			FeeResult result = new HarborMaintenanceFeeCalculator(Factory).CalculateFee(invoiceLine);
			AssertEquals(0m, result.Amount);
			AssertEquals(false, result.IsRequired);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
