using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class WarehouseLocationTypeCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			Assert(true);
		}

		public override void TestMapping()
		{
			Assert(true);
		}

		public void TestIsProductAllowed()
		{
			AssertEquals(false, TestCalculator.IsProductAllowed);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(WarehouseLocationTypeCalculator.Code);
		}

		public override void TestGetCloneLineItems()
		{
			Line.ViewAgentRates = false;
			var newItem = TestCalculator.AddRateLineItem("AAA", 0m, 100m);
			newItem.TM_AgentDeclaredRate = 150m;
			newItem = TestCalculator.AddRateLineItem("BBB", 0m, 200m);
			newItem.TM_AgentDeclaredRate = 250m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(180m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("AAA").TM_RelevantValue);
			AssertEquals(280m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("BBB").TM_RelevantValue);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(230m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("AAA").TM_RelevantValue);
			AssertEquals(330m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("BBB").TM_RelevantValue);

			source.BaseRate = 0m;
			source.Percent = 50m;

			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(150m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("AAA").TM_RelevantValue);
			AssertEquals(300m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("BBB").TM_RelevantValue);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(225m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("AAA").TM_RelevantValue);
			AssertEquals(375m, clonedLine.GetCalculator<WarehouseLocationTypeCalculator>().FindRateLineItem("BBB").TM_RelevantValue);
		}

		public void TestLocationTypeValidation()
		{
			Line.TL_RX_NKCurrency = "USD";
			Line.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = "OPN";
			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = "OPN";

			AssertHasError(item2.TM_TypeInfo, "You can only specify one rate for each location type. You should not specify the same location type more than once.");

			item2.TM_Type = "RNO";
			AssertNoError(item2.TM_TypeInfo, "You can only specify one rate for each location type. You should not specify the same location type more than once.");
		}

		public override void TestQuotationLines()
		{
			Line.TL_RX_NKCurrency = "USD";
			Line.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.WHS);

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

		public override void TestDocLineAmount()
		{
			Assert("Quotation Standard Pricing Page doesn't print Warehouse RateEntry", true);
		}

		public void TestCalculation()
		{
			var client = Helper.NewOrgHeader();
			var locationInfo1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");
			var locationInfo2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "LOC2");
			var locationInfo3 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC3");
			var product1 = Helper.NewOrgSupplierPart(client);
			var product2 = Helper.NewOrgSupplierPart(client);
			var product3 = Helper.NewOrgSupplierPart(client);
			var product4 = Helper.NewOrgSupplierPart(client);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var measures = Criteria.RateableMeasures;
			measures.CreateLocationPalletList(null);
			measures.AddLocationPallet(10m, ZGuid.Empty, locationInfo1, product1.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(11m, ZGuid.Empty, locationInfo1, product2.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(12m, ZGuid.Empty, locationInfo2, product3.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(13m, ZGuid.Empty, locationInfo2, product2.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(14m, ZGuid.Empty, locationInfo2, product1.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(15m, ZGuid.Empty, locationInfo3, product4.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(16m, ZGuid.Empty, locationInfo3, product2.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(17m, ZGuid.Empty, locationInfo3, product3.PK, ProductAttributesMeasure.Empty, "");
			measures.AddLocationPallet(18m, ZGuid.Empty, locationInfo3, product1.PK, ProductAttributesMeasure.Empty, "");
			for (int partIndex = 0; partIndex < measures.GetPartCount(MeasureType.LocationPallet); ++partIndex)
			{
				parameters.AddLineMeasureMatch(MeasureType.LocationPallet, Line, partIndex);
			}

			TestCalculator.AddRateLineItem("AAA", 0m, 100m);
			TestCalculator.AddRateLineItem("BBB", 0m, 200m);

			AssertCalculation(parameters, "empty location filter.");

			var filteredParams = parameters.CreatedFilteredParameters_ForTest(locationInfo1);
			AssertCalculation(filteredParams, 100m, "Base Rate AUD 100.00 (LOC1 (AAA) contains (PROD1, PROD2))");

			filteredParams = parameters.CreatedFilteredParameters_ForTest(locationInfo2);
			AssertCalculation(filteredParams, 200m, "Base Rate AUD 200.00 (LOC2 (BBB) contains (PROD1, PROD2, PROD3))");

			filteredParams = parameters.CreatedFilteredParameters_ForTest(locationInfo3);
			AssertCalculation(filteredParams, 100m, "Base Rate AUD 100.00 (LOC3 (AAA) contains (PROD1, PROD2, PROD3, ...))");
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(WarehouseLocationTypeCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return WarehouseLocationTypeCalculator.Code; }
		}

		new WarehouseLocationTypeCalculator TestCalculator
		{
			get { return (WarehouseLocationTypeCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
