using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.RatingTests.Testing;

namespace Enterprise.Rating.Testing.GUI
{
	internal class RateLineUnitFactorTest : BaseRatingIntegrationTest
	{
		public void TestAutorate_ShouldSplitCharges_WhenUnitFactorIsPerContainer()
		{
			//Objective of this feature(WI00523932 - !CR7 Logwin - Create Invoice per Container by Rating per Container):
			//Client can print invoices based on container
			//To achieve this,
			//1. Splitting charges based on per container 
			//2. Setting Sell Reference Number as container number, if we set sell reference number, code generates invoices based on this number,
			//3. To explicitly identify for which container invoice is generated, we also update the charge description by appending container number

			var clientRate = Helper.NewClientRate(Consignee);

			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RatingConstants.TransportMode.SEA, container: "20GP", removeLines: true);
			var rateLine11 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateLine11.TL_UnitFactor = UnitFactorList.Codes.CTN;
			rateLine11.GetCalculator<UnitCalculator>().PerUnit = 12;
			var rateLine12 = rateEntry1.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			rateLine12.GetCalculator<UnitCalculator>().PerUnit = 14;

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RatingConstants.TransportMode.SEA, container: "40GP", removeLines: true);
			var rateLine21 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateLine21.TL_UnitFactor = UnitFactorList.Codes.CTN;
			rateLine21.GetCalculator<UnitCalculator>().PerUnit = 10;

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", number: "CONT00001", packLines: new[] { shipment.AddPackLine(weight: 30) });
			consol.AddContainer("20GP", number: "CONT00002", packLines: new[] { shipment.AddPackLine(weight: 70) });
			var container3 = consol.AddContainer("40GP", packLines: new[] { shipment.AddPackLine(weight: 80) });
			var container4 = consol.AddContainer("40GP", packLines: new[] { shipment.AddPackLine(weight: 90) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					//we need this to update container number, so it can reflect while printing Invoices
					JR_Desc = "International Freight - Container #CONT00001",
					JR_OSSellAmt = 12.00m,
					//we set Sell Reference number as Container number, so invoices can split as per sell reference number
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "FRT: CONT00001 (20GP) - 1 20GP Container(s) @ USD 12.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #CONT00002",
					JR_OSSellAmt = 12.00m,
					SellReferenceNumber = "CONT00002",
					RevenueCalculationDescription = "FRT: CONT00002 (20GP) - 1 20GP Container(s) @ USD 12.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_Desc = "Bunker Adjustment Factor",
					JR_OSSellAmt = 28.00m,
					SellReferenceNumber = "",
					RevenueCalculationDescription = "BAF: 2 20GP Container(s) @ USD 14.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #40GP (2)",
					JR_OSSellAmt = 20.00m,
					SellReferenceNumber = "40GP (2)",
					RevenueCalculationDescription = "FRT: 2 40GP Container(s) @ USD 10.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false);

			//Re-autorate by changing
			//1. rate for FRT line1
			//2. specifying container type for rateline 2
			//3. changing container count 
			//to ensure that it merge charges correctly and have charge desc & sell ref
			rateLine11.GetCalculator<UnitCalculator>().PerUnit = 16;
			rateLine12.TL_UnitFactor = UnitFactorList.Codes.CTN;
			container3.JC_ContainerCount = 2;
			container4.JC_ContainerCount = 3;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #CONT00001",
					JR_OSSellAmt = 16.00m,
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "FRT: CONT00001 (20GP) - 1 20GP Container(s) @ USD 16.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #CONT00002",
					JR_OSSellAmt = 16.00m,
					SellReferenceNumber = "CONT00002",
					RevenueCalculationDescription = "FRT: CONT00002 (20GP) - 1 20GP Container(s) @ USD 16.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_Desc = "Bunker Adjustment Factor - Container #CONT00001",
					JR_OSSellAmt = 14.00m,
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "BAF: CONT00001 (20GP) - 1 20GP Container(s) @ USD 14.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_Desc = "Bunker Adjustment Factor - Container #CONT00002",
					JR_OSSellAmt = 14.00m,
					SellReferenceNumber = "CONT00002",
					RevenueCalculationDescription = "BAF: CONT00002 (20GP) - 1 20GP Container(s) @ USD 14.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #40GP (5)",
					JR_OSSellAmt = 50.00m,
					SellReferenceNumber = "40GP (5)",
					RevenueCalculationDescription = "FRT: 5 40GP Container(s) @ USD 10.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false);
		}

		public void TestAutorate_ShouldSplitPenaltyCharges_WhenUnitFactorIsPerContainer()
		{
			Helper.ChargeCodes.New("DETEN", "Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			var clientRate = Helper.NewClientRate(Consignee);

			var shipment = CreateShipment(origin: "USLAX", destination: "AUSYD");
			var consol = CreateConsol(origin: "USLAX", destination: "AUSYD");
			consol.Shipments.Add(shipment);
			var container1 = consol.AddContainer("20GP", number: "CONT00001", packLines: new[] { shipment.AddPackLine(weight: 30) });
			var container2 = consol.AddContainer("20GP", number: "CONT00002", packLines: new[] { shipment.AddPackLine(weight: 70) });
			var container3 = consol.AddContainer("20GP", count: 5, packLines: new[] { shipment.AddPackLine(weight: 70) });

			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.DST, RatingConstants.TransportMode.SEA, "USLAX", "AUSYD", "DETEN", 10, QuantityUnit.CN, "USD", "20GP");
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.CTN;

			void MakeDetentionPenalty(CommonContainer container)
			{
				var penalty = shipment.DeliveryPenalties.AddNew();
				penalty.CPY_JS_Shipment = shipment.PK;
				penalty.CPY_JC_Container = container.PK;
				penalty.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
				penalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_TimeUnit = Constants.ContainerPenaltyTimeUnit.Codes.Days;
				penalty.FreeTimeAsDays = 0;
				penalty.DurationAsDays = 3;
				penalty.CPY_PerUnitCost = 10;
				container.ArrivalCarrierDetentionDays = 3;
				container.ArrivalCarrierDetentionCost = 250;
			}

			MakeDetentionPenalty(container1);
			MakeDetentionPenalty(container2);
			MakeDetentionPenalty(container3);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DETEN",
					JR_Desc = "Detention - Container #CONT00001",
					JR_OSSellAmt = 10.00m,
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "DETEN: CONT00001 (20GP) - 1 20GP Container(s) @ USD 10.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "DETEN",
					JR_Desc = "Detention - Container #CONT00002",
					JR_OSSellAmt = 10.00m,
					SellReferenceNumber = "CONT00002",
					RevenueCalculationDescription = "DETEN: CONT00002 (20GP) - 1 20GP Container(s) @ USD 10.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "DETEN",
					JR_Desc = "Detention - Container #20GP (5)",
					JR_OSSellAmt = 50.00m,
					SellReferenceNumber = "20GP (5)",
					RevenueCalculationDescription = "DETEN: 5 20GP Container(s) @ USD 10.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false);
		}

		public void TestAutorate_ShouldSplitPenaltyCharges_WhenUnitFactorIsPerContainer_UsingTimeCalculator()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var shipment = CreateShipment(origin: "USLAX", destination: "AUSYD");
			var consol = CreateConsol(origin: "USLAX", destination: "AUSYD");
			consol.Shipments.Add(shipment);
			var container1 = consol.AddContainer("20GP", number: "CONT00001", packLines: new[] { shipment.AddPackLine(weight: 30) });
			var container2 = consol.AddContainer("20GP", number: "CONT00002", packLines: new[] { shipment.AddPackLine(weight: 70) });
			var container3 = consol.AddContainer("20GP", count: 5, packLines: new[] { shipment.AddPackLine(weight: 70) });

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.TransportMode.SEA, "USLAX", "AUSYD", container: "20GP");
			var rateLine1 = rateEntry.AddRateLine("DSTOR", TimeCalculator.Code, QuantityUnit.CN);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.CTN;
			var calc = rateLine1.GetCalculator<TimeCalculator>();
			calc.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 1, QuantityUnit.WK);

			void MakeDetentionPenalty(CommonContainer container)
			{
				var penalty = shipment.DeliveryPenalties.AddNew();
				penalty.CPY_JS_Shipment = shipment.PK;
				penalty.CPY_JC_Container = container.PK;
				penalty.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Storage;
				penalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.CTO;
				penalty.CPY_TimeUnit = Constants.ContainerPenaltyTimeUnit.Codes.Days;
				penalty.FreeTimeAsDays = 0;
				penalty.DurationAsDays = 28;
			}

			MakeDetentionPenalty(container1);
			MakeDetentionPenalty(container2);
			MakeDetentionPenalty(container3);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DSTOR",
					JR_Desc = "Destination Storage / Warehousing - Container #CONT00001",
					JR_OSSellAmt = 4.00m,
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "DSTOR: CONT00001 (20GP) - 4 Container x Week (1 20GP (CONT00001) x 4 Week(s)) @ AUD 1.00/Container x Week"
				},
				new AssertionCharge
				{
					ChargeCode = "DSTOR",
					JR_Desc = "Destination Storage / Warehousing - Container #CONT00002",
					JR_OSSellAmt = 4.00m,
					SellReferenceNumber = "CONT00002",
					RevenueCalculationDescription = "DSTOR: CONT00002 (20GP) - 4 Container x Week (1 20GP (CONT00002) x 4 Week(s)) @ AUD 1.00/Container x Week"
				},
				new AssertionCharge
				{
					ChargeCode = "DSTOR",
					JR_Desc = "Destination Storage / Warehousing - Container #20GP (5)",
					JR_OSSellAmt = 20.00m,
					SellReferenceNumber = "20GP (5)",
					RevenueCalculationDescription = "DSTOR: 20 Container x Week (5 20GP x 4 Week(s)) @ AUD 1.00/Container x Week"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false);
		}

		public void TestAutorate_ShouldSplitPenaltyCharges_WhenUnitFactorIsPerContainer_And_OneDetentionPenaltyForTwoContainers()
		{
			Helper.ChargeCodes.New("DETEN", "Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			var clientRate = Helper.NewClientRate(Consignee);

			var shipment = CreateShipment(origin: "USLAX", destination: "AUSYD");
			var consol = CreateConsol(origin: "USLAX", destination: "AUSYD");
			consol.Shipments.Add(shipment);
			var container1 = consol.AddContainer("20GP", number: "CONT00001", packLines: new[] { shipment.AddPackLine(weight: 30) });
			var container2 = consol.AddContainer("20GP", number: "CONT00002", packLines: new[] { shipment.AddPackLine(weight: 70) });

			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.DST, RatingConstants.TransportMode.SEA, "USLAX", "AUSYD", "DETEN", 10, QuantityUnit.CN, "USD", "20GP");
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.CTN;

			void MakeDetentionPenalty(CommonContainer container)
			{
				var penalty = shipment.DeliveryPenalties.AddNew();
				penalty.CPY_JS_Shipment = shipment.PK;
				penalty.CPY_JC_Container = container.PK;
				penalty.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
				penalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_TimeUnit = Constants.ContainerPenaltyTimeUnit.Codes.Days;
				penalty.FreeTimeAsDays = 0;
				penalty.DurationAsDays = 3;
				penalty.CPY_PerUnitCost = 10;
				container.ArrivalCarrierDetentionDays = 3;
				container.ArrivalCarrierDetentionCost = 250;
			}

			MakeDetentionPenalty(container1);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DETEN",
					JR_Desc = "Detention - Container #CONT00001",
					JR_OSSellAmt = 10.00m,
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "DETEN: CONT00001 (20GP) - 1 20GP Container(s) @ USD 10.00/Container"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false);
		}

		public void TestAutorate_ShouldNotMergeRevenueChargesWithCost_WhenUnitFactorIsPerContainer()
		{
			AddRate(Helper.NewCosting(TransportProvider1), 10, false);
			AddRate(Helper.NewClientRate(Consignee), 12, true);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", number: "CONT00001", packLines: new[] { shipment.AddPackLine(weight: 30) });
			consol.AddContainer("20GP", number: "CONT00002", packLines: new[] { shipment.AddPackLine(weight: 70) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight",
					JR_OSCostAmt = 20.00m,
					SellReferenceNumber = "",
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ USD 10.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #CONT00001",
					JR_OSSellAmt = 12.00m,
					SellReferenceNumber = "CONT00001",
					RevenueCalculationDescription = "FRT: CONT00001 (20GP) - 1 20GP Container(s) @ USD 12.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = "International Freight - Container #CONT00002",
					JR_OSSellAmt = 12.00m,
					SellReferenceNumber = "CONT00002",
					RevenueCalculationDescription = "FRT: CONT00002 (20GP) - 1 20GP Container(s) @ USD 12.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee);

			void AddRate(RatingHeader ratingHeader, ZDecimal rate, bool isRevenue)
			{
				var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.FCL, RatingConstants.TransportMode.SEA, container: "20GP", removeLines: true);
				var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
				rateLine.GetCalculator<UnitCalculator>().PerUnit = rate;

				if (isRevenue)
				{
					rateLine.TL_UnitFactor = UnitFactorList.Codes.CTN;
				}
			}
		}

		// Test similar to Highest Rate with other calculator
		// Combined Calculator being null unit
		public void TestAutoRate_RatelineWithHighestRateCalculator_MissingUnits_NoExceptionThrown()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", "STD", commodity: "GEN", removeLines: true);
			rateEntry1.AddHighestRateCharge("OQUAR", 10m, "", 10m, "");

			var oneOffQuote = CreateQuotedBooking(Constants.RateMode.LSE, ZString.Empty, PrepaidCollectFreightForwardingList.Codes.PPD, Consignee, Consignee, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.TransportMode = Constants.TransportModes.Air;
			oneOffQuote.ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			AutorateAndAssert
			("Given a matching RateLine has any RateLineItem of calculator type HRC with missing/blank units, it does not throw an exception and the line is filtered",
				Array.Empty<AssertionCharge>(),
				oneOffQuote,
				Consignee,
				autorateCosts: false,
				autorateRevenue: true
			);
		}
	}
}
