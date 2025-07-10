using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class FreightAutoRaterJobUpdatingTests : RatingTestCase
	{
		#region Update FMC Tariff, RateCommodity and DetailedGoodsDescription

		public void TestShipmentRatingAdapter_UpdateFMCTariff_UpdateCommodity_SetDetailedGoodsDescription()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.DetailedGoodsDescriptionNoteText = "Potato";
			shipment.JS_RH_NKRateCommodity = "SHIP";
			shipment.JS_FMCTariffID = "BBB";

			var updater = (IJobDataUpdater)shipment.RatingAdapter;
			updater.UpdateRateCommodityCodeAndFMCTariffID("XYZ", "CCC");
			updater.UpdateDetailedGoodsDescription("Potato", true);

			AssertEquals("CCC", shipment.JS_FMCTariffID);
			AssertEquals("XYZ", shipment.JS_RH_NKRateCommodity);
			AssertEquals("Potato\r\nPotato", shipment.DetailedGoodsDescriptionNoteText);

			updater.UpdateDetailedGoodsDescription("Carrot", false);
			AssertEquals("Carrot", shipment.DetailedGoodsDescriptionNoteText);
		}

		#endregion

		#region Update Chargeable

		void TestChargeable(string message, bool isIncludedCHGRateLine, ZDecimal[] expectedChargeableAmounts, ZDecimal expectedActualChargeable)
		{
			var origin = "AUSYD";
			var destination = "USLAX";
			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "FRT", 1m, QuantityUnit.M3, "USD", "", "HAZ");
			entry.RateLines[0].TL_Rounding = isIncludedCHGRateLine ? RatingRoundingTypes.Chargeable : RatingRoundingTypes.DefaultFromRegistry;
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "FRT", 1m, QuantityUnit.M3, "USD", "", "PER");
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "FRT", 1m, QuantityUnit.M3, "USD", "", "GEN");

			Factory.Save();

			var shipment = TestHelper.CreateForwardingShipment(Factory, "SEA", "LCL", Consignor.PK, Consignee.PK, origin, destination, 50, 1);

			// This line is crucial because it removes the default pack line that is added from shipment details automatically
			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.AddPackLine(1, "PLT", 200, 2, commodity: "HAZ");
			shipment.AddPackLine(1, "PLT", 400, 3, commodity: "PER");
			shipment.AddPackLine(1, "PLT", 300, 4, commodity: "GEN");

			var proxy = new AutoRatingProxy(shipment.RatingAdapter);
			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			AssertContainsExactElementsInAnyOrder(
				"Individual chargeable",
				expectedChargeableAmounts,
				results.SelectMany(r => r.CalculationLogs.Logs.Select(l => l.Chargeable)));
			AssertEquals(message, expectedActualChargeable, proxy.InvoicingSupporter.ActualChargeable);
		}

		public void TestUpdateChargeable_WhenNoCHGFreightChargeFound() =>
			TestChargeable(
				"No CHG Freight Charge Found: Job chargeable should be a sum amount from all FRT charges",
				isIncludedCHGRateLine: false,
				expectedChargeableAmounts: new ZDecimal[] { 2, 3, 4 },
				expectedActualChargeable: 9); // 2 + 3 + 4

		public void TestUpdateChargeable_WhenThereIsACHGFreightCharge() =>
			TestChargeable(
				"There is at least 1 FRT charge with CHG rounding: Job chargeable should not be changed",
				isIncludedCHGRateLine: true,
				expectedChargeableAmounts: new ZDecimal[] { 1, 3, 4 }, // 1 is the overridden chargeable amount from the CHG rate line (HAZ)
				expectedActualChargeable: 1);

		public void TestUpdateChargeableOnHost_ShipmentRatingAdapterImplemented_ShipmentChargeableUpdated()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "");
			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG, "AUD");
			line.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)55m;
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)20m;
			line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ActualVolume = 0.5M;
			shipment.JS_ActualWeight = 55M;
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var frtChargePK = Helper.ChargeCodes["FRT"].PK;
			Env.Registry.FreightChargeCode = frtChargePK.ToGuid();

			var proxy = new AutoRatingProxy(shipment.RatingAdapter);
			var results = testAutoRater.AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == frtChargePK);

			AssertEquals("Pre-condition: results count should be 1.", 1, results.Count);
			AssertEquals("Pre-condition: There should be 2 payment bases.", 2, results[0].Bases.Count);
			Assert("Pre-condition: Chargeable Amount for one of the payment bases should be 83.333M.", results[0].Bases.Any(x => x.Chargeable.Amount == 83.333M));
			Assert("Pre-condition: Chargeable Amount for one of the payment bases should be 125M.", results[0].Bases.Any(x => x.Chargeable.Amount == 125M));

			AssertEquals("Chargeable populated on shipment should be max of (125M and 83.333M)", 125M, shipment.JS_ActualChargeable);
			AssertEquals("FRT Chargeable Weight", 125M, info.GetChargeableFromBasisTest.Amount); // shipment.JS_ActualVolume * ConversionFactor
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 2500m, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			AssertEquals(125M, proxy.InvoicingSupporter.ActualChargeable);
			AssertEquals(Core.Constants.Weight.Kilograms, proxy.InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestUpdateChargeableOnHost_ShipmentRatingAdapterImplementedWithSpotRate_ShipmentChargeableNotUpdated()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_ActualChargeable = 50m;
			shipment.JS_DocumentedChargeable = 30m;
			shipment.JS_DocumentedWeight = 30m;
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;

			shipment.JS_FreightCostRate = 100m;
			shipment.JS_FreightCostRateAutoratingMode = "FRT";
			shipment.JS_RX_NKFreightCostRateCurrency = "USD";

			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Consignee.MainAddress.PK;

			Factory.Save();

			AssertEquals("Precondition: Chargeable Unit", Constants.Weight.Kilograms, shipment.JS_ChargeableUnit);
			AssertEquals("Precondition: Chargeable Amount", 50m, shipment.JS_ActualChargeable);

			var proxy = new AutoRatingProxy(shipment.RatingAdapter);
			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(proxy, CostSell.Cost).RateInfoCollection;

			AssertEquals("Chargeable Unit Not Updated", Constants.Weight.Kilograms, shipment.JS_ChargeableUnit);
			AssertEquals("Chargeable Amount Not Updated", 50m, shipment.JS_ActualChargeable);

			var rateInfo = results.First();
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, rateInfo.GetChargeableFromBasisTest.Unit);
			AssertEquals("FRT Chargeable Weight", 30m, rateInfo.GetChargeableFromBasisTest.Amount);
		}

		public void TestUpdateChargeableOnHost_BookingRatingAdapterImplemented_BookingChargeableUpdated()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "");
			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG, "AUD");
			line.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)55m;
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)20m;
			line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var booking = Factory.New<ForwardingShipment>();
			booking.BuyerSupplierLinksHelper.Deregister();
			booking.BuyerSupplierLinksHelper = null;
			booking.JS_A_BKD = ZDateTime.Now;
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_ShipmentStatus = "BKD";
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_PackingMode = Constants.ContainerModes.Loose;
			booking.JS_ActualVolume = 0.5M;
			booking.JS_ActualWeight = 55M;
			booking.ConsigneePK = Consignee.PK;
			booking.ConsignorPK = Consignor.PK;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var frtChargePK = Helper.ChargeCodes["FRT"].PK;
			Env.Registry.FreightChargeCode = frtChargePK.ToGuid();

			var proxy = new AutoRatingProxy(booking.RatingAdapter);
			var results = testAutoRater.AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == frtChargePK);

			AssertEquals("Pre-condition: results count should be 1.", 1, results.Count);
			AssertEquals("Pre-condition: There should be 2 payment bases.", 2, results[0].Bases.Count);
			Assert("Pre-condition: Chargeable Amount for one of the payment bases should be 83.333M.", results[0].Bases.Any(x => x.Chargeable.Amount == 83.333M));
			Assert("Pre-condition: Chargeable Amount for one of the payment bases should be 125M.", results[0].Bases.Any(x => x.Chargeable.Amount == 125M));

			AssertEquals("Chargeable populated on shipment should be max of (125M and 83.333M)", 125M, booking.JS_ActualChargeable);
			AssertEquals("FRT Chargeable Weight", 125M, info.GetChargeableFromBasisTest.Amount); // shipment.JS_ActualVolume * ConversionFactor
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 2500m, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			AssertEquals(125M, proxy.InvoicingSupporter.ActualChargeable);
			AssertEquals(Core.Constants.Weight.Kilograms, proxy.InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestUpdateChargeableOnHost_HostImplemented_NoFreightCharge()
		{
			SetupExportClientRatesForAutoRater();
			var testObject = new AutoRatingObjectWithUpdatePlugin("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			AssertEquals(0m, ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeable);
		}

		public void TestUpdateChargeableOnHost_HostImplemented_NoChargeableUnit()
		{
			SetupExportClientRatesForAutoRater();
			var testObject = new AutoRatingObjectWithUpdatePlugin("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());

			Env.Registry.FreightChargeCode = TestFRT.PK.ToGuid();
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			AssertEquals(0m, ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeable);
			AssertEquals("", ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestUpdateChargeableOnHost_HostImplemented_FreightChargeExists()
		{
			SetupExportClientRatesForAutoRater();
			var testObject = new AutoRatingObjectWithUpdatePlugin("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");
			testObject.ChargeableUnit = Core.Constants.Weight.Kilograms;

			var testAutoRater = new FreightAutoRater(new RatingContext());

			Env.Registry.FreightChargeCode = TestFRT.PK.ToGuid();
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			AssertEquals(83.333m, ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeable);
			AssertEquals(Core.Constants.Weight.Kilograms, ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestUpdateChargeableOnHost_HostImplemented_FreightChargeExists_DifferentChargeableUnit()
		{
			SetupExportClientRatesForAutoRater();
			var testObject = new AutoRatingObjectWithUpdatePlugin("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");
			testObject.ChargeableUnit = Core.Constants.Weight.Pounds;

			var testAutoRater = new FreightAutoRater(new RatingContext());

			Env.Registry.FreightChargeCode = TestFRT.PK.ToGuid();
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			var expectedWeightPounds = Core.Constants.Weight.Convert(83.333m, Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds);
			AssertEquals(expectedWeightPounds, ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeable);
			AssertEquals(Core.Constants.Weight.Pounds, ((IJobInvoicingPlugIn)testObject).InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestUpdateChargeableOnHost_HostImplemented_FreightChargeExists_WithMultiplePaymentBasis()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			line1.GetCalculator<UnitCalculator>().PerUnit = (ZDecimal)1.1m;
			var line2 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			line2.GetCalculator<UnitCalculator>().PerUnit = (ZDecimal)1.2m;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ActualVolume = 10M;
			shipment.JS_ActualWeight = 1000M;
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var proxy = new AutoRatingProxy(shipment.RatingAdapter);

			AssertEquals("Pre-condition: Chargeable on shipment before AutoRate.", 1666.667M, shipment.JS_ActualChargeable);

			var results = testAutoRater.AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			AssertEquals("Pre-condition: result count should be 1.", 1, results.Count);
			AssertEquals("Pre-condition: There should be 2 payment bases.", 2, results[0].Bases.Count);
			AssertEquals("Pre-condition: Chargeable Amount for first payment bases should be 1666.667M.", 1666.667M, results[0].Bases[0].Chargeable.Amount);
			AssertEquals("Pre-condition: Chargeable Amount for second payment bases should be 1666.667M.", 1666.667M, results[0].Bases[1].Chargeable.Amount);

			AssertEquals("Chargeable populated on shipment should not be doubled ie. 3333.334M(1666.667M + 1666.667M), should be 1666.667M.", 1666.667M, shipment.JS_ActualChargeable);
			AssertEquals(1666.667M, proxy.InvoicingSupporter.ActualChargeable);
			AssertEquals(Constants.Weight.Kilograms, proxy.InvoicingSupporter.ActualChargeableUnit);

			line2.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			proxy = new AutoRatingProxy(shipment.RatingAdapter);
			results = testAutoRater.AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			AssertEquals("Pre-condition: results count should be 1.", 1, results.Count);
			AssertEquals("Pre-condition: There should be 2 payment bases.", 2, results[0].Bases.Count);
			Assert("Pre-condition: Chargeable Amount for one of the payment bases should be 1666.667M.", results[0].Bases.Any(x => x.Chargeable.Amount == 1666.667M));
			Assert("Pre-condition: Chargeable Amount for one of the payment bases should be 2500M.", results[0].Bases.Any(x => x.Chargeable.Amount == 2500M));

			AssertEquals("Chargeable populated on shipment should be max of (1666.667M and 2500M) i.e. 2500M", 2500M, shipment.JS_ActualChargeable);
			AssertEquals(2500M, proxy.InvoicingSupporter.ActualChargeable);
			AssertEquals(Constants.Weight.Kilograms, proxy.InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestUpdateChargeableOnHost_HostImplemented_FreightChargeExists_CMBCalculatorWithCumulativeBreaks()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG, "AUD");
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100, 10);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100, 20);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 300, 30);
			line.Calculator.IsAccumulated = true;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ActualVolume = 10M;
			shipment.JS_ActualWeight = 1000M;
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var proxy = new AutoRatingProxy(shipment.RatingAdapter);

			AssertEquals("Pre-condition: Chargeable on shipment before AutoRate.", 1666.667M, shipment.JS_ActualChargeable);

			var results = testAutoRater.AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			AssertEquals("Pre-condition: result count should be 1.", 1, results.Count);

			var paymentBases = results[0].Bases;
			AssertEquals("Pre-condition: There should be 3 payment bases.", 3, paymentBases.Count);
			Assert("Pre-condition: Chargeable Amount for first payment bases should be 100.00M.", paymentBases.Any(x => x.Chargeable.Amount == 100M));
			Assert("Pre-condition: Chargeable Amount for second payment bases should be 200.00M.", paymentBases.Any(x => x.Chargeable.Amount == 200M));
			Assert("Pre-condition: Chargeable Amount for third payment bases should be 1366.667M.", paymentBases.Any(x => x.Chargeable.Amount == 1366.667M));

			AssertEquals("Chargeable populated on shipment should be 1666.667M(100M + 200M + 1366.667M), should be 1666.667M.", 1666.667M, shipment.JS_ActualChargeable);
			AssertEquals(1666.667M, proxy.InvoicingSupporter.ActualChargeable);
			AssertEquals(Constants.Weight.Kilograms, proxy.InvoicingSupporter.ActualChargeableUnit);
		}

		#endregion

		#region Test helpers

		public class AutoRatingObjectWithUpdatePlugin : AutoRatingObject, IJobInvoicingPlugIn, IRatingSupporter, IJobDataUpdater
		{
			public AutoRatingObjectWithUpdatePlugin(ZString origin, ZString destination, FreightMode freightMode, TestContainers containers, ZDecimal weightInKG, ZDecimal volumeInM3, ZString clientName)
				: base(origin, destination, freightMode, containers, weightInKG, volumeInM3, clientName)
			{
			}

			ZDecimal UpdatedChargeable;

			public ZString ChargeableUnit;

			#region IJobInvoicingPlugIn Members

			AutoRatingObject_WithUpdatePluginInvoicingSupporter fInvoicingSupporter;
			public new IJobInvoicingSupporter InvoicingSupporter
			{
				get { return fInvoicingSupporter ?? (fInvoicingSupporter = new AutoRatingObject_WithUpdatePluginInvoicingSupporter(this)); }
			}

			#endregion

			#region IJobHeaderParent Unused Members

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { throw new NotImplementedException(); }
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			ZGuid IJobHeaderParentCore.PK
			{
				get { throw new NotImplementedException(); }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}

			string IJobHeaderParentCore.TableName
			{
				get { throw new NotImplementedException(); }
			}

			bool IJobHeaderParentCore.IsInDatabase
			{
				get { throw new NotImplementedException(); }
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { throw new NotImplementedException(); }
			}

			bool IJobHeaderParent.IsDeleted
			{
				get { return false; }
			}

			#endregion

			#region IJobNumber Unused Members

			string IJobNumber.JobNumber
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region Invoicing Supporter

			public class AutoRatingObject_WithUpdatePluginInvoicingSupporter : JobInvoicingSupporter
			{
				public AutoRatingObject_WithUpdatePluginInvoicingSupporter(AutoRatingObjectWithUpdatePlugin parent)
					: base(parent)
				{
					Parent = parent;
				}

				protected readonly AutoRatingObjectWithUpdatePlugin Parent;

				public override ZString ActualChargeableUnit
				{
					get { return Parent.ChargeableUnit; }
				}

				public override ZDecimal ActualChargeable
				{
					get { return Parent.UpdatedChargeable; }
				}
			}

			#endregion

			#region IRatingSupporter Members

			RatingAdaptersProvider IRatingSupporter.AdaptersProvider
			{
				get { return AdaptersProviderGetter(); }
			}

			#endregion

			#region IJobDataUpdater

			bool IJobDataUpdater.CanUpdateDate => false;

			void IJobDataUpdater.UpdateServiceLevel(ZString newServiceLevel) { }

			void IJobDataUpdater.UpdateCarrier(OrgHeader newCarrier) { }

			bool IJobDataUpdater.UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
			{
				confirmationMessage = null;
				return false;
			}

			void IJobDataUpdater.UpdateOrigin(ZString newOrigin) { }

			bool IJobDataUpdater.UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
			{
				confirmationMessage = null;
				return false;
			}

			void IJobDataUpdater.UpdateDestination(ZString newDestination) { }

			bool IJobDataUpdater.UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
			{
				confirmationMessage = null;
				return false;
			}

			void IJobDataUpdater.UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates) { }

			bool IJobDataUpdater.UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
			{
				confirmationMessage = null;
				return false;
			}

			void IJobDataUpdater.UpdatePaymentTerms(ZString newPaymentTerms) { }

			void IJobDataUpdater.UpdateNamedAccount(ZString namedAccount) { }

			void IJobDataUpdater.UpdateCarrierQuoteNumber(ZString carrierQuoteNumber) { }

			void IJobDataUpdater.UpdateSpotBookingTerms(ZString termsAsText) { }

			DataUpdateResult IJobDataUpdater.UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token) => DataUpdateResult.NoAction;

			DataUpdateResult IJobDataUpdater.UpdateClientContractNumber(IEnumerable<string> newNumbers)
			{
				return DataUpdateResult.NoAction;
			}

			void IJobDataUpdater.UpdateTransports(IEnumerable<ITransport> transports) { }

			void IJobDataUpdater.UpdateChargeable(ZDecimal newChargeable)
			{
				UpdatedChargeable = newChargeable;
			}

			bool IJobDataUpdater.IsMultipleClientContractNumberSupported => IsMultipleClientContractNumberSupported_ForTest;

			public bool IsMultipleClientContractNumberSupported_ForTest { get; set; }

			#endregion
		}
		#endregion
	}
}
