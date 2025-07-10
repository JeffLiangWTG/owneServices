using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class WarehousePackCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			Assert(true);
		}

		public override void TestMapping()
		{
			Assert(true);
		}

		public void TestDefaultWeightVolume()
		{
			AssertEquals("Default weight volume must be UNT", "UNT", TestCalculator.DefaultWeightVolume);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Line.ViewAgentRates = false;
			var newItem = TestCalculator.AddRateLineItem("AAA", 0m, 100m);
			newItem.TM_AgentDeclaredRate = 150m;
			newItem = TestCalculator.AddRateLineItem("BBB", 0m, 200m);
			newItem.TM_AgentDeclaredRate = 250m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			// BaseRate does not affect unit calculation
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertRelevantValue(100m, "AAA");
			AssertRelevantValue(200m, "BBB");
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertRelevantValue(150m, "AAA");
			AssertRelevantValue(250m, "BBB");

			// Percent does not affect unit calculation
			source.BaseRate = 0m;
			source.Percent = 50m;

			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertRelevantValue(100m, "AAA");
			AssertRelevantValue(200m, "BBB");
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertRelevantValue(150m, "AAA");
			AssertRelevantValue(250m, "BBB");

			// PerUnit does affect calculation
			source.Percent = 0m;
			source.PerUnit = 80m;

			clonedLine.Calculator.Line.ViewAgentRates = false;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertRelevantValue(180m, "AAA");
			AssertRelevantValue(280m, "BBB");
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertRelevantValue(230m, "AAA");
			AssertRelevantValue(330m, "BBB");

			// PerUnitPercent does affect calculation
			source.PerUnit = 0m;
			source.PerUnitPercent = 50m;

			clonedLine.Calculator.Line.ViewAgentRates = false;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertRelevantValue(150m, "AAA");
			AssertRelevantValue(300m, "BBB");
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertRelevantValue(225m, "AAA");
			AssertRelevantValue(375m, "BBB");

			void AssertRelevantValue(ZDecimal expected, string itemType) =>
				AssertEquals(expected, clonedLine.GetCalculator<WarehousePackCalculator>().FindRateLineItem(itemType).TM_RelevantValue);
		}

		public void TestPackageTypeValidation()
		{
			Line.TL_RX_NKCurrency = "USD";
			Line.TL_RateCalculator = WarehousePackCalculator.Code;

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = "BOX";
			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = "BOX";

			AssertHasError(item2.TM_TypeInfo, "You can only specify one rate for each package type. You should not specify the same package type more than once.");

			item2.TM_Type = "KEG";
			AssertNoError(item2.TM_TypeInfo, "You can only specify one rate for each package type. You should not specify the same package type more than once.");
		}

		public void TestRoundingIsEnabled()
		{
			Line.TL_RateCalculator = WarehousePackCalculator.Code;
			Assert(!Line.TL_RoundingInfo.ReadOnly);
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddWarehousePackRateLine(rateEntry, "FRT", QuantityUnit.PK, "AUD", (Constants.PkgUnit.Pallet, 100), (Constants.PkgUnit.Case, 101));
			var rateLine11 = AddWarehousePackRateLine(rateEntry, "FRT", QuantityUnit.PK, "AUD", (Constants.PkgUnit.Pallet, 110), (Constants.PkgUnit.Bag, 111));

			var rateLine20 = AddWarehousePackRateLine(rateEntry, "FRT", QuantityUnit.PK, "USD", (Constants.PkgUnit.Pallet, 200), (Constants.PkgUnit.Case, 201));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"|AUD|210.00|Pallet",
					"|AUD|101.00|Case",
					"|AUD|111.00|Bag",
					"|USD|200.00|Pallet",
					"|USD|201.00|Case"
				}
			);
		}

		static RateLine AddWarehousePackRateLine(RateEntry rateEntry, ZString chargeCode, string unit, string currency, params (string Code, decimal Rate)[] items)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<WarehousePackCalculator>();
			foreach (var item in items)
			{
				calculator.AddRateLineItem(item.Code, 0m, item.Rate);
			}

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			Line.TL_RX_NKCurrency = "USD";
			Line.TL_RateCalculator = WarehousePackCalculator.Code;

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(0, quotationLines.Count);

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = "AAA";

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = "BBB";
			item2.TM_RelevantValue = 60m;

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("||Not Charged|", quotationLines[1].ToString());
			AssertEquals("|USD|60.00|", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("||Not Charged|", quotationLines[1].ToString());
			AssertEquals("|USD|60.00|", quotationLines[2].ToString());
		}

		public void TestCalculation()
		{
			// Recursion through Parent Package
			var product1 = CreateTestProduct("Part1");
			AddTestPartUnit(product1, 10m, "UNT", "BOX");
			AddTestPartUnit(product1, 10m, "BOX", "PLT");

			// Same Package different ParentPackage
			var product2 = CreateTestProduct("Part2");
			AddTestPartUnit(product2, 10m, "UNT", "BOX");
			AddTestPartUnit(product2, 100m, "UNT", "PLT");

			// Unsolveble Loop (BOX into PLT and PLT into BOX with the same units)
			var product3 = CreateTestProduct("Part3");
			AddTestPartUnit(product3, 10m, "UNT", "BOX");
			AddTestPartUnit(product3, 10m, "BOX", "PLT");
			AddTestPartUnit(product3, 10m, "PLT", "BOX");

			// Solveble Loop
			var product4 = CreateTestProduct("Part4");
			AddTestPartUnit(product4, 10m, "UNT", "BOX");
			AddTestPartUnit(product4, 10m, "BOX", "PLT");
			AddTestPartUnit(product4, 0.1m, "PLT", "BOX");

			// Stock Keeping Unit isn't Base Pack Type
			var product5 = CreateTestProduct("Part5");
			AddTestPartUnit(product5, 2m, "KG", "UNT");
			AddTestPartUnit(product5, 10m, "UNT", "BOX");
			AddTestPartUnit(product5, 10m, "BOX", "PLT");

			// Fist Rate is Missing, can't rate all items
			var product6 = CreateTestProduct("Part6", "BAG");
			AddTestPartUnit(product6, 10m, "BAG", "BOX");
			AddTestPartUnit(product6, 10m, "BOX", "PLT");

			// Fist Rate is Missing, rating is possible
			var product7 = CreateTestProduct("Part7", "BAG");
			AddTestPartUnit(product7, 10m, "BAG", "BOX");
			AddTestPartUnit(product7, 10m, "BOX", "PLT");

			// Middle Rate is Missing
			var product8 = CreateTestProduct("Part8");
			AddTestPartUnit(product8, 2m, "UNT", "BAG");
			AddTestPartUnit(product8, 5m, "BAG", "BOX");
			AddTestPartUnit(product8, 10m, "BOX", "PLT");

			// Last Rate is Missing
			var product9 = CreateTestProduct("Part9");
			AddTestPartUnit(product9, 10m, "UNT", "BOX");
			AddTestPartUnit(product9, 10m, "BOX", "PLT");
			AddTestPartUnit(product9, 10m, "PLT", "CNT");

			// INVERTED PART UNIT PACKAGES AND PARENT PACKAGES

			// Recursion through Package
			var product10 = CreateTestProduct("Part10");
			AddTestPartUnit(product10, 0.1m, "BOX", "UNT");
			AddTestPartUnit(product10, 0.1m, "PLT", "BOX");

			// Same ParentPackage different Package
			var product11 = CreateTestProduct("Part11");
			AddTestPartUnit(product11, 0.1m, "BOX", "UNT");
			AddTestPartUnit(product11, 0.01m, "PLT", "UNT");

			// Stock Keeping Unit isn't Base Pack Type
			var product12 = CreateTestProduct("Part12");
			AddTestPartUnit(product12, 0.5m, "UNT", "KG");
			AddTestPartUnit(product12, 0.1m, "BOX", "UNT");
			AddTestPartUnit(product12, 0.1m, "PLT", "BOX");

			// Fist Rate is Missing, can't rate all items
			var product13 = CreateTestProduct("Part13", "BAG");
			AddTestPartUnit(product13, 0.1m, "BOX", "BAG");
			AddTestPartUnit(product13, 0.1m, "PLT", "BOX");

			// Fist Rate is Missing, rating is possible
			var product14 = CreateTestProduct("Part14", "BAG");
			AddTestPartUnit(product14, 0.1m, "BOX", "BAG");
			AddTestPartUnit(product14, 0.1m, "PLT", "BOX");

			// Middle Rate is Missing
			var product15 = CreateTestProduct("Part15");
			AddTestPartUnit(product15, 0.5m, "BAG", "UNT");
			AddTestPartUnit(product15, 0.2m, "BOX", "BAG");
			AddTestPartUnit(product15, 0.1m, "PLT", "BOX");

			// Last Rate is Missing
			var product16 = CreateTestProduct("Part16");
			AddTestPartUnit(product16, 0.1m, "BOX", "UNT");
			AddTestPartUnit(product16, 0.1m, "PLT", "BOX");
			AddTestPartUnit(product16, 0.1m, "CNT", "PLT");

			// TWO WAY RECURSION
			var product17 = CreateTestProduct("Part17");
			AddTestPartUnit(product17, 2m, "UNT", "BAG");
			AddTestPartUnit(product17, 5m, "BAG", "BOX");
			AddTestPartUnit(product17, 1m, "BOX", "KG");
			AddTestPartUnit(product17, 0.5m, "CTN", "KG");
			AddTestPartUnit(product17, 0.4m, "M3", "CTN");
			AddTestPartUnit(product17, 0.5m, "PLT", "M3");

			// No rates for product stock keeping unit (BAG) were found and no conversions were possible
			var product18 = CreateTestProduct("Part18", "BAG");
			AddTestPartUnit(product18, 10m, "BAG", "CTN");
			AddTestPartUnit(product18, 10m, "CTN", "CNT");

			// Redundant unit conversion
			var product19 = CreateTestProduct("Part19");
			AddTestPartUnit(product19, 10m, "UNT", "BOX");
			AddTestPartUnit(product19, 10m, "BOX", "PLT");
			AddTestPartUnit(product19, 100m, "UNT", "PLT"); // this is redundant as the first 2 conversions have the same meaning

			// No conversions, part is a unit that doesn't match any rate
			var product20 = CreateTestProduct("Part20", Constants.PkgUnit.Bundle);

			// Failed conversion - zero value
			var product21 = CreateTestProduct("Part21");
			AddTestPartUnit(product21, 0m, "UNT", "BOX");

			// No conversions, 2 decimal places
			var product22 = CreateTestProduct("Part22", Constants.PkgUnit.Unit);
			product22.OP_CountDecimalPlaces = 2;

			Factory.Save();

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			Line.TL_WeightVolume = "UNT";
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator.AddRateLineItem("PLT", 0m, 3.00m); // Rate Hundreds of Units
			TestCalculator.AddRateLineItem("BOX", 0m, 1.50m); // Rate Tens of Units
			TestCalculator.AddRateLineItem("UNT", 0m, 0.20m); // Rate Single Units

			var measures = Criteria.RateableMeasures;
			measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			measures.AddWarehouseProduct((0, null), (0, null), 987m, ZGuid.Empty, product1.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 721m, ZGuid.Empty, product2.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 123m, ZGuid.Empty, product3.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 785m, ZGuid.Empty, product4.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 463m, ZGuid.Empty, product5.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 644m, ZGuid.Empty, product6.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 640m, ZGuid.Empty, product7.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 234m, ZGuid.Empty, product8.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 2834m, ZGuid.Empty, product9.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 612m, ZGuid.Empty, product10.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 934m, ZGuid.Empty, product11.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 467m, ZGuid.Empty, product12.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 265m, ZGuid.Empty, product13.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 260m, ZGuid.Empty, product14.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 368m, ZGuid.Empty, product15.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 4973m, ZGuid.Empty, product16.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 838m, ZGuid.Empty, product17.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 123m, ZGuid.Empty, product18.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 512m, ZGuid.Empty, product19.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 50m, ZGuid.Empty, product20.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 100.5m, ZGuid.Empty, product21.PK, "");
			measures.AddWarehouseProduct((0, null), (0, null), 71.57m, ZGuid.Empty, product22.PK, "");
			for (int partIndex = 0; partIndex < measures.GetPartCount(MeasureType.Unit); ++partIndex)
			{
				calculatorParams.AddLineMeasureMatch(MeasureType.Unit, Line, partIndex);
			}

			IList<ZGuid> allProducts = calculatorParams.GetProductPKs(Line);
			AssertEquals("Precondition", 22, allProducts.Count);

			AssertCalculation(allProducts, product1, calculatorParams, 40.4m, "Product PART1 - 9 Pallet @ AUD 3.00/PLT + 8 Box @ AUD 1.50/BOX + 7 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product2, calculatorParams, 24.2m, "Product PART2 - 7 Pallet @ AUD 3.00/PLT + 2 Box @ AUD 1.50/BOX + 1 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product3, calculatorParams, 0m, "Product PART3 - Calculation not complete, please check the package types definition");
			AssertCalculation(allProducts, product4, calculatorParams, 34.0m, "Product PART4 - 7 Pallet @ AUD 3.00/PLT + 8 Box @ AUD 1.50/BOX + 5 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product5, calculatorParams, 21.6m, "Product PART5 - 4 Pallet @ AUD 3.00/PLT + 6 Box @ AUD 1.50/BOX + 3 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product6, calculatorParams, 24.0m, "Product PART6 - 6 Pallet @ AUD 3.00/PLT + 4 Box @ AUD 1.50/BOX (There is a remainder of 4 BAG that could not be rated. No rates for product stock keeping unit (BAG) were found and no conversions were possible)");
			AssertCalculation(allProducts, product7, calculatorParams, 24.0m, "Product PART7 - 6 Pallet @ AUD 3.00/PLT + 4 Box @ AUD 1.50/BOX");
			AssertCalculation(allProducts, product8, calculatorParams, 11.3m, "Product PART8 - 2 Pallet @ AUD 3.00/PLT + 3 Box @ AUD 1.50/BOX + 4 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product9, calculatorParams, 89.3m, "Product PART9 - 28 Pallet @ AUD 3.00/PLT + 3 Box @ AUD 1.50/BOX + 4 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product10, calculatorParams, 19.9m, "Product PART10 - 6 Pallet @ AUD 3.00/PLT + 1 Box @ AUD 1.50/BOX + 2 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product11, calculatorParams, 32.3m, "Product PART11 - 9 Pallet @ AUD 3.00/PLT + 3 Box @ AUD 1.50/BOX + 4 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product12, calculatorParams, 22.4m, "Product PART12 - 4 Pallet @ AUD 3.00/PLT + 6 Box @ AUD 1.50/BOX + 7 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product13, calculatorParams, 15.0m, "Product PART13 - 2 Pallet @ AUD 3.00/PLT + 6 Box @ AUD 1.50/BOX (There is a remainder of 5 BAG that could not be rated. No rates for product stock keeping unit (BAG) were found and no conversions were possible)");
			AssertCalculation(allProducts, product14, calculatorParams, 15.0m, "Product PART14 - 2 Pallet @ AUD 3.00/PLT + 6 Box @ AUD 1.50/BOX");
			AssertCalculation(allProducts, product15, calculatorParams, 19.6m, "Product PART15 - 3 Pallet @ AUD 3.00/PLT + 6 Box @ AUD 1.50/BOX + 8 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product16, calculatorParams, 158.1m, "Product PART16 - 49 Pallet @ AUD 3.00/PLT + 7 Box @ AUD 1.50/BOX + 3 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product17, calculatorParams, 30.1m, "Product PART17 - 8 Pallet @ AUD 3.00/PLT + 3 Box @ AUD 1.50/BOX + 8 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product18, calculatorParams, 0m, "Product PART18 - There is a remainder of 123 BAG that could not be rated. No rates for product stock keeping unit (BAG) were found and no conversions were possible");
			AssertCalculation(allProducts, product19, calculatorParams, 16.9m, "Product PART19 - 5 Pallet @ AUD 3.00/PLT + 1 Box @ AUD 1.50/BOX + 2 Unit @ AUD 0.20/UNT");
			AssertCalculation(allProducts, product20, calculatorParams, 0m, "Product PART20 - There is a remainder of 50 BND that could not be rated. No rates for product stock keeping unit (BND) were found and no conversions were possible");
			AssertCalculation(allProducts, product21, calculatorParams, 0m, "Product PART21 - Calculation not complete, please check the package types definition");
			AssertCalculation(allProducts, product22, calculatorParams, 14.31m, "71.57 Unit (Product PART22) @ AUD 0.20/UNT");
		}

		public void TestCalculation_CurrencySymbol()
		{
			// Recursion through Parent Package
			var product1 = CreateTestProduct("Part1");
			AddTestPartUnit(product1, 10m, "UNT", "BOX");
			AddTestPartUnit(product1, 10m, "BOX", "PLT");

			Factory.Save();

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			Line.TL_WeightVolume = "UNT";
			Line.TL_RX_NKCurrency = "GBP";
			TestCalculator.AddRateLineItem("PLT", 0m, 3.00m); // Rate Hundreds of Units
			TestCalculator.AddRateLineItem("BOX", 0m, 1.50m); // Rate Tens of Units
			TestCalculator.AddRateLineItem("UNT", 0m, 0.20m); // Rate Single Units

			var measures = Criteria.RateableMeasures;
			measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			measures.AddWarehouseProduct((0, null), (0, null), 987m, ZGuid.Empty, product1.PK, "");
			calculatorParams.AddLineMeasureMatch(MeasureType.Unit, Line, 0);

			IList<ZGuid> allProducts = calculatorParams.GetProductPKs(Line);
			AssertEquals("Precondition", 1, allProducts.Count);

			AssertCalculation(allProducts, product1, calculatorParams, 40.4m, "Product PART1 - 9 Pallet @ GBP 3.00/PLT + 8 Box @ GBP 1.50/BOX + 7 Unit @ GBP 0.20/UNT");
		}

		#region TestCalculate

		public void TestCalculateRecursiveBackward_QuantityInParentShouldNotBeZero()
		{
			var product = CreateTestProduct("Bad Product");
			AddTestPartUnit(product, 10m, "UNT", "BOX");
			AddTestPartUnit(product, 10m, "BOX", "PLT");
			AddTestPartUnit(product, 0m, "PLT", "BOX");

			Factory.Save();

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			Line.TL_WeightVolume = "UNT";
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator.AddRateLineItem("PLT", 0m, 3.00m); // Rate Hundreds of Units
			TestCalculator.AddRateLineItem("BOX", 0m, 1.50m); // Rate Tens of Units
			TestCalculator.AddRateLineItem("UNT", 0m, 0.20m); // Rate Single Units

			var measures = Criteria.RateableMeasures;
			measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			measures.AddWarehouseProduct((0, null), (0, null), 123m, ZGuid.Empty, product.PK, "");
			calculatorParams.AddLineMeasureMatch(MeasureType.Unit, Line, 0);
			var filteredParams = calculatorParams.CreatedFilteredParameters_ForTest(product);

			var (result, error) = TestCalculator.Calculate(filteredParams);
			AssertNotEquals("Calculation Result should not be null", null, result);
			AssertEquals("Could not convert between PLT and parent pack BOX for this product as the Quantity in Parent is zero.", error);
		}

		#region TestCalculate_Packages

		public void TestCalculate_Packages()
		{
			// setup rates
			var chargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", WarehousePackCalculator.Code);
			var client = Helper.NewOrgHeader("CLIENT");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 100m); // $100 per pallet
			calculator.AddRateLineItem(Constants.PkgUnit.Case, 0m, 20m); // $20 per Case
			calculator.AddRateLineItem(Constants.PkgUnit.Package, 0m, 10m); // $10 per Package
			Factory.Save();

			// setup bizO info
			var criteria = new TestRatingCriteria();
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				measures.AddWarehousePackage(0, 0, 2m, ZGuid.Empty, Constants.PkgUnit.Pallet, "");
				measures.AddWarehousePackage(0, 0, 3m, ZGuid.Empty, Constants.PkgUnit.Case, "");
				measures.AddWarehousePackage(0, 0, 5m, ZGuid.Empty, Constants.PkgUnit.Package, "");
			};
			criteria.RateableMeasures.CreateWarehousePackageList(lazyPopulate);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var results = calculator.Calculate(parameters).results.Single();
			var autoRateInfo = new AutoRateInfo(results, parameters, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Description:", "2 Pallet @ AUD 100.00/PLT + 3 Case @ AUD 20.00/CAS + 5 Package @ AUD 10.00/PKG", results.Description);
				AssertEquals("Rounded amount:", 310m, autoRateInfo.Amount);
			});
		}

		public void TestCalculate_Packages_LoadedPackagesOnly()
		{
			// setup rates
			var chargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", WarehousePackCalculator.Code);
			var client = Helper.NewOrgHeader("CLIENT");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);
			rateLine.TL_UnitFactor = "LPO";
			rateLine.TL_RateCalculator = "WPK";

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 100m); // $100 per pallet
			calculator.AddRateLineItem(Constants.PkgUnit.Case, 0m, 20m); // $20 per Case
			calculator.AddRateLineItem(Constants.PkgUnit.Package, 0m, 10m); // $10 per Package

			Factory.Save();

			// setup bizO info
			var criteria = new TestRatingCriteria();
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				measures.AddWarehousePackage(0, 0, 2m, ZGuid.Empty, Constants.PkgUnit.Pallet, "", isPackageLoaded: true);
				measures.AddWarehousePackage(0, 0, 3m, ZGuid.Empty, Constants.PkgUnit.Case, "", isPackageLoaded: false);
				measures.AddWarehousePackage(0, 0, 5m, ZGuid.Empty, Constants.PkgUnit.Package, "", isPackageLoaded: true);
				measures.AddWarehousePackage(0, 0, 0m, ZGuid.Empty, Constants.PkgUnit.Bag, "", isPackageLoaded: true);
				measures.AddWarehousePackage(0, 0, 0m, ZGuid.Empty, Constants.PkgUnit.Keg, "", isPackageLoaded: false);
			};
			criteria.RateableMeasures.CreateWarehousePackageList(lazyPopulate);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var results = calculator.Calculate(parameters).results.Single();
			var autoRateInfo = new AutoRateInfo(results, parameters, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Description:", "2 Pallet @ AUD 100.00/PLT + 5 Package @ AUD 10.00/PKG", results.Description);
				AssertEquals("Rounded amount:", 250m, autoRateInfo.Amount);
			});
		}

		public void TestCalculate_Packages_ZeroQuantity()
		{
			// setup rates
			var chargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", WarehousePackCalculator.Code);
			var client = Helper.NewOrgHeader("CLIENT");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);
			rateLine.TL_UnitFactor = "LPO";
			rateLine.TL_RateCalculator = "WPK";

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Package, 0m, 10m); // $10 per Package

			Factory.Save();

			// setup bizO info
			var criteria = new TestRatingCriteria();
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				measures.AddWarehousePackage(0, 0, 0m, ZGuid.Empty, Constants.PkgUnit.Package, "", isPackageLoaded: true);
				measures.AddWarehousePackage(0, 0, 0m, ZGuid.Empty, Constants.PkgUnit.Package, "", isPackageLoaded: false);
			};
			criteria.RateableMeasures.CreateWarehousePackageList(lazyPopulate);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var results = calculator.Calculate(parameters).results;

			AssertEquals("No rating result since Package's quantity is zero:", 0, results.Count());
		}

		public void TestCalculate_Packages_MissingRates()
		{
			// setup rates
			var chargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", WarehousePackCalculator.Code);
			var client = Helper.NewOrgHeader("CLIENT");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 10.25m); // $10.25 per pallet
			Factory.Save();

			// setup bizO info
			var criteria = new TestRatingCriteria();
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				measures.AddWarehousePackage(0, 0, 2m, ZGuid.Empty, Constants.PkgUnit.Pallet, "");
				measures.AddWarehousePackage(0, 0, 3m, ZGuid.Empty, Constants.PkgUnit.Box, ""); // no rates for Box
				measures.AddWarehousePackage(0, 0, 5m, ZGuid.Empty, Constants.PkgUnit.Bag, ""); // no rates for Bag
			};
			criteria.RateableMeasures.CreateWarehousePackageList(lazyPopulate);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var results = calculator.Calculate(parameters).results.Single();
			var autoRateInfo = new AutoRateInfo(results, parameters, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Description:", "2 Pallet @ AUD 10.25/PLT (There is a remainder of 5 BAG, 3 BOX that could not be rated. No rates for the Package Type(s) were found)", results.Description);
				AssertEquals("Rounded amount:", 20.50m, Utilities.Round(autoRateInfo.Amount, results.Currency?.Decimals ?? 2));
			});
		}

		public void TestCalculate_Packages_NoMatchingRates()
		{
			// setup rates
			var chargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", WarehousePackCalculator.Code);
			var client = Helper.NewOrgHeader("CLIENT");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = rateLine.Calculator;
			Factory.Save();

			// setup bizO info
			var criteria = new TestRatingCriteria();
			Action<RateableMeasureSet> lazyPopulate = measures =>
			{
				measures.AddWarehousePackage(0, 0, 5m, ZGuid.Empty, Constants.PkgUnit.Bag, ""); // no rates for Bag
			};
			criteria.RateableMeasures.CreateWarehousePackageList(lazyPopulate);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var results = calculator.Calculate(parameters).results.Single();
			var autoRateInfo = new AutoRateInfo(results, parameters, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Description:", "There is a remainder of 5 BAG that could not be rated. No rates for the Package Type(s) were found", results.Description);
				AssertEquals("Rounded amount:", 0.0m, Utilities.Round(autoRateInfo.Amount, results.Currency?.Decimals ?? 2));
			});
		}

		public void TestCalculate_Packages_NoPackages()
		{
			// setup rates
			var chargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", WarehousePackCalculator.Code);
			var client = Helper.NewOrgHeader("CLIENT");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 3.00m); // $3.00 per pallet
			Factory.Save();

			// setup bizO info
			var criteria = new TestRatingCriteria(); // no measures setup
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var (results, error) = calculator.Calculate(parameters);

			AssertEquals("Results should be empty as there are no packages.", 0, results.Count());
			AssertNullOrEmpty("Error message should be null or empty as the calculation succeeds.", error);
		}

		#endregion

		#endregion

		#region Implementation

		OrgSupplierPart CreateTestProduct(string partNum, string stockKeepingUnit = Constants.PkgUnit.Unit)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNum;
			product.OP_StockKeepingUnit = stockKeepingUnit;

			return product;
		}

		OrgPartUnit AddTestPartUnit(OrgSupplierPart product, ZDecimal quantityInParent, string package, string parentPackage)
		{
			var partUnit = product.PartUnits.AddNew();
			partUnit.OF_QuantityInParent = quantityInParent;
			partUnit.OF_PackType = package;
			partUnit.OF_ParentPackType = parentPackage;

			return partUnit;
		}

		void AssertCalculation(IEnumerable<ZGuid> allProducts, OrgSupplierPart productToCheck, AutoRatingCalculatorParametersWithoutFilter parameters, ZDecimal expectedAmount, ZString expectedDescription)
		{
			bool hasRun = false;
			foreach (var productPK in allProducts)
			{
				if (productPK == productToCheck.PK)
				{
					var filteredParams = parameters.CreatedFilteredParameters_ForTest(productToCheck);

					AssertCalculation(filteredParams, expectedAmount, expectedDescription);
					hasRun = true;
				}
			}

			Assert("AssertCalculation has not run as the productToCheck doesn't exist in allProducts", hasRun);
		}

		protected override Type CalculatorType
		{
			get { return typeof(WarehousePackCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return WarehousePackCalculator.Code; }
		}

		new WarehousePackCalculator TestCalculator
		{
			get { return (WarehousePackCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
