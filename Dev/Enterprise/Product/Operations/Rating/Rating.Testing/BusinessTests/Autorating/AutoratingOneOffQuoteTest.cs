using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;

namespace Enterprise.Rating.Business.Testing
{
	public sealed class AutoratingOneOffQuoteTest : BaseRatingIntegrationTest
	{
		#region Potential Carriers

		public void TestPotentialCarriers_TransitTime_Revenue()
		{
			var transportProvider1 = TransportProvider1;
			var transportProvider2 = TransportProvider2;
			Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient);
			AddRateEntryWithFlatRateLine(clientRate, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110m, description: $"CLIENT-10", transitTime: "10");
			AddRateEntryWithFlatRateLine(clientRate, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120m, description: $"CLIENT-20", transitTime: "20");
			AddRateEntryWithFlatRateLine(clientRate, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 130m, description: $"CLIENT-30", transitTime: "30");

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransitTime = "30";
			oneOffQuote.OH_Carrier = EmiratesAirlines.PK;

			var possibleCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = transportProvider1.PK;
			possibleCarrier1.TTC_TransitTime = "10";

			var possibleCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_OH_Carrier = transportProvider2.PK;
			possibleCarrier2.TTC_TransitTime = "20";

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 130m, }, },
				oneOffQuote,
				NewClient,
				autorateCosts: false,
				autorateRevenue: true
			);
		}

		public void TestPotentialCarriers_TransitTime_Costing()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			AddRateEntryWithFlatRateLine(costing, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m, transitTime: "10");
			AddRateEntryWithFlatRateLine(costing, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m, transitTime: "20");

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);

			var possibleCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = TransportProvider1.PK;
			possibleCarrier1.TTC_TransitTime = "20";

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 200m, }, },
				oneOffQuote,
				NewClient,
				autorateCosts: true,
				autorateRevenue: false
			);
		}

		public void TestPotentialCarriers_TransitTime_MultipleRates_Costing()
		{
			var costing1 = Helper.NewCosting(TransportProvider1);
			AddRateEntryWithFlatRateLine(costing1, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110m, description: $"CARRIER1-10", transitTime: "10");
			AddRateEntryWithFlatRateLine(costing1, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120m, description: $"CARRIER1-20", transitTime: "20");
			AddRateEntryWithFlatRateLine(costing1, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 130m, description: $"CARRIER1-30", transitTime: "30");

			var costing2 = Helper.NewCosting(TransportProvider2);
			AddRateEntryWithFlatRateLine(costing2, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 210m, description: $"CARRIER2-10", transitTime: "10");
			AddRateEntryWithFlatRateLine(costing2, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 220m, description: $"CARRIER2-20", transitTime: "20");
			AddRateEntryWithFlatRateLine(costing2, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 230m, description: $"CARRIER2-30", transitTime: "30");

			var costing3 = Helper.NewCosting(EmiratesAirlines);
			AddRateEntryWithFlatRateLine(costing3, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 310m, description: $"CARRIER3-10", transitTime: "10");
			AddRateEntryWithFlatRateLine(costing3, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 320m, description: $"CARRIER3-20", transitTime: "20");
			AddRateEntryWithFlatRateLine(costing3, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 330m, description: $"CARRIER3-30", transitTime: "30");

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransitTime = "30";
			oneOffQuote.OH_Carrier = EmiratesAirlines.PK;

			var possibleCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = TransportProvider1.PK;
			possibleCarrier1.TTC_TransitTime = "10";

			var possibleCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_OH_Carrier = TransportProvider2.PK;
			possibleCarrier2.TTC_TransitTime = "20";

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 110m, },
					new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 220m, },
					new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 330m, },
				},
				oneOffQuote,
				NewClient,
				autorateCosts: true,
				autorateRevenue: false
			);
		}

		public void TestPotentialCarriers_TransitTime_EmptyPotentialCarrierTransitTime_Costing()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			AddRateEntryWithFlatRateLine(costing, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m, transitTime: "10");
			AddRateEntryWithFlatRateLine(costing, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m, transitTime: "20");

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransitTime = "20";

			var possibleCarrier = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier.TTC_OH_Carrier = TransportProvider1.PK;

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 200m, }, },
				oneOffQuote,
				NewClient,
				autorateCosts: true,
				autorateRevenue: false
			);
		}

		public void TestPotentialCarriers_TransitTime_CarrierExistInPotentialCarrier_Costing()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			AddRateEntryWithFlatRateLine(costing, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m, transitTime: "10");
			AddRateEntryWithFlatRateLine(costing, Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m, transitTime: "20");

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransitTime = "10";
			oneOffQuote.OH_Carrier = TransportProvider1.PK;

			var possibleCarrier = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier.TTC_OH_Carrier = TransportProvider1.PK;
			possibleCarrier.TTC_TransitTime = "20";

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 100m, }, },
				oneOffQuote,
				NewClient,
				autorateCosts: true,
				autorateRevenue: false
			);
		}

		public void TestAutorating_CostAndRevenue_PotentialCarriers_SellRateIsCstCalculator_ShouldCrateOneChargePerCarrier()
		{
			var supplier1 = Helper.NewOrgHeader();
			var supplier2 = Helper.NewOrgHeader();
			var supplier3 = Helper.NewOrgHeader();

			var costing1 = Helper.NewCosting(supplier1);
			costing1.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 1m, QuantityUnit.KG).TI_TransitTime = "10";
			costing1.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 2m, QuantityUnit.KG).TI_TransitTime = "20";
			costing1.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 3m, QuantityUnit.KG).TI_TransitTime = "30";

			var costing2 = Helper.NewCosting(supplier2);
			costing2.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 11m, QuantityUnit.KG).TI_TransitTime = "10";
			costing2.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 22m, QuantityUnit.KG).TI_TransitTime = "20";
			costing2.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 33, QuantityUnit.KG).TI_TransitTime = "30";

			var costing3 = Helper.NewCosting(supplier3);
			costing3.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 44m, QuantityUnit.KG).TI_TransitTime = "10";
			costing3.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 55m, QuantityUnit.KG).TI_TransitTime = "20";
			costing3.AddRateEntryWithUnitRateLine(Category.AIR, RateMode.LSE, "AU", "US", "FRT", 66m, QuantityUnit.KG).TI_TransitTime = "30";

			Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient);
			var entry1 = clientRate.AddRateEntry(Category.AIR, RateMode.LSE, "AU", "US");
			entry1.TI_OH_TransportProvider = supplier1.PK;
			entry1.RateLines.RemoveAndDeleteAll();
			entry1.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG).
				GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 10m;

			var entry2 = clientRate.AddRateEntry(Category.AIR, RateMode.LSE, "AU", "US");
			entry2.TI_OH_TransportProvider = supplier2.PK;
			entry2.RateLines.RemoveAndDeleteAll();
			entry2.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG).
				GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 20m;

			var entry3 = clientRate.AddRateEntry(Category.AIR, RateMode.LSE, "AU", "US");
			entry3.TI_OH_TransportProvider = supplier3.PK;
			entry3.RateLines.RemoveAndDeleteAll();
			entry3.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG).
				GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 30m;

			Factory.Save();

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;

			var carrier1 = oneOffShipment.PossibleCarriers.AddNew();
			carrier1.TTC_OH_Carrier = supplier1.PK;
			carrier1.TTC_TransitTime = "11";

			var carrier2 = oneOffShipment.PossibleCarriers.AddNew();
			carrier2.TTC_OH_Carrier = supplier2.PK;
			carrier2.TTC_TransitTime = "19";

			var carrier3 = oneOffShipment.PossibleCarriers.AddNew();
			carrier3.TTC_OH_Carrier = supplier3.PK;
			carrier3.TTC_TransitTime = "28";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 166.67m,
					JR_OSSellAmt = 176.67m,
					CostAccountCode = supplier1.OH_Code,
					CostCalculationDescription = "FRT: 166.667 Kilogram(s) @ AUD 1.00/KG",
					RevenueCalculationDescription = "FRT: Base Rate AUD 10.00 + 166.667 Kilogram(s) @ AUD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 3666.67m,
					JR_OSSellAmt = 3686.67m,
					CostAccountCode = supplier2.OH_Code,
					CostCalculationDescription = "FRT: 166.667 Kilogram(s) @ AUD 22.00/KG",
					RevenueCalculationDescription = "FRT: Base Rate AUD 20.00 + 166.667 Kilogram(s) @ AUD 22.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 11000.02m,
					JR_OSSellAmt = 11030.02m,
					CostAccountCode = supplier3.OH_Code,
					CostCalculationDescription = "FRT: 166.667 Kilogram(s) @ AUD 66.00/KG",
					RevenueCalculationDescription = "FRT: Base Rate AUD 30.00 + 166.667 Kilogram(s) @ AUD 66.00/KG"
				}
			};

			AutorateAndAssert("Each sell rate must match 1 cost charge by carrier", expectedCharges, quotedBooking, Consignee, autorateCosts: true, autorateRevenue: true, testInteractor: new TestInteractor());

			//	This test is different to the above one as in this case we don't have cost charges. So, it uses a different workflow of CST calculator
			//	which finds and calculates cost rates and then apply changes to them.
			var revenueCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 176.67m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 10.00 + 166.667 Kilogram(s) @ AUD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3686.67m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 20.00 + 166.667 Kilogram(s) @ AUD 22.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 11030.02m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 30.00 + 166.667 Kilogram(s) @ AUD 66.00/KG"
				}
			};

			AutorateAndAssert("Each sell rate must match 1 cost rate by carrier", revenueCharges, quotedBooking, Consignee, autorateCosts: false, autorateRevenue: true, testInteractor: new TestInteractor());
		}

		#endregion

		#region Autorating TransportMode/ContainerMode RAI

		public void TestAutorate_ModePair_RAI_LRA()
			=> AssertRateMatchedGivenMode("RAI", "RAI", "LCL", "LRA");

		public void TestAutorate_ModePair_RAI_FWL()
			=> AssertRateMatchedGivenMode("RAI", "RAI", "LCL", "FWL");

		public void TestAutorate_ModePair_RAI_FCL()
			=> AssertRateMatchedGivenMode("RAI", "RAI", "FCL", "RAI");

		public void TestAutorate_ModePair_RAI_ALL()
			=> AssertRateMatchedGivenMode("RAI", "RAI", "ORG", "ALL");

		public void TestAutorate_ModePair_SEA_LQD_Rates_LCL_LCL()
			=> AssertRateMatchedGivenMode("SEA", "LQD", "LCL", "LCL");

		public void TestAutorate_ModePair_SEA_LQD_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("SEA", "LQD", "ORG", "ALL");

		public void TestAutorate_ModePair_SEA_LQD_Rates_ORG_SEA()
			=> AssertRateMatchedGivenMode("SEA", "LQD", "ORG", "SEA");

		public void TestAutorate_ModePair_SEA_LQD_Rates_ORG_LCL()
			=> AssertRateMatchedGivenMode("SEA", "LQD", "ORG", "LCL");

		public void TestAutorate_ModePair_RAI_LQD_Rates_LCL_LRA()
			=> AssertRateMatchedGivenMode("RAI", "LQD", "LCL", "LRA");

		public void TestAutorate_ModePair_RAI_LQD_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("RAI", "LQD", "ORG", "ALL");

		public void TestAutorate_ModePair_RAI_LQD_Rates_ORG_RAI()
			=> AssertRateMatchedGivenMode("RAI", "LQD", "ORG", "RAI");

		public void TestAutorate_ModePair_RAI_LQD_Rates_ORG_LRA()
			=> AssertRateMatchedGivenMode("RAI", "LQD", "ORG", "LRA");

		public void TestAutorate_ModePair_FSA_LCL_Rates_LCL_LCL()
			=> AssertRateMatchedGivenMode("FSA", "LCL", "LCL", "LCL");

		public void TestAutorate_ModePair_FSA_LCL_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("FSA", "LCL", "ORG", "ALL");

		public void TestAutorate_ModePair_FSA_LCL_Rates_ORG_SEA()
			=> AssertRateMatchedGivenMode("FSA", "LCL", "ORG", "SEA");

		public void TestAutorate_ModePair_FSA_LCL_Rates_ORG_LCL()
			=> AssertRateMatchedGivenMode("FSA", "LCL", "ORG", "LCL");

		public void TestAutorate_ModePair_FSA_LSE_Rates_LCL_LCL()
			=> AssertRateMatchedGivenMode("FSA", "LSE", "LCL", "LCL");

		public void TestAutorate_ModePair_FSA_LSE_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("FSA", "LSE", "ORG", "ALL");

		public void TestAutorate_ModePair_FSA_LSE_Rates_ORG_SEA()
			=> AssertRateMatchedGivenMode("FSA", "LSE", "ORG", "SEA");

		public void TestAutorate_ModePair_FSA_LSE_Rates_ORG_LCL()
			=> AssertRateMatchedGivenMode("FSA", "LSE", "ORG", "LCL");

		public void TestAutorate_ModePair_FSA_ULD_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("FSA", "ULD", "ORG", "ALL");

		public void TestAutorate_ModePair_FAS_LCL_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("FAS", "LCL", "ORG", "ALL");

		public void TestAutorate_ModePair_FAS_LSE_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("FAS", "LSE", "ORG", "ALL");

		public void TestAutorate_ModePair_FAS_LSE_Rates_ORG_LSE()
			=> AssertRateMatchedGivenMode("FAS", "LSE", "ORG", "LSE");

		public void TestAutorate_ModePair_FAS_LSE_Rates_ORG_AIR()
			=> AssertRateMatchedGivenMode("FAS", "LSE", "ORG", "AIR");

		public void TestAutorate_ModePair_FAS_ULD_Rates_AIR_ULD()
			=> AssertRateMatchedGivenMode("FAS", "ULD", "AIR", "ULD");

		public void TestAutorate_ModePair_FAS_ULD_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("FAS", "ULD", "ORG", "ALL");

		public void TestAutorate_ModePair_FAS_ULD_Rates_ORG_ULD() 
			=> AssertRateMatchedGivenMode("FAS", "ULD", "ORG", "ULD");

		public void TestAutorate_ModePair_COU_OBC_Rates_LCL_OBC()
			=> AssertRateMatchedGivenMode("COU", "OBC", "LCL", "OBC");

		public void TestAutorate_ModePair_COU_OBC_Rates_ORG_OBC()
			=> AssertRateMatchedGivenMode("COU", "OBC", "ORG", "OBC");

		public void TestAutorate_ModePair_COU_OBC_Rates_ORG_COU()
			=> AssertRateMatchedGivenMode("COU", "OBC", "ORG", "COU");

		public void TestAutorate_ModePair_COU_OBC_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("COU", "OBC", "ORG", "ALL");

		public void TestAutorate_ModePair_COU_UNA_Rates_LCL_UNA()
			=> AssertRateMatchedGivenMode("COU", "UNA", "LCL", "UNA");

		public void TestAutorate_ModePair_COU_UNA_Rates_ORG_UNA()
			=> AssertRateMatchedGivenMode("COU", "UNA", "ORG", "UNA");

		public void TestAutorate_ModePair_COU_UNA_Rates_ORG_COU()
			=> AssertRateMatchedGivenMode("COU", "UNA", "ORG", "COU");

		public void TestAutorate_ModePair_COU_UNA_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("COU", "UNA", "ORG", "ALL");

		public void TestAutorate_ModePair_AIR_SCN_Rates_AIR_SCN()
			=> AssertRateMatchedGivenMode("AIR", "SCN", "AIR", "SCN");

		public void TestAutorate_ModePair_AIR_SCN_Rates_ORG_SCN()
			=> AssertRateMatchedGivenMode("AIR", "SCN", "ORG", "SCN");
		public void TestAutorate_ModePair_AIR_SCN_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("AIR", "SCN", "ORG", "ALL");

		public void TestAutorate_ModePair_SEA_SCN_Rates_FCL_SCN()
			=> AssertRateMatchedGivenMode("SEA", "SCN", "FCL", "SCN");

		public void TestAutorate_ModePair_SEA_SCN_Rates_ORG_SCN()
			=> AssertRateMatchedGivenMode("SEA", "SCN", "ORG", "SCN");
		public void TestAutorate_ModePair_SEA_SCN_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("SEA", "SCN", "ORG", "ALL");
		public void TestAutorate_ModePair_SEA_SCN_Rates_LCL_SCN()
			=> AssertRateMatchedGivenMode("SEA", "SCN", "LCL", "SCN");
		public void TestAutorate_ModePair_ROA_SCN_Rates_LCL_SCN()
			=> AssertRateMatchedGivenMode("ROA", "SCN", "LCL", "SCN");

		public void TestAutorate_ModePair_ROA_SCN_Rates_ORG_SCN()
			=> AssertRateMatchedGivenMode("ROA", "SCN", "ORG", "SCN");
		public void TestAutorate_ModePair_ROA_SCN_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("ROA", "SCN", "ORG", "ALL");

		public void TestAutorate_ModePair_RAI_SCN_Rates_LCL_SCN()
			=> AssertRateMatchedGivenMode("RAI", "SCN", "LCL", "SCN");

		public void TestAutorate_ModePair_RAI_SCN_Rates_ORG_SCN()
			=> AssertRateMatchedGivenMode("RAI", "SCN", "ORG", "SCN");
		public void TestAutorate_ModePair_RAI_SCN_Rates_ORG_ALL()
			=> AssertRateMatchedGivenMode("RAI", "SCN", "ORG", "ALL");

		public void TestAutorate_CourierTransportOOQ_ShouldMatchLCL_UNARate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[] { (category: "LCL", rateMode: "UNA", charge: "FRT", flat: 10m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 10m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchLCL_OBCRate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[] { (category: "LCL", rateMode: "OBC", charge: "FRT", flat: 20m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 20m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchLCL_BothUNAAndOBCRates()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[]
				{
					(category: "LCL", rateMode: "UNA", charge: "FRT", flat: 10m, container: ""),
					(category: "LCL", rateMode: "OBC", charge: "FRT", flat: 20m, container: "")
				},
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 10m },
					new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 20m }
				}
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchORG_UNARate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[] { (category: "ORG", rateMode: "UNA", charge: "ODOC", flat: 30m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 30m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchORG_OBCRate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[] { (category: "ORG", rateMode: "OBC", charge: "ODOC", flat: 40m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 40m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchORG_COURate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[] { (category: "ORG", rateMode: "COU", charge: "ODOC", flat: 50m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 50m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchORG_ALLRate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[] { (category: "ORG", rateMode: "ALL", charge: "ODOC", flat: 60m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 60m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchDST_UNARate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "USLAX", destination: "AUSYD"),
				rates: new[] { (category: "DST", rateMode: "UNA", charge: "DDOC", flat: 30m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_LocalSellAmt = 30m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchDST_OBCRate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "USLAX", destination: "AUSYD"),
				rates: new[] { (category: "DST", rateMode: "OBC", charge: "DDOC", flat: 40m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_LocalSellAmt = 40m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchDST_COURate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "USLAX", destination: "AUSYD"),
				rates: new[] { (category: "DST", rateMode: "COU", charge: "DDOC", flat: 50m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_LocalSellAmt = 50m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldMatchDST_ALLRate()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "USLAX", destination: "AUSYD"),
				rates: new[] { (category: "DST", rateMode: "ALL", charge: "DDOC", flat: 60m, container: "") },
				expectedCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_LocalSellAmt = 60m } }
			);

		public void TestAutorate_CourierTransportOOQ_ShouldPrintORGDSTChargesFilterOutLogCorrectly()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "USLAX", destination: "AUSYD"),
				rates: new[]
				{
					(category: "DST", rateMode: "LCL", charge: "DDOC", flat: 60m, container: ""),
				},
				expectedCharges: Array.Empty<AssertionCharge>(),
				expectedLog: "Information: RateEntry Filtered Client Rate CONSIGSYD reason: Transport Mode didn't match job COU."
			);

		public void TestAutorate_CourierTransportOOQ_ShouldPrintFRTChargesFilterOutLogCorrectly()
			=> AssertAutorate(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "USLAX", destination: "AUSYD"),
				rates: new[]
				{
					(category: "LCL", rateMode: "LCL", charge: "FRT", flat: 50m, container: ""),
				},
				expectedCharges: Array.Empty<AssertionCharge>(),
				expectedLog: "Information: RateEntry Filtered Client Rate CONSIGSYD reason: Transport Mode didn't match job COU."
			);

		public void TestAutorate_CourierTransportOOQ_OBC_UNARateShouldPrioritizedOverCOU_ALL()
			=> AssertAutorate
			(
				ooq: (transportMode: "COU", containerMode: "COU", container: null, origin: "AUSYD", destination: "USLAX"),
				rates: new[]
				{
					("ORG", "COU", "ODOC", 10m, ""),
					("ORG", "ALL", "ODOC", 20m, ""),
					("ORG", "OBC", "ODOC", 30m, ""),
					("ORG", "UNA", "ODOC", 40m, "")
				},
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 30m },
					new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 40m }
				}
			);

		public void TestAutorate_ModePair_RAI_DuplicateLCL()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine("LCL", "LRA", "AUSYD", "USLAX", "ODOC", 123m);
			clientRate.AddRateEntryWithFlatRateLine("LCL", "FWL", "AUSYD", "USLAX", "ODOC", 345m);

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking("RAI", "RAI", "CFR", Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = "RAI";
			oneOffQuote.ContainerMode = "RAI";

			AutorateAndAssert(new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 123m },
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 345m }
			}, oneOffQuote, Consignee, autorateCosts: false, testInteractor: new TestInteractor());
		}

		public void TestAutorate_ModePair_RAI_MixedFCLAndLCL()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine("FCL", "RAI", "AUSYD", "USLAX", "FRT", 200m, "CN", container: "20GP");
			clientRate.AddRateEntryWithUnitRateLine("LCL", "LRA", "AUSYD", "USLAX", "FRT", 5m, "KG");
			clientRate.AddRateEntryWithUnitRateLine("LCL", "FWL", "AUSYD", "USLAX", "FRT", 6m, "KG");

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking("RAI", "RAI", "CFR", Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = "RAI";
			oneOffQuote.ContainerMode = "RAI";

			// FCL
			var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			// LCL
			var loose = oneOffQuote.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
			loose.TPL_PackLineCount = 1;
			loose.TPL_F3_NKPackType = PkgUnit.Pallet;
			loose.TPL_Weight = 500m;
			loose.TPL_WeightUQ = Weight.Kilograms;
			loose.TPL_Volume = 0.5m;

			AutorateAndAssert(new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 200m }, // FCL $200 * 1CN = $200
				new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 2500m }, // LRA $5 * 500KG = $2500
				new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 3000m }, // FWL $6 * 500KG = $3000
			}, oneOffQuote, Consignee, autorateCosts: false, testInteractor: new TestInteractor());
		}

		#endregion

		#region Autorating TransportMode/ContainerMode ROA

		public void TestAutorate_ModePair_ROA_LRO()
			=> AssertRateMatchedGivenMode("ROA", "ROA", "LCL", "LRO");

		public void TestAutorate_ModePair_ROA_FTL()
			=> AssertRateMatchedGivenMode("ROA", "ROA", "LCL", "FTL");

		public void TestAutorate_ModePair_ROA_FCL()
			=> AssertRateMatchedGivenMode("ROA", "ROA", "FCL", "ROA");

		public void TestAutorate_ModePair_ROA_ALL()
			=> AssertRateMatchedGivenMode("ROA", "ROA", "ORG", "ALL");

		public void TestAutorate_ModePair_ROA_DuplicateLCL()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine("LCL", "LRO", "AUSYD", "USLAX", "ODOC", 123m);
			clientRate.AddRateEntryWithFlatRateLine("LCL", "FTL", "AUSYD", "USLAX", "ODOC", 345m);

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking("ROA", "ROA", "CFR", Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = "ROA";
			oneOffQuote.ContainerMode = "ROA";

			AutorateAndAssert(new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 123m },
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 345m }
			}, oneOffQuote, Consignee, autorateCosts: false);
		}

		public void TestAutorate_ModePair_ROA_MixedFCLAndLCL()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine("FCL", "ROA", "AUSYD", "USLAX", "FRT", 200m, "CN", container: "20GP");
			clientRate.AddRateEntryWithUnitRateLine("LCL", "LRO", "AUSYD", "USLAX", "FRT", 5m, "KG");
			clientRate.AddRateEntryWithUnitRateLine("LCL", "FTL", "AUSYD", "USLAX", "FRT", 6m, "KG");

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking("ROA", "ROA", "CFR", Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = "ROA";
			oneOffQuote.ContainerMode = "ROA";

			// FCL
			var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			// LCL
			var loose = oneOffQuote.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
			loose.TPL_PackLineCount = 1;
			loose.TPL_F3_NKPackType = PkgUnit.Pallet;
			loose.TPL_Weight = 500m;
			loose.TPL_WeightUQ = Weight.Kilograms;

			AutorateAndAssert(new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 200m }, // FCL $200 * 1CN = $200
				new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 2500m }, // LRO $5 * 500KG = $2500
				new AssertionCharge { ChargeCode = "FRT", JR_LocalSellAmt = 3000m }, // FTL $6 * 500KG = $3000
			}, oneOffQuote, Consignee, autorateCosts: false);
		}

		#endregion

		#region Autorating HBL Delivery Mode

		public void TestGivenDifferentHBLDeliveryModeRateEntries_WhenAutoRating_ThenRatingEntriesWithHighestPriorityShouldBeMatched()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry1 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", commodity: "GEN", removeLines: true);
			var frt1 = rateEntry1.AddFlatRateLine("FRT", 10m);
			var caf1 = rateEntry1.AddFlatRateLine("CAF", 20m);

			var rateEntry2 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", commodity: "GEN", removeLines: true);
			rateEntry2.TI_HBLDeliveryMode = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			var frt2 = rateEntry2.AddFlatRateLine("FRT", 30m);
			var baf2 = rateEntry2.AddFlatRateLine("BAF", 40m);

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, ZString.Empty, Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = TransportModes.Sea;
			oneOffQuote.ContainerMode = ContainerModes.FCL;
			oneOffQuote.PaymentTerms = PrepaidCollectFreightForwardingList.Codes.PPD;

			var revenueCalculationDescription = DescriptionHelpers.FormatWithTab("HBL Delivery Mode:");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 10m, JR_OSSellAmt = 10m },
				new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
			};

			AutorateAndAssert
			("Given One Of Quote's HBL Delivery Mode is empty, When auto rate, Then only rates with blank HBL Delivery Mode should be matched",
				expected,
				oneOffQuote,
				Consignee,
				autorateCosts: false,
				autorateRevenue: true,
				testInteractor: new TestInteractor()
			);

			oneOffQuote.ContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 30m, JR_OSSellAmt = 30m, RevenueCalculationDescription = $"{revenueCalculationDescription}DOOR/DOOR" },
				new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 40m, JR_OSSellAmt = 40m, RevenueCalculationDescription = $"{revenueCalculationDescription}DOOR/DOOR" }
			};

			AutorateAndAssert
			("Given no registry settings for HBL Delivery Priority, When auto rate, Then use One Of Quote's HBL Delivery as Priority",
				expected,
				oneOffQuote,
				Consignee,
				autorateCosts: false,
				autorateRevenue: true,
				testInteractor: new TestInteractor()
			);

			var configurations = new HBLDeliveryPriorityConfigCollection();
			var configuration = configurations.AddNew();
			configuration.ContainerMode = "FCL";
			configuration.HBLDeliveryMode = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			var setting = configuration.Settings.AddNew();
			setting.HBLDeliveryModePriority = Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			var setting1 = configuration.Settings.AddNew();
			setting1.HBLDeliveryModePriority = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			var setting2 = configuration.Settings.AddNew();
			setting2.HBLDeliveryModePriority = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;

			using (RatingDataRegistry.Instance.HBLDeliveryPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				var rateEntry3 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", commodity: "GEN", removeLines: true);
				rateEntry3.TI_HBLDeliveryMode = Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
				var frt3 = rateEntry3.AddFlatRateLine("FRT", 50m);
				var war3 = rateEntry3.AddFlatRateLine("WAR", 60m);

				Factory.Save();

				expected = new[]
				{
					new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 50m, JR_OSSellAmt = 50m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" }, // CFS/DOOR priority is greater than DOOR/DOOR in registry
					new AssertionCharge { ChargeCode = "WAR", JR_OSCostAmt = 60m, JR_OSSellAmt = 60m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" },
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 40m, JR_OSSellAmt = 40m, RevenueCalculationDescription = $"{revenueCalculationDescription}DOOR/DOOR" }
				};

				AutorateAndAssert
				("Given HBL Delivery Mode priority registry is set, When auto rate, Rate entry should be selected by the priority rule ",
					expected,
					oneOffQuote,
					Consignee,
					autorateCosts: false,
					autorateRevenue: true,
					testInteractor: new TestInteractor()
				);
			}

			configuration.Settings.Remove(setting1.PK);

			using (RatingDataRegistry.Instance.HBLDeliveryPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				expected = new[]
				{
					new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 50m, JR_OSSellAmt = 50m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" },
					new AssertionCharge { ChargeCode = "WAR", JR_OSCostAmt = 60m, JR_OSSellAmt = 60m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" },
				};

				AutorateAndAssert
				("Given HBL Delivery Mode priority registry is set without DOOR/DOOR, When auto rate, Then DOOR/DOOR modes rate entry should be filtered",
					expected,
					oneOffQuote,
					Consignee,
					autorateCosts: false,
					autorateRevenue: true,
					testInteractor: new TestInteractor()
				);
			}
		}

		public void TestAutorate_NotExistingCommodity()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine("LCL", "LCL", "AUSYD", "USLAX", "ODOC", 1m, commodity: "WOOL");

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking("SEA", "SEA", "CFR", Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.Commodity = "WOOL";
			oneOffQuote.TransportMode = "SEA";
			oneOffQuote.ContainerMode = "SEA";

			var testInteractor = new Moq.Mock<IAutoRatingGUIInteractor>();

			// Check that a OOQ can find a rate for a commodity that exists
			AutorateAndAssert(
				new[]
				{
					new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 1m }
				},
				oneOffQuote,
				Consignee,
				autorateCosts: false,
				testInteractor: testInteractor.Object
			);

			// Check that a OOQ can NOT find a rate for a commodity that does not exist
			// but where the commodity field is not blank.
			oneOffQuote.Commodity = "FAKE";
			AutorateAndAssert(
				Array.Empty<AssertionCharge>(),
				oneOffQuote,
				Consignee,
				autorateCosts: false,
				testInteractor: testInteractor.Object
			);
		}

		#endregion

		#region BBK BLK ROR

		public void TestAutoRateAIR_BCN_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BCN", "AIR", "BCN");
		public void TestAutoRateSEA_BBK_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BBK", "SEA", "BBK");
		public void TestAutoRateSEA_BLK_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BLK", "SEA", "BLK");
		public void TestAutoRateSEA_ROR_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "ROR", "SEA", "ROR");
		public void TestAutoRateSEA_BCN_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BCN", "SEA", "BCN");
		public void TestAutoRateRAI_BBK_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BBK", "RAI", "BBK");
		public void TestAutoRateRAI_BLK_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BLK", "RAI", "BLK");
		public void TestAutoRateRAI_BCN_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BCN", "RAI", "BCN");
		public void TestAutoRateROA_BCN_ClientRate() => AssertAutoRateBBK_BLK_ROR_BCN(ClientRate, "BCN", "ROA", "BCN");

		public void TestAutoRateSEA_BBK_Costing() => AssertAutoRateBBK_BLK_ROR_BCN(Costing, "BBK", "SEA", "BBK");
		public void TestAutoRateSEA_BLK_Costing() => AssertAutoRateBBK_BLK_ROR_BCN(Costing, "BLK", "SEA", "BLK");
		public void TestAutoRateSEA_ROR_Costing() => AssertAutoRateBBK_BLK_ROR_BCN(Costing, "ROR", "SEA", "ROR");
		public void TestAutoRateRAI_BBK_Costing() => AssertAutoRateBBK_BLK_ROR_BCN(Costing, "BBK", "RAI", "BBK");
		public void TestAutoRateRAI_BLK_Costing() => AssertAutoRateBBK_BLK_ROR_BCN(Costing, "BLK", "RAI", "BLK");
		public void TestAutoRateSEA_BBK_CompanyTariff() => AssertAutoRateBBK_BLK_ROR_BCN(Helper.NewCompanyTariff(), "BBK", "SEA", "BBK");
		public void TestAutoRateSEA_BLK_CompanyTariff() => AssertAutoRateBBK_BLK_ROR_BCN(Helper.NewCompanyTariff(), "BLK", "SEA", "BLK");
		public void TestAutoRateSEA_ROR_CompanyTariff() => AssertAutoRateBBK_BLK_ROR_BCN(Helper.NewCompanyTariff(), "ROR", "SEA", "ROR");
		public void TestAutoRateRAI_BBK_CompanyTariff() => AssertAutoRateBBK_BLK_ROR_BCN(Helper.NewCompanyTariff(), "BBK", "RAI", "BBK");
		public void TestAutoRateRAI_BLK_CompanyTariff() => AssertAutoRateBBK_BLK_ROR_BCN(Helper.NewCompanyTariff(), "BLK", "RAI", "BLK");

		#endregion

		#region Implementation

		void AssertAutorate(
			(string transportMode, string containerMode, string container, string origin, string destination) ooq,
			(string category, string rateMode, string charge, decimal flat, string container)[] rates,
			AssertionCharge[] expectedCharges,
			string expectedLog = null)
		{
			var clientRate = Helper.NewClientRate(Consignee);

			foreach (var rate in rates)
			{
				clientRate.AddRateEntryWithFlatRateLine(rate.category, rate.rateMode, ooq.origin, ooq.destination, rate.charge, rate.flat, container: rate.container);
			}

			var oneOffQuote = CreateQuotedBooking(ooq.transportMode, ooq.containerMode, "CFR", Consignee, Consignee, Consignee, null, ooq.origin, ooq.destination, 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = ooq.transportMode;
			oneOffQuote.ContainerMode = ooq.containerMode;

			if (ooq.container != null)
			{
				var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, ooq.container).PK;
				container.TC_ContainerCount = 1;
			}

			Factory.Save();

			AutorateAndAssert(expectedCharges, oneOffQuote, Consignee, autorateCosts: false);
			if (!string.IsNullOrEmpty(expectedLog))
			{
				AssertAutoratingAuditLogNoteContainsLines(oneOffQuote, "Should print log correctly", expectedLog);
			}
		}

		void AssertRateMatchedGivenMode(string transportMode, string containerMode, string rateCategory, string rateEntryMode)
		{
			var containerType = rateCategory == Category.FCL ? "20GP" : containerMode == ContainerModes.ULD ? "LD-3" : "";
			ClientRate.AddRateEntryWithFlatRateLine(rateCategory, rateEntryMode, "AUSYD", "USLAX", "ODOC", 123m, container: containerType);

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking(transportMode, containerMode, "CFR", Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = transportMode;
			oneOffQuote.ContainerMode = containerMode;

			if (rateCategory == Category.FCL)
			{
				var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.TC_ContainerCount = 1;
			}
			if (containerMode == ContainerModes.ULD)
			{
				var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-3").PK;
				container.TC_ContainerCount = 1;
			}

			AutorateAndAssert(new[] { new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 123 } }, oneOffQuote, Consignee, autorateCosts: false, testInteractor: new TestInteractor());
		}

		static RateEntry AddRateEntryWithFlatRateLine(RatingHeader ratingHeader, ZString rateCategory, ZString rateMode, ZString origin, ZString destination, ZString chargeCode, ZDecimal flatRateAmount, string currency = "", string container = "", string description = "", string transitTime = "", string hblDeliveryMode = "")
		{
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, chargeCode, flatRateAmount, currency, container, description);
			rateEntry.TI_TransitTime = transitTime;
			rateEntry.TI_HBLDeliveryMode = hblDeliveryMode;
			return rateEntry;
		}

		void AssertAutoRateBBK_BLK_ROR_BCN(RatingHeader ratingHeader, string rateMode, string transportMode, string containerMode)
		{
			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var costSell = ratingHeader.IsCosting() ? CostSell.Cost : CostSell.Revenue;
			var expectedCharges = CreateCharges(ratingHeader, costSell, rateMode, transportMode, containerMode);
			var oneOffQuote = CreateQuotedBooking(transportMode, containerMode, "CFR", Consignee, Consignee, Consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			ratingHeader.Factory.Save();

			AssertAutoRateBBK_BLK_ROR
			(
				oneOffQuote,
				rateMode,
				costSell: costSell,
				expectedCharges.expectedForEnable,
				expectedCharges.expectedForDisable
			);
		}

		(IEnumerable<AssertionCharge> expectedForEnable, IEnumerable<AssertionCharge> expectedForDisable) CreateCharges(RatingHeader ratingHeader, CostSell costSell, string rateMode, string transportMode, string containerMode)
		{
			var expectedForDisable = new List<AssertionCharge>();
			var expectedForEnable = new List<AssertionCharge>();
			var rateModeCompare = transportMode == TransportModes.Sea ? RateMode.LCL
				: transportMode == TransportModes.Rail ? RateMode.LRA
				: transportMode == TransportModes.Air ? RateMode.LSE : RateMode.LRO;
			
			ratingHeader.AddRateEntryWithFlatRateLine(Category.AIR, RateMode.LSE, "AUSYD", "USLAX", "ODOC", 70m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(Category.AIR, RateMode.BCN, "AUSYD", "USLAX", "ODOC", 80m, currency: "USD");

			if (transportMode != "AIR")
			{
				ratingHeader.AddRateEntryWithFlatRateLine(Category.LCL, rateModeCompare, "AUSYD", "USLAX", "ODOC", 10m, currency: "USD");
				ratingHeader.AddRateEntryWithFlatRateLine(Category.LCL, rateMode, "AUSYD", "USLAX", "ODOC", 20m, currency: "USD"); 
			}

			ratingHeader.AddRateEntryWithFlatRateLine(Category.ORG, rateModeCompare, "AUSYD", "USLAX", "ODOC", 30m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(Category.ORG, rateMode, "AUSYD", "USLAX", "ODOC", 40m, currency: "USD");

			ratingHeader.AddRateEntryWithFlatRateLine(Category.DST, rateModeCompare, "AUSYD", "USLAX", "ODOC", 50m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(Category.DST, rateMode, "AUSYD", "USLAX", "ODOC", 60m, currency: "USD");

			if (transportMode == "AIR")
			{
				expectedForDisable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 70.00", 70m));
				expectedForEnable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 80.00", 80m));
			}
			else
			{
				expectedForDisable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 10.00", 10m));
				expectedForEnable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 20.00", 20m));
			}
			expectedForDisable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 30.00", 30m));
			expectedForEnable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 40.00", 40m));
			expectedForDisable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 50.00", 50m));
			expectedForEnable.Add(NewAssertionCharge(costSell, "ODOC", "ODOC: Base Rate USD 60.00", 60m));

			return (expectedForEnable, expectedForDisable);
		}

		void AssertAutoRateBBK_BLK_ROR(QuotedBooking oneOffQuote, string rateMode, CostSell costSell, IEnumerable<AssertionCharge> expectedForEnable, IEnumerable<AssertionCharge> expectedForDisable)
		{
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert
				(
					expected: expectedForDisable,
					oneOffQuote,
					Consignee,
					autorateCosts: costSell == CostSell.Cost,
					autorateRevenue: costSell == CostSell.Revenue,
					testInteractor: new TestInteractor()
				);
			}

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert
				(
					expected: expectedForEnable,
					oneOffQuote,
					Consignee,
					autorateCosts: costSell == CostSell.Cost,
					autorateRevenue: costSell == CostSell.Revenue,
					testInteractor: new TestInteractor()
				);
			}
		}

		static AssertionCharge NewAssertionCharge(CostSell costSell, ZString chargeCode, ZString description, ZDecimal amount)
		{
			return costSell == CostSell.Cost
				? new AssertionCharge { ChargeCode = chargeCode, JR_OSCostAmt = amount, CostCalculationDescription = description }
				: new AssertionCharge { ChargeCode = chargeCode, JR_OSSellAmt = amount, RevenueCalculationDescription = description };
		}

		RatingHeader Costing
		{
			get
			{
				if (costing == null)
				{
					costing = Helper.NewCosting(TransportProvider1);
				}

				return costing;
			}
		}

		RatingHeader ClientRate
		{
			get
			{
				if (clientRate == null)
				{
					clientRate = Helper.NewClientRate(Consignee);
				}

				return clientRate;
			}
		}

		RatingHeader costing;
		RatingHeader clientRate;

		#endregion
	}
}
