using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class ValueRangeCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ApplyTo));
			InitialiseTestCalculator();
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ApplyTo));
			AssertEquals(1, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ApplyTo).TM_TextInfo, TestCalculator.String1Info);
		}

		public override void TestMapping()
		{
			TestMapping(CalculatorConstants.Type.ApplyTo, "String1");
		}

		public override void TestList1()
		{
			AssertEquals("List1", typeof(ValueApplyToListCodeDescriptionPairList), TestCalculator.List1.GetType());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddValueRangeRateLine(rateEntry, "FRT", "AUD", min: 100, ("-5", 101), ("+5", 102));
			var rateLine11 = AddValueRangeRateLine(rateEntry, "FRT", "AUD", min: 110, ("-5", 111), ("+5", 112));

			var rateLine20 = AddValueRangeRateLine(rateEntry, "FRT", "USD", min: 200, ("-5", 201), ("+5", 202));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|100.00|",
					"Less than $5|AUD|212.00|",
					"$5 and above|AUD|214.00|",
					"Minimum|USD|200.00|",
					"Less than $5|USD|201.00|",
					"$5 and above|USD|202.00|"
				}
			);
		}

		static RateLine AddValueRangeRateLine(RateEntry rateEntry, ZString chargeCode, string currency, ZDecimal min, params (string Break, decimal Amount)[] items)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, ValueRangeCalculator.Code, currencyCode: currency);
			rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, min, 0m);
			foreach (var item in items)
			{
				rateLine.Calculator[item.Break] = (ZDecimal)item.Amount;
			}

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|100.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator["-10000"] = (ZDecimal)50m;
			TestCalculator["+10000"] = (ZDecimal)60m;
			TestCalculator["+20000"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than $10000|USD|50.00|", quotationLines[1].ToString());
			AssertEquals("$10000 to less than $20000|USD|60.00|", quotationLines[2].ToString());
			AssertEquals("$20000 and above|USD|70.00|", quotationLines[3].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Less than $10000|USD|50.00|", quotationLines[1].ToString());
			AssertEquals("$10000 to less than $20000|USD|60.00|", quotationLines[2].ToString());
			AssertEquals("$20000 and above|USD|70.00|", quotationLines[3].ToString());

			Line.TL_RX_NKCurrency = "EUR";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than \u20ac10000|EUR|50.00|", quotationLines[1].ToString());
			AssertEquals("\u20ac10000 to less than \u20ac20000|EUR|60.00|", quotationLines[2].ToString());
			AssertEquals("\u20ac20000 and above|EUR|70.00|", quotationLines[3].ToString());

			Line.TL_RX_NKCurrency = "SGD";
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.IncludeContractNumbers, parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|SGD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than SGD 10000|SGD|50.00|", quotationLines[2].ToString());
			AssertEquals("SGD 10000 to less than SGD 20000|SGD|60.00|", quotationLines[3].ToString());
			AssertEquals("SGD 20000 and above|SGD|70.00|", quotationLines[4].ToString());
		}

		public void TestCalculationForUSBondAmount()
		{
			Line.TL_RX_NKCurrency = "USD";
			Line.Parent.Company().GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Line.Parent.Company().GC_RX_NKLocalCurrency = "USD";

			TestCalculator["-5164.58"] = (ZDecimal)45m;
			TestCalculator["+5164.58"] = (ZDecimal)62.4m;
			TestCalculator["+25822.85"] = (ZDecimal)81.53m;
			TestCalculator["+41316.56"] = (ZDecimal)114.57m;

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.MonetaryValues = new MoneyType();
			parameters.Criteria.MonetaryValues.Add(MoneyType.ValueType.SingleTransactionBondAmount, new Money(1000m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")));

			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount;
			AssertCalculation(parameters, 45m, "USD 1000.00 (Single Transaction Bond Amount) @ Base Rate USD 45.00 (USD 0.00 - USD 5164.57)");

			parameters.Criteria.MonetaryValues.Clear();
			parameters.Criteria.MonetaryValues.Add(MoneyType.ValueType.SingleTransactionBondAmount, new Money(6000m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")));
			AssertCalculation(parameters, 62.4m, "USD 6000.00 (Single Transaction Bond Amount) @ Base Rate USD 62.40 (USD 5164.58 - USD 25822.84)");
		}

		public void TestApplyToListForUS()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var tariff = new TestHelper(Factory).NewFullyPopulatedCompanyTariff();
			tariff.TH_GC = company.PK;

			var entry = tariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];
			var line = entry.RateLines[0];
			line.TL_RateCalculator = ValueRangeCalculator.Code;
			var calculator = (ValueRangeCalculator)line.Calculator;

			AssertEquals("Should Contain BND for US companies", true, calculator.List1.ContainsCode(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount));
		}

		public void TestApplyToListForNonUS()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var tariff = new TestHelper(Factory).NewFullyPopulatedCompanyTariff();
			tariff.TH_GC = company.PK;

			var entry = tariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];
			var line = entry.RateLines[0];
			line.TL_RateCalculator = ValueRangeCalculator.Code;
			var calculator = (ValueRangeCalculator)line.Calculator;

			AssertEquals("Should not Contain BND for non-US companies", false, calculator.List1.ContainsCode(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount));
		}

		public void TestCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Line.TL_RX_NKCurrency = "EUR";

			TestCalculator["-5164.58"] = (ZDecimal)45m;
			TestCalculator["+5164.58"] = (ZDecimal)62.4m;
			TestCalculator["+25822.85"] = (ZDecimal)81.53m;
			TestCalculator["+41316.56"] = (ZDecimal)114.57m;
			TestCalculator["+103291.39"] = (ZDecimal)0m;

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			parameters.Criteria.Entries = new EntryInfoCollection();
			parameters.Criteria.Entries.AddNew(5, 8, 500m);
			parameters.Criteria.Entries.AddNew(9, 25, 1000m);
			parameters.Criteria.Entries.AddNew(15, 24, 600m);

			parameters.Criteria.Invoices = new InvoiceInfoCollection();
			parameters.Criteria.Invoices.AddNew(new Money(16000, Line.Currency), 25);
			parameters.Criteria.Invoices.AddNew(new Money(19600, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")), 32);

			RangeCalculation(MoneyType.ValueType.GoodsValue, parameters);
			RangeCalculation(MoneyType.ValueType.InsuranceValue, parameters);

			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.CustomsValue;
			AssertCalculation(parameters, 45.00m, "EUR 1050.00 (Customs Value) @ Base Rate EUR 45.00 (EUR 0.00 - EUR 5164.57)");

			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.InvoiceValue;
			AssertCalculation(parameters, 81.53m, "EUR 30000.00 (Invoice Value) @ Base Rate EUR 81.53 (EUR 25822.85 - EUR 41316.55)");

			TestCalculator["+103291.39"] = (ZDecimal)200m;
			var monetaryValues = new MoneyType();
			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(105000, Line.Currency));
			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(105000, Line.Currency));
			parameters.Criteria.MonetaryValues = monetaryValues;

			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods;
			AssertCalculation(parameters, 200m, "EUR 105000.00 (Value of Goods) @ Base Rate EUR 200.00 (EUR 103291.39 or more)");

			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;
			AssertCalculation(parameters, 200m, "EUR 105000.00 (Insurance Value) @ Base Rate EUR 200.00 (EUR 103291.39 or more)");
		}

		public void TestCalculationWithCallForPricingFlag()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Line.TL_RX_NKCurrency = "EUR";

			TestCalculator["-5164.58"] = (ZDecimal)45m;
			TestCalculator["+5164.58"] = (ZDecimal)62.4m;
			TestCalculator["+25822.85"] = (ZDecimal)81.53m;

			TestCalculator.RateLineItems[2].TM_CallForPricing = true;
			TestCalculator.RateLineItems[2].TM_Text = "Call us for pricing";

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);

			calculatorParams.Criteria.Entries = new EntryInfoCollection();
			calculatorParams.Criteria.Entries.AddNew(5, 8, 500m);
			calculatorParams.Criteria.Entries.AddNew(9, 25, 1000m);
			calculatorParams.Criteria.Entries.AddNew(15, 24, 600m);

			calculatorParams.Criteria.Invoices = new InvoiceInfoCollection();
			calculatorParams.Criteria.Invoices.AddNew(new Money(16000, Line.Currency), 25);
			calculatorParams.Criteria.Invoices.AddNew(new Money(19600, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")), 32);

			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.InvoiceValue;
			using (_Rating.Start(new LoggerDecorator()))
			{
				AssertExceptionThrown<AutoRater.CallForPriceException>(() => TestCalculator.Calculate(calculatorParams));
			}
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.ApplyTo = CalculatorConstants.Text.ApplyTo.Value.CustomsValue;
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator["-1000"] = (ZDecimal)80m;
			TestCalculator["+1000"] = (ZDecimal)90m;
			TestCalculator["+5000"] = (ZDecimal)120m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator["-1000"] = (ZDecimal)100m;
			TestCalculator["+1000"] = (ZDecimal)120m;
			TestCalculator["+5000"] = (ZDecimal)150m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.Minimum = 100m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(CalculatorConstants.Text.ApplyTo.Value.CustomsValue, clonedLine.GetCalculator<ValueRangeCalculator>().ApplyTo);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(80m, clonedLine.Calculator["-1000"]);
			AssertEquals(90m, clonedLine.Calculator["+1000"]);
			AssertEquals(120m, clonedLine.Calculator["+5000"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(100m, clonedLine.Calculator["-1000"]);
			AssertEquals(120m, clonedLine.Calculator["+1000"]);
			AssertEquals(150m, clonedLine.Calculator["+5000"]);

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus);
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.Minimum = 70m;
			TestCalculator["-1000"] = (ZDecimal)80m;
			TestCalculator["+1000"] = (ZDecimal)90m;
			TestCalculator["+5000"] = (ZDecimal)120m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.Minimum = 80m;
			TestCalculator["-1000"] = (ZDecimal)100m;
			TestCalculator["+1000"] = (ZDecimal)120m;
			TestCalculator["+5000"] = (ZDecimal)150m;

			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(CalculatorConstants.Text.ApplyTo.Value.CustomsValue, clonedLine.GetCalculator<ValueRangeCalculator>().ApplyTo);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(170m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(80m, clonedLine.Calculator["-1000"]);
			AssertEquals(90m, clonedLine.Calculator["+1000"]);
			AssertEquals(120m, clonedLine.Calculator["+5000"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(180m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(100m, clonedLine.Calculator["-1000"]);
			AssertEquals(120m, clonedLine.Calculator["+1000"]);
			AssertEquals(150m, clonedLine.Calculator["+5000"]);

			source.Minimum = 0m;
			source.BaseRate = 50m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(70m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(130m, clonedLine.Calculator["-1000"]);
			AssertEquals(140m, clonedLine.Calculator["+1000"]);
			AssertEquals(170m, clonedLine.Calculator["+5000"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(80m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(150m, clonedLine.Calculator["-1000"]);
			AssertEquals(170m, clonedLine.Calculator["+1000"]);
			AssertEquals(200m, clonedLine.Calculator["+5000"]);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = Calculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(84m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(146m, clonedLine.Calculator["-1000"]);
			AssertEquals(158m, clonedLine.Calculator["+1000"]);
			AssertEquals(194m, clonedLine.Calculator["+5000"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(96m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(170m, clonedLine.Calculator["-1000"]);
			AssertEquals(194m, clonedLine.Calculator["+1000"]);
			AssertEquals(230m, clonedLine.Calculator["+5000"]);

			source.CalculationOrder = Calculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(84m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(156m, clonedLine.Calculator["-1000"]);
			AssertEquals(168m, clonedLine.Calculator["+1000"]);
			AssertEquals(204m, clonedLine.Calculator["+5000"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(96m, clonedLine.GetCalculator<ValueRangeCalculator>().Minimum);
			AssertEquals(180m, clonedLine.Calculator["-1000"]);
			AssertEquals(204m, clonedLine.Calculator["+1000"]);
			AssertEquals(240m, clonedLine.Calculator["+5000"]);
		}

		public void TestValidateRateOperator()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = ValueRangeCalculator.Code;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("DST");

			var dSTRateLine = testEntry.RateLines.AddNew();
			dSTRateLine.TL_AC = dummyChargeCode.PK;

			var line1 = dSTRateLine.RateLineItems.AddNew();
			line1.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.BASNotAllowed, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.UNTNotAllowed, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			var line2 = dSTRateLine.RateLineItems.AddNew();
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.BASNotAllowed, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line2.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line2.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line2.TM_TypeInfo, ErrorMessages.UNTNotAllowed);

			line2.TM_Type = Calculator.Items.Operator.MIN;
			AssertNoErrors(line2.TM_TypeInfo);

			var line3 = dSTRateLine.RateLineItems.AddNew();
			line3.TM_Type = Calculator.Items.Operator.BAS;
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.BASNotAllowed);

			line3.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line3.TM_TypeInfo.GetErrors().GetFirstMessage());

			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			line3.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.UNTNotAllowed);

			line3.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line3.TM_TypeInfo.GetErrors().GetFirstMessage());
		}

		public void TestApplyToValueValidation()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "", "AU");
			var rateLine = rateEntry.AddRateLine("DDOC", ValueRangeCalculator.Code);
			var applyToItem = rateLine.FindRateLineItem(CalculatorConstants.Type.ApplyTo) as RateLineItem;

			AssertNotNull("Should be created by default", applyToItem.TM_Text);
			AssertEquals("Should default as per calculator property", CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods, applyToItem.TM_Text);
			AssertNoErrors("Should be valid", applyToItem);

			applyToItem.TM_Text = "BLA";
			AssertHasError(applyToItem.TM_TextInfo, "Enter a valid selection.");

			applyToItem.TM_Text = ZString.Empty;
			AssertHasError(applyToItem.TM_TextInfo, "Please enter a value.");

			applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;
			AssertNoErrors(applyToItem);
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(ValueRangeCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return ValueRangeCalculator.Code; }
		}

		new ValueRangeCalculator TestCalculator
		{
			get { return (ValueRangeCalculator)base.TestCalculator; }
		}

		void RangeCalculation(MoneyType.ValueType valueType, AutoRatingCalculatorParameters @params)
		{
			var strValueDescr = (valueType == MoneyType.ValueType.GoodsValue) ?
				CalculatorConstants.Text.ApplyTo.Value.Descriptions.ValueOfGoods :
				CalculatorConstants.Text.ApplyTo.Value.Descriptions.InsuranceValue;

			var strValueType = (valueType == MoneyType.ValueType.GoodsValue) ?
				CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods :
				CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;

			TestCalculator.ApplyTo = strValueType;

			var monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(4000, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 45.00m, "EUR 4000.00 (" + strValueDescr + ") @ Base Rate EUR 45.00 (EUR 0.00 - EUR 5164.57)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(5200, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 62.40m, "EUR 5200.00 (" + strValueDescr + ") @ Base Rate EUR 62.40 (EUR 5164.58 - EUR 25822.84)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(25822.84M, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 62.40m, "EUR 25822.84 (" + strValueDescr + ") @ Base Rate EUR 62.40 (EUR 5164.58 - EUR 25822.84)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(25822.85M, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 81.53m, "EUR 25822.85 (" + strValueDescr + ") @ Base Rate EUR 81.53 (EUR 25822.85 - EUR 41316.55)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(26000, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 81.53m, "EUR 26000.00 (" + strValueDescr + ") @ Base Rate EUR 81.53 (EUR 25822.85 - EUR 41316.55)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(42000, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 114.57m, "EUR 42000.00 (" + strValueDescr + ") @ Base Rate EUR 114.57 (EUR 41316.56 - EUR 103291.38)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(105000, Line.Currency));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 0m, "EUR 105000.00 (" + strValueDescr + ") @ Base Rate EUR 0.00 (EUR 103291.39 or more: On Request)");

			monetaryValues = new MoneyType();
			monetaryValues.Add(valueType, new Money(1000, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")));
			@params.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(@params, 45.00m, "EUR 714.29 (" + strValueDescr + ") @ Base Rate EUR 45.00 (EUR 0.00 - EUR 5164.57)");
		}

		#endregion
	}
}
