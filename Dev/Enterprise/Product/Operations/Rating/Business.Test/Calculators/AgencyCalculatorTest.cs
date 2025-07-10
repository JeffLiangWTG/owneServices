using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class AgencyCalculatorTest : CalculatorTest
	{
		public void TestIncludedLinesCanCastToInt()
		{
			Line.TL_RateCalculator = AgencyCalculator.Code;
			Line.RateLineItems.RemoveAndDeleteAll();
			Line.Calculator.AddRateLineItem("-", 45m, 0m, 10m);

			var agencyCalculator = (AgencyCalculator)Line.Calculator;

			ZInt x;
			AssertNoExceptionThrown(() => x = agencyCalculator.IncludedLines);
		}

		public void TestIncludedHeadersCanCastToInt()
		{
			Line.TL_RateCalculator = AgencyCalculator.Code;
			Line.RateLineItems.RemoveAndDeleteAll();
			var agencyCalculator = (AgencyCalculator)Line.Calculator;

			ZInt x;
			AssertNoExceptionThrown(() => x = agencyCalculator.IncludedHeaders);
		}

		public void TestShouldValueBeDiscounted()
		{
			AssertEquals(true, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyRate)));
			AssertEquals(true, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.CostPerAdditionalLine)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedLines)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MaximumLines)));
			AssertEquals(true, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AdditionalRate)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyFeeType)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyLineType)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideFeeLineTypeOnQuote)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType)));
			AssertEquals(false, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideMessageTypeOnQuote)));
		}

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyRate));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.CostPerAdditionalLine));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedLines));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MaximumLines));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AdditionalRate));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyFeeType));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedHeaders));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyLineType));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideFeeLineTypeOnQuote));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideMessageTypeOnQuote));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyRate));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.CostPerAdditionalLine));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedLines));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MaximumLines));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AdditionalRate));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyFeeType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedHeaders));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyLineType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideFeeLineTypeOnQuote));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideMessageTypeOnQuote));

			AssertEquals(13, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyRate).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.CostPerAdditionalLine).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedLines).TM_RelevantValueInfo, TestCalculator.Int1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MaximumLines).TM_RelevantValueInfo, TestCalculator.Int2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AdditionalRate).TM_RelevantValueInfo, TestCalculator.Decimal5Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.IncludedHeaders).TM_RelevantValueInfo, TestCalculator.Int3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyFeeType).TM_TextInfo, TestCalculator.String1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyLineType).TM_TextInfo, TestCalculator.String2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideFeeLineTypeOnQuote).TM_TextInfo, TestCalculator.Bool1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX).TM_RelevantValueInfo, TestCalculator.Decimal6Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType).TM_TextInfo, TestCalculator.String3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType).TM_TextInfo, TestCalculator.String4Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.HideMessageTypeOnQuote).TM_TextInfo, TestCalculator.Bool2Info);
		}

		public override void TestMapping()
		{
			TestMapping(AgencyCalculator.Items.AgencyRate, "Decimal1");
			TestMapping(AgencyCalculator.Items.CostPerAdditionalLine, "Decimal2");
			TestMapping(AgencyCalculator.Items.IncludedLines, "Int1");
			TestMapping(AgencyCalculator.Items.MaximumLines, "Int2");
			TestMapping(AgencyCalculator.Items.AdditionalRate, "Decimal5");
			TestMapping(Calculator.Items.Operator.MAX, "Decimal6");
			TestMapping(AgencyCalculator.Items.IncludedHeaders, "Int3");
			TestMapping(AgencyCalculator.Items.AgencyFeeType, "String1");
			TestMapping(AgencyCalculator.Items.AgencyLineType, "String2");
			TestMapping(AgencyCalculator.Items.HideFeeLineTypeOnQuote, "Bool1");
			TestMapping(AgencyCalculator.Items.MessageType, "String3");
			TestMapping(AgencyCalculator.Items.MessageSubType, "String4");
			TestMapping(AgencyCalculator.Items.HideMessageTypeOnQuote, "Bool2");
		}

		public override void TestList1()
		{
			AssertEquals("List1", typeof(RateFeeTypeList), TestCalculator.List1.GetType());
			Assert("Count > 0", TestCalculator.List1.Count > 1);
		}

		public override void TestList2()
		{
			AssertEquals("List2", typeof(RateLineTypeList), TestCalculator.List2.GetType());
			Assert("Count > 0", TestCalculator.List2.Count > 1);

			AssertEquals(false, TestCalculator.List2.ContainsCode(RateLineTypeList.Codes.PerTariffLinePerInvoice));
		}

		public override void TestList3()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("List3", typeof(Customs.Common.AU.AUJobMessageTypeList), TestCalculator.List3.GetType());
				AssertEquals("Count", 8, TestCalculator.List3.Count);
			}
		}

		public override void TestList4()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("List4", typeof(CodeDescriptionPairList), TestCalculator.List4.GetType());
				AssertEquals("Count", 3, TestCalculator.List4.Count);
			}
		}

		public void TestList4NotAU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ukraine))
			{
				AssertEquals("List4", typeof(CodeDescriptionPairList), TestCalculator.List4.GetType());
				AssertEquals("Count", 0, TestCalculator.List4.Count);
			}
		}

		public void TestMessageSubTypeForUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				TestCalculator.MessageType = "EXP";
				AssertEquals(0, TestCalculator.RateLineBizO.Lookups.MessageSubTypeList.Count);

				TestCalculator.MessageType = "IMP";
				AssertNotEquals(0, TestCalculator.RateLineBizO.Lookups.MessageSubTypeList.Count);
			}
		}

		public void TestMessageTypeForCA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AssertEquals(8, TestCalculator.RateLineBizO.Lookups.MessageTypeList.Count);
				AssertEquals(true, TestCalculator.RateLineBizO.Lookups.MessageTypeList.ContainsCode("MSC"));
			}
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddAgencyRateLine(rateEntry, "FRT", "AUD", hideFeeLineTypeOnQuote: true, agencyRate: 101, agencyFeeType: RateFeeTypeList.Codes.PerEntryPage, agencyLineType: RateLineTypeList.Codes.PerInvoiceLinePerEntry, additionalRate: 102, perAdditionalLine: 103, includedLines: 1, maxLines: 10);
			var rateLine11 = AddAgencyRateLine(rateEntry, "FRT", "AUD", hideFeeLineTypeOnQuote: true, agencyRate: 111, agencyFeeType: RateFeeTypeList.Codes.PerEntryPage, agencyLineType: RateLineTypeList.Codes.PerInvoiceLinePerEntry, additionalRate: 112, perAdditionalLine: 113, includedLines: 1, maxLines: 10);
			var rateLine12 = AddAgencyRateLine(rateEntry, "FRT", "AUD", hideFeeLineTypeOnQuote: true, agencyRate: 121, agencyFeeType: RateFeeTypeList.Codes.PerSupplier, agencyLineType: RateLineTypeList.Codes.PerInvoiceLinePerEntry, additionalRate: 122, perAdditionalLine: 123, includedLines: 3, maxLines: 12);
			var rateLine13 = AddAgencyRateLine(rateEntry, "FRT", "AUD", hideFeeLineTypeOnQuote: true, agencyRate: 131, agencyFeeType: RateFeeTypeList.Codes.PerEntryPage, agencyLineType: RateLineTypeList.Codes.PerHTSCodePerInvoice, additionalRate: 132, perAdditionalLine: 133, includedLines: 4, maxLines: 13);

			var rateLine20 = AddAgencyRateLine(rateEntry, "FRT", "USD", hideFeeLineTypeOnQuote: true, agencyRate: 200, agencyFeeType: RateFeeTypeList.Codes.PerEntryPage, agencyLineType: RateLineTypeList.Codes.PerInvoiceLinePerEntry, additionalRate: 202, perAdditionalLine: 203, includedLines: 5, maxLines: 20);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine13.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"for Import Formal Entry - Per Entry Page Per Entry, Per Invoice Line Per Entry|||",
					"First 1 Entry Page Per Entry, First 1 Invoice Line for each entry|AUD|212.00|",
					"Additional Entry Page Per Entry|AUD|214.00|",
					"Thereafter|AUD|216.00|per line",

					"for Import Formal Entry - Per Supplier, Per Invoice Line Per Entry|||",
					"First 1 Supplier, First 3 Invoice Lines for each entry|AUD|121.00|",
					"Additional Supplier|AUD|122.00|",
					"Thereafter|AUD|123.00|per line",

					"for Import Formal Entry - Per Entry Page Per Entry, Per HTS Code Per Invoice|||",
					"First 1 Entry Page Per Entry, First 4 Tariff Lines for each invoice|AUD|131.00|",
					"Additional Entry Page Per Entry|AUD|132.00|",
					"Thereafter|AUD|133.00|per line",

					"for Import Formal Entry - Per Entry Page Per Entry, Per Invoice Line Per Entry|||",
					"First 1 Entry Page Per Entry, First 5 Invoice Lines for each entry|USD|200.00|",
					"Additional Entry Page Per Entry|USD|202.00|",
					"Thereafter|USD|203.00|per line"
				}
			);
		}

		static RateLine AddAgencyRateLine(RateEntry rateEntry, ZString chargeCode, string currency, bool hideFeeLineTypeOnQuote, decimal agencyRate, string agencyFeeType, string agencyLineType, decimal additionalRate, double perAdditionalLine, int includedLines, int maxLines)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, AgencyCalculator.Code, currencyCode: currency);
			var calculator = rateLine.GetCalculator<AgencyCalculator>();
			calculator.HideFeeLineTypeOnQuote = hideFeeLineTypeOnQuote;
			calculator.AgencyRate = agencyRate;
			calculator.AgencyFeeType = agencyFeeType;
			calculator.AgencyLineType = agencyLineType;
			calculator.AdditionalRate = additionalRate;
			calculator.PerAdditionalLine = perAdditionalLine;
			calculator.IncludedLines = includedLines;
			calculator.MaximumLines = maxLines;

			calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			calculator.MessageSubType = "FRM";
			calculator.HideMessageTypeOnQuote = false;
			calculator.HideFeeLineTypeOnQuote = false;

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var costing = Helper.NewCosting(null);
			var parentEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "", "");

			Line.TL_RateCalculator = AgencyCalculator.Code;

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate - Per Shipment, Flat Fee||Not Charged|", quotationLines[0].ToString());

			TestCalculator.HideFeeLineTypeOnQuote = true;
			TestCalculator.AgencyRate = 45m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|45.00|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|45.00|", quotationLines[0].ToString());

			TestCalculator.HideFeeLineTypeOnQuote = false;
			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate - Per Entry, Flat Fee|||", quotationLines[0].ToString());
			AssertEquals("First 1 Entry|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Entry||Not Charged|", quotationLines[2].ToString());

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerSupplier;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate - Per Supplier, Flat Fee|||", quotationLines[0].ToString());
			AssertEquals("First 1 Supplier|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Supplier||Not Charged|", quotationLines[2].ToString());

			TestCalculator.AdditionalRate = 30m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate - Per Supplier, Flat Fee|||", quotationLines[0].ToString());
			AssertEquals("First 1 Supplier|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Supplier|USD|30.00|", quotationLines[2].ToString());

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntryPage;
			TestCalculator.AdditionalRate = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate - Per Entry Page Per Entry, Flat Fee|||", quotationLines[0].ToString());
			AssertEquals("First 1 Entry Page Per Entry|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Entry Page Per Entry|USD|10.00|", quotationLines[2].ToString());

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			TestCalculator.AdditionalRate = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate - Per Entry, Flat Fee|||", quotationLines[0].ToString());
			AssertEquals("First 1 Entry|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Entry|USD|10.00|", quotationLines[2].ToString());

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			TestCalculator.IncludedLines = 1;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate - Per Entry, Per Tariff Line Per Entry|||", quotationLines[0].ToString());
			AssertEquals("First 1 Entry, First 1 Tariff Line for each entry|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Entry|USD|10.00|", quotationLines[2].ToString());

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			TestCalculator.PerAdditionalLine = 0.8m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate - Per Entry, Per Invoice Line Per Entry|||", quotationLines[0].ToString());
			AssertEquals("First 1 Entry, First 1 Invoice Line for each entry|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Entry|USD|10.00|", quotationLines[2].ToString());
			AssertEquals("Thereafter|USD|0.80|per line", quotationLines[3].ToString());

			TestCalculator.HideFeeLineTypeOnQuote = true;
			TestCalculator.IncludedLines = 5;
			TestCalculator.MaximumLines = 20;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First 1 Entry, First 5 Invoice Lines for each entry|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Additional Entry|USD|10.00|", quotationLines[2].ToString());
			AssertEquals("Thereafter|USD|0.80|per line", quotationLines[3].ToString());
			AssertEquals("Maximum Lines||20|lines", quotationLines[4].ToString());

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate (5 Invoice Lines for each entry Included)|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Thereafter|USD|0.80|per line", quotationLines[2].ToString());
			AssertEquals("Maximum Lines||20|lines", quotationLines[3].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Base Rate (5 Invoice Lines for each entry Included)|USD|45.00|", quotationLines[1].ToString());
			AssertEquals("Thereafter|USD|0.80|per line", quotationLines[2].ToString());
			AssertEquals("Maximum Lines||20|lines", quotationLines[3].ToString());

			TestCalculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate for Import|||", quotationLines[0].ToString());

			TestCalculator.MessageSubType = "FRM";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate for Import Formal Entry|||", quotationLines[0].ToString());

			TestCalculator.HideMessageTypeOnQuote = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());

			TestCalculator.Maximum = 1000;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Maximum|USD|1000.00|", quotationLines[4].ToString());
		}

		public void TestCalculateFirstThreeInvoicesIncluded()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "USD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerInvoice;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerInvoice;
			TestCalculator.AgencyRate = 100m;
			TestCalculator.AdditionalRate = 10m;
			TestCalculator.IncludedHeaders = 3;

			entries.AddNew(7, 7, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 1, org1, 1);
			invoices.AddNew(Money.Empty, 2, org2, 2);
			invoices.AddNew(Money.Empty, 2, org3, 2);
			invoices.AddNew(Money.Empty, 2, org1, 2);

			parameters.Criteria.Invoices = invoices;

			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 1;

			AssertCalculation(parameters, 125m, "Base Rate USD 100.00 + 1 Invoice @ USD 10.00/Invoice + 3 additional tariff lines above 1 (incl) for each invoice @ USD 5.00/Tariff Line");
		}

		public void TestCalculateFirstTwoSuppliersIncluded()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "USD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerSupplier;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerInvoice;
			TestCalculator.AgencyRate = 100m;
			TestCalculator.AdditionalRate = 20m;
			TestCalculator.IncludedHeaders = 2;

			entries.AddNew(7, 7, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 1, org1, 1);
			invoices.AddNew(Money.Empty, 2, org2, 2);
			invoices.AddNew(Money.Empty, 2, org3, 2);
			invoices.AddNew(Money.Empty, 2, org1, 2);

			parameters.Criteria.Invoices = invoices;

			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 1;

			AssertCalculation(parameters, 135m, "Base Rate USD 100.00 + 1 Supplier @ USD 20.00/Supplier + 3 additional tariff lines above 1 (incl) for each invoice @ USD 5.00/Tariff Line");
		}

		public void TestCalculationPerEntry()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			entries.AddNew(5, 8, 0M);
			entries.AddNew(9, 25, 0M);
			entries.AddNew(15, 24, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 5, org1);
			invoices.AddNew(Money.Empty, 11, org2);
			invoices.AddNew(Money.Empty, 45, org1);
			parameters.Criteria.Invoices = invoices;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			TestCalculator.AgencyRate = 75m;
			TestCalculator.AdditionalRate = 0m;

			//Flat
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertCalculation(parameters, 75m, "Base Rate AUD 75.00");

			TestCalculator.AdditionalRate = 35m;
			AssertCalculation(parameters, 145m, "Base Rate AUD 75.00 + 2 Entries @ AUD 35.00/Entry");

			//Non-Flat
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			AssertCalculation(parameters, 145m, "Base Rate AUD 75.00 + 2 Entries @ AUD 35.00/Entry");

			TestCalculator.PerAdditionalLine = 3.75m;
			AssertCalculation(parameters, 253.75m, "Base Rate AUD 75.00 + 2 Entries @ AUD 35.00/Entry + 29 tariff lines for each entry @ AUD 3.75/Tariff Line");

			TestCalculator.IncludedLines = 10;
			AssertCalculation(parameters, 163.75m, "Base Rate AUD 75.00 + 2 Entries @ AUD 35.00/Entry + 5 additional tariff lines above 10 (incl) for each entry @ AUD 3.75/Tariff Line");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			AssertCalculation(parameters, 253.75m, "Base Rate AUD 75.00 + 2 Entries @ AUD 35.00/Entry + 29 additional invoice lines above 10 (incl) for each entry @ AUD 3.75/Invoice Line");

			parameters.Criteria.Entries = new EntryInfoCollection();
			AssertCalculation(parameters, 75m, "Base Rate AUD 75.00");
			parameters.Criteria.Entries = entries;

			TestCalculator.IncludedLines = 0;
			TestCalculator.MaximumLines = 20;
			AssertCalculation(parameters, 325m, "Base Rate AUD 75.00 + 2 Entries @ AUD 35.00/Entry + 48 invoice lines up to 20 (max) for each entry @ AUD 3.75/Invoice Line");

			parameters.Criteria.Entries = new EntryInfoCollection();
			parameters.Criteria.Entries.AddNew(15, 24, 0M);
			AssertCalculation(parameters, 150m, "Base Rate AUD 75.00 + 20 invoice lines up to 20 (max) for each entry @ AUD 3.75/Invoice Line");

			TestCalculator.Maximum = 100m;
			AssertCalculation(parameters, 100m, "Maximum AUD 100.00");

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerInvoice;
			TestCalculator.AgencyRate = 50m;
			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 0;
			TestCalculator.MaximumLines = 0;
			TestCalculator.Maximum = 0;
			parameters.Criteria.Entries = new EntryInfoCollection();
			AssertCalculation(parameters, 355m, "Base Rate AUD 50.00 + 61 invoice lines for each invoice @ AUD 5.00/Invoice Line");
		}

		public void TestCalculationPerEntryPage()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			entries.AddNew(5, 8, 0M);
			entries.AddNew(9, 25, 0M);
			entries.AddNew(15, 24, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 5, org1);
			invoices.AddNew(Money.Empty, 11, org2);
			invoices.AddNew(Money.Empty, 45, org1);
			parameters.Criteria.Invoices = invoices;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntryPage;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;

			TestCalculator.AgencyRate = 200m;
			TestCalculator.AdditionalRate = 15m;

			Env.Registry.Rating.AgencyCalcLinesFirstPageExport = 1;
			Env.Registry.Rating.AgencyCalcLinesAdditionalPageExport = 3;
			AssertCalculation(parameters, 750m, "3 entry pages per entry @ AUD 200.00/Entry Page Per Entry + 10 additional entry pages per entry @ AUD 15.00/Entry Page Per Entry");

			TestCalculator.IncludedHeaders = 2;//The first two pages are included in the base rate
			AssertCalculation(parameters, 705m, "3 entry pages per entry @ AUD 200.00/Entry Page Per Entry + 7 additional entry pages per entry above 2 (incl) @ AUD 15.00/Entry Page Per Entry");

			TestCalculator.IncludedHeaders = 1;
			Env.Registry.Rating.AgencyCalcLinesFirstPageExport = 5;
			Env.Registry.Rating.AgencyCalcLinesAdditionalPageExport = 5;
			AssertCalculation(parameters, 645m, "3 entry pages per entry @ AUD 200.00/Entry Page Per Entry + 3 additional entry pages per entry @ AUD 15.00/Entry Page Per Entry");

			Env.Registry.Rating.AgencyCalcLinesFirstPageExport = 5;
			Env.Registry.Rating.AgencyCalcLinesAdditionalPageExport = 20;
			AssertCalculation(parameters, 630m, "3 entry pages per entry @ AUD 200.00/Entry Page Per Entry + 2 additional entry pages per entry @ AUD 15.00/Entry Page Per Entry");
		}

		public void TestCalculationPerSupplier()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			entries.AddNew(5, 8, 0M);
			entries.AddNew(9, 25, 0M);
			entries.AddNew(15, 24, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 5, org1);
			invoices.AddNew(Money.Empty, 11, org2);
			invoices.AddNew(Money.Empty, 45, org1);
			parameters.Criteria.Invoices = invoices;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerSupplier;
			TestCalculator.AgencyRate = 75m;
			TestCalculator.AdditionalRate = 0m;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertCalculation(parameters, 75m, "Base Rate AUD 75.00");

			TestCalculator.AdditionalRate = 35m;
			AssertCalculation(parameters, 110m, "Base Rate AUD 75.00 + 1 Supplier @ AUD 35.00/Supplier");

			//non-flat
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerInvoice;
			TestCalculator.PerAdditionalLine = 3.75m;
			TestCalculator.IncludedLines = 10;
			AssertCalculation(parameters, 245m, "Base Rate AUD 75.00 + 1 Supplier @ AUD 35.00/Supplier + 36 additional invoice lines above 10 (incl) for each invoice @ AUD 3.75/Invoice Line");
		}

		public void TestCalculationForInvoice()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "USD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerInvoice;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerInvoice;
			TestCalculator.AgencyRate = 100m;
			TestCalculator.AdditionalRate = 10m;

			entries.AddNew(7, 7, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 1, org1, 1);
			invoices.AddNew(Money.Empty, 2, org2, 2);
			invoices.AddNew(Money.Empty, 2, org3, 2);
			invoices.AddNew(Money.Empty, 2, org1, 2);

			parameters.Criteria.Invoices = invoices;

			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 1;

			AssertCalculation(parameters, 145m, "Base Rate USD 100.00 + 3 Invoices @ USD 10.00/Invoice + 3 additional tariff lines above 1 (incl) for each invoice @ USD 5.00/Tariff Line");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertCalculation(parameters, 130m, "Base Rate USD 100.00 + 3 Invoices @ USD 10.00/Invoice");
		}

		public void TestCalculationPerTariffLinePerShipment()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 5, org1);
			invoices.AddNew(Money.Empty, 11, org2);
			invoices.AddNew(Money.Empty, 45, org1);
			parameters.Criteria.Invoices = invoices;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerShipment;
			TestCalculator.AgencyRate = 50m;
			TestCalculator.PerAdditionalLine = 5m;
			AssertCalculation(parameters, 50m, "Base Rate AUD 50.00");
		}

		public void TestCalculationPerHTSCode()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "USD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerHTSCodePerInvoice;
			TestCalculator.AgencyRate = 100m;

			invoices.AddNew(Money.Empty, 3, org1);
			invoices.AddNew(Money.Empty, 2, org2);

			parameters.Criteria.TariffsPerInvoice = invoices;

			TestCalculator.PerAdditionalLine = 10m;
			TestCalculator.IncludedLines = 1;
			AssertCalculation(parameters, 130m, "Base Rate USD 100.00 + 3 additional tariff lines above 1 (incl) for each invoice @ USD 10.00/Tariff Line");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerHTSCodePerShipment;
			invoices = new InvoiceInfoCollection();
			invoices.AddNew(Money.Empty, 7, org1);
			parameters.Criteria.TariffsPerShipment = invoices;
			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 3;
			AssertCalculation(parameters, 120m, "Base Rate USD 100.00 + 4 additional tariff lines above 3 (incl) for shipment @ USD 5.00/Tariff Line");
		}

		public void TestCalculationForShipment()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			TestCalculator.AgencyRate = 75m;
			AssertCalculation(parameters, 75m, "Base Rate AUD 75.00");

			entries.AddNew(5, 8, 0M);
			entries.AddNew(9, 25, 0M);
			entries.AddNew(15, 24, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 5, org1);
			invoices.AddNew(Money.Empty, 11, org2);
			invoices.AddNew(Money.Empty, 45, org1);
			parameters.Criteria.Invoices = invoices;

			AssertCalculation(parameters, 75m, "Base Rate AUD 75.00");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerShipment;
			TestCalculator.MaximumLines = 100;
			TestCalculator.IncludedLines = 10;
			TestCalculator.PerAdditionalLine = 3.75m;
			AssertCalculation(parameters, 146.25m, "Base Rate AUD 75.00 + 19 additional tariff lines between 10 (incl) and 100 (max) for shipment @ AUD 3.75/Tariff Line");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerShipment;
			TestCalculator.MaximumLines = 50;
			AssertCalculation(parameters, 225m, "Base Rate AUD 75.00 + 40 additional invoice lines between 10 (incl) and 50 (max) for shipment @ AUD 3.75/Invoice Line");
		}

		public void TestCalculationPerSubHeader()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "USD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.Entries = entries;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerSubHeader;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerInvoice;
			TestCalculator.AgencyRate = 100m;
			TestCalculator.AdditionalRate = 10m;

			entries.AddNew(7, 7, 0M);
			entries.AddNew(9, 25, 0M);
			entries.AddNew(15, 24, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 1, org1, 1);
			invoices.AddNew(Money.Empty, 2, org2, 2);
			invoices.AddNew(Money.Empty, 2, org3, 2);
			invoices.AddNew(Money.Empty, 2, org1, 2);

			parameters.Criteria.Invoices = invoices;

			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 1;

			AssertCalculation(parameters, 115m, "Base Rate USD 100.00 + 3 additional tariff lines above 1 (incl) for each invoice @ USD 5.00/Tariff Line");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertCalculation(parameters, 100m, "Base Rate USD 100.00");
		}

		public void TestCalculationTotalHSLCountPerDeclaration()
		{
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			invoices.AddNew(Money.Empty, 11, null, 9);

			parameters.Criteria.TariffsPerShipment = invoices;

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.TotalHTSCountPerDeclaration;
			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.IncludedLines = 2;

			AssertCalculation(parameters, 35m, "7 additional tariff lines above 2 (incl) for declaration @ AUD 5.00/Tariff Line");
		}

		public void TestValidateAgencyFeeType()
		{
			TestCalculator.AgencyFeeType = "###";
			AssertHasError(TestCalculator.String1Info, "Enter a valid selection.");

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			AssertNoErrors(TestCalculator.String1Info);

			TestCalculator.AgencyFeeType = "";
			AssertHasError(TestCalculator.String1Info, "Please enter a value.");

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			AssertNoErrors(TestCalculator.String1Info);

			TestCalculator.AgencyFeeType = RateLineTypeList.Codes.FLAT;
			AssertHasError(TestCalculator.String1Info, "Enter a valid selection.");

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntryPage;
			AssertNoErrors(TestCalculator.String1Info);

			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerSubHeader;
			AssertNoErrors(TestCalculator.String1Info);
		}

		public void TestValidateAgencyLineType()
		{
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertNoErrors(TestCalculator.String2Info);

			TestCalculator.AgencyLineType = "###";
			AssertHasError(TestCalculator.String2Info, "Enter a valid selection.");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			AssertNoErrors(TestCalculator.String2Info);

			TestCalculator.AgencyLineType = "";
			AssertHasError(TestCalculator.String2Info, "Please enter a value.");

			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			AssertNoErrors(TestCalculator.String2Info);
		}

		public void TestValidateMessageType()
		{
			TestCalculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertNoErrors(TestCalculator.String3Info);

			TestCalculator.MessageType = "###";
			AssertHasError(TestCalculator.String3Info, "Enter a valid selection.");

			TestCalculator.MessageType = "";
			AssertNoErrors(TestCalculator.String3Info);

			TestCalculator2.MessageType = "";
			AssertHasError(TestCalculator2.String3Info, ErrorMessages.MessageTypeOrSubTypeDuplicate);

			TestCalculator2.MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertNoErrors(TestCalculator2.String3Info);
			AssertNoErrors(((RateLine)TestCalculator2.Line).TL_ACInfo);
		}

		public void TestValidateMessageType_CompTariff()
		{
			ChargeCode.AC_RateCalculator = AgencyCalculator.Code;
			var line1 = TestCalculator.Line as RateLine;
			line1.TL_AC = ChargeCode.PK;

			var agencyCalculator = (AgencyCalculator)line1.Calculator;

			agencyCalculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			agencyCalculator.MessageSubType = "SAC";
			agencyCalculator.AgencyRate = 80m;
			agencyCalculator.AdditionalRate = 30m;
			agencyCalculator.PerAdditionalLine = 3m;
			agencyCalculator.MaximumLines = 100;

			var line2 = line1.Parent.AddRateLine(ChargeCode.AC_Code);
			line2.TL_AC = ChargeCode.PK;
			line2.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var ctCalculator = (CompanyTariffOrCostBasedCalculator)line2.Calculator;
			ctCalculator.String3 = TestCalculator.MessageType;
			ctCalculator.String4 = TestCalculator.MessageSubType;

			AssertNoErrors(ctCalculator.String3Info);
			AssertNoErrors(ctCalculator.String4Info);
		}

		public void TestValidateMessageSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				TestCalculator.MessageType = SharedJobMessageTypeList.Codes.Import;
				TestCalculator.MessageSubType = "SAC";
				AssertNoErrors(TestCalculator.String4Info);

				TestCalculator.MessageSubType = "###";
				AssertHasError(TestCalculator.String4Info, "Enter a valid selection.");

				TestCalculator.MessageSubType = "";
				AssertNoErrors(TestCalculator.String4Info);

				TestCalculator2.MessageType = SharedJobMessageTypeList.Codes.Import;
				TestCalculator2.MessageSubType = "";
				AssertHasError(TestCalculator2.String4Info, ErrorMessages.MessageTypeOrSubTypeDuplicate);

				TestCalculator2.MessageSubType = "FRM";
				AssertNoErrors(TestCalculator2.String4Info);
			}
		}

		public void TestValidateTM_Value()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("ORG", "ALL", "AUSYD", "CNSHA");
			var newLine = rateEntry.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;

			newLine.GetCalculator<AgencyCalculator>().MaximumLines = 10;
			newLine.GetCalculator<AgencyCalculator>().IncludedHeaders = 20;
			newLine.GetCalculator<AgencyCalculator>().IncludedLines = 30;

			Factory.Save();

			var mliRateLineItem = newLine.Calculator.FindRateLineItem("MLI");
			var inhRateLineItem = newLine.Calculator.FindRateLineItem("INH");
			var incRateLineItem = newLine.Calculator.FindRateLineItem("INC");

			AssertEquals("Has no errors.", 0, mliRateLineItem.TM_ValueInfo.GetErrors().ToArray().Length);
			AssertEquals("Has no errors.", 0, inhRateLineItem.TM_ValueInfo.GetErrors().ToArray().Length);
			AssertEquals("Has no errors.", 0, incRateLineItem.TM_ValueInfo.GetErrors().ToArray().Length);

			newLine.GetCalculator<AgencyCalculator>().MaximumLines = -10;
			newLine.GetCalculator<AgencyCalculator>().IncludedHeaders = -20;
			newLine.GetCalculator<AgencyCalculator>().IncludedLines = -30;

			Factory.Save();

			AssertEquals("Has 1 error.", 1, mliRateLineItem.TM_ValueInfo.GetErrors().ToArray().Length);
			AssertEquals("Has 1 error.", 1, inhRateLineItem.TM_ValueInfo.GetErrors().ToArray().Length);
			AssertEquals("Has 1 error.", 1, incRateLineItem.TM_ValueInfo.GetErrors().ToArray().Length);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			TestCalculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.AgencyRate = 80m;
			TestCalculator.AdditionalRate = 30m;
			TestCalculator.PerAdditionalLine = 3m;
			TestCalculator.MaximumLines = 100;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.AgencyRate = 100m;
			TestCalculator.AdditionalRate = 50m;
			TestCalculator.PerAdditionalLine = 5m;
			TestCalculator.MaximumLines = 120;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.BaseRate = 60m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(RateFeeTypeList.Codes.PerEntry, clonedLine.GetCalculator<AgencyCalculator>().AgencyFeeType);
			AssertEquals(RateLineTypeList.Codes.PerInvoiceLinePerEntry, clonedLine.GetCalculator<AgencyCalculator>().AgencyLineType);
			clonedLine.GetCalculator<AgencyCalculator>().Line.ViewAgentRates = false;
			AssertEquals(140m, clonedLine.GetCalculator<AgencyCalculator>().AgencyRate);
			AssertEquals(30m, clonedLine.GetCalculator<AgencyCalculator>().AdditionalRate);
			AssertEquals(3m, clonedLine.GetCalculator<AgencyCalculator>().PerAdditionalLine);
			AssertEquals(100, clonedLine.GetCalculator<AgencyCalculator>().MaximumLines);
			clonedLine.GetCalculator<AgencyCalculator>().Line.ViewAgentRates = true;
			AssertEquals(160m, clonedLine.GetCalculator<AgencyCalculator>().AgencyRate);
			AssertEquals(50m, clonedLine.GetCalculator<AgencyCalculator>().AdditionalRate);
			AssertEquals(5m, clonedLine.GetCalculator<AgencyCalculator>().PerAdditionalLine);
			AssertEquals(120, clonedLine.GetCalculator<AgencyCalculator>().MaximumLines);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(RateFeeTypeList.Codes.PerEntry, clonedLine.GetCalculator<AgencyCalculator>().AgencyFeeType);
			AssertEquals(RateLineTypeList.Codes.PerInvoiceLinePerEntry, clonedLine.GetCalculator<AgencyCalculator>().AgencyLineType);
			clonedLine.GetCalculator<AgencyCalculator>().Line.ViewAgentRates = false;
			AssertEquals(156m, clonedLine.GetCalculator<AgencyCalculator>().AgencyRate);
			AssertEquals(36m, clonedLine.GetCalculator<AgencyCalculator>().AdditionalRate);
			AssertEquals(3.6m, clonedLine.GetCalculator<AgencyCalculator>().PerAdditionalLine);
			AssertEquals(100, clonedLine.GetCalculator<AgencyCalculator>().MaximumLines);
			clonedLine.GetCalculator<AgencyCalculator>().Line.ViewAgentRates = true;
			AssertEquals(180m, clonedLine.GetCalculator<AgencyCalculator>().AgencyRate);
			AssertEquals(60m, clonedLine.GetCalculator<AgencyCalculator>().AdditionalRate);
			AssertEquals(6m, clonedLine.GetCalculator<AgencyCalculator>().PerAdditionalLine);
			AssertEquals(120, clonedLine.GetCalculator<AgencyCalculator>().MaximumLines);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(RateFeeTypeList.Codes.PerEntry, clonedLine.GetCalculator<AgencyCalculator>().AgencyFeeType);
			AssertEquals(RateLineTypeList.Codes.PerInvoiceLinePerEntry, clonedLine.GetCalculator<AgencyCalculator>().AgencyLineType);
			clonedLine.GetCalculator<AgencyCalculator>().Line.ViewAgentRates = false;
			AssertEquals(168m, clonedLine.GetCalculator<AgencyCalculator>().AgencyRate);
			AssertEquals(36m, clonedLine.GetCalculator<AgencyCalculator>().AdditionalRate);
			AssertEquals(3.6m, clonedLine.GetCalculator<AgencyCalculator>().PerAdditionalLine);
			AssertEquals(100, clonedLine.GetCalculator<AgencyCalculator>().MaximumLines);
			clonedLine.GetCalculator<AgencyCalculator>().Line.ViewAgentRates = true;
			AssertEquals(192m, clonedLine.GetCalculator<AgencyCalculator>().AgencyRate);
			AssertEquals(60m, clonedLine.GetCalculator<AgencyCalculator>().AdditionalRate);
			AssertEquals(6m, clonedLine.GetCalculator<AgencyCalculator>().PerAdditionalLine);
			AssertEquals(120, clonedLine.GetCalculator<AgencyCalculator>().MaximumLines);
		}

		public void TestAgencyLineReadOnly()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_RateCalculator = AgencyCalculator.Code;

			var testQuote = Factory.New<Quote>();
			var testEntry = testQuote.AddRateEntry("ORG");
			var testRateLine = testEntry.RateLines.AddNew();

			testRateLine.TL_AC = chargeCode.PK;
			var calculator = (AgencyCalculator)testRateLine.Calculator;

			calculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;

			calculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertEquals(false, calculator.Decimal1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal5Info.ReadOnly);
			AssertEquals(true, calculator.Int1Info.ReadOnly);
			AssertEquals(true, calculator.Decimal2Info.ReadOnly);
			AssertEquals(true, calculator.Int2Info.ReadOnly);

			calculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			AssertEquals(false, calculator.Decimal1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal5Info.ReadOnly);
			AssertEquals(false, calculator.Int1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal2Info.ReadOnly);
			AssertEquals(false, calculator.Int2Info.ReadOnly);

			calculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			AssertEquals(false, calculator.Decimal1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal5Info.ReadOnly);
			AssertEquals(false, calculator.Int1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal2Info.ReadOnly);
			AssertEquals(false, calculator.Int2Info.ReadOnly);
			AssertEquals(false, calculator.Int3Info.ReadOnly);

			calculator.AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			AssertEquals(true, calculator.Int3Info.ReadOnly);

			calculator.AgencyLineType = RateLineTypeList.Codes.FLAT;
			AssertEquals(false, calculator.Decimal1Info.ReadOnly);
			AssertEquals(true, calculator.Decimal5Info.ReadOnly);
			AssertEquals(true, calculator.Int1Info.ReadOnly);
			AssertEquals(true, calculator.Decimal2Info.ReadOnly);
			AssertEquals(true, calculator.Int2Info.ReadOnly);

			calculator.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerShipment;
			AssertEquals(false, calculator.Decimal1Info.ReadOnly);
			AssertEquals(true, calculator.Decimal5Info.ReadOnly);
			AssertEquals(false, calculator.Int1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal2Info.ReadOnly);
			AssertEquals(false, calculator.Int2Info.ReadOnly);

			calculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerShipment;
			AssertEquals(false, calculator.Decimal1Info.ReadOnly);
			AssertEquals(true, calculator.Decimal5Info.ReadOnly);
			AssertEquals(false, calculator.Int1Info.ReadOnly);
			AssertEquals(false, calculator.Decimal2Info.ReadOnly);
			AssertEquals(false, calculator.Int2Info.ReadOnly);
		}

		public void TestDefaultIncludeHeaders()
		{
			TestCalculator.String1 = RateFeeTypeList.Codes.PerInvoice;
			AssertEquals(1, TestCalculator.Int3);
			AssertEquals(1, TestCalculator.IncludedHeaders);

			TestCalculator.IncludedHeaders = 3;
			AssertEquals(3, TestCalculator.Int3);
			AssertEquals(3, TestCalculator.IncludedHeaders);

			TestCalculator.String1 = RateFeeTypeList.Codes.PerShipment;
			AssertEquals(0, TestCalculator.Int3);
			AssertEquals(0, TestCalculator.IncludedHeaders);
		}

		public void TestDefaultFeeLineTypes()
		{
			var newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 1";
			var testCalc = newLine.Calculator as AgencyCalculator;

			AssertEquals("Default Fee Type must be taken from Registry", RateFeeTypeList.Codes.PerShipment, testCalc.AgencyFeeType);
			AssertEquals("Default Line Type must be taken from Registry", RateLineTypeList.Codes.FLAT, testCalc.AgencyLineType);

			RatingDataRegistry.Instance.DefaultFeeType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RateFeeTypeList.Codes.PerEntryPage);
			RatingDataRegistry.Instance.DefaultLineType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RateLineTypeList.Codes.PerInvoiceLinePerEntry);

			newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 2";
			testCalc = newLine.Calculator as AgencyCalculator;

			AssertEquals("Default Fee Type must be taken from Registry", RateFeeTypeList.Codes.PerEntryPage, testCalc.AgencyFeeType);
			AssertEquals("Default Line Type must be taken from Registry", RateLineTypeList.Codes.PerInvoiceLinePerEntry, testCalc.AgencyLineType);
		}

		public void TestAgencyCalculatorForMSCJobTypeInCA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,DST");

				var chargeCode = Helper.ChargeCodes.New("TESTAC", "TEST Agency Calculator", CalculatorCode, ChargeCodeGroupList.Codes.Destination);
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var clientRate = Helper.NewClientRate(importer);
				var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ALL, ZString.Empty, Core.Constants.CountryCodes.Canada);
				var rateLine = entry.AddRateLine(chargeCode);
				rateLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Canada;
				var calculator = (AgencyCalculator)rateLine.Calculator;
				calculator.Decimal1 = 10m;
				calculator.String1 = RateFeeTypeList.Codes.PerShipment;
				calculator.String3 = SharedJobMessageTypeList.Codes.MiscellaneousCustoms;

				Factory.Save();

				var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.CA.IJobDeclaration>();
				declaration[JobDeclarationSchema.JE_DeclarationReference] = "B00001000";
				declaration[JobDeclarationSchema.JE_OH_Importer] = importer.PK;
				declaration[JobDeclarationSchema.JE_MessageType] = "MSC";
				declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = "CAYTO";

				var job = new JobHeader.Loader((IJobHeaderParent)declaration).TryLoadOrCreate();
				job.LocalZAddressWithContact.OrgPK = importer.PK;

				var ratingSupporter = (IRatingSupporterWithAdapter)declaration;
				var testAutoRater = new FreightAutoRater(new RatingContext());
				var results = testAutoRater.AutoRate(new AutoRatingProxy(ratingSupporter.RatingAdapter), CostSell.Revenue).RateInfoCollection;
				AssertEquals("results.Count should be 1", 1, results.Count);
				var result = results[0];
				AssertEquals("result.ChargeCode should match", chargeCode.PK, result.ChargeCode.PK);
				AssertEquals("result.LocalAmount should be 10", 10m, result.LocalAmount);
				AssertEquals("result.LocalCurrency should be CAD", Core.Constants.CurrencyCodes.Canada, result.LocalCurrency);
			}
		}

		public void TestHandlingEmptyAgencyLineType_PerEntryPage() => TestHandlingEmptyAgencyLineTypeByAgencyFeeType(RateFeeTypeList.Codes.PerEntryPage);

		public void TestHandlingEmptyAgencyLineType_PerEntry() => TestHandlingEmptyAgencyLineTypeByAgencyFeeType(RateFeeTypeList.Codes.PerEntry);

		void TestHandlingEmptyAgencyLineTypeByAgencyFeeType(string agencyFeeType)
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var entries = new EntryInfoCollection();
			var invoices = new InvoiceInfoCollection();

			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			entries.AddNew(5, 8, 0M);
			entries.AddNew(9, 25, 0M);
			entries.AddNew(15, 24, 0M);
			parameters.Criteria.Entries = entries;

			invoices.AddNew(Money.Empty, 5, org1);
			invoices.AddNew(Money.Empty, 11, org2);
			invoices.AddNew(Money.Empty, 45, org1);
			parameters.Criteria.Invoices = invoices;

			TestCalculator.AgencyFeeType = agencyFeeType;
			TestCalculator.AgencyLineType = ZString.Empty;

			TestCalculator.AgencyRate = 200m;
			TestCalculator.AdditionalRate = 15m;

			using (_Rating.Start(new LoggerDecorator()))
			{
				var (results, error) = TestCalculator.Calculate(parameters);
				AssertEquals("Results should be an empty collection.", 0, results.Count());
				AssertEquals("Error message should indicate incorrect or missing data.", "incorrect or missing data", error);

				AssertNotNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(AgencyCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return AgencyCalculator.Code; }
		}

		new AgencyCalculator TestCalculator
		{
			get { return (AgencyCalculator)base.TestCalculator; }
		}

		AgencyCalculator TestCalculator2
		{
			get
			{
				if (fTestCalculator2 == null)
				{
					var line2 = Line.Parent.RateLines.AddNew();
					line2.TL_AC = ChargeCode.PK;
					line2.TL_RateDesc = "Test Rate #2";
					fTestCalculator2 = line2.Calculator as AgencyCalculator;
				}

				return fTestCalculator2;
			}
		}

		AgencyCalculator fTestCalculator2;

		#endregion
	}
}
