using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CombinedCalculatorTest : BaseCombinedCalculatorTest<CombinedCalculator>
	{
		#region Empty Unit

		public void TestEmptyUnit_ClientRate()
			=> TestEmptyUnit
			(
				ratingHeader: Helper.NewClientRate(NewClient),
				expectedLog: @"Rate Type: SAL
Service Provider/Client: Test Client #1 (TESTORG1)
RateEntry: FCL-SEA AU>>US
Charge: BAF
Calculator: Combined"
			);

		public void TestEmptyUnit_CompanyTariff()
			=> TestEmptyUnit
			(
				ratingHeader: Helper.NewCompanyTariff(),
				expectedLog: @"Rate Type: GLB
Service Provider/Client: 
RateEntry: FCL-SEA AU>>US
Charge: BAF
Calculator: Combined"
			);

		void TestEmptyUnit(RatingHeader ratingHeader, string expectedLog)
		{
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");

			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = Helper.ChargeCodes["BAF"].PK;
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-10"] = (ZDecimal)200m;
			calculator["+10"] = (ZDecimal)20m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(100m, QuantityUnit.CN);

			CombineAssertions
			(
				"Precondition",
				() =>
				{
					AssertNullOrEmpty("LastMessageReported", ErrorReporter.LastMessageReported);
					AssertNullOrEmpty("Unit", calculator.Unit);
				}
			);

			var result = calculator.Calculate(parameters);

			AssertEquals
			(
				"GIVEN CMB calculator with empty unit WHEN calculate THEN should send error reporter",
				expectedLog,
				ErrorReporter.LastMessageReported
			);

			AssertEquals("Unit is blank for CMB Calculator", result.error);

			ErrorReporter.Clear();
		}

		#endregion

		public void TestPacksWeight()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			Entry.TI_RateCategory = "WHS";
			Entry.TI_Mode = "ALL";
			Line.TL_AC = Helper.ChargeCodes["DDOC"].PK;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = Line.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;
			calculator["+9"] = (ZDecimal)15m;

			Factory.Save();

			var measures = new RateableMeasureSet(AdapterType.Shipment);
			Criteria.RateableMeasures = measures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((100, null), (0, null), 1, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertCalculation(parameters, 500m, "100 Kilogram(s) @ AUD 5.00/KG (for 4 KG/PLT Packs Weight)");
			}
		}

		public void TestPacksWeight_Rounding()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 1m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 1.111111m);

			Entry.TI_RateCategory = "WHS";
			Entry.TI_Mode = "ALL";
			Line.TL_AC = Helper.ChargeCodes["DDOC"].PK;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = Line.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;
			calculator["+9"] = (ZDecimal)15m;

			Factory.Save();

			var measures = new RateableMeasureSet(AdapterType.Shipment);
			Criteria.RateableMeasures = measures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((100, null), (0, null), 1, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertCalculation(parameters, 500m, "100 Kilogram(s) @ AUD 5.00/KG (for 1.1111 KG/PLT Packs Weight)");

				var partUnit = product.PartUnits.GetUnitConversion(PkgUnit.Pallet, PkgUnit.Unit);
				partUnit.OF_QuantityInParent = 1.555555;
				Factory.Save();
				AssertCalculation(parameters, 500m, "100 Kilogram(s) @ AUD 5.00/KG (for 1.5556 KG/PLT Packs Weight)");

				partUnit = product.PartUnits.GetUnitConversion(PkgUnit.Pallet, PkgUnit.Unit);
				partUnit.OF_QuantityInParent = 1.666666;
				Factory.Save();
				AssertCalculation(parameters, 500m, "100 Kilogram(s) @ AUD 5.00/KG (for 1.6667 KG/PLT Packs Weight)");
			}
		}

		public void TestWeightBreaks_NotCreated_Importing()
		{
			AssertWeightBreaksCreatedForImport(true, false);
		}

		public void TestWeightBreaks_Created_NotImporting()
		{
			AssertWeightBreaksCreatedForImport(false, true);
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddCombinedRateLine(rateEntry, "FRT", CombinedCalculator.Code, "KG", "AUD", min: 100m, max: 109m, flat: 101m, perUnit: 102m);
			rateLine10.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 6m, 103m, 104m);
			rateLine10.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 6m, 105m, 106m);

			var rateLine11 = AddCombinedRateLine(rateEntry, "FRT", CombinedCalculator.Code, "KG", "AUD", min: 110m, max: 119m, flat: 111m, perUnit: 112m);
			rateLine11.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 6m, 113m, 114m);
			rateLine11.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 6m, 115m, 116m);
			rateLine11.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 8m, 117m, 118m);

			var rateLine12 = AddCombinedRateLine(rateEntry, "FRT", CombinedCalculator.Code, "M3", "AUD", min: 120m, max: 129m, flat: 121m, perUnit: 122m);
			rateLine12.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 6m, 123m, 124m);
			rateLine12.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 6m, 125m, 126m);

			var rateLine20 = AddCombinedRateLine(rateEntry, "FRT", CombinedCalculator.Code, "KG", "USD", min: 200m, max: 209m, flat: 201m, perUnit: 202m);
			rateLine20.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 6m, 203m, 204m);
			rateLine20.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 6m, 205m, 206m);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|100.00|",
					"|AUD|333.00|",
					"Maximum|AUD|129.00|",
					"|AUD|214.00|KG",
					"Less than 6 Kilogram(s)|AUD|216.00|per KG",
					"Less than 6 Kilogram(s)|AUD|218.00|",
					"6 Kilogram(s) and above|AUD|105.00|per KG",
					"6 Kilogram(s) and above|AUD|106.00|",
					"6 Kilogram(s) to less than 8 Kilogram(s)|AUD|115.00|per KG",
					"6 Kilogram(s) to less than 8 Kilogram(s)|AUD|116.00|",
					"8 Kilogram(s) and above|AUD|117.00|per KG",
					"8 Kilogram(s) and above|AUD|118.00|",
					"|AUD|122.00|M3",
					"Less than 6 Cubic Meter(s)|AUD|123.00|per M3",
					"Less than 6 Cubic Meter(s)|AUD|124.00|",
					"6 Cubic Meter(s) and above|AUD|125.00|per M3",
					"6 Cubic Meter(s) and above|AUD|126.00|",
					"Minimum|USD|200.00|",
					"|USD|201.00|",
					"Maximum|USD|209.00|",
					"|USD|202.00|KG",
					"Less than 6 Kilogram(s)|USD|203.00|per KG",
					"Less than 6 Kilogram(s)|USD|204.00|",
					"6 Kilogram(s) and above|USD|205.00|per KG",
					"6 Kilogram(s) and above|USD|206.00|"
				},
				message: "Calculation component with same currency, unit, type(min/max/flat/unit/break) should be grouped/rolledUp"
			);
		}

		static RateLine AddCombinedRateLine(RateEntry rateEntry, ZString chargeCode, string calculatorCode, string lineUnit, string currencyCode, decimal min, decimal max, decimal flat, decimal perUnit)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, calculatorCode, lineUnit, currencyCode);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator.Minimum = min;
			calculator.Maximum = max;
			calculator.BaseRate = flat;
			calculator.PerUnit = perUnit;

			return rateLine;
		}

		public void TestQuotationLines_BaseRateText()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "", "");

			RatingDataRegistry.Instance.BaseRateText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Zubin Rate");
			try
			{
				Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
				Line.TL_WeightVolume = "KG";
				Line.TL_RX_NKCurrency = "USD";
				Line.ConversionFactor = ConversionFactor.Standard.Metric.Air;

				var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				TestCalculator.BaseRate = 50m;
				TestCalculator.PerUnit = 2m;
				quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				AssertEquals(3, quotationLines.Count);
				AssertEquals("Test Rate|||", quotationLines[0].ToString());
				AssertEquals("Zubin Rate|USD|50.00|", quotationLines[1].ToString());
				AssertEquals("Per Unit|USD|2.00|per KG / 6000 CC", quotationLines[2].ToString());
			}
			finally
			{
				RatingDataRegistry.Instance.BaseRateText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Base Rate");
			}
		}

		public void TestQuotationLines_Container()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FCL, "AUSYD", "USLAX");

			((RateEntry)Line.ParentRateEntry).TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.Parent.TI_Mode = Constants.RateMode.FCL;
			Line.TL_WeightVolume = "CN";
			Line.TL_RX_NKCurrency = "USD";

			TestCalculator["-10"] = (ZDecimal)10m;
			TestCalculator["+10"] = (ZDecimal)20m;
			var quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.AlternativeFormat, parentEntry);
			AssertMultilineASCIIEquals
			(
				@"Test Rate|||
Less than 10 Container(s)|USD|10.00|per 20GP Container
10 Container(s) and above|USD|20.00|per 20GP Container",
				string.Join("\r\n", quotationLines.Select(x => x.ToString()))
			);
		}

		public void TestQuotationLines_Container_Unit()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FCL, "AUSYD", "USLAX");

			((RateEntry)Line.ParentRateEntry).TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.Parent.TI_Mode = Constants.RateMode.FCL;
			Line.TL_WeightVolume = "CN";
			Line.TL_RX_NKCurrency = "USD";

			TestCalculator.PerUnit = (ZDecimal)10m;
			var quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.AlternativeFormat, parentEntry);
			AssertMultilineASCIIEquals
			(
				@"Test Rate|USD|10.00|per 20GP Container",
				string.Join("\r\n", quotationLines.Select(x => x.ToString()))
			);
		}

		public override void TestQuotationLines()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "", "");

			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.ConversionFactor = ConversionFactor.Standard.Metric.Air;

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 50m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|50.00|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 0m;
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.PerUnit = 2m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|2.00|per KG / 6000 CC", quotationLines[0].ToString());

			TestCalculator.PerUnit = 0m;
			TestCalculator.Maximum = 700m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			TestCalculator.BaseRate = 50m;
			TestCalculator.PerUnit = 2m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Base Rate|USD|50.00|", quotationLines[2].ToString());
			AssertEquals("Per Unit|USD|2.00|per KG / 6000 CC", quotationLines[3].ToString());
			AssertEquals("Maximum|USD|700.00|", quotationLines[4].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Base Rate|USD|50.00|", quotationLines[2].ToString());
			AssertEquals("Per Unit|USD|2.00|per KG / 6000 CC", quotationLines[3].ToString());
			AssertEquals("Maximum|USD|700.00|", quotationLines[4].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.BaseRate = 0m;
			TestCalculator.PerUnit = 0m;
			TestCalculator.Maximum = 0m;
			TestCalculator["-100"] = (ZDecimal)0m;
			TestCalculator["+100"] = (ZDecimal)2.5m;
			TestCalculator["+1000"] = (ZDecimal)0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 100 KG||Not Charged|", quotationLines[1].ToString());
			AssertEquals("100 KG to less than 1000 KG|USD|2.50|per KG / 6000 CC", quotationLines[2].ToString());
			AssertEquals("1000 KG and above||Not Charged|", quotationLines[3].ToString());

			Line.ChargeCode.AC_SuppressOnQuoteIfZero = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("100 KG to less than 1000 KG|USD|2.50|per KG / 6000 CC", quotationLines[1].ToString());
			AssertEquals("1000 KG and above||Not Charged|", quotationLines[2].ToString());
			Line.ChargeCode.AC_SuppressOnQuoteIfZero = false;

			TestCalculator.Minimum = 100m;
			TestCalculator.Maximum = 700m;
			TestCalculator["-100"] = (ZDecimal)3m;
			TestCalculator["+100"] = (ZDecimal)2.5m;
			TestCalculator["+1000"] = (ZDecimal)2m;
			TestCalculator.IsAccumulated = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Up to 100 KG|USD|3.00|per KG / 6000 CC", quotationLines[2].ToString());
			AssertEquals("More than 100 KG to 1000 KG|USD|2.50|per additional KG", quotationLines[3].ToString());
			AssertEquals("More than 1000 KG|USD|2.00|per additional KG", quotationLines[4].ToString());
			AssertEquals("Maximum|USD|700.00|", quotationLines[5].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();
			Line.TL_WeightVolume = RatingConstants.Units.CN;
			TestCalculator["-10"] = (ZDecimal)90m;
			TestCalculator["+10"] = (ZDecimal)80m;
			TestCalculator["+20"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 10 Container(s)|USD|90.00|per Container", quotationLines[1].ToString());
			AssertEquals("10 Container(s) to less than 20 Container(s)|USD|80.00|per Container", quotationLines[2].ToString());
			AssertEquals("20 Container(s) and above|USD|70.00|per Container", quotationLines[3].ToString());

			AssertEquals(Calculator.Items.Operator.Minus, Line.RateLineItems[0].TM_Type);
			Line.RateLineItems[0].TM_BreakWeightVolume = RatingConstants.Units.M3;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 10 M3|USD|90.00|per Container", quotationLines[1].ToString());
			AssertEquals("10 M3 to less than 20 M3|USD|80.00|per Container", quotationLines[2].ToString());
			AssertEquals("20 M3 and above|USD|70.00|per Container", quotationLines[3].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();
			Line.TL_WeightVolume = RatingConstants.Units.KG;
			TestCalculator["-45"] = (ZDecimal)4m;
			TestCalculator["+45"] = (ZDecimal)3m;
			TestCalculator["+100"] = (ZDecimal)2.5m;
			TestCalculator.UseInclusiveBreaks = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 45 KG|USD|4.00|per KG / 6000 CC", quotationLines[1].ToString());
			AssertEquals("More than 45 KG to 100 KG|USD|3.00|per KG / 6000 CC", quotationLines[2].ToString());
			AssertEquals("More than 100 KG|USD|2.50|per KG / 6000 CC", quotationLines[3].ToString());

			var callForPriceItem = TestCalculator.AddRateLineItem("+", 250m, 0m, 0m);
			callForPriceItem.TM_CallForPricing = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 45 KG|USD|4.00|per KG / 6000 CC", quotationLines[1].ToString());
			AssertEquals("More than 45 KG to 100 KG|USD|3.00|per KG / 6000 CC", quotationLines[2].ToString());
			AssertEquals("More than 100 KG to 250 KG|USD|2.50|per KG / 6000 CC", quotationLines[3].ToString());
			AssertEquals("More than 250 KG||Call for Price|", quotationLines[4].ToString());

			callForPriceItem.TM_Text = "Some Reason";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals("More than 250 KG||Some Reason|", quotationLines[4].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.ConversionFactor = ConversionFactor.Standard.Metric.Air;

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 50m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|50.00|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 0m;
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.PerUnit = 2m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|2.00|per W/M", quotationLines[0].ToString());

			TestCalculator.PerUnit = 0m;
			TestCalculator.Maximum = 700m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			TestCalculator.BaseRate = 50m;
			TestCalculator.PerUnit = 2m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Base Rate|USD|50.00|", quotationLines[2].ToString());
			AssertEquals("Per Unit|USD|2.00|per W/M", quotationLines[3].ToString());
			AssertEquals("Maximum|USD|700.00|", quotationLines[4].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.BaseRate = 0m;
			TestCalculator.PerUnit = 0m;
			TestCalculator.Maximum = 0m;
			TestCalculator["-100"] = (ZDecimal)0m;
			TestCalculator["+100"] = (ZDecimal)2.5m;
			TestCalculator["+1000"] = (ZDecimal)0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 100 W/M||Not Charged|", quotationLines[1].ToString());
			AssertEquals("100 W/M to less than 1000 W/M|USD|2.50|per W/M", quotationLines[2].ToString());
			AssertEquals("1000 W/M and above||Not Charged|", quotationLines[3].ToString());

			Line.ChargeCode.AC_SuppressOnQuoteIfZero = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("100 W/M to less than 1000 W/M|USD|2.50|per W/M", quotationLines[1].ToString());
			AssertEquals("1000 W/M and above||Not Charged|", quotationLines[2].ToString());
			Line.ChargeCode.AC_SuppressOnQuoteIfZero = false;

			TestCalculator.Minimum = 100m;
			TestCalculator.Maximum = 700m;
			TestCalculator["-100"] = (ZDecimal)3m;
			TestCalculator["+100"] = (ZDecimal)2.5m;
			TestCalculator["+1000"] = (ZDecimal)2m;
			TestCalculator.IsAccumulated = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Up to 100 W/M|USD|3.00|per W/M", quotationLines[2].ToString());
			AssertEquals("More than 100 W/M to 1000 W/M|USD|2.50|per additional W/M", quotationLines[3].ToString());
			AssertEquals("More than 1000 W/M|USD|2.00|per additional W/M", quotationLines[4].ToString());
			AssertEquals("Maximum|USD|700.00|", quotationLines[5].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();
			Line.TL_WeightVolume = RatingConstants.Units.CN;
			TestCalculator["-10"] = (ZDecimal)90m;
			TestCalculator["+10"] = (ZDecimal)80m;
			TestCalculator["+20"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 10 Container(s)|USD|90.00|per Container", quotationLines[1].ToString());
			AssertEquals("10 Container(s) to less than 20 Container(s)|USD|80.00|per Container", quotationLines[2].ToString());
			AssertEquals("20 Container(s) and above|USD|70.00|per Container", quotationLines[3].ToString());

			AssertEquals(Calculator.Items.Operator.Minus, Line.RateLineItems[0].TM_Type);
			Line.RateLineItems[0].TM_BreakWeightVolume = RatingConstants.Units.M3;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 10 W/M|USD|90.00|per Container", quotationLines[1].ToString());
			AssertEquals("10 W/M to less than 20 W/M|USD|80.00|per Container", quotationLines[2].ToString());
			AssertEquals("20 W/M and above|USD|70.00|per Container", quotationLines[3].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();
			Line.TL_WeightVolume = RatingConstants.Units.KG;
			TestCalculator["-45"] = (ZDecimal)4m;
			TestCalculator["+45"] = (ZDecimal)3m;
			TestCalculator["+100"] = (ZDecimal)2.5m;
			TestCalculator.UseInclusiveBreaks = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 45 W/M|USD|4.00|per W/M", quotationLines[1].ToString());
			AssertEquals("More than 45 W/M to 100 W/M|USD|3.00|per W/M", quotationLines[2].ToString());
			AssertEquals("More than 100 W/M|USD|2.50|per W/M", quotationLines[3].ToString());

			var callForPriceItem = TestCalculator.AddRateLineItem("+", 250m, 0m, 0m);
			callForPriceItem.TM_CallForPricing = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 45 W/M|USD|4.00|per W/M", quotationLines[1].ToString());
			AssertEquals("More than 45 W/M to 100 W/M|USD|3.00|per W/M", quotationLines[2].ToString());
			AssertEquals("More than 100 W/M to 250 W/M|USD|2.50|per W/M", quotationLines[3].ToString());
			AssertEquals("More than 250 W/M||Call for Price|", quotationLines[4].ToString());

			callForPriceItem.TM_Text = "Some Reason";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals("More than 250 W/M||Some Reason|", quotationLines[4].ToString());
		}

		public void TestQuotationLinesWeightRange()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			ChargeCode.AC_SuppressOnQuoteIfZero = true;

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			var item1 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100m, 0m, 0m);
			var item2 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 0m, 60m);
			var item3 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1000m, 0m, 0m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("100 KG to less than 1000 KG|USD|60.00|", quotationLines[1].ToString());
			AssertEquals("1000 KG and above||Not Charged|", quotationLines[2].ToString());

			TestCalculator.Minimum = 100m;
			item1.TM_FlatAmount = 50m;
			item2.TM_FlatAmount = 60m;
			item3.TM_FlatAmount = 70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 100 KG|USD|50.00|", quotationLines[2].ToString());
			AssertEquals("100 KG to less than 1000 KG|USD|60.00|", quotationLines[3].ToString());
			AssertEquals("1000 KG and above|USD|70.00|", quotationLines[4].ToString());

			var item4 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 2000m, 0.45m, 0m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 100 KG|USD|50.00|", quotationLines[2].ToString());
			AssertEquals("100 KG to less than 1000 KG|USD|60.00|", quotationLines[3].ToString());
			AssertEquals("1000 KG to less than 2000 KG|USD|70.00|", quotationLines[4].ToString());
			AssertEquals("2000 KG and above|USD|0.45|per KG", quotationLines[5].ToString());

			item4.TM_FlatAmount = 20m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(7, quotationLines.Count);
			AssertEquals("2000 KG and above|USD|0.45|per KG", quotationLines[5].ToString());
			AssertEquals("Flat Amount|USD|20.00|", quotationLines[6].ToString());
		}

		public void TestQuotationLinesForPivot()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			TestCalculator.IsAccumulated = true;
			TestCalculator.UseInclusiveBreaks = true;

			var item1 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 500m, 0m, 1000m);
			var item2 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 500m, 0m, 1500m);
			var item3 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1000m, 5m, 1500m);
			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 500 KG|USD|1000.00|", quotationLines[1].ToString());
			AssertEquals("More than 500 KG to 1000 KG|USD|1500.00|", quotationLines[2].ToString());
			AssertEquals("More than 1000 KG|USD|5.00|per additional KG", quotationLines[3].ToString());

			Line.Parent.TI_Mode = "ULD";
			Line.TL_WeightVolume = "KG";

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 500 KG|USD|1000.00|", quotationLines[1].ToString());
			AssertEquals("More than 500 KG to 1000 KG|USD|1500.00|", quotationLines[2].ToString());
			AssertEquals("Over Pivot Rate|USD|5.00|per KG", quotationLines[3].ToString());

			item2.TM_FlatAmount = 1400m;

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Up to 500 KG|USD|1000.00|", quotationLines[1].ToString());
			AssertEquals("More than 500 KG to 1000 KG|USD|1400.00|", quotationLines[2].ToString());
			AssertEquals("More than 1000 KG|USD|5.00|per additional KG", quotationLines[3].ToString());
			AssertEquals("Flat Amount|USD|1500.00|", quotationLines[4].ToString());
		}

		public void TestQuotationLinesDecimals()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var item1 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 1.5, 1000m, 0m);
			var item2 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1.5, 1500m, 0m);
			var item3 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 3.75, 2000, 0m);
			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 1.5 KG|USD|1000.00|per KG", quotationLines[1].ToString());
			AssertEquals("1.5 KG to less than 3.75 KG|USD|1500.00|per KG", quotationLines[2].ToString());
			AssertEquals("3.75 KG and above|USD|2000.00|per KG", quotationLines[3].ToString());
		}

		public void TestCalculation()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.Minimum = 50m;
			TestCalculator["-45"] = (ZDecimal)2.00m;
			TestCalculator["+45"] = (ZDecimal)1.75m;
			TestCalculator["+100"] = (ZDecimal)1.50m;
			TestCalculator["+250"] = (ZDecimal)1.25m;
			TestCalculator["+500"] = (ZDecimal)1.00m;
			TestCalculator["+1000"] = (ZDecimal)0.75m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, 0, "");
			AssertCalculation(parameters, 0m, "0 Kilogram(s) @ AUD 2.00/KG");

			parameters.ChargeableAmount = new Quantity(15M, QuantityUnit.KG);
			AssertCalculation(parameters, 50m, "Minimum AUD 50.00");

			parameters.ChargeableAmount = new Quantity(30M, QuantityUnit.KG);
			AssertCalculation(parameters, 60m, "30 Kilogram(s) @ AUD 2.00/KG");

			parameters.ChargeableAmount = new Quantity(45M, QuantityUnit.KG);
			AssertCalculation(parameters, 78.75m, "45 Kilogram(s) @ AUD 1.75/KG");

			parameters.ChargeableAmount = new Quantity(99M, QuantityUnit.KG);
			AssertCalculation(parameters, 173.25m, "99 Kilogram(s) @ AUD 1.75/KG");

			parameters.ChargeableAmount = new Quantity(100M, QuantityUnit.KG);
			AssertCalculation(parameters, 150m, "100 Kilogram(s) @ AUD 1.50/KG");

			parameters.ChargeableAmount = new Quantity(999M, QuantityUnit.KG);
			AssertCalculation(parameters, 999m, "999 Kilogram(s) @ AUD 1.00/KG");

			parameters.ChargeableAmount = new Quantity(1000M, QuantityUnit.KG);
			AssertCalculation(parameters, 750m, "1000 Kilogram(s) @ AUD 0.75/KG");

			parameters.ChargeableAmount = new Quantity(2000M, QuantityUnit.KG);
			AssertCalculation(parameters, 1500m, "2000 Kilogram(s) @ AUD 0.75/KG");

			TestCalculator.BaseRate = 50m;
			AssertCalculation(parameters, 1550m, "Base Rate AUD 50.00 + 2000 Kilogram(s) @ AUD 0.75/KG");

			TestCalculator.Maximum = 1550m;
			AssertCalculation(parameters, 1550m, "Maximum AUD 1550.00");

			TestCalculator.Maximum = 1000m;
			AssertCalculation(parameters, 1000m, "Maximum AUD 1000.00");

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.PerUnit = 5m;
			TestCalculator.Maximum = 500m;
			parameters.ChargeableAmount = new Quantity(50M, QuantityUnit.KG);
			AssertCalculation(parameters, 250m, "50 Kilogram(s) @ AUD 5.00/KG");

			parameters.ChargeableAmount = new Quantity(100M, QuantityUnit.KG);
			AssertCalculation(parameters, 500m, "Maximum AUD 500.00");

			parameters.ChargeableAmount = new Quantity(101M, QuantityUnit.KG);
			AssertCalculation(parameters, 500m, "Maximum AUD 500.00");

			Line.RateLineItems.RemoveAndDeleteAll();

			Line.TL_WeightVolume = "KG";
			Line.TL_WeightVolumeMultiple = 100;
			Line.TL_Rounding = RatingRoundingTypes.UpTo1;
			TestCalculator["-100"] = (ZDecimal)20m;
			TestCalculator["+100"] = (ZDecimal)19m;
			TestCalculator["+300"] = (ZDecimal)18m;
			TestCalculator["+700"] = (ZDecimal)17m;
			TestCalculator["+1000"] = (ZDecimal)16m;

			RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			parameters.ChargeableAmount = new Quantity(80, QuantityUnit.KG);
			AssertCalculation(parameters, 19m, "100 Kilogram(s) @ AUD 19.00/100 KG");

			parameters.ChargeableAmount = new Quantity(100, QuantityUnit.KG);
			AssertCalculation(parameters, 19m, "100 Kilogram(s) @ AUD 19.00/100 KG");

			parameters.ChargeableAmount = new Quantity(102, QuantityUnit.KG);
			AssertCalculation(parameters, 38m, "200 Kilogram(s) @ AUD 19.00/100 KG");

			parameters.ChargeableAmount = new Quantity(300, QuantityUnit.KG);
			AssertCalculation(parameters, 54m, "300 Kilogram(s) @ AUD 18.00/100 KG");

			parameters.ChargeableAmount = new Quantity(301, QuantityUnit.KG);
			AssertCalculation(parameters, 72m, "400 Kilogram(s) @ AUD 18.00/100 KG");

			parameters.ChargeableAmount = new Quantity(1000, QuantityUnit.KG);
			AssertCalculation(parameters, 160m, "1000 Kilogram(s) @ AUD 16.00/100 KG");

			parameters.ChargeableAmount = new Quantity(1050, QuantityUnit.KG);
			AssertCalculation(parameters, 176m, "1100 Kilogram(s) @ AUD 16.00/100 KG");

			Line.TL_Rounding = RatingRoundingTypes.NoRounding;
			TestCalculator.UseInclusiveBreaks = true;
			parameters.ChargeableAmount = new Quantity(80, QuantityUnit.KG);
			AssertCalculation(parameters, 16m, "80 Kilogram(s) @ AUD 20.00/100 KG");

			parameters.ChargeableAmount = new Quantity(100, QuantityUnit.KG);
			AssertCalculation(parameters, 20m, "100 Kilogram(s) @ AUD 20.00/100 KG");

			parameters.ChargeableAmount = new Quantity(110, QuantityUnit.KG);
			AssertCalculation(parameters, 20.9m, "110 Kilogram(s) @ AUD 19.00/100 KG");

			parameters.ChargeableAmount = new Quantity(300, QuantityUnit.KG);
			AssertCalculation(parameters, 57m, "300 Kilogram(s) @ AUD 19.00/100 KG");

			parameters.ChargeableAmount = new Quantity(1000, QuantityUnit.KG);
			AssertCalculation(parameters, 170m, "1000 Kilogram(s) @ AUD 17.00/100 KG");

			parameters.ChargeableAmount = new Quantity(1100, QuantityUnit.KG);
			AssertCalculation(parameters, 176m, "1100 Kilogram(s) @ AUD 16.00/100 KG");

			Line.RateLineItems.RemoveAndDeleteAll();

			Line.TL_WeightVolume = "KG";
			Line.TL_WeightVolumeMultiple = 0;
			Line.TL_Rounding = RatingRoundingTypes.NoRounding;
			TestCalculator.IsAccumulated = true;
			TestCalculator["-100"] = (ZDecimal)4m;
			TestCalculator["+100"] = (ZDecimal)3m;
			TestCalculator["+200"] = (ZDecimal)2m;
			TestCalculator["+500"] = (ZDecimal)1m;

			parameters.ChargeableAmount = new Quantity(1m, QuantityUnit.KG);
			AssertCalculation(parameters, 4m, "1 Kilogram(s) @ AUD 4.00/KG");

			parameters.ChargeableAmount = new Quantity(50m, QuantityUnit.KG);
			AssertCalculation(parameters, 200m, "50 Kilogram(s) @ AUD 4.00/KG");

			parameters.ChargeableAmount = new Quantity(60m, QuantityUnit.KG);
			AssertCalculation(parameters, 240m, "60 Kilogram(s) @ AUD 4.00/KG");

			parameters.ChargeableAmount = new Quantity(150m, QuantityUnit.KG);
			AssertCalculation(parameters, 550m, "100 Kilogram(s) @ AUD 4.00/KG + 50 Kilogram(s) @ AUD 3.00/KG");

			parameters.ChargeableAmount = new Quantity(250m, QuantityUnit.KG);
			AssertCalculation(parameters, 800m, "100 Kilogram(s) @ AUD 4.00/KG + 100 Kilogram(s) @ AUD 3.00/KG + 50 Kilogram(s) @ AUD 2.00/KG");

			parameters.ChargeableAmount = new Quantity(500m, QuantityUnit.KG);
			AssertCalculation(parameters, 1300m, "100 Kilogram(s) @ AUD 4.00/KG + 100 Kilogram(s) @ AUD 3.00/KG + 300 Kilogram(s) @ AUD 2.00/KG");

			parameters.ChargeableAmount = new Quantity(550m, QuantityUnit.KG);
			AssertCalculation(parameters, 1350m, "100 Kilogram(s) @ AUD 4.00/KG + 100 Kilogram(s) @ AUD 3.00/KG + 300 Kilogram(s) @ AUD 2.00/KG + 50 Kilogram(s) @ AUD 1.00/KG");

			Line.RateLineItems.RemoveAndDeleteAll();

			Line.TL_WeightVolume = "KG";
			Line.TL_WeightVolumeMultiple = 100;
			Line.TL_Rounding = RatingRoundingTypes.UpTo1;
			TestCalculator.IsAccumulated = true;
			TestCalculator["-200"] = (ZDecimal)40m;
			TestCalculator["+200"] = (ZDecimal)30m;
			TestCalculator["+500"] = (ZDecimal)20m;
			TestCalculator["+1000"] = (ZDecimal)10m;

			parameters.ChargeableAmount = new Quantity(250m, QuantityUnit.KG);
			AssertCalculation(parameters, 110m, "200 Kilogram(s) @ AUD 40.00/100 KG + 100 Kilogram(s) @ AUD 30.00/100 KG");
		}

		public void TestCalculationPerTeu()
		{
			Line.Parent.TI_Mode = Core.Constants.RateMode.ALL;
			Line.TL_WeightVolume = QuantityUnit.TU;
			Line.TL_RX_NKCurrency = "AUD";

			var measures = Criteria.RateableMeasures;
			new TestContainers(Factory, "20GP",
				new[]
				{
					new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 1, "CONT00001"),
					new MeasureInfo.ContainerInfo(4000m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 40, 1, "CONT00002"),
					new MeasureInfo.ContainerInfo(3000m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 3, 1, "CONT00003")
				}).PopulateContainerList(measures);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.Minimum = 50m;
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			AssertCalculation(parameters, 300m, "3 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit");

			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = QuantityUnit.TU;

			new TestContainers(Factory, "20GP",
				new[]
				{
					new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 1, "CONT00004"),
					new MeasureInfo.ContainerInfo(4000m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 40, 1, "CONT00005"),
					new MeasureInfo.ContainerInfo(3000m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 3, 0, "CONT00006")
				}).PopulateContainerList(measures);

			AssertCalculation(parameters, "containers in this job missing TEU information.");

			new TestContainers(Factory, "20GP",
				new[]
				{
					new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 3, "CONT00006"),
					new MeasureInfo.ContainerInfo(4000m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 40, 2, "CONT00007"),
					new MeasureInfo.ContainerInfo(3000m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 3, 2.3m, "CONT00008")
				}).PopulateContainerList(measures);

			AssertCalculation(parameters, 300m, "3 Container(s) @ AUD 100.00/Container");

			new TestContainers(Factory, "20GP",
				new[]
				{
					new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 6, "CONT00009"),
					new MeasureInfo.ContainerInfo(4000m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 40, 2, "CONT00010"),
					new MeasureInfo.ContainerInfo(3000m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 3, 7.3m, "CONT00011")
				}).PopulateContainerList(measures);

			AssertCalculation(parameters, 340m, "2 Container(s) @ AUD 120.00/Container + 1 Container(s) @ AUD 100.00/Container");
		}

		public void TestCalculationPerContainer()
		{
			Line.TL_WeightVolume = RatingConstants.Units.CN;
			Line.TL_RX_NKCurrency = "AUD";
			Line.UseOnlyActualWeightMeasure = true;

			var measures = Criteria.RateableMeasures;
			new TestContainers(Factory, "20GP", 3).PopulateContainerList(measures);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.Minimum = 50m;
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			AssertCalculation(parameters, 300m, "3 Container(s) @ AUD 100.00/Container");

			Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = RatingConstants.Units.M3;
			AssertCalculation(parameters, "Weight / Volume / Packages information was not specified for Pack Lines on this job. Rating based on container weight / volume / packages cannot be performed.");

			new TestContainers(Factory, "20GP",
				new MeasureInfo.ContainerInfo[]
				{
					new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 0, "CONT00001"),
					new MeasureInfo.ContainerInfo(4000m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 40, 0, "CONT00002"),
					new MeasureInfo.ContainerInfo(3000m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 3, 0, "CONT00003")
				}).PopulateContainerList(measures);
			AssertCalculation(parameters, 340m, "2 Container(s) @ AUD 120.00/Container + 1 Container(s) @ AUD 100.00/Container");

			Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = "T";
			AssertCalculation(parameters, 320m, "1 Container(s) @ AUD 120.00/Container + 2 Container(s) @ AUD 100.00/Container");

			Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = "PK";
			AssertCalculation(parameters, 340m, "2 Container(s) @ AUD 120.00/Container + 1 Container(s) @ AUD 100.00/Container");

			new TestContainers(Factory, "20GP", 1).PopulateContainerList(measures);
			AssertCalculation(parameters, "Weight / Volume / Packages information was not specified for Pack Lines on this job. Rating based on container weight / volume / packages cannot be performed.");

			Criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			AssertCalculation(parameters, "Warehouse Periodic - Weight / Volume / Packages information was not specified on this job.");

			Criteria.ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			AssertCalculation(parameters, "Weight information was not specified for the Containers on this job. Please enter weight details on the Containers tab (on the Declaration screen) and then autorate again.");

			var measureInfos = new[] { new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres, 1, 0, "CONT00004") };
			new TestContainers(Factory, "20GP", measureInfos).PopulateContainerList(measures);
			AssertCalculation(parameters, "declaration charge (CCC) as it is based on container volume or packages. Declaration charges can only be based on container weight.");

			Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = "T";
			AssertCalculation(parameters, 120m, "1 Container(s) @ AUD 120.00/Container");
		}

		public void TestCalculationWeightRange()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.Minimum = 300m;
			var item1 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100m, 0m, 250m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 0m, 350m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 250m, 0m, 450m);
			var item3 = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 500m, 0m, 550m);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(0.1M, QuantityUnit.KG);
			AssertCalculation(parameters, 300m, "Minimum AUD 300.00");

			parameters.ChargeableAmount = new Quantity(100M, QuantityUnit.KG);
			AssertCalculation(parameters, 350m, "Base Rate AUD 350.00 for 100 KG");

			parameters.ChargeableAmount = new Quantity(100.1M, QuantityUnit.KG);
			AssertCalculation(parameters, 350m, "Base Rate AUD 350.00 for 100.1 KG");

			parameters.ChargeableAmount = new Quantity(250, QuantityUnit.KG);
			AssertCalculation(parameters, 450m, "Base Rate AUD 450.00 for 250 KG");

			parameters.ChargeableAmount = new Quantity(250.1M, QuantityUnit.KG);
			AssertCalculation(parameters, 450m, "Base Rate AUD 450.00 for 250.1 KG");

			parameters.ChargeableAmount = new Quantity(500, QuantityUnit.KG);
			AssertCalculation(parameters, 550m, "Base Rate AUD 550.00 for 500 KG");

			parameters.ChargeableAmount = new Quantity(500.1M, QuantityUnit.KG);
			AssertCalculation(parameters, 550m, "Base Rate AUD 550.00 for 500.1 KG");

			parameters.ChargeableAmount = new Quantity(1000M, QuantityUnit.KG);
			AssertCalculation(parameters, 550m, "Base Rate AUD 550.00 for 1000 KG");

			Line.RateLineItems.RemoveAndDelete(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			parameters.ChargeableAmount = new Quantity(0.1M, QuantityUnit.KG);
			AssertCalculation(parameters, 250m, "Base Rate AUD 250.00 for 0.1 KG");

			item3.TM_RelevantValue = 0.5m;
			parameters.ChargeableAmount = new Quantity(1000M, QuantityUnit.KG);
			AssertCalculation(parameters, 1050m, "Base Rate AUD 550.00 + 1000 Kilogram(s) @ AUD 0.50/KG");

			item3.TM_FlatAmount = 0m;
			AssertCalculation(parameters, 500m, "1000 Kilogram(s) @ AUD 0.50/KG");
		}

		public void TestCalculationUsingHigherChargeableLowerRateRule()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.Minimum = 50m;
			TestCalculator["-45"] = (ZDecimal)4m;
			TestCalculator["+45"] = (ZDecimal)3.5m;
			TestCalculator["+100"] = (ZDecimal)3m;
			TestCalculator["+250"] = (ZDecimal)2.5m;
			TestCalculator["+500"] = (ZDecimal)2m;
			TestCalculator["+1000"] = (ZDecimal)1.75m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(40M, QuantityUnit.KG);
			AssertCalculation(parameters, 160m, "40 Kilogram(s) @ AUD 4.00/KG");
			TestCalculator.UseHigherChargeableLowerRateRule = true;
			AssertCalculation(parameters, 157.5m, "45 Kilogram(s) (HBLR is applied) @ AUD 3.50/KG");

			parameters.ChargeableAmount = new Quantity(200M, QuantityUnit.KG);
			AssertCalculation(parameters, 600m, "200 Kilogram(s) @ AUD 3.00/KG");

			parameters.ChargeableAmount = new Quantity(240M, QuantityUnit.KG);
			AssertCalculation(parameters, 625m, "250 Kilogram(s) (HBLR is applied) @ AUD 2.50/KG");
			TestCalculator.UseHigherChargeableLowerRateRule = false;
			AssertCalculation(parameters, 720m, "240 Kilogram(s) @ AUD 3.00/KG");

			parameters.ChargeableAmount = new Quantity(1100M, QuantityUnit.KG);
			AssertCalculation(parameters, 1925m, "1100 Kilogram(s) @ AUD 1.75/KG");
			TestCalculator.UseHigherChargeableLowerRateRule = true;
			AssertCalculation(parameters, 1925m, "1100 Kilogram(s) @ AUD 1.75/KG");

			TestCalculator.RateLineItems[TestCalculator.RateLineItems.Count - 3].TM_FlatAmount = 50m;
			TestCalculator.RateLineItems[TestCalculator.RateLineItems.Count - 2].TM_FlatAmount = 300m;
			parameters.ChargeableAmount = new Quantity(480M, QuantityUnit.KG);
			AssertCalculation(parameters, 1250m, "Base Rate AUD 50.00 + 480 Kilogram(s) @ AUD 2.50/KG");
			TestCalculator.RateLineItems[TestCalculator.RateLineItems.Count - 2].TM_FlatAmount = 50m;
			AssertCalculation(parameters, 1050m, "Base Rate AUD 50.00 + 500 Kilogram(s) (HBLR is applied) @ AUD 2.00/KG");
		}

		#region TestHigherChargeableLowerRateComparison

		public void TestHigherChargeableLowerRateComparison_WithoutUnitMultiplier_LowerChargeableCheaper()
		{
			var parameters = SetupForHigherChargeableLowerRateComparison(50, null);
			AssertCalculation(parameters, 1000m, "50 Kilogram(s) @ AUD 20.00/KG");
		}

		public void TestHigherChargeableLowerRateComparison_WithoutUnitMultiplier_HigherChargeableCheaper()
		{
			var parameters = SetupForHigherChargeableLowerRateComparison(150, null);
			AssertCalculation(parameters, 2000m, "200 Kilogram(s) (HBLR is applied) @ AUD 10.00/KG");
		}

		public void TestHigherChargeableLowerRateComparison_WithUnitMultiplier_LowerChargeableCheaper()
		{
			var parameters = SetupForHigherChargeableLowerRateComparison(50, 100);
			AssertCalculation(parameters, 10m, "50 Kilogram(s) @ AUD 20.00/100 KG");
		}

		public void TestHigherChargeableLowerRateComparison_WithUnitMultiplier_HigherChargeableCheaper()
		{
			var parameters = SetupForHigherChargeableLowerRateComparison(150, 100);
			AssertCalculation(parameters, 20m, "200 Kilogram(s) (HBLR is applied) @ AUD 10.00/100 KG");
		}

		AutoRatingCalculatorParametersForTesting SetupForHigherChargeableLowerRateComparison(ZDecimal chargeableAmount, ZDecimal? unitMultiple = null)
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator["-45"] = (ZDecimal)30m;
			TestCalculator["+45"] = (ZDecimal)20m;
			TestCalculator["+200"] = (ZDecimal)10m;
			TestCalculator.UseHigherChargeableLowerRateRule = true;

			if (unitMultiple.HasValue)
			{
				Line.TL_WeightVolumeMultiple = unitMultiple.Value;
			}

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria)
			{
				ChargeableAmount = new Quantity(chargeableAmount, QuantityUnit.KG)
			};

			return calculatorParams;
		}

		#endregion

		public void TestCalculationNonPrintableItemBetweenWeightBreaks()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.Minimum = 50m;
			TestCalculator["-45"] = (ZDecimal)4m;
			TestCalculator["+45"] = (ZDecimal)3.5m;
			TestCalculator["+100"] = (ZDecimal)3m;
			TestCalculator["+250"] = (ZDecimal)2.5m;

			Line.RateLineItems.Load();

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(150M, QuantityUnit.KG);
			AssertCalculation(parameters, 450m, "150 Kilogram(s) @ AUD 3.00/KG");
		}

		public void TestCalculationWithFlatAmountAndMultipled()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_WeightVolumeMultiple = 100m;
			Line.TL_Rounding = RatingRoundingTypes.UpTo1;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 100m, 0m, 10m);
			TestCalculator.AddRateLineItem("+", 100m, 0m, 15m);
			TestCalculator.AddRateLineItem("+", 200m, 0m, 17m);
			TestCalculator.AddRateLineItem("+", 400m, 0.5m, 20m);

			RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(40m, QuantityUnit.KG);
			AssertCalculation(parameters, 15m, "Base Rate AUD 15.00 for 100 KG");
			parameters.ChargeableAmount = new Quantity(150m, QuantityUnit.KG);
			AssertCalculation(parameters, 17m, "Base Rate AUD 17.00 for 200 KG");
			parameters.ChargeableAmount = new Quantity(270m, QuantityUnit.KG);
			AssertCalculation(parameters, 17m, "Base Rate AUD 17.00 for 300 KG");
			parameters.ChargeableAmount = new Quantity(460m, QuantityUnit.KG);
			AssertCalculation(parameters, 22.5m, "Base Rate AUD 20.00 + 500 Kilogram(s) @ AUD 0.50/100 KG");

			TestCalculator.IsAccumulated = true;
			Assert("PreCondition", TestCalculator.UseInclusiveBreaks);
			parameters.ChargeableAmount = new Quantity(40m, QuantityUnit.KG);
			AssertCalculation(parameters, 10m, "Base Rate AUD 10.00");
			parameters.ChargeableAmount = new Quantity(100m, QuantityUnit.KG);
			AssertCalculation(parameters, 10m, "Base Rate AUD 10.00");
			parameters.ChargeableAmount = new Quantity(150m, QuantityUnit.KG);
			AssertCalculation(parameters, 15m, "Base Rate AUD 15.00");
			parameters.ChargeableAmount = new Quantity(200m, QuantityUnit.KG);
			AssertCalculation(parameters, 15m, "Base Rate AUD 15.00");
			parameters.ChargeableAmount = new Quantity(220m, QuantityUnit.KG);
			AssertCalculation(parameters, 17m, "Base Rate AUD 17.00");
			parameters.ChargeableAmount = new Quantity(400m, QuantityUnit.KG);
			AssertCalculation(parameters, 17m, "Base Rate AUD 17.00");
			parameters.ChargeableAmount = new Quantity(460m, QuantityUnit.KG);
			AssertCalculation(parameters, 20.5m, "Base Rate AUD 20.00 + 100 Kilogram(s) @ AUD 0.50/100 KG");
			parameters.ChargeableAmount = new Quantity(560m, QuantityUnit.KG);
			AssertCalculation(parameters, 21m, "Base Rate AUD 20.00 + 200 Kilogram(s) @ AUD 0.50/100 KG");
		}

		#region FirstPlusSubstituteForMinus

		public void TestFirstPlusSubstituteForMinus()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			TestCalculator.AddRateLineItem("+", 45m, 4.00m, 0m);
			TestCalculator.AddRateLineItem("+", 100m, 2.00m, 0m);

			parameters.ChargeableAmount = new Quantity(15m, QuantityUnit.KG);
			AssertCalculation(parameters, 60m, "15 Kilogram(s) @ AUD 4.00/KG");

			parameters.ChargeableAmount = new Quantity(60m, QuantityUnit.KG);
			AssertCalculation(parameters, 240m, "60 Kilogram(s) @ AUD 4.00/KG");

			parameters.ChargeableAmount = new Quantity(110m, QuantityUnit.KG);
			AssertCalculation(parameters, 220m, "110 Kilogram(s) @ AUD 2.00/KG");

			TestCalculator.AddRateLineItem("+", 250m, 1.00m, 0m);

			parameters.ChargeableAmount = new Quantity(300, QuantityUnit.KG);
			AssertCalculation(parameters, 300m, "300 Kilogram(s) @ AUD 1.00/KG");
		}

		public void TestFirstPlusSubstituteForMinus_AccumulatedCalculation()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.AddRateLineItem("+", 45m, 4.00m, 0m);
			TestCalculator.AddRateLineItem("+", 100m, 2.00m, 0m);
			TestCalculator.AddRateLineItem("+", 250m, 1.00m, 0m);
			TestCalculator.IsAccumulated = true;

			parameters.ChargeableAmount = new Quantity(15m, QuantityUnit.KG);
			AssertCalculation(parameters, 60m, "15 Kilogram(s) @ AUD 4.00/KG");

			parameters.ChargeableAmount = new Quantity(110m, QuantityUnit.KG);
			AssertCalculation(parameters, 420m, "100 Kilogram(s) @ AUD 4.00/KG + 10 Kilogram(s) @ AUD 2.00/KG");

			parameters.ChargeableAmount = new Quantity(300, QuantityUnit.KG);
			AssertCalculation(parameters, 750m, "100 Kilogram(s) @ AUD 4.00/KG + 150 Kilogram(s) @ AUD 2.00/KG + 50 Kilogram(s) @ AUD 1.00/KG");
		}

		public void TestFirstPlusSubstituteForMinus_CalculationWithCallForPricingFlag()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			TestCalculator.AddRateLineItem("+", 45m, 4.00m, 0m);

			var secondPlusItem = TestCalculator.AddRateLineItem("+", 100m, 2.00m, 0m);
			secondPlusItem.TM_CallForPricing = true;
			secondPlusItem.TM_Text = "Call us for pricing";
			parameters.ChargeableAmount = new Quantity(110m, QuantityUnit.KG);
			using (_Rating.Start(new LoggerDecorator()))
			{
				AssertExceptionThrown<AutoRater.CallForPriceException>(() => TestCalculator.Calculate(parameters));

				TestCalculator.AddRateLineItem("+", 250m, 1.00m, 0m);

				parameters.ChargeableAmount = new Quantity(300, QuantityUnit.KG);
				AssertCalculation(parameters, 300m, "300 Kilogram(s) @ AUD 1.00/KG");
			}
		}

		#endregion

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Minimum = 50m;
			TestCalculator.PerUnit = 5m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			source.Minimum = 100m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(Line.RateLineItems.Count, clonedLine.RateLineItems.Count);
			AssertEquals(150m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);

			source.Minimum = 0m;
			source.BaseRate = 25m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(Line.RateLineItems.Count + 1, clonedLine.RateLineItems.Count);
			AssertEquals(50m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(25m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			TestCalculator.BaseRate = 40m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(Line.RateLineItems.Count, clonedLine.RateLineItems.Count);
			AssertEquals(50m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(65m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			source.PerUnit = 0m;
			source.PerUnitPercent = 20m;
			source.Percent = 20m;
			source.PerUnitPercent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(Line.RateLineItems.Count, clonedLine.RateLineItems.Count);
			AssertEquals(60m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(6m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(73m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(60m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(6m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(78m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT);
			TestCalculator["-45"] = (ZDecimal)6m;
			TestCalculator["+45"] = (ZDecimal)5m;
			TestCalculator["+100"] = (ZDecimal)4m;
			TestCalculator["+250"] = (ZDecimal)3m;
			TestCalculator.IsAccumulated = true;

			source.Percent = 0m;
			source.PerUnitPercent = 0m;
			source.BaseRate = 70m;
			source["-100"] = (ZDecimal)2m;
			source["+100"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(70m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);
			AssertEquals(8m, clonedLine.Calculator["-45"]);
			AssertEquals(7m, clonedLine.Calculator["+45"]);
			AssertEquals(5m, clonedLine.Calculator["+100"]);
			AssertEquals(4m, clonedLine.Calculator["+250"]);
			Assert(clonedLine.GetCalculator<CombinedCalculator>().IsAccumulated);
		}

		public void TestValidateRateOperator()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = CombinedCalculator.Code;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("DST");

			var dSTRateLine = testEntry.RateLines.AddNew();
			dSTRateLine.TL_AC = dummyChargeCode.PK;

			var line1 = dSTRateLine.RateLineItems.AddNew();
			line1.TM_Type = Calculator.Items.Operator.MAX;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			var line2 = dSTRateLine.RateLineItems.AddNew();

			line1.TM_Type = Calculator.Items.Operator.Plus;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Should have no errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.UNT;
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			var line3 = dSTRateLine.RateLineItems.AddNew();

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError("UNT and sliding operators are incompatible", line3.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("It's okay for BAS to be after Minus", false, line3.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.UNT;
			line2.TM_Type = Calculator.Items.Operator.Plus;
			line3.TM_Type = Calculator.Items.Operator.Minus;
			AssertHasError(line2.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			var line4 = dSTRateLine.RateLineItems.AddNew();

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Why not!", false, line4.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line4.TM_TypeInfo.HasErrors());

			line3.TM_Type = ZString.Empty;
			line4.TM_Type = ZString.Empty;

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.Plus;
			line3.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.Plus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());
		}

		public void TestValidateRateOperatorFRTAIROrLCL()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = CombinedCalculator.Code;

			var testRate = Factory.New<ClientRate>();
			var testRateEntry = testRate.AddRateEntry("AIR");
			var aIRRateLine = testRateEntry.RateLines.AddNew();

			aIRRateLine.TL_AC = dummyChargeCode.PK;

			var line1 = aIRRateLine.RateLineItems.AddNew();
			line1.TM_RelevantValue = 50M;
			line1.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			var line2 = aIRRateLine.RateLineItems.AddNew();
			line2.TM_RelevantValue = 5M;

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.BAS;

			var line3 = aIRRateLine.RateLineItems.AddNew();
			line3.TM_RelevantValue = 4M;

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			var line4 = aIRRateLine.RateLineItems.AddNew();
			line4.TM_RelevantValue = 3.5M;

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line4.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line4.TM_TypeInfo.HasErrors());
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.Minimum = 100m;
			TestCalculator.BaseRate = 50m;
			TestCalculator.PerUnit = 5m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(3, items.Count);
			AssertChargesSummaryItem(items[0], "MIN", 100m);
			AssertChargesSummaryItem(items[1], "BAS", 50m);
			AssertChargesSummaryItem(items[2], "UNT", 5m);

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100m, 4m, 0m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 3m, 0m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 250m, 0m, 200m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1000m, 0m, 500m);

			items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(4, items.Count);
			AssertChargesSummaryItem(items[0], "-", 100m, 4m, 0m);
			AssertChargesSummaryItem(items[1], "+", 100m, 3m, 0m);
			AssertChargesSummaryItem(items[2], "+", 250m, 0m, 200m);
			AssertChargesSummaryItem(items[3], "+", 1000m, 0m, 500m);
		}

		public void TestCalculationLog_Accumulated()
		{
			var calculator = TestCalculator;
			Line.TL_WeightVolume = "KG";
			Line.TL_AC = Env.Registry.FreightChargeCode;

			calculator.AddRateLineItem("-", 45m, 4m, 40m);
			calculator.AddRateLineItem("+", 45m, 3m, 30m);
			calculator.AddRateLineItem("+", 100m, 2m, 20m);
			calculator.AddRateLineItem("+", 300m, 1m, 10m);
			calculator.IsAccumulated = true;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(20m, QuantityUnit.KG);
			var calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			AssertEquals("Has calculation step", 1, calcLog.Steps.Count);
			calcLog.Steps[0].AssertPerUnit(20m, 4m, 40m);

			parameters.ChargeableAmount = new Quantity(200m, QuantityUnit.KG);
			calcLog.Steps.Clear();
			calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			AssertEquals("Has multiple calculation steps", 3, calcLog.Steps.Count);
			calcLog.Steps[0].AssertPerUnit(45m, 4m, 0m);
			calcLog.Steps[1].AssertPerUnit(55m, 3m, 0m);
			calcLog.Steps[2].AssertPerUnit(100m, 2m, 20m);

			parameters.ChargeableAmount = new Quantity(700m, QuantityUnit.KG);
			calcLog.Steps.Clear();
			calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			AssertEquals("Has multiple calculation steps", 4, calcLog.Steps.Count);
			calcLog.Steps[0].AssertPerUnit(45m, 4m, 0m);
			calcLog.Steps[1].AssertPerUnit(55m, 3m, 0m);
			calcLog.Steps[2].AssertPerUnit(200m, 2m, 0m);
			calcLog.Steps[3].AssertPerUnit(400m, 1m, 10m);
		}

		public void TestCalculationLog_DefaultsToLowestPlusBreakWhenLineHasNoMinusBreaks()
		{
			var calculator = TestCalculator;
			Line.TL_WeightVolume = "KG";
			Line.TL_AC = Env.Registry.FreightChargeCode;

			AssertEquals("Pre-condition:", 0, calculator.RateLineItems.Count);

			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 4m, 30m);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 2m, 20m);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 300m, 1m, 10m);

			AssertEquals(3, calculator.RateLineItems.Count);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(20m, QuantityUnit.KG);
			var calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			calcLog.Steps[0].AssertPerUnit(20m, 4m, 30m);

			parameters.ChargeableAmount = new Quantity(45m, QuantityUnit.KG);
			calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			calcLog.Steps[0].AssertPerUnit(45m, 4m, 30m);

			parameters.ChargeableAmount = new Quantity(50, QuantityUnit.KG);
			calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			calcLog.Steps[0].AssertPerUnit(50m, 4m, 30m);

			parameters.ChargeableAmount = new Quantity(200m, QuantityUnit.KG);
			calcLog = calculator.Calculate(parameters).results.Single().FreightChargeCodeCalculationLog;

			calcLog.Steps[0].AssertPerUnit(200m, 2m, 20m);
		}

		public void TestCalculationLog_CalculatePerContainerWithWeightVolumeBreaks()
		{
			var calculator = TestCalculator;
			calculator.AddRateLineItem("-", 200m, 40m, 400m).TM_BreakWeightVolume = QuantityUnit.KG;
			calculator.AddRateLineItem("+", 200m, 30m, 300m);

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_WeightVolume = QuantityUnit.CN;

			var measures = Criteria.RateableMeasures;
			new TestContainers(Factory, "20GP",
				new MeasureInfo.ContainerInfo[]
				{
					new MeasureInfo.ContainerInfo(100m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres, 0, 0, "CONT00001"),
					new MeasureInfo.ContainerInfo(500m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres, 0, 0, "CONT00002")
				}).PopulateContainerList(measures);

			var calcParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			var calcLog = calculator.Calculate(calcParams).results.Single().FreightChargeCodeCalculationLog;

			AssertEquals(QuantityUnit.CN, calcLog.Unit);
			AssertEquals(2, calcLog.Steps.Count);
			AssertEquals(440m, calcLog.Steps[0].Result);
			AssertEquals(400m, calcLog.Steps[0].Flat);
			AssertEquals(330m, calcLog.Steps[1].Result);
			AssertEquals(300m, calcLog.Steps[1].Flat);
		}

		public void TestCalculationLog_BreakPerContainerType()
		{
			var calculator = TestCalculator;
			calculator.AddRateLineItem("-", 300m, 0m, 1000m);
			calculator.AddRateLineItem("+", 300m, 1m, 1000m);
			calculator.IsAccumulated = true;
			calculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_WeightVolume = QuantityUnit.KG;

			var weight = new RateablePartList { HasContainerType = true, WeightUnit = "KG" };
			weight.AddPart(new RateablePart { Weight = 400, ContainerTypePk = NullableHelper.ToNullable(Helper.Containers["20GP"].PK), ContainerPK = Guid.NewGuid() });
			weight.AddPart(new RateablePart { Weight = 200, ContainerTypePk = NullableHelper.ToNullable(Helper.Containers["20GP"].PK), ContainerPK = Guid.NewGuid() });
			weight.AddPart(new RateablePart { Weight = 500, ContainerTypePk = NullableHelper.ToNullable(Helper.Containers["20GP"].PK), ContainerPK = Guid.NewGuid() });

			var measures = Criteria.RateableMeasures;
			measures.AddPartList(MeasureType.Weight, weight);

			var calcParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			var calcLog = calculator.Calculate(calcParams).results.Single().FreightChargeCodeCalculationLog;

			AssertEquals(QuantityUnit.KG, calcLog.Unit);

			var actualSteps = calcLog.Steps
				.Select(s => $"{s.UnitCount}|{s.UnitPrice}|{s.Result}|{s.Flat}")
				.ToArray();

			var expectedSteps = new[]
			{
				"900|0|0|0",
				"200|1|3200|3000",
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculation steps should match the expected values",
				expectedSteps,
				actualSteps
			);
		}

		public void TestCalculate_WhenServiceUnitWithNoServices_ReturnsEmpty()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var line = rateEntry.AddRateLine("ODOC", CombinedCalculator.Code, "SV");

			var items = line.RateLineItems;
			var plus25Item = items.AddNew();
			plus25Item.TM_Type = Calculator.Items.Operator.Minus;
			plus25Item.TM_Break = 25m;
			plus25Item.TM_Value = 18m;

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			line.Calculator.IsAccumulated = true;
			var results = line.Calculator.Calculate(parameters).results;

			AssertEquals(0, results.Count());
		}

		public override void TestPricePerSingleChargeable()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 4m, 40m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 3m, 30m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 2m, 20m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 300m, 1m, 10m);
			TestCalculator.IsAccumulated = true;
			Line.TL_AC = Env.Registry.FreightChargeCode;

			AssertEquals("AUD 4.0000", Line.ParentRateEntry.AllInCost());
			AssertEquals("AUD 4.0000/KG", Line.ParentRateEntry.FreightRatePerChargeableUnit());

			TestCalculator.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 4m, 30m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 2m, 20m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 300m, 1m, 10m);

			AssertEquals("AUD 4.0000", Line.ParentRateEntry.AllInCost());
			AssertEquals("AUD 4.0000/KG", Line.ParentRateEntry.FreightRatePerChargeableUnit());

			TestCalculator.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 4m, 30m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100m, 2m, 20m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 300m, 1m, 10m);

			AssertEquals("AUD 4.0000", Line.ParentRateEntry.AllInCost());
			AssertEquals("AUD 4.0000/KG", Line.ParentRateEntry.FreightRatePerChargeableUnit());

			TestCalculator.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.Minimum = 100m;
			TestCalculator.BaseRate = 50m;
			TestCalculator.PerUnit = 5m;

			AssertEquals("AUD 100.0000", Line.ParentRateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG", Line.ParentRateEntry.FreightRatePerChargeableUnit());

			TestCalculator.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.BaseRate = 50m;
			TestCalculator.PerUnit = 5m;

			AssertEquals("AUD 55.0000", Line.ParentRateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG", Line.ParentRateEntry.FreightRatePerChargeableUnit());
		}

		public void TestCalculate_UnitIsCN_PopulateCartageLegForOneContainerPerCharge()
		{
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.SCO;
			Line.Parent.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.TL_RX_NKCurrency = "AUD";

			var containers = new[]
			{
				new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 1, "CONT00001"),
			};

			var measures = Criteria.RateableMeasures;
			var cartageLegPK = ZGuid.NewZGuid();
			measures.AddContainerWithNumberAndCartageLeg(Line.Parent.TI_RC, "", cartageLegPK, containers[0]);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, Line, 0);
			var filteredParams = parameters.CreatedFilteredParametersByCartageLeg_ForTest(cartageLegPK);

			var expectedAttribute = new RateAttribute(JobChargeAttribTypeList.Codes.CartageLegPK, cartageLegPK.ToString(), 0);
			var result = AssertCalculation(filteredParams, 100, "1 20GP Container(s) @ AUD 100.00/Container");
			AssertCollectionContains(expectedAttribute, result.Attributes.Attributes);

			// Using original params with no filter stops attribute being added
			result = AssertCalculation(parameters, 100, "1 20GP Container(s) @ AUD 100.00/Container");
			AssertCollectionNotContains(expectedAttribute, result.Attributes.Attributes);
		}

		public void TestCalculate_UnitIsTU_PopulateCartageLegForOneContainerPerCharge()
		{
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.SCO;
			Line.Parent.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.Parent.TI_RH_NKCommodityCode = "HAZ";
			Line.Parent.TI_ContractNumber = "MCLAREN";
			Line.TL_WeightVolume = QuantityUnit.TU;
			Line.TL_RX_NKCurrency = "AUD";

			var containers = new[]
			{
				new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 1, "CONT00001"),
			};

			var measures = Criteria.RateableMeasures;
			var cartageLegPK = ZGuid.NewZGuid();
			measures.AddContainerWithNumberAndCartageLeg(Line.Parent.TI_RC, "", cartageLegPK, containers[0]);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, Line, 0);
			var filteredParams = parameters.CreatedFilteredParametersByCartageLeg_ForTest(cartageLegPK);

			var result = AssertCalculation(filteredParams, 100, "1 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit");
			var expectedAttributes = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageLegPK, cartageLegPK.ToString(), 0)
			};
			AssertCollectionContains(
				"The expected cartage leg attribute was not found in the calculation result",
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageLegPK, cartageLegPK.ToString(), 0),
				result.Attributes.Attributes
			);
		}

		public void TestCalculate_UnitIsCN_PopulateContainerNumberForOneContainerPerCharge()
		{
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.SCO;
			Line.Parent.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.TL_RX_NKCurrency = "AUD";

			var containers = new[]
			{
				new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 1, "CONT00001"),
			};

			var measures = Criteria.RateableMeasures;
			measures.AddContainerWithNumber(Line.Parent.TI_RC, "CONT00001", containers[0]);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, Line, 0);
			var filteredParams = parameters.CreatedFilteredParametersByContainer_ForTest("CONT00001");

			var expectedAttribute = new RateAttribute(JobChargeAttribTypeList.Codes.ContainerNumber, "CONT00001", 0);
			var result = AssertCalculation(filteredParams, 100, "1 20GP Container(s) @ AUD 100.00/Container");
			AssertCollectionContains(expectedAttribute, result.Attributes.Attributes);

			// Using original params with no filter stops attribute being added
			result = AssertCalculation(parameters, 100, "1 20GP Container(s) @ AUD 100.00/Container");
			AssertCollectionNotContains(expectedAttribute, result.Attributes.Attributes);
		}

		public void TestCalculate_UnitIsTU_PopulateContainerNumberForOneContainerPerCharge()
		{
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.SCO;
			Line.Parent.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.Parent.TI_RH_NKCommodityCode = "HAZ";
			Line.Parent.TI_ContractNumber = "MCLAREN";
			Line.TL_WeightVolume = QuantityUnit.TU;
			Line.TL_RX_NKCurrency = "AUD";

			var containers = new[]
			{
				new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 1, "CONT00001"),
			};

			var measures = Criteria.RateableMeasures;
			measures.AddContainerWithNumber(Line.Parent.TI_RC, "CONT00001", containers[0]);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, Line, 0);
			var filteredParams = parameters.CreatedFilteredParametersByContainer_ForTest("CONT00001");

			var result = AssertCalculation(filteredParams, 100, "1 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit");

			var expectedAttributes = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.ContainerNumber, "CONT00001", 0),
			};

			AssertCollectionContains("Expected attribute not found in result attributes", new RateAttribute(JobChargeAttribTypeList.Codes.ContainerNumber, "CONT00001", 0), result.Attributes.Attributes);
		}

		public void TestCalculate_EntryHasNoContainerAndTheRateIsPerContainer_DontPopulateContainerTypeChargeAttribute()
		{
			Line.Parent.TI_RC = ZGuid.Empty;
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(2M, QuantityUnit.CN);

			var result = AssertCalculation(parameters, 200m, "2 Container(s) @ AUD 100.00/Container");
			AssertCollectionNotContains(
				"A container charge attribute should not be populated",
				new { Code = JobChargeAttribTypeList.Codes.ContainerCode },
				result.Attributes.Attributes.Select(a => new { a.Code })
			);
		}

		public void TestCalculate_LCLContainerCountEmpty()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");

			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;
			Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = RatingConstants.Units.KG;
			Line.TL_AC = Env.Registry.FreightChargeCode;

			var calculator = Line.Calculator;

			Criteria.RateableMeasures.AddLCL("GEN", 10500m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 0);
			Criteria.AdapterType = AdapterType.OneOffQuote;
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			var weightParts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			weightParts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48 });
			Criteria.RateableMeasures.AddPartList(MeasureType.Weight, weightParts);

			var volumeParts = new RateablePartList { VolumeUnit = "M3", HasContainerType = true };
			volumeParts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Volume = 10 });
			Criteria.RateableMeasures.AddPartList(MeasureType.Volume, volumeParts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Volume, Line, 0);
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, 10500, "KG");
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Volume, 10, "M3");

			var (results, error) = calculator.Calculate(parameters);
			AssertEquals(0, results.Count());
			AssertNullOrEmpty(error);
		}

		public void TestLoadWithMissingHigherChargeableLowerRateItem_DoesNotSetHasChanges()
		{
			RatingDataRegistry.Instance.UseHigherWeightOrUnitLowerRateRuleDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Line.TL_WeightVolume = "KG";
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;
			var item = Line.RateLineItems.Cast<RateLineItem>().Single(x => x.TM_Type == Calculator.Items.HigherChargeableLowerRate);
			item.Delete();
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var lineInFactory2 = factory2.Load<RateLine>(Line.PK);
			AssertNotNull("PRE:calculator is initialized", lineInFactory2.ViewCalculator);
			AssertEquals(false, lineInFactory2.HasChanges);
		}

		public void TestCalculate_MultipleEquipmentsOverMaxWeightVolume_Actual()
		{
			var con20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			con20GP.RC_GrossWeight = 6000;
			con20GP.RC_TareWeight = 1000;
			con20GP.RC_CubicCapacity = 10;

			Factory.Save();

			AssertEquals(5000m, con20GP.RC_NetWeight);

			Entry.TI_Mode = Constants.RateMode.ULD;

			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.Parent.TI_RC = con20GP.PK;
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";
			Line.TL_ActualPercentage = 100;

			Criteria
				.RateableMeasures
				.AddContainerWithNumber
				(
					con20GP.PK,
					"CONT00001",
					new MeasureInfo.ContainerInfo(5250m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, "CONT00001", container: con20GP) // Weight is more than capacity
				);

			Criteria
				.RateableMeasures
				.AddContainerWithNumber
				(
					con20GP.PK,
					"CONT00002",
					new MeasureInfo.ContainerInfo(5250m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, "CONT00002", container: con20GP) // Weight is more than capacity
				);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, 10500, "KG");
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Volume, 10, "M3");
			parameters.ChargeableAmount = new Quantity(10500, QuantityUnit.KG);

			Line.TL_RateCalculator = CombinedCalculator.Code;
			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 1700;
			item1.TM_FlatAmount = 100m;
			item1.TM_Value = 0m;

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 1700;
			item2.TM_FlatAmount = 100m;
			item2.TM_Value = 1.8m;

			var calculator = Line.Calculator;
			calculator.MultipleEquipmentsOverMaxWeightVolume = false;
			calculator.IsAccumulated = true;

			var (results, error) = calculator.Calculate(parameters);
			AssertGreaterThan("Results should not be empty.", results.Count(), 0);
			AssertNullOrEmpty("Error should be null or empty.", error);

			var result = results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Normal CMB Calculation", delegate
			{
				AssertEquals("Result description mismatch.", "Base Rate AUD 100.00 + 8800 Kilogram(s) @ AUD 1.80/KG", result.Description);
				AssertEquals("Auto rate amount mismatch.", 15940m, autoRateInfo.Amount);
			});

			calculator.MultipleEquipmentsOverMaxWeightVolume = true;

			(results, error) = calculator.Calculate(parameters);
			AssertGreaterThan("Results should not be empty.", results.Count(), 0);
			AssertNullOrEmpty("Error should be null or empty.", error);

			result = results.Single();
			autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Should calculate per each container then sum up all", delegate
			{
				// Total Weight = 10500 KG
				// We calculate each container separately:
				// First one: CONT00001 with 5250 Weight
				// Number of containers needed = 2
				// Pivot Weight = 2 * 1700 = 3400 KG
				// OverPivot Weight = 5250 - 3400 = 1850 KG
				// Calculation For First Container: 2 * 100(Pivot Rate) + (1850 (Over Pivot Weight) * 1.8 (Over Pivot Rate Per Unit)
				// Calculation For Second Container is exactly the same as first one: 2 * 100(Pivot Rate) + (1850 (Over Pivot Weight) * 1.8 (Over Pivot Rate Per Unit)
				// Calculation Sum up: 4 * 100(Pivot Rate) + (3700 (Over Pivot Weight) * 1.8 (Over Pivot Rate Per Unit)
				AssertEquals("Result description mismatch.", "4 20GP Container(s) @ AUD 100.00/Container + 3700 Kilogram(s) @ AUD 1.80/KG", result.Description);
				AssertEquals("Auto rate amount mismatch.", 7060m, autoRateInfo.Amount);
			});
		}

		public void TestCalculate_MultipleEquipmentsOverMaxWeightVolume_Actual_RateUnitIsDifferentThanJobWeightUnit()
		{
			var con20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			con20GP.RC_GrossWeight = 6000;
			con20GP.RC_TareWeight = 1000;
			con20GP.RC_CubicCapacity = 0; // Volume Capacity is not provided, but we expect to calculate required number of containers based on job's weight

			Factory.Save();

			AssertEquals("Con20GP Net weight calculation mismatch", 5000m, con20GP.RC_NetWeight);

			Entry.TI_Mode = Constants.RateMode.ULD;

			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = "LB"; //Weight Unit is different than KG
			Line.Parent.TI_RC = con20GP.PK;
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";
			Line.TL_ActualPercentage = 100;

			Criteria
				.RateableMeasures
				.AddContainerWithNumber(
					con20GP.PK,
					"CONT00001",
					new MeasureInfo.ContainerInfo(5250m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, "CONT00001", container: con20GP)
				); //Weight is more than capacity

			Criteria
				.RateableMeasures
				.AddContainerWithNumber(
					con20GP.PK,
					"CONT00002",
					new MeasureInfo.ContainerInfo(5250m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, "CONT00002", container: con20GP)
				); //Weight is more than capacity

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, 10500, "KG");
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Volume, 10, "M3");
			parameters.ChargeableAmount = new Quantity(10500, QuantityUnit.KG);

			Line.TL_RateCalculator = CombinedCalculator.Code;
			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 1700;
			item1.TM_FlatAmount = 100m;
			item1.TM_Value = 0m;

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 1700;
			item2.TM_FlatAmount = 100m;
			item2.TM_Value = 1.8m;

			var calculator = Line.Calculator;
			calculator.MultipleEquipmentsOverMaxWeightVolume = false;
			calculator.IsAccumulated = true;

			var (results, error) = calculator.Calculate(parameters);
			AssertGreaterThan("Results should not be empty after calculation", results.Count(), 0);
			AssertNullOrEmpty("Error should be null or empty after calculation", error);

			var result = results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Normal CMB Calculation", delegate
			{
				AssertEquals("Description mismatch for normal calculation", "Base Rate AUD 100.00 + 21448.5375 Pound(s) @ AUD 1.80/LB", result.Description);
				AssertEquals("Amount mismatch for normal calculation", 38707.37m, autoRateInfo.Amount);
			});

			calculator.MultipleEquipmentsOverMaxWeightVolume = true;

			(results, error) = calculator.Calculate(parameters);
			AssertGreaterThan("Results should not be empty after multiple equipment calculation", results.Count(), 0);
			AssertNullOrEmpty("Error should be null or empty after multiple equipment calculation", error);

			result = results.Single();
			autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Should calculate per each container then sum up all", delegate
			{
				AssertEquals(
					"Description mismatch for multiple equipment calculation",
					"4 20GP Container(s) @ AUD 100.00/Container + 16348.5375 Pound(s) @ AUD 1.80/LB",
					result.Description
				);
				AssertEquals(
					"Amount mismatch for multiple equipment calculation",
					29827.37m,
					autoRateInfo.Amount
				);
			});
		}

		public void TestCalculate_MultipleEquipmentsOverMaxWeightVolume_RateHasMinChargeableWeight_PerUnit()
		{
			var rknContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "RKN");
			rknContainer.RC_NetWeight = 700;
			rknContainer.RC_CubicCapacity = 10;

			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.Parent.TI_RC = rknContainer.PK;
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";
			Line.TL_ActualPercentage = 100;
			Line.TL_RateCalculator = CombinedCalculator.Code;

			var calc = Line.GetCalculator<CombinedCalculator>();
			calc.MultipleEquipmentsOverMaxWeightVolume = true;

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.UNT;
			item1.TM_FlatAmount = 100;
			item1.TM_Value = 2m;

			// Minimum chargeable is 600 KG
			var minItem = Line.RateLineItems.AddNew();
			minItem.TM_Type = Calculator.Items.Operator.MIN;
			minItem.TM_Break = 600m;

			var containers = new RateablePartList { HasContainerType = true };
			containers.AddPart(new RateableContainer { ContainerCount = 1, ContainerWeightInKG = 1000, ContainerTypePk = NullableHelper.ToNullable(rknContainer.PK) });
			Criteria.RateableMeasures.AddPartList(MeasureType.ContainerCount, containers);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			var (results, error) = calc.Calculate(parameters);
			AssertGreaterThan("Results should not be empty", results.Count(), 0);
			AssertNullOrEmpty("Error should be null or empty", error);

			var result = results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Should calculate per each container then sum up all", delegate
			{
				// Job has 1 container with weight 1000 KG
				//
				// Since MultipleEquipmentsOverMaxWeightVolume rule is enabled, we calculate the actual number of containers required
				// based on the container capacity.
				// The container has capacity 700 KG (RC_NetWeight), so we need 2 containers to fit 1000 KG
				// It means that 1000 KG will be shared between these 2 containers, i.e. 500 KG each
				//
				// The rate has minimum chargeable 600 KG and has price $2 per KG
				//
				// Since each container has weight less than min chargeable weight (500KG vs 600KG), the min chargeable weight is used
				// So, the calculation is: 600 KG * $2 per KG + 600 KG * $2 per KG = $2400
				AssertEquals("Result description should match", "1200 Kilogram(s) @ AUD 2.00/KG", result.Description);
				AssertEquals("Calculated amount should match", 2400m, autoRateInfo.Amount);
			});
		}

		public void TestCalculate_MultipleEquipmentsOverMaxWeightVolume_RateHasMinChargeableWeight_Sliding()
		{
			var rknContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "RKN");
			rknContainer.RC_NetWeight = 700;
			rknContainer.RC_CubicCapacity = 10;

			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.Parent.TI_RC = rknContainer.PK;
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";
			Line.TL_ActualPercentage = 100;
			Line.TL_RateCalculator = CombinedCalculator.Code;

			var calc = Line.GetCalculator<CombinedCalculator>();
			calc.MultipleEquipmentsOverMaxWeightVolume = true;

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 300;
			item1.TM_FlatAmount = 100;
			item1.TM_Value = 2m;

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 300;
			item2.TM_FlatAmount = 100;
			item2.TM_Value = 1m;

			// Minimum chargeable is 600 KG
			var minItem = Line.RateLineItems.AddNew();
			minItem.TM_Type = Calculator.Items.Operator.MIN;
			minItem.TM_Break = 600m;

			var containers = new RateablePartList { HasContainerType = true };
			containers.AddPart(new RateableContainer { ContainerCount = 1, ContainerWeightInKG = 1000, ContainerTypePk = NullableHelper.ToNullable(rknContainer.PK) });
			Criteria.RateableMeasures.AddPartList(MeasureType.ContainerCount, containers);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			var (results, error) = calc.Calculate(parameters);

			// Refactored assertions
			AssertGreaterThan(results.Count(), 0); // Fluent assertion replaced: `results.Should().NotBeEmpty();`
			AssertNullOrEmpty(error);             // Fluent assertion replaced: `error.Should().BeNullOrEmpty();`

			var result = results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Should calculate per each container then sum up all", delegate
			{
				// Job has 1 container with weight 1000 KG
				//
				// Since MultipleEquipmentsOverMaxWeightVolume rule is enabled, we calculate the actual number of containers required
				// based on the container capacity.
				// The container has capacity 700 KG (RC_NetWeight), so we need 2 containers to fit 1000 KG
				// It means that 1000 KG will be shared between these 2 containers, i.e. 500 KG each
				//
				// The rate has minimum chargeable 600 KG and has price $2 per KG if weight is < 300KG and 1$ per KG if weight is >300 KG.
				// Also, it has $100 per container.
				//
				// Since each container has weight less than min chargeable weight (500KG vs 600KG), the min chargeable weight is used
				// So, the calculation is: 1 CN * $100 per CN + 600 KG * $1 per KG +  1 CN * $100 per CN 600 KG * $1 per KG = $1400
				AssertEquals("2 RKN Container(s) @ AUD 100.00/Container + 1200 Kilogram(s) @ AUD 1.00/KG", result.Description);
				AssertEquals(1400m, autoRateInfo.Amount);
			});
		}

		public void TestCalculate_MultipleEquipmentsOverMaxWeightVolume_PerUnitRateNoBreaksOrOverPivotRates()
		{
			var con20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			con20GP.RC_GrossWeight = 6000;
			con20GP.RC_TareWeight = 1000;
			con20GP.RC_CubicCapacity = 0; // Volume Capacity is not provided, but we expect to calculate required number of containers based on job's weight

			Factory.Save();

			AssertEquals(5000m, con20GP.RC_NetWeight);

			Entry.TI_Mode = Constants.RateMode.ULD;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = "CN";
			Line.Parent.TI_RC = con20GP.PK;
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";

			Criteria
				.RateableMeasures
				.AddContainerGroup(con20GP.PK, "GEN", new[]
				{
					new MeasureInfo.ContainerInfo(3000, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, "CONT00001", container: con20GP, containerCount: 1) //Weight is less than capacity
				});

			Criteria
				.RateableMeasures
				.AddContainerGroup(con20GP.PK, "HAZ", new[]
				{
					new MeasureInfo.ContainerInfo(7000, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, "CONT00002", container: con20GP, containerCount: 1) //Weight is more than capacity
				});

			Criteria
				.RateableMeasures
				.AddContainerGroup(con20GP.PK, "GEN", new[]
				{
					new MeasureInfo.ContainerInfo(13000, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 1, container: con20GP, containerCount: 2) //Weight is more than capacity
				});

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			var calculator = Line.GetCalculator<CombinedCalculator>();
			calculator.PerUnit = 500;
			calculator.MultipleEquipmentsOverMaxWeightVolume = false;

			var (results, error) = calculator.Calculate(parameters);
			AssertNotEquals(0, results.Count());
			AssertNullOrEmpty("error", error);

			var result = results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Normal CMB Calculation", delegate
			{
				AssertEquals("Expected description does not match.", "1 20GP Container(s) @ AUD 500.00/Container + 1 20GP Container(s) @ AUD 500.00/Container + 2 20GP Container(s) @ AUD 500.00/Container", result.Description);
				AssertEquals("Expected amount does not match.", 2000m, autoRateInfo.Amount);
			});

			calculator.MultipleEquipmentsOverMaxWeightVolume = true;

			(results, error) = calculator.Calculate(parameters);
			AssertNotEquals(0, results.Count());
			AssertNullOrEmpty("error", error);

			result = results.Single();
			autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			CombineAssertions("Should calculate the number of required containers based on weight and container capacity", delegate
			{
				// Basically we have the following setup:
				// 1 x 20GP CONT00001	3000 KG
				// 1 x 20GP CONT00002	7000 KG
				// 2 x 20GP				13000 KG
				//
				// 20GP container has capacity 5000 KG, so, the required containers are recalculated as the following:
				// 1 x 20GP CONT00001	3000 KG		-> 1 container (since we need 1 20GP container as 3000 KG fits 5000 KG capacity)
				// 1 x 20GP CONT00002	7000 KG		-> 2 containers (since we need 2 20GP containers to move 7000 KG (7000 KG > 5000 KG))
				// 2 x 20GP				13000 KG	-> 3 containers (since we need 3 20GP containers to move 13000 KG (13000 KG > 5000 KG))
				//
				// Since we can have only 1 container with a specific container number, the extra container for CONT00002 is added to containers
				// with empty container number, i.e. instead of:
				// 1 (CONT00001) + 2 (CONT00002) + 3 (empty container number)
				// we have:
				// 1 (CONT00001) + 1 (CONT00002) + 4 (empty container number)
				AssertEquals("Expected recalculated description does not match.", "1 20GP Container(s) @ AUD 500.00/Container + 1 20GP Container(s) @ AUD 500.00/Container + 4 20GP Container(s) @ AUD 500.00/Container", result.Description);
				AssertEquals("Expected recalculated amount does not match.", 3000m, autoRateInfo.Amount);
			});
		}

		public void TestCalculate_PivotBreak_OverridesBreakValue()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			var contLD7 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-7");
			Factory.Save();

			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			(var line1, var line1Calc) = AddLineAndCalculator("LD-6", CombinedCalculator.Code);//-45 and +45
			(var line2, var line2Calc) = AddLineAndCalculator("LD-7", CartageCalculator.Code);//-45 and +45
			(var line3, var line3Calc) = AddLineAndCalculator("LD-7", CartageZoneDistanceCalculator.Code);//-45 and +45
			(var line4, var line4Calc) = AddLineAndCalculator("LD-6", CombinedCalculator.Code, addAdditionalPlusBreak: true);//-45,+45 and +90
			(var line5, var line5Calc) = AddLineAndCalculator("LD-6", CombinedCalculator.Code, addMinusBreak: false);//+45
			(var line6, var line6Calc) = AddLineAndCalculator("LD-6", CombinedCalculator.Code, addMinusBreak: false, addAdditionalPlusBreak: true);//+45 and +90

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48, PivotBreak = 47 });
			parts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Weight = 44, PivotBreak = 43 });
			parts.AddPart(new RateablePart { ContainerTypePk = contLD7.PK.ToGuid(), Weight = 50, PivotBreak = 100 });
			parts.AddPart(new RateablePart { ContainerTypePk = contLD7.PK.ToGuid(), Weight = 50, PivotBreak = 48 });
			parts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48, PivotBreak = 47 });
			parts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48, PivotBreak = 47 });
			parts.AddPart(new RateablePart { ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48, PivotBreak = 47 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, line1, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line1, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line3, 3);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line4, 4);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line5, 5);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line6, 6);

			AssertCalculation("Should override 47 for line1 as 47 is max of LD6's pivot Breaks(47,43)",
				line1Calc, 1490m,
				"Base Rate AUD 100.00 + 47 Kilogram(s) @ AUD 20.00/KG + 45 Kilogram(s) @ AUD 10.00/KG");

			AssertCalculation("Should override 100 for line2",
				line2Calc, 1200m,
				"Base Rate AUD 200.00 + 50 Kilogram(s) @ AUD 20.00/KG");

			AssertCalculation("Should override 48 for line3",
				line3Calc, 1080m,
				"Base Rate AUD 100.00 + 48 Kilogram(s) @ AUD 20.00/KG + 2 Kilogram(s) @ AUD 10.00/KG");

			AssertCalculation("Should not override 47 for line4 as there are more than 1 break(-45,+45 and +90)",
				line4Calc, 1030m,
				"Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");

			AssertCalculation("Should not override 47 for line5 though there is only 1 positive break(+45), but it won't have any impact in end calculation",
				line5Calc, 580m,
				"Base Rate AUD 100.00 + 48 Kilogram(s) @ AUD 10.00/KG");

			AssertCalculation("Should not override 47 for line6 as there are more than 1 break(+45 and +90)",
				line6Calc, 580m,
				"Base Rate AUD 100.00 + 48 Kilogram(s) @ AUD 10.00/KG");

			(RateLine line, Calculator calculator) AddLineAndCalculator(string container, string calculatorCode, bool addMinusBreak = true, bool addAdditionalPlusBreak = false)
			{
				var entry = rate.AddRateEntry("AIR", "ULD", container: container);
				var line = entry.AddRateLine("FRT", calculatorCode, "KG");
				var calc = line.Calculator;
				if (addMinusBreak)
				{
					calc.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 20M, 200m);
				}
				calc.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
				if (addAdditionalPlusBreak)
				{
					calc.AddRateLineItem(Calculator.Items.Operator.Plus, 90m, 5m, 50m);
				}
				calc.IsAccumulated = true;

				return (line, calc);
			}

			void AssertCalculation(ZString message, Calculator calculator, ZDecimal expectedAmount, ZString expectedDescription)
			{
				var (results, error) = calculator.Calculate(parameters);
				AssertGreaterThan($"{message}: Results should not be empty", results.Count(), 0);
				AssertNullOrEmpty($"{message}: Error should be null or empty", error);

				var result = results.Single();
				var autoRateInfo = new AutoRateInfo(result, parameters, Factory);
				var calculatorType = CalculatorType.ToString();

				CombineAssertions($"{calculatorType} : {message}", delegate
				{
					AssertEquals("description:", expectedDescription, result.Description);
					AssertEquals("amount:", expectedAmount, Utilities.Round(autoRateInfo.Amount, result.Currency?.Decimals ?? 2));
				});
			}
		}

		public void TestCalculate_BreaksPer_Accumulated_ApplicableContainersTotalExceeedsBreaksTotal()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
			TestCalculator.IsAccumulated = true;

			var line2 = Entry.AddFlatRateLine("FRT", 200M);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 44 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);

			//The description looks incorrect, it should be
			//Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG (0 - 45 Kilogram(s)) + 47 Kilogram(s) @ AUD 10.00/KG (45 Kilogram(s) or more)
			//Since, this is existing behaviour, so Product will raise it as defect.
			//And so, I am not including this info in bracket in case of BreaksPerContainerTypeOrClass or BreaksPerContainer
			AssertCalculation(parameters, 1470m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 47 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2120m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 2 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2110m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.UseHigherChargeableLowerRateRule = true; //should not affect the calculation as per existing behavior
			TestCalculator.BreaksPer = string.Empty;
			AssertCalculation(parameters, 1470m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 47 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2120m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 2 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2110m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestCalculate_BreaksPer_Accumulated_ApplicableContainersTotalBelowBreaksTotal()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
			TestCalculator.IsAccumulated = true;

			var line2 = Entry.AddFlatRateLine("FRT", 200M);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 46 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 43 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);

			AssertCalculation(parameters, 1440m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 44 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2080m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2070m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 88 Kilogram(s) @ AUD 20.00/KG + 1 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.UseHigherChargeableLowerRateRule = true; //should not affect the calculation as per existing behavior
			TestCalculator.BreaksPer = string.Empty;
			AssertCalculation(parameters, 1440m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 44 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2080m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2070m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 88 Kilogram(s) @ AUD 20.00/KG + 1 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestCalculate_RateHasMinChargeableAndPerUnitRate_UseMinChargeableIfActualIsSmaller()
		{
			var con20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			Entry.TI_RC = con20GP.PK;
			Entry.TI_Mode = RateMode.ULD;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = "KG";
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";

			var calculator = Line.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 2m, ZString.Empty);
			calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 500m, 0m, ZString.Empty);

			var parts = new RateablePartList { WeightUnit = "KG", HasCommodity = true };
			parts.AddPart(new RateablePart { Weight = 200m, CommodityCode = "GEN" });
			Criteria.RateableMeasures.AddPartList(MeasureType.Weight, parts);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var (results, error) = calculator.Calculate(parameters);

			AssertNullOrEmpty("Error message should be null or empty", error);

			AssertEquals(
				"Should charge 500 KG as min chargeable on the rate is 500 KG while actual weight is 200 KG",
				"Min 500 Kilogram(s) @ AUD 2.00/KG",
				results.Single().Description
			);
		}

		public void TestCalculate_RateHasMinChargeableAndPerUnitWithBreaks_UseMinChargeableIfActualIsSmaller()
		{
			var con20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			Entry.TI_RC = con20GP.PK;
			Entry.TI_Mode = RateMode.ULD;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = "KG";
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";

			var calculator = Line.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 200, 2m, ZString.Empty);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 200, 1m, ZString.Empty);
			calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 500m, 0m, ZString.Empty);

			var parts = new RateablePartList { WeightUnit = "KG", HasCommodity = true };
			parts.AddPart(new RateablePart { Weight = 200m, CommodityCode = "GEN" });
			Criteria.RateableMeasures.AddPartList(MeasureType.Weight, parts);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var (results, error) = calculator.Calculate(parameters);

			AssertNullOrEmpty(nameof(error), error);
			AssertEquals(
				"Should charge 500 KG as min chargeable on the rate is 500 KG while actual weight is 200 KG",
				"Min 500 Kilogram(s) @ AUD 1.00/KG",
				results.Single().Description
			);
		}

		public void TestCalculate_RateHasNoMinChargeableAndJobContainerHasPivot_UseActualWeightAsChargeable()
		{
			var con20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			Entry.TI_RC = con20GP.PK;
			Entry.TI_Mode = RateMode.ULD;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_WeightVolume = "KG";
			Line.Parent.TI_RH_NKCommodityCode = "GEN";
			Line.TL_RX_NKCurrency = "AUD";

			var calculator = Line.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100, 2m, ZString.Empty);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100, 1m, ZString.Empty);
			calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 100m, ZString.Empty);

			var parts = new RateablePartList { WeightUnit = "KG", HasCommodity = true };
			parts.AddPart(new RateablePart { Weight = 200m, CommodityCode = "GEN", PivotBreak = 300 });
			Criteria.RateableMeasures.AddPartList(MeasureType.Weight, parts);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var (results, error) = calculator.Calculate(parameters);

			AssertNullOrEmpty("error", error);

			var result = results.Single().Description;
			var expectedDescription = "200 Kilogram(s) @ AUD 1.00/KG";
			AssertEquals(
				"Should charge 200 KG as the rate doesn't have min chargeable and we ignore pivot break on the Job Container 300 KG as it only overrides rate min chargeable if it exists",
				expectedDescription,
				result
			);
		}

		public void TestCalculate_BreaksPer_NonAccumulated_ApplicableContainersTotalExceedsBreaksTotal()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 0M, 1100M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 2M, 1100M);

			var line2 = Entry.AddFlatRateLine("FRT", 200M);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 44 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);

			AssertCalculation(parameters, 1284m, "Base Rate AUD 1100.00 + 92 Kilogram(s) @ AUD 2.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2204m, "Base Rate AUD 1100.00 + Base Rate AUD 1100.00 + 2 Kilogram(s) @ AUD 2.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2206m, "Base Rate AUD 1100.00 + Base Rate AUD 1100.00 + 3 Kilogram(s) @ AUD 2.00/KG");
		}

		public void TestCalculate_BreaksPer_NonAccumulated_ApplicableContainersTotalBelowBreaksTotal()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 0M, 1100M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45M, 2m, 1100M);

			var line2 = Entry.AddFlatRateLine("FRT", 200M);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 46 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 43 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);

			AssertCalculation(parameters, 1278m, "Base Rate AUD 1100.00 + 89 Kilogram(s) @ AUD 2.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2200m, "Base Rate AUD 1100.00 + Base Rate AUD 1100.00");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2202m, "Base Rate AUD 1100.00 + Base Rate AUD 1100.00 + 1 Kilogram(s) @ AUD 2.00/KG");
		}

		public void TestCalculate_BreaksPer_UseInclusiveBreaks_ShouldHaveNoImpactIfBreaksPerIsNotEmpty()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45M, 10m, 100M);

			var line2 = Entry.AddFlatRateLine("FRT", 200M);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 45 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);

			TestCalculator.UseInclusiveBreaks = true;
			AssertCalculation(parameters, 1030m, "Base Rate AUD 100.00 + 93 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2130m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2130m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.UseInclusiveBreaks = false;
			TestCalculator.BreaksPer = string.Empty;
			AssertCalculation(parameters, 1030m, "Base Rate AUD 100.00 + 93 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2130m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2130m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestCalculate_BreaksPer_ContainerHasMultiplePackLines()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
			TestCalculator.IsAccumulated = true;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var containerPK1 = ZGuid.NewZGuid();
			var containerPK2 = ZGuid.NewZGuid();

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true, HasCommodity = true };
			parts.AddPart(new RateablePart { Weight = 20m, ContainerPK = containerPK1.ToGuid(), ContainerTypePk = contLD6.PK.ToGuid(), CommodityCode = "GEN" });
			parts.AddPart(new RateablePart { Weight = 28m, ContainerPK = containerPK1.ToGuid(), ContainerTypePk = contLD6.PK.ToGuid(), CommodityCode = "GEN" });
			parts.AddPart(new RateablePart { Weight = 10m, ContainerPK = containerPK2.ToGuid(), ContainerTypePk = contLD6.PK.ToGuid(), CommodityCode = "GEN" });
			parts.AddPart(new RateablePart { Weight = 34m, ContainerPK = containerPK2.ToGuid(), ContainerTypePk = contLD6.PK.ToGuid(), CommodityCode = "GEN" });
			Criteria.RateableMeasures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 2);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 3);

			AssertCalculation(parameters, 1470m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 47 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2120m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 2 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2110m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestCalculate_BreaksPer_ChargeableAmountUsingActualPercentage()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.TL_ActualPercentage = 0;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
			TestCalculator.IsAccumulated = true;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", VolumeUnit = "M3", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48, Volume = 0.36m });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 44, Volume = 0.24m });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);
			measures.AddPartList(MeasureType.Volume, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);

			var totalChargeableAmount = parameters.GetChargeableAmount(Line);
			AssertEquals("Pre-condition: Total Chargeable Amount", 100M, totalChargeableAmount.Amount);
			AssertEquals("Pre-condition: Total Chargeable Unit", "KG", totalChargeableAmount.Unit);

			AssertCalculation(parameters, 1550m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 55 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2200m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 10 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2230m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 15 Kilogram(s) @ AUD 10.00/KG");

			Line.TL_ActualPercentage = 50;

			TestCalculator.BreaksPer = string.Empty;
			//Total Weight = 92 * 0.5 = 46 KG
			//Total Chargeable Weight 100 * 0.5 = 50
			//Applicable After 50% of Actual is 46 + 50 = 96 KG
			AssertCalculation(parameters, 1510m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 51 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			//Same as above 96 KG
			AssertCalculation(parameters, 2160m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 6 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			//CONT00001 Weight = 48 * 0.5 = 24, Chargeable = 60 * 0.5 = 30
			//	Applicable after 50% of Actual is 24 + 30 = 54
			//CONT00002 Weight = 44 * 0.5 = 22, Chargeable = 44 * 0.5 = 22
			//	Applicable after 50% of Actual is 22 + 22 = 44
			//Total Applicable is 54 (45 + 9) + 44 = 98 KG
			AssertCalculation(parameters, 2170m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 9 Kilogram(s) @ AUD 10.00/KG");

			Line.TL_ActualPercentage = 100;

			TestCalculator.BreaksPer = string.Empty;
			AssertCalculation(parameters, 1470m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 47 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2120m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 2 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2110m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 3 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestCalculate_BreaksPer_ChargeableAmountUsingActualPercentage_LoadingMeters()
		{
			FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100m);
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "R1FT");
			Factory.Save();

			Entry.TI_Mode = RateMode.ROA;
			Entry.TI_RC = refContainer.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.TL_ActualPercentage = 0;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
			TestCalculator.IsAccumulated = true;

			var criteria = new TestRatingCriteria("", "", FreightMode.ROA, 92m, 0.6m, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", VolumeUnit = "M3", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = refContainer.PK.ToGuid(), Weight = 48, LoadingMeter = 0.56m });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = refContainer.PK.ToGuid(), Weight = 44, LoadingMeter = 0.4m });
			measures.AddPartList(MeasureType.Weight, parts);
			measures.AddPartList(MeasureType.Volume, parts);
			measures.AddPartList(MeasureType.LoadingMeters, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);

			var totalChargeableAmount = parameters.GetChargeableAmount(Line);
			AssertEquals("Pre-condition: Total Chargeable Amount", 96M, totalChargeableAmount.Amount);
			AssertEquals("Pre-condition: Total Chargeable Unit", "KG", totalChargeableAmount.Unit);

			AssertCalculation(parameters, 1510m, "Base Rate AUD 100.00 + 45 Kilogram(s) @ AUD 20.00/KG + 51 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2160m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 90 Kilogram(s) @ AUD 20.00/KG + 6 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2190m, "Base Rate AUD 100.00 + Base Rate AUD 200.00 + 89 Kilogram(s) @ AUD 20.00/KG + 11 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestCalculate_BreaksPer_BaseRateApplicableOnEachIndividualContainerWeight()
		{
			var contLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			Factory.Save();

			Entry.TI_Mode = RateMode.ULD;
			Entry.TI_RC = contLD6.PK;

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.KG;
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 90m, 5m, 50m);

			var line2 = Entry.AddFlatRateLine("FRT", 200M);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = parameters.Criteria.RateableMeasures;

			var parts = new RateablePartList { WeightUnit = "KG", HasContainerType = true };
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 48 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 46 });
			parts.AddPart(new RateablePart { ContainerPK = Guid.NewGuid(), ContainerTypePk = contLD6.PK.ToGuid(), Weight = 24 });
			measures.AddPartList(MeasureType.Weight, parts);

			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 1);
			parameters.AddLineMeasureMatch(MeasureType.Weight, line2, 2);

			TestCalculator.UseInclusiveBreaks = true;
			AssertCalculation(parameters, 520m, "Base Rate AUD 50.00 + 94 Kilogram(s) @ AUD 5.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertCalculation(parameters, 2040m, "Base Rate AUD 100.00 + Base Rate AUD 100.00 + 90 Kilogram(s) @ AUD 20.00/KG + 4 Kilogram(s) @ AUD 10.00/KG");

			TestCalculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			AssertCalculation(parameters, 2040m, "Base Rate AUD 100.00 + Base Rate AUD 100.00 + 90 Kilogram(s) @ AUD 20.00/KG + 4 Kilogram(s) @ AUD 10.00/KG");
		}

		#region Calculator Description Conversion

		public void TestCalculate_WhenAddingBasOperatorLine_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 3m, 0m);
			var expected = @"BAS. Rate USD 3";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 100m, 0m);
			expected = @"BAS. Rate USD 3 +
MIN. Rate USD 100.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 300m, 0m);
			expected = @"BAS. Rate USD 3 +
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN/MAX");
		}

		public void TestCalculate_WhenAddingMinusPlusOperatorLine_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, 8m, 40m);
			var expected = @"Base USD 40 + USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 9m, 30m);
			expected = @"Unit total < 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with multiple PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, 10m, 20m);
			expected = @"Unit total < 10 Day(s) Base USD 20 + USD 10/Day
Unit total >= 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MINUS/PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 30m, 0m, 50m);
			expected = @"Unit total < 10 Day(s) Base USD 20 + USD 10/Day
Unit total >= 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD 8/Day
Unit total >= 30 Day(s) Base USD 50";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with zero rate or base in MINUS/PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 40m, 20m, 0m);
			expected = @"Unit total < 10 Day(s) Base USD 20 + USD 10/Day
Unit total >= 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD 8/Day
Unit total >= 30 Day(s) Base USD 50
Unit total >= 40 Day(s) USD 20/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with zero rate or base in MINUS/PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50m, 0m, 0m);
			expected = @"Unit total < 10 Day(s) Base USD 20 + USD 10/Day
Unit total >= 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD 8/Day
Unit total >= 30 Day(s) Base USD 50
Unit total >= 40 Day(s) USD 20/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with zero rate or base in MINUS/PLUS");

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, -8m, 40m);
			expected = @"Base USD 40 + USD -8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with negative number in PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 9m, -30m);
			expected = @"Unit total < 10 Day(s) Base USD -30 + USD 9/Day
Unit total >= 10 Day(s) Base USD -30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD -8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with negative number in multiple PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, -10m, -20m);
			expected = @"Unit total < 10 Day(s) Base USD -20 + USD -10/Day
Unit total >= 10 Day(s) Base USD -30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD -8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with negative number in MINUS/PLUS");
		}

		public void TestCalculate_WhenAddingNonBasMinusPlusBasOperatorLine_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, 3m, 0m);
			var expected = "USD 3/Day.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 5m, 0m, 0m);
			expected = "USD 3/Day, MIN. 5 Day(s).";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT/MIN");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 300m, 0m);
			expected = "USD 3/Day, MIN. 5 Day(s), MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT/MIN/MAX");
		}

		public void TestCalculate_WhenAddingAllTypesOfLines_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_RX_NKCurrency = CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var measures = Criteria.RateableMeasures;
			measures.AddContainerWithNumber(ZGuid.Empty, "CONT00001", new MeasureInfo.ContainerInfo(100m, Core.Constants.Weight.Pounds, 15m, Core.Constants.Volume.CubicMetres, 1, 1, "CONT00001"));
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, 10m, 20m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, 8m, 40m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 9m, 30m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 3m, 0m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 100m, 0m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 300m, 0m);
			var expected = @"BAS. Rate USD 3 +
Unit total < 10 Day(s) Base USD 20 + USD 10/Day
Unit total >= 10 Day(s) Base USD 30 + USD 9/Day
Unit total >= 20 Day(s) Base USD 40 + USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN/MAX/MINUS/PLUS");

			TestCalculator.UseInclusiveBreaks = true;
			expected = @"BAS. Rate USD 3 +
Unit total <= 10 Day(s) Base USD 20 + USD 10/Day
Unit total > 10 Day(s) Base USD 30 + USD 9/Day
Unit total > 20 Day(s) Base USD 40 + USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN/MAX/MINUS/PLUS with inclusive breaks");

			TestCalculator.IsAccumulated = true;
			expected = @"BAS. Rate USD 3 +
Unit portion <= 10 Day(s) Base USD 20 + USD 10/Day
Unit portion > 10 Day(s) Base USD 30 + USD 9/Day
Unit portion > 20 Day(s) Base USD 40 + USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS with cumulative calculation");

			TestCalculator.UseHigherChargeableLowerRateRule = true;
			expected = @"BAS. Rate USD 3 +
Unit portion <= 10 Day(s) Base USD 20 + USD 10/Day
Unit portion > 10 Day(s) Base USD 30 + USD 9/Day
Unit portion > 20 Day(s) Base USD 40 + USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300, Higher Break Lower Rate is applied.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS with HigherChargeableLowerRateRule");

			Line.TL_RX_NKCurrency = CurrencyCodes.Australia;
			Line.TL_WeightVolume = QuantityUnit.KG;

			expected = @"BAS. Rate AUD 3 +
Unit portion <= 10 Kilogram(s) Base AUD 20 + AUD 10/KG
Unit portion > 10 Kilogram(s) Base AUD 30 + AUD 9/KG
Unit portion > 20 Kilogram(s) Base AUD 40 + AUD 8/KG
MIN. Rate AUD 100, MAX. Rate AUD 300, Higher Break Lower Rate is applied.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS while using other currency/unit");

			Line.TL_WeightVolume = QuantityUnit.CN;
			TestCalculator.RateLineBizO.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			expected = @"BAS. Rate AUD 3 +
Unit portion <= 10 Kilogram(s) Base AUD 20 + AUD 10/KG/Container
Unit portion > 10 Kilogram(s) Base AUD 30 + AUD 9/KG/Container
Unit portion > 20 Kilogram(s) Base AUD 40 + AUD 8/KG/Container
MIN. Rate AUD 100, MAX. Rate AUD 300, Higher Break Lower Rate is applied.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS while using different units in rate line and rate line item");
		}

		#endregion

		#region Implementation

		void AssertWeightBreaksCreatedForImport(bool importing, bool weightBreaksExpected)
		{
			((ISupportDataImporting)Line).IsImportingData = importing;
			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.AIR;
			Line.TL_WeightVolume = "KG";
			Line.LockCalculator = true;
			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = CombinedCalculator.Code;

			if (weightBreaksExpected)
			{
				AssertEquals(true, Line.Calculator.Line.ChildRateLineItems.Count() > 4);
			}
			else
			{
				AssertEquals(4, Line.Calculator.Line.ChildRateLineItems.Count());
			}
		}

		void AssertCalculatorDescriptionAfterCalculating(AutoRatingCalculatorParametersForTesting param, RateLine line, string expectedDescription, string message = "")
		{
			var result = line.Calculator.Calculate(param).results.Single();
			var line1Attributes = result.Attributes.Attributes;

			AssertCollectionContains(
				message,
				new RateAttribute(JobChargeAttribTypeList.Codes.CalculatorDescription, expectedDescription),
				line1Attributes
			);
		}

		protected override Type CalculatorType
		{
			get { return typeof(CombinedCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return CombinedCalculator.Code; }
		}

		new CombinedCalculator TestCalculator
		{
			get { return (CombinedCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
