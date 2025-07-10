using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	internal class SpotRateIntegrationTest : BaseRatingIntegrationTest
	{
		[TestDate(2015, 1, 1)]
		public void TestSpotRate_CourierModeShipment()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var shipment = CreateForwardingShipment(TransportModes.Courier, consignor.PK, consignee.PK, "AUSYD", "CNSHA", 20);
			shipment.JS_UnitFreightRate = 30m;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;

			var shipmentDate = ZDateTime.Now.AddYears(-2);
			shipment.JS_E_DEP = shipmentDate;
			shipment.JS_E_ARV = shipmentDate.AddDays(30);

			Factory.Save();

			AutorateAndAssert
			(
				"GIVEN Courier Mode Shipment WHEN autorate SpotRate THEN should return SpotRate",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 600m, }
				},
				shipment,
				consignor
			);
		}

		#region SpotRateApplicableToIncoterm

		[TestDate(2015, 1, 1)]
		public void TestSpotRateApplicableToIncoterm()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "CNSHA", 20);
			shipment.JS_UnitFreightRate = 30m;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;

			var shipmentDate = ZDateTime.Now.AddYears(-2);
			shipment.JS_E_DEP = shipmentDate;
			shipment.JS_E_ARV = shipmentDate.AddDays(30);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 600m,
						}
				};

			AutorateAndAssert("Even though shipment is old, spot rate still applies because spot rate has no date", expected, shipment, consignor);

			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";

			AutorateAndAssert("Payment terms are fine, should not be filtered", expected, shipment, consignee);

			shipment.JS_INCO = ZString.Empty;
			AutorateAndAssert(expected, shipment, consignee);
		}

		public void TestSpotRateApplicableToIncoterm_ConflictingConsolSuppressesFiltering()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);
			var containerRef = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, ContainerModes.AIR)).PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "GBSUN", 20);
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_UnitFreightRate = 30m;

			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var container = consol.Containers.AddNew();
			container.JC_RC = containerRef;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 30m,
							RevenueCalculationDescription = "FRT: 1 Container(s) @ AUD 30.00/Container"
						}
				};

			AutorateAndAssert("Payment term filtering should not be applied as Shipment is CLT but Consol is PPD", expected, shipment, consignor);

			consol.JK_PrepaidCollect = PaymentType.Collect;

			AutorateAndAssert("Both consol and shipment are CLT so should not apply FRT for EXP shipment", Array.Empty<AssertionCharge>(), shipment, consignor);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_INCO = IncoTerms.ExWorks;

			AutorateAndAssert("Consol + Shipment are both collect so should not apply rate for export", Array.Empty<AssertionCharge>(), shipment, consignee);

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			AutorateAndAssert("Payment term filtering should not be applied as shipment and consol conflict", expected, shipment, consignee);
		}

		public void TestNoFreightChargeCodeInRegistry_AutoRatingSpotQuote_WarningNotException()
		{
			Env.Registry.FreightChargeCode = Guid.Empty;

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "USLAX");
			var calculator = entry.AddRateLine("ODOC", FlatPlusPerUnitCalculator.Code, Weight.Kilograms).GetCalculator<FlatPlusPerUnitCalculator>();
			calculator.BaseRate = 200;
			calculator.PerUnit = 5;

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, client.PK, "AUSYD", "USLAX", 50m);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_UnitFreightRate = 5m;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 450m,
					RevenueCalculationDescription = "Base Rate AUD 200.00 + 50 Kilogram(s) @ AUD 5.00/KG"
				}
			};

			var expectedErrors = new[]
			{
				@"Error Auto-rating could not find the 'Freight Charge Code' for this company. Either set a valid 'Freight Charge Code' in the Registry or revert the Freight Autorating Mode on this Job to Standard."
			};

			AutorateAndAssert("Expected only the origin job to come through, not the spot rate", expected, shipment, client, autorateCosts: false, expectedErrors: expectedErrors);
		}

		#endregion

		#region Freight spot, cost and gateway rates autorating

		[TestDate(2016, 06, 06)]
		public void TestFreightAutorateStandardModeWithSpotRate()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20m, 2m);
			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_UnitFreightRate = 55m;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.UnitedStates;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			var expectedLogLines = new[]
			{
				@"Information: AUTORATING REVENUE FOR Shipment S100216
Information: RatingHeader Found Client Rate TESTORG1 Entries: 1
Information: RateLine Found BAF-FLT-Client Rate TESTORG1
Information: RateLine Found FRT-UNT-KG-Client Rate TESTORG1",
				@"Information: CHARGES CALCULATED:
	BAF: Base Rate AUD 20.00
	FRT: 333.333 Kilogram(s) @ AUD 20.00/KG"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "STD autorating mode should ignore Spot Rate Amount", expectedLogLines);
		}

		[TestDate(2016, 06, 06)]
		public void TestFreightAutorateAllInclusiveModeWithSpotRate()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20m, 2m);
			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_UnitFreightRate = 30;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.UnitedStates;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			AddParityExchangeRate(shipment.FrtRateCurrency);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  9999.99m,
							RevenueCalculationDescription = "333.333 Kilogram(s) @ USD 30.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			var expectedLogLines = new[] { @"Warning: Shipment S100216 was auto-costed.
	No costs were found.",
			"Information: RateLine Found FRT-Job One Off Freight Rate" };

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Only the Spot Rate should be found, client rate should be ignored", expectedLogLines);
		}

		[TestDate(2016, 06, 06)]
		public void TestFreightAutorateFreightPlusModeWithSpotRate()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20m, 2m);
			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_UnitFreightRate = 30;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.UnitedStates;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			AddParityExchangeRate(shipment.FrtRateCurrency);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  9999.99m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt =  20m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			var expectedLogLines =
@"Information: AUTORATING REVENUE FOR Shipment S100216
Information: RatingHeader Found Client Rate TESTORG1 Entries: 1
Information: RateLine Found BAF-FLT-Client Rate TESTORG1
Information: RateLine Found FRT-UNT-KG-Client Rate TESTORG1
Information: RateLine Found FRT-Job One Off Freight Rate
Information: RateLine Filtered FRT-UNT-KG-Client Rate TESTORG1	reason:	replaced by Job Negotiated Cost/Gateway Sell/One Off Freight Rate";

			AssertAutoratingAuditLogNoteContainsLines(shipment, "FRT mode should override client rate for FRT charge but include other client rate charge codes", expectedLogLines);
		}

		public void TestAutoRateUsesSpotFreightRateRatherThanClientRate()
		{
			Env.Registry.FreightWeightUnit = Weight.Pounds;

			var client = Helper.NewOrgHeader(1);
			var rateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, Weight.Pounds);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 150m;
			rateLine.TL_Rounding = RatingRoundingTypes.Chargeable;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "USLAX", "AUBNE", 2130m);
			shipment.JS_UniqueConsignRef = "S01651";
			shipment.JS_UnitOfWeight = Weight.Pounds;
			shipment.JS_UnitOfVolume = Volume.CubicFeet;

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_UnitFreightRate = 113;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;

			Factory.Save();

			AssertEquals("Pre-condition", Weight.Pounds, shipment.JS_ChargeableUnit);
			AssertEquals("Pre-condition", 2130m, shipment.JS_ActualChargeable);

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 240690m,
							JR_RX_NKSellCurrency = CurrencyCodes.Australia,
							RevenueCalculationDescription = "One Off Freight Rate is applicable for Shipment S01651."
						}
				};

			AutorateAndAssert("Chargeable Spot Freight Rate Used ($113 * 2130 LB) not the rate on the unit calculator", expected, shipment, client);
		}

		public void TestSpotRateUsesChargeable()
		{
			var collection = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;
			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;

			using (FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var consignor = Helper.NewOrgHeader();

				var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 125m, 1.628m);
				shipment.JS_UnitFreightRate = 5m;
				shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
				shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;

				Factory.Save();

				AssertEquals("Pre-condition: Expected shipment chargeable", 271.5m, shipment.JS_ActualChargeable);

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 1357.5m,
						RevenueCalculationDescription = "FRT: 271.5 Kilogram(s) @ AUD 5.00/KG"
					}
				};

				AutorateAndAssert("Spot quotes should use chargeable from shipment", expected, shipment, consignor);

				var roundings = new DefaultRoundingsCollection();
				var rounding = roundings.AddNew();
				rounding.Code = RatingConstants.RateCategory.AIR;
				rounding.RoundingType = RatingRoundingTypes.UpTo1;

				using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
				{
					AutorateAndAssert("Spot quotes should use chargeable from shipment, should not try to round up", expected, shipment, consignor);
				}

				shipment.JS_ActualChargeable = 500m;
				expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 2500m,
						RevenueCalculationDescription = "FRT: 500 Kilogram(s) @ AUD 5.00/KG"
					}
				};

				AutorateAndAssert(expected, shipment, consignor);
			}
		}

		#endregion

		#region Spot Rates on Containers

		public void TestSpotRatesOnContainers_DifferentSpotRatesOnContainer()
		{
			var localClient = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			#region Cost

			var costing = Helper.NewCosting(carrier);
			var cost20GP_Freight = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			cost20GP_Freight.RateLines.RemoveAndDeleteAll();
			var costFRT20GPRateLine = cost20GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			costFRT20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cost20GP_Origin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "20GP");
			cost20GP_Origin.RateLines.RemoveAndDeleteAll();

			var costOTHC20GPRateLine = cost20GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			costOTHC20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 30m;

			var cost40GP_Freight = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			cost40GP_Freight.RateLines.RemoveAndDeleteAll();
			var costFRT40GPRateLine = cost40GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			costFRT40GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var cost40GP_Origin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "40GP");
			cost40GP_Origin.RateLines.RemoveAndDeleteAll();

			var costOTHC40GPRateLine = cost40GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			costOTHC40GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 40m;

			var costEntryNonContainer = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", ZString.Empty, ZString.Empty);
			costEntryNonContainer.RateLines.RemoveAndDeleteAll();

			var costLineNonContainer = costEntryNonContainer.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			costLineNonContainer.GetCalculator<UnitCalculator>().PerUnit = 1m;

			#endregion

			#region Sell

			var rate = Helper.NewClientRate(localClient);
			var rate20GP_Freight = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			rate20GP_Freight.RateLines.RemoveAndDeleteAll();
			var rateFRT20GPRateline = rate20GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateFRT20GPRateline.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var rate20GP_Origin = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "20GP");
			rate20GP_Origin.RateLines.RemoveAndDeleteAll();

			var rateOTHC20GPRateline = rate20GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			rateOTHC20GPRateline.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var rate40GP_Freight = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			rate40GP_Freight.RateLines.RemoveAndDeleteAll();
			var rateFRT40GPRateline = rate40GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateFRT40GPRateline.TL_WeightVolume = QuantityUnit.CN;
			rateFRT40GPRateline.GetCalculator<UnitCalculator>().PerUnit = 200m;

			var rate40GP_Origin = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "40GP");
			rate40GP_Origin.RateLines.RemoveAndDeleteAll();

			var rateOTHC40GPRateline = rate40GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			rateOTHC40GPRateline.GetCalculator<UnitCalculator>().PerUnit = 60m;

			var rateEntryNonContainer = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", ZString.Empty, ZString.Empty);
			rateEntryNonContainer.RateLines.RemoveAndDeleteAll();

			var rateLineNonContainer = rateEntryNonContainer.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLineNonContainer.GetCalculator<UnitCalculator>().PerUnit = 2m;

			#endregion

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			SetupContainersWithDifferentSpotRatesTypes(consol);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = localClient.PK;

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			packLine1.JL_JC = consol.Containers[0].PK;
			packLine2.JL_JC = consol.Containers[1].PK;

			Factory.Save();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 150m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 150m,
					JR_OSSellAmt = 200m,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 3000m,
					JR_OSSellAmt = 6000m
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 30m,
					JR_OSSellAmt = 50m,
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 40m,
					JR_OSSellAmt = 60m,
				}
			};

			AutorateAndAssert("Should apply Spot Rates correctly to both containers", expected, shipment, localClient, null, testJob);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain information about ignored spot rates", new[]
			{
				"Information: Container SPOTRATE40GP has Negotiated Cost which was ignored due to invalid configuration.",
				"Information: Container SPOTCOST20GP has Negotiated Cost which was ignored due to invalid configuration."
			});
		}

		public void TestSpotRatesOnContainers_CorrectContainerSpotRates()
		{
			var localClient = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			SetupRatesForContainerSpotRateTest(localClient, carrier);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			SetupContainerWithSpotRates(consol);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = localClient.PK;

			Factory.Save();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 888m,
					JR_OSSellAmt = 999m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 888.00/Container",
					RevenueCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 999.00/Container",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 10m,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 3000m,
					JR_OSSellAmt = 6000m
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 30m,
					CostCalculationDescription = @"OTHC: 1 20GP Container(s) @ AUD 30.00/Container"
				}
			};

			AutorateAndAssert("Should apply container spot rates correctly and filter only container related rates", expected, shipment, localClient, null, testJob);
			var expectedLogLines = new[]
			{
				@"Information: AUTORATING COSTS FOR Shipment SHP00001
Information: RatingHeader Found Costing TESTORG2 Entries: 3",
				@"Information: RateLine Found ODOC-UNT-KG-Costing TESTORG2
Information: RateLine Found BAF-UNT-CN-20GP-Costing TESTORG2
Information: RateLine Found FRT-UNT-CN-20GP-Costing TESTORG2
Information: RateLine Found FRT-Container Negotiated Cost
Information: RateLine Found OTHC-UNT-CN-20GP-Costing TESTORG2
Information: RateLine Filtered FRT-UNT-CN-20GP-Costing TESTORG2	reason:	replaced by Container Negotiated Cost",
@"Information: Chargeable was added for
				RateLine ODOC-UNT-KG-Costing TESTORG2
					Job's info:
					: 3 Chargeable
					Container 20GP: 3000 Weight
					Container 20GP: 0 Volume
				RateLine BAF-UNT-CN-20GP-Costing TESTORG2
					Job's info:
					Container 20GP: 1 ContainerCount
				RateLine OTHC-UNT-CN-20GP-Costing TESTORG2
					Job's info:
					Container 20GP: 1 ContainerCount",
@"Information: CHARGES CALCULATED:
	BAF: 1 20GP Container(s) @ USD 10.00/Container
	FRT: 1 20GP Container(s) @ USD 888.00/Container
	ODOC: 3000 Kilogram(s) @ AUD 1.00/KG
	OTHC: 1 20GP Container(s) @ AUD 30.00/Container",
@"Information: Shipment SHP00001 was auto-costed.
	The following costs were found:
	  • BAF charge from Costing TESTORG2
	  • FRT charge from Container Negotiated Cost
	  • ODOC charge from Costing TESTORG2
	  • OTHC charge from Costing TESTORG2
	Charges created: BAF, FRT, ODOC, OTHC",
@"Information: AUTORATING REVENUE FOR Shipment SHP00001
Information: RatingHeader Found Client Rate TESTORG1 Entries: 3",
@"Information: RateLine Found ODOC-UNT-KG-Client Rate TESTORG1
Information: RateLine Found BAF-UNT-CN-20GP-Client Rate TESTORG1
Information: RateLine Found FRT-UNT-CN-20GP-Client Rate TESTORG1
Information: RateLine Found FRT-Container One Off Freight Rate
Information: RateLine Found OTHC-UNT-CN-20GP-Client Rate TESTORG1
Information: RateLine Filtered BAF-UNT-CN-20GP-Client Rate TESTORG1	reason:	replaced by Container One Off Freight Rate
Information: RateLine Filtered FRT-UNT-CN-20GP-Client Rate TESTORG1	reason:	replaced by Container One Off Freight Rate",
@"Information: Chargeable was added for
				RateLine ODOC-UNT-KG-Client Rate TESTORG1
					Job's info:
					: 3 Chargeable
					Container 20GP: 3000 Weight
					Container 20GP: 0 Volume
				RateLine OTHC-UNT-CN-20GP-Client Rate TESTORG1
					Job's info:
					Container 20GP: 1 ContainerCount",
@"Information: CHARGES CALCULATED:
	FRT: 1 20GP Container(s) @ USD 999.00/Container
	ODOC: 3000 Kilogram(s) @ AUD 2.00/KG
	OTHC: 1 20GP Container(s) @ AUD 50.00/Container",
@"Information: Shipment SHP00001 was auto-rated.
	The following rates were found:
	  • FRT charge from Container One Off Freight Rate
	  • ODOC charge from Client Rate TESTORG1
	  • OTHC charge from Client Rate TESTORG1
	Charges created: FRT, ODOC, OTHC"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain correct information", expectedLogLines);
		}

		public void TestSpotRatesOnContainers_ConsolCosts()
		{
			var localClient = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			SetupRatesForContainerSpotRateTest(localClient, carrier);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			SetupContainerWithSpotRates(consol);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = localClient.PK;

			Factory.Save();

			var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 888m,
						CostCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 888.00/Container

International Freight

Negotiated Cost is applicable for Container Number SPOTCOST20GP."
					},
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 10m,
						CostCalculationDescription = @"BAF: 1 20GP Container(s) @ USD 10.00/Container

Bunker Adjustment Factor"
					}
				};

			AutoCostAndAssert("Should properly add Costs from Spot Rates", null, expectedCosts, consol, false);
		}

		public void TestSpotRatesOnContainers_RateShipmentGatewaySell()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			var localClient = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			SetupRatesForContainerSpotRateTest(localClient, carrier);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;
			consol.JK_UniqueConsignRef = "AAAA";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts.O5_PortOrCountry = "AUSYD";
			appointedAgentPorts.O5_OA_AgentOfficeAddress = orgAddress.PK;
			appointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts);

			var cont20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var cont20GPSpotRate = consol.Containers.AddNew();

			cont20GPSpotRate.JC_RC = cont20GP_PK;
			cont20GPSpotRate.JC_ContainerCount = 1;
			cont20GPSpotRate.JC_ContainerNum = "SPOTCOST20GP";
			cont20GPSpotRate.JC_GatewaySellSpotRate = 50m;
			cont20GPSpotRate.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			cont20GPSpotRate.JC_SellSpotRate = 80m;
			cont20GPSpotRate.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			AssertEquals(true, consol.IsGateway());
			Assert(!cont20GPSpotRate.JC_GatewaySellSpotRateModeInfo.HasNotifications());

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.JS_ActualWeight = 150m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 3m;
			shipment.JS_UnitOfVolume = "M3";

			shipment.JS_OH_DeliveryAgent = Consignee.PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsigneePK = Consignee.PK;

			Factory.Save();

			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 80m,
						JR_OSCostAmt = 50m,
						CostCalculationDescription = @"Gateway Sell is applicable for Container Number SPOTCOST20GP.",
						RevenueCalculationDescription = @"One Off Freight Rate is applicable for Container Number SPOTCOST20GP."
					}
				};

				AutorateAndAssert(expected, shipment, localClient);
			}
		}

		public void TestSpotRatesOnContainers_ContainerSpotRatesOverridenByShipmentSpotRate()
		{
			var localClient = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			SetupRatesForContainerSpotRateTest(localClient, carrier);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			SetupContainerWithSpotRates(consol);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = localClient.PK;
			shipment.JS_FreightCostRate = 500m;
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_UnitFreightRate = 600m;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;

			Factory.Save();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 500m,
					JR_OSSellAmt = 600m,
					CostCalculationDescription = @"FRT: 1 Container(s) @ USD 500.00/Container",
					RevenueCalculationDescription = @"FRT: 1 Container(s) @ USD 600.00/Container",
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 3000m,
					JR_OSSellAmt = 6000m
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 30m,
					JR_OSSellAmt = 50m
				}
			};

			AutorateAndAssert("Should ignore Container Spot rates and use Shipment Spot Rates", expected, shipment, localClient, null, testJob);
			var expectedLogLines = new[]
			{
				"Information: AUTORATING COSTS FOR Shipment SHP00001",
				"Information: RatingHeader Found Costing TESTORG2 Entries: 3",
				@"Information: RateLine Found FRT-Job Negotiated Cost
Information: RateLine Found ODOC-UNT-KG-Costing TESTORG2
Information: RateLine Found OTHC-UNT-CN-20GP-Costing TESTORG2
Information: Container SPOTCOST20GP has Negotiated Cost which was ignored due to invalid configuration.",
				"Information: AUTORATING REVENUE FOR Shipment SHP00001",
				@"Information: RatingHeader Found Client Rate TESTORG1 Entries: 3
Information: RateLine Found FRT-Job One Off Freight Rate
Information: RateLine Found ODOC-UNT-KG-Client Rate TESTORG1
Information: RateLine Found OTHC-UNT-CN-20GP-Client Rate TESTORG1
Information: Container SPOTCOST20GP has One Off Freight Rate which was ignored due to invalid configuration.",
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain correct information", expectedLogLines);
		}

		public void TestSpotRatesOnContainers_AINSpotRates()
		{
			var localClient = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			SetupRatesForContainerSpotRateTest(localClient, carrier);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			SetupContainerWithSpotRates(consol);
			consol.Containers[0].JC_CostSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = localClient.PK;

			Factory.Save();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 888m,
					JR_OSSellAmt = 999m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 888.00/Container",
					RevenueCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 999.00/Container",
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 3000m,
					JR_OSSellAmt = 6000m
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSSellAmt = 50m,
					JR_OSCostAmt = 30m
				}
			};

			AutorateAndAssert("Should apply container spot rates correctly and filter only container related rates", expected, shipment, localClient, null, testJob);
			var expectedLogLines = new[]
			{
				@"Information: RateLine Found ODOC-UNT-KG-Costing TESTORG2
Information: RateLine Found BAF-UNT-CN-20GP-Costing TESTORG2
Information: RateLine Found FRT-UNT-CN-20GP-Costing TESTORG2
Information: RateLine Found FRT-Container Negotiated Cost
Information: RateLine Found OTHC-UNT-CN-20GP-Costing TESTORG2
Information: RateLine Filtered BAF-UNT-CN-20GP-Costing TESTORG2	reason:	replaced by Container Negotiated Cost
Information: RateLine Filtered FRT-UNT-CN-20GP-Costing TESTORG2	reason:	replaced by Container Negotiated Cost",
@"Information: RateLine Found ODOC-UNT-KG-Client Rate TESTORG1
Information: RateLine Found BAF-UNT-CN-20GP-Client Rate TESTORG1
Information: RateLine Found FRT-UNT-CN-20GP-Client Rate TESTORG1
Information: RateLine Found FRT-Container One Off Freight Rate
Information: RateLine Found OTHC-UNT-CN-20GP-Client Rate TESTORG1
Information: RateLine Filtered BAF-UNT-CN-20GP-Client Rate TESTORG1	reason:	replaced by Container One Off Freight Rate
Information: RateLine Filtered FRT-UNT-CN-20GP-Client Rate TESTORG1	reason:	replaced by Container One Off Freight Rate"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain correct information", expectedLogLines);
		}

		public void TestSpotRatesOnContainers_CompanyTariffIsReplacedWithContainerSpotRate()
		{
			var localClient = Helper.NewOrgHeader(1);
			var carrier = Helper.NewOrgHeader();

			var companyTariff = Helper.NewCompanyTariff();
			var tariff20GP_Freight = companyTariff.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			tariff20GP_Freight.RateLines.RemoveAndDeleteAll();
			var tariffFRT20GPRateLine = tariff20GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			tariffFRT20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var tariffBAF20GPRateLine = tariff20GP_Freight.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			tariffBAF20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var tariff20GP_Origin = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "20GP");
			tariff20GP_Origin.RateLines.RemoveAndDeleteAll();

			var tariffOTHC20GPRateLine = tariff20GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			tariffOTHC20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 30m;

			companyTariff.Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			SetupContainerWithSpotRates(consol);
			consol.Containers[0].JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			consol.Containers[0].JC_SellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = localClient.PK;

			Factory.Save();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 888m,
					JR_OSSellAmt = 999m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 888.00/Container",
					RevenueCalculationDescription = @"FRT: 1 20GP Container(s) @ USD 999.00/Container",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 10m,
					JR_OSSellAmt = 10m
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 30m,
					JR_OSSellAmt = 30m
				},
			};

			AutorateAndAssert("Should apply container spot rates correctly and filter only container related rates", expected, shipment, localClient, null, testJob);
			var expectedLogLines = new[]
			{
				"Information: AUTORATING COSTS FOR Shipment SHP00001",
				@"Information: RateLine Found FRT-Container Negotiated Cost
Information: CHARGES CALCULATED:
	FRT: 1 20GP Container(s) @ USD 888.00/Container
Information: Shipment SHP00001 was auto-costed.
	The following costs were found:
	  • FRT charge from Container Negotiated Cost
	Charges created: FRT
Information: AUTORATING REVENUE FOR Shipment SHP00001
Information: RatingHeader Found Base Company Tariff Entries: 2
Information: RateLine Found BAF-UNT-CN-20GP-Base Company Tariff
Information: RateLine Found FRT-UNT-CN-20GP-Base Company Tariff
Information: RateLine Found FRT-Container One Off Freight Rate
Information: RateLine Found OTHC-UNT-CN-20GP-Base Company Tariff
Information: RateLine Filtered FRT-UNT-CN-20GP-Base Company Tariff	reason:	replaced by Container One Off Freight Rate
Information: Chargeable was added for
				RateLine BAF-UNT-CN-20GP-Base Company Tariff
					Job's info:
					Container 20GP: 1 ContainerCount
				RateLine OTHC-UNT-CN-20GP-Base Company Tariff
					Job's info:
					Container 20GP: 1 ContainerCount
Information: CHARGES CALCULATED:
	BAF: 1 20GP Container(s) @ USD 10.00/Container
	FRT: 1 20GP Container(s) @ USD 999.00/Container
	OTHC: 1 20GP Container(s) @ AUD 30.00/Container
Information: Shipment SHP00001 was auto-rated.
	The following rates were found:
	  • BAF charge from Base Company Tariff
	  • FRT charge from Container One Off Freight Rate
	  • OTHC charge from Base Company Tariff
	Charges created: BAF, FRT, OTHC"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain correct information", expectedLogLines);
		}

		public void TestSpotRatesOnContainer_ConsolWithFCLAndLCLShipments()
		{
			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;
			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			SetupRatesForContainerSpotRateTest(localClient, carrier);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			Factory.Save();

			var cont20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var cont40GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = "STD";
			shipment1.JS_UniqueConsignRef = "SHP00001";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_ActualWeight = 1000m;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.ConsignorPK = localClient.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = "LCL";
			shipment2.JS_ShipmentType = "STD";
			shipment2.JS_UniqueConsignRef = "SHP00002";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_ActualWeight = 250m;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.ConsignorPK = localClient.PK;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_TransportMode = TransportModes.Sea;
			shipment3.JS_PackingMode = "LCL";
			shipment3.JS_ShipmentType = "STD";
			shipment3.JS_UniqueConsignRef = "SHP00003";
			shipment3.JS_INCO = "CFR";
			shipment3.JS_ActualWeight = 300m;
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.ConsignorPK = localClient.PK;

			var cont20GPAIN = consol.Containers.AddNew();
			var cont40GPFreighPlus = consol.Containers.AddNew();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();
			shipment3.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			var packLine3 = shipment3.OuterPackLines.AddNew();

			packLine1.JL_ActualVolume = 25m;
			packLine1.JL_ActualWeight = 1000m;
			packLine2.JL_ActualVolume = 11.2m;
			packLine2.JL_ActualWeight = 250m;
			packLine3.JL_ActualVolume = 6.2m;
			packLine3.JL_ActualWeight = 300m;

			cont20GPAIN.JC_RC = cont20GP_PK;
			cont20GPAIN.JC_ContainerNum = "FRT20GP";
			cont20GPAIN.JC_SellSpotRate = 2500m;
			cont20GPAIN.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont20GPAIN.JC_CostSpotRate = 1500m;
			cont20GPAIN.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont20GPAIN.JC_GatewaySellSpotRate = 500;
			cont20GPAIN.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont20GPAIN.PackLines.Add(packLine1);

			cont40GPFreighPlus.JC_RC = cont40GP_PK;
			cont40GPFreighPlus.JC_ContainerNum = "FRT40GP";
			cont40GPFreighPlus.JC_CostSpotRate = 4000m;
			cont40GPFreighPlus.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont40GPFreighPlus.JC_SellSpotRate = 5000m;
			cont40GPFreighPlus.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont40GPFreighPlus.JC_GatewaySellSpotRate = 3000m;
			cont40GPFreighPlus.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont40GPFreighPlus.PackLines.Add(packLine2);
			cont40GPFreighPlus.PackLines.Add(packLine3);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 4000m,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 1500m,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 10m,
				}
			};

			var expectedChargesShipment1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2500m,
					JR_OSCostAmt = 967.74m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1290.33m,
					JR_OSCostAmt = 2580.65m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 6.45m,
					JR_OSSellAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2000m,
				},
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSSellAmt = 50m,
					JR_OSCostAmt = 30m,
				}
			};

			var expectedChargesShipment2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 120.97,
					JR_OSCostAmt = 241.94m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5000m, // 40GP spot rate should override sell FRT from cost for 40GP
					JR_OSCostAmt = 645.16m,
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1.61m,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 250m,
					JR_OSSellAmt = 500m,
				}
			};

			var expectedChargesShipment3 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 145.16,
					JR_OSCostAmt = 290.32m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5000m, // 40GP spot rate should override sell FRT from cost for 40GP
					JR_OSCostAmt = 774.19m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1.94m,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 300m,
					JR_OSSellAmt = 600m,
				}
			};

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>()
			{
				{ shipment1, expectedChargesShipment1 },
				{ shipment2, expectedChargesShipment2 },
				{ shipment3, expectedChargesShipment3 }
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("Should properly add Costs from Spot Rates", expectedCharges, expectedCosts, consol);
		}

		public void TestSpotRatesOnContainer_ApportionmentByCapacityPerContainer()
		{
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.CapacityPerContainer);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_ContainerNum = "CONT00001";
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_CostSpotRate = 1500m;
			container20GP.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";
			container40GP.JC_CostSpotRate = 4000m;
			container40GP.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 25.2m;
			packline1.JL_ActualWeight = 100m;
			packline1.JL_JC = container20GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.LCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 5.8m;
			packline2.JL_ActualWeight = 300m;
			packline2.JL_JC = container40GP.PK;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment3.JS_TransportMode = TransportModes.Sea;
			shipment3.JS_PackingMode = ContainerModes.LCL;
			shipment3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_INCO = "CFR";
			shipment3.JS_OH_DeliveryAgent = consignee.PK;
			shipment3.ConsignorPK = consignor.PK;
			shipment3.ConsigneePK = consignee.PK;

			var packline3 = shipment3.OuterPackLines.AddNew();
			packline3.JL_ActualVolume = 11.6m;
			packline3.JL_ActualWeight = 600m;
			packline3.JL_JC = container40GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job3.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 4000m,
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1500m,
					}
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();

				AssertEquals(3, consolCosts[0].ApportionmentCharges.Count);
				AssertEquals(3, consolCosts[1].ApportionmentCharges.Count);

				Factory.Save();

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment1, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 1500.00m
							}
						}
					},
					{
						shipment2, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 1333.33m
							}
						}
					},
					{
						shipment3, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 2666.67m
							}
						}
					}
				};

				AssertCharges("Charges should be apportioned based on what fraction of total volume loaded into a container each shipment has contributed", expectedCharges);
			}
		}

		public void TestSpotRatesOnContainer_AINRatesConsol()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;
			var bafCostLine20GP = costEntry20GP.AddRateLine("BAF", PercentageCalculator.Code);
			bafCostLine20GP.GetCalculator<PercentageCalculator>().Percent = 10m;
			bafCostLine20GP.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;
			var bafCostLine40GP = costEntry40GP.AddRateLine("BAF", PercentageCalculator.Code);
			bafCostLine40GP.GetCalculator<PercentageCalculator>().Percent = 10m;
			bafCostLine40GP.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_ContainerNum = "CONT00001";
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_CostSpotRate = 1500m;
			container20GP.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;

			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";
			container40GP.JC_CostSpotRate = 3000m;
			container40GP.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 25.2m;
			packline1.JL_ActualWeight = 100m;
			packline1.JL_JC = container20GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.LCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 5.8m;
			packline2.JL_ActualWeight = 300m;
			packline2.JL_JC = container40GP.PK;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment3.JS_TransportMode = TransportModes.Sea;
			shipment3.JS_PackingMode = ContainerModes.LCL;
			shipment3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_INCO = "CFR";
			shipment3.JS_OH_DeliveryAgent = consignee.PK;
			shipment3.ConsignorPK = consignor.PK;
			shipment3.ConsigneePK = consignee.PK;

			var packline3 = shipment3.OuterPackLines.AddNew();
			packline3.JL_ActualVolume = 11.6m;
			packline3.JL_ActualWeight = 600m;
			packline3.JL_JC = container40GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job3.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1500m,
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 3000m,
					}
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);
				var expectedLog = new[]
				{
					@"Information: RateLine Found BAF-PER-20GP-Costing XVBQP68SIYXQ
Information: RateLine Found FRT-UNT-CN-20GP-Costing XVBQP68SIYXQ
Information: RateLine Found FRT-Container Negotiated Cost (x2)
Information: RateLine Found BAF-PER-40GP-Costing XVBQP68SIYXQ
Information: RateLine Found FRT-UNT-CN-40GP-Costing XVBQP68SIYXQ
Information: RateLine Filtered BAF-PER-20GP-Costing XVBQP68SIYXQ	reason:	replaced by Container Negotiated Cost
Information: RateLine Filtered FRT-UNT-CN-20GP-Costing XVBQP68SIYXQ	reason:	replaced by Container Negotiated Cost
Information: RateLine Filtered BAF-PER-40GP-Costing XVBQP68SIYXQ	reason:	replaced by Container Negotiated Cost
Information: RateLine Filtered FRT-UNT-CN-40GP-Costing XVBQP68SIYXQ	reason:	replaced by Container Negotiated Cost"
				};

				AssertAutoratingAuditLogNoteContainsLines(consol, "Should not contain other charges", expectedLog);
			}
		}

		void SetupRatesForContainerSpotRateTest(OrgHeader localClient, OrgHeader carrier)
		{
			// Setting Up Following Rates:
			// Cost:
			// 20GP FRT 100, BAF 10, OTHC 30
			// ODOC 1/KG
			// Sell:
			// 20GP FRT 150, BAF 20, OTHC 40
			// ODOC 2/KG

			var costing = Helper.NewCosting(carrier);
			var cost20GP_Freight = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			cost20GP_Freight.RateLines.RemoveAndDeleteAll();
			var costFRT20GPRateLine = cost20GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			costFRT20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var costBAF20GPRateLine = cost20GP_Freight.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			costBAF20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var cost20GP_Origin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "20GP");
			cost20GP_Origin.RateLines.RemoveAndDeleteAll();

			var costOTHC20GPRateLine = cost20GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			costOTHC20GPRateLine.GetCalculator<UnitCalculator>().PerUnit = 30m;

			var costEntryNonContainer = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "", "");
			costEntryNonContainer.RateLines.RemoveAndDeleteAll();

			var costLineNonContainer = costEntryNonContainer.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			costLineNonContainer.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var rate = Helper.NewClientRate(localClient);
			var rate20GP_Freight = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			rate20GP_Freight.RateLines.RemoveAndDeleteAll();
			var rateFRT20GPRateline = rate20GP_Freight.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateFRT20GPRateline.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var rateBAF20GPRateline = rate20GP_Freight.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			rateBAF20GPRateline.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rate20GP_Origin = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "", "", "20GP");
			rate20GP_Origin.RateLines.RemoveAndDeleteAll();

			var rateOTHC20GPRateline = rate20GP_Origin.AddRateLine("OTHC", UnitCalculator.Code, QuantityUnit.CN);
			rateOTHC20GPRateline.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var rateEntryNonContainer = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", ZString.Empty, ZString.Empty);
			rateEntryNonContainer.RateLines.RemoveAndDeleteAll();

			var rateLineNonContainer = rateEntryNonContainer.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLineNonContainer.GetCalculator<UnitCalculator>().PerUnit = 2m;

			Factory.Save();
		}

		void SetupContainersWithDifferentSpotRatesTypes(CommonConsol consol)
		{
			var cont20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var cont40GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var cont20GPSpotRate = consol.Containers.AddNew();
			var cont40GPSpotRate = consol.Containers.AddNew();

			cont20GPSpotRate.JC_RC = cont20GP_PK;
			cont20GPSpotRate.JC_ContainerCount = 1;
			cont20GPSpotRate.JC_ContainerNum = "SPOTCOST20GP";

			//Set cost spot rate to 888m for 20GP container, with FRT only
			cont20GPSpotRate.JC_CostSpotRate = 888m;
			cont20GPSpotRate.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			cont20GPSpotRate.JC_RX_NKCostSpotRateCurrency = "AUD";

			cont40GPSpotRate.JC_RC = cont40GP_PK;
			cont40GPSpotRate.JC_ContainerCount = 1;
			cont40GPSpotRate.JC_ContainerNum = "SPOTRATE40GP";

			//Set sell spot rate to 999m for 40GP container with ALL IN charges
			cont40GPSpotRate.JC_SellSpotRate = 999m;
			cont40GPSpotRate.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;
			cont40GPSpotRate.JC_RX_NKSellSpotRateCurrency = "AUD";
		}

		void SetupContainerWithSpotRates(CommonConsol consol)
		{
			var cont20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var cont20GPSpotRate = consol.Containers.AddNew();

			cont20GPSpotRate.JC_RC = cont20GP_PK;
			cont20GPSpotRate.JC_ContainerCount = 1;
			cont20GPSpotRate.JC_ContainerNum = "SPOTCOST20GP";

			//Set cost spot rate to 888m for 20GP container, with FRT only
			cont20GPSpotRate.JC_CostSpotRate = 888m;
			cont20GPSpotRate.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			//Set sell spot rate to 999m for 20GP container with ALL IN charges
			cont20GPSpotRate.JC_SellSpotRate = 999m;
			cont20GPSpotRate.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;
		}

		#endregion
	}
}
