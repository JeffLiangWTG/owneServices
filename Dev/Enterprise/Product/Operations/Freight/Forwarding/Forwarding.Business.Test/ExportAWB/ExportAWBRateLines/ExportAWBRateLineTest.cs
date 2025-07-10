using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBRateLine))]
	class ExportAWBRateLineTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("", RateLine.ER_WeightInLBsOrKGs);
		}

		public void TestClear()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_WeightInLBsOrKGs = "E";
			RateLine.ER_GrossWeight = 10;
			RateLine.ER_ChargeableWeight = 10;
			RateLine.ER_RateChargeOrDiscount = 10;
			RateLine.ER_CommodityItemNumber = "L";
			RateLine.ER_RateClass = "L";

			RateLine.Clear();

			AssertEquals("0", RateLine.ER_NoOfPiecesOrRCP);
			AssertEquals("", RateLine.ER_WeightInLBsOrKGs);
			AssertEquals(0M, RateLine.ER_GrossWeight);
			AssertEquals(0M, RateLine.ER_ChargeableWeight);
			AssertEquals(0M, RateLine.ER_RateChargeOrDiscount);
			AssertEquals("", RateLine.ER_CommodityItemNumber);
			AssertEquals("", RateLine.ER_RateClass);
		}

		public void TestIsEmpty()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_WeightInLBsOrKGs = "E";
			RateLine.ER_GrossWeight = 10;
			RateLine.ER_ChargeableWeight = 10;
			RateLine.ER_RateChargeOrDiscount = 10;
			RateLine.ER_CommodityItemNumber = "L";
			RateLine.ER_RateClass = "L";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.ER_NoOfPiecesOrRCP = "0";
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.ER_NoOfPiecesOrRCP = "   ";
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.ER_NoOfPiecesOrRCP = "10";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_WeightInLBsOrKGs = "E";
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_GrossWeight = 10;
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_ChargeableWeight = 10;
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_RateChargeOrDiscount = 10;
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_CommodityItemNumber = "L";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_RateClass = "L";
			AssertEquals(false, RateLine.IsEmpty);
		}

		public void TestIsSavedByFactory()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			AssertEquals("Object w/o header behaves normally", true, rateLine.IsSavedByFactory);
			rateLine.Delete();

			var aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			rateLine = aWBHeader.AWBRateLines.AddNew();
			aWBHeader.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			aWBHeader.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Not overridden new AWB is never saved", false, rateLine.IsSavedByFactory);
			Factory.Save();
			AssertEquals(false, rateLine.IsInDatabase);

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			aWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			aWBHeader.EH_AreRateLinesOverridden = false;
			aWBHeader.AWBRateLines.RemoveAndDelete(rateLine);
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed and repopulated", false, rateLine.IsSavedByFactory);
			AssertEquals(true, rateLine.IsDeleted);

			rateLine = aWBHeader.AWBRateLines.AddNew();
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, rateLine.IsSavedByFactory);

			aWBHeader.ForceSavingByFactory = true;
			Assert(rateLine.IsEmpty);
			AssertEquals("Forced empty rate line is not saved", false, rateLine.IsSavedByFactory);

			rateLine.ER_GrossWeight = 100M;
			AssertEquals("Forced filled new rate line is saved", true, rateLine.IsSavedByFactory);

			Factory.Save();
			AssertEquals(true, rateLine.IsInDatabase);

			aWBHeader.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, rateLine.IsSavedByFactory);

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("Saved when 'Override' is ticked", true, rateLine.IsSavedByFactory);
			rateLine.Clear();
			AssertEquals("Empty in database is still saved", true, rateLine.IsSavedByFactory);
			rateLine.Delete();
			AssertEquals("Deleted in database is still saved", true, rateLine.IsSavedByFactory);
			rateLine = aWBHeader.AWBRateLines.AddNew();
			rateLine.ER_GrossWeight = 100M;
			Factory.Save();

			AssertEquals("rateLine is overridden and saved", true, rateLine.IsSavedByFactory);

			rateLine.ER_GrossWeight = 1000M;
			AssertEquals("Overridden is saved when has changes", true, rateLine.IsSavedByFactory);
			Factory.Save();

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			aWBHeader.EH_AreRateLinesOverridden = false;
			AssertEquals("Overridden is saved when 'Override' has changes and already in the database", true, rateLine.IsSavedByFactory);
			Factory.Save();

			rateLine.ER_GrossWeight = 200M;
			AssertEquals("Not overridden and saved is not saved when has changes", false, rateLine.IsSavedByFactory);

			aWBHeader.EH_AreRateLinesOverridden = true;
			AssertEquals("Rate section is overridden and there are changes", true, rateLine.IsSavedByFactory);
		}

		public void TestIsSavedByFactory_OverrideChangedTwice_Shipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 100M;
			shipment.PopulateAWB();
			shipment.AWBHeader.EH_AreRateLinesOverridden = true;
			Factory.Save();
			AssertEquals(100M, shipment.AWBHeader.AWBRateLine1.ER_GrossWeight);

			shipment.JS_ActualWeight = 200M;
			shipment.AWBHeader.EH_AreRateLinesOverridden = false;
			shipment.AWBHeader.EH_AreRateLinesOverridden = true;
			Factory.Save();
			AssertEquals(200M, shipment.AWBHeader.AWBRateLine1.ER_GrossWeight);

			var anotherFactory = new BusinessObjectFactory();
			var loadedShipment = anotherFactory.Load<ForwardingShipment>(shipment.PK);
			AssertEquals(200M, loadedShipment.AWBHeader.AWBRateLine1.ER_GrossWeight);
		}

		public void TestIsSavedByFactory_OverrideChangedTwice_Consol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 100M;
			consol.PopulateAWB();
			consol.AWBHeader.EH_AreRateLinesOverridden = true;
			Factory.Save();
			AssertEquals(100M, consol.AWBHeader.AWBRateLine1.ER_GrossWeight);

			shipment.JS_ActualWeight = 200M;
			consol.AWBHeader.EH_AreRateLinesOverridden = false;
			consol.AWBHeader.EH_AreRateLinesOverridden = true;
			Factory.Save();
			AssertEquals(200M, consol.AWBHeader.AWBRateLine1.ER_GrossWeight);

			var anotherFactory = new BusinessObjectFactory();
			var loadedConsol = anotherFactory.Load<ForwardingConsol>(consol.PK);
			AssertEquals(200M, loadedConsol.AWBHeader.AWBRateLine1.ER_GrossWeight);
		}

		public void TestER_NoOfPiecesOrRCP()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			AssertEquals("10", RateLine.ER_NoOfPiecesOrRCP);
		}

		public void TestER_NoOfPiecesOrRCPAsInt()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			AssertEquals(10, RateLine.ER_NoOfPiecesOrRCPAsInt);
			RateLine.ER_NoOfPiecesOrRCP = "";
			AssertEquals(0, RateLine.ER_NoOfPiecesOrRCPAsInt);
			RateLine.ER_NoOfPiecesOrRCP = "WER";
			AssertEquals(0, RateLine.ER_NoOfPiecesOrRCPAsInt);
			RateLine.ER_NoOfPiecesOrRCP = "0";
			AssertEquals(0, RateLine.ER_NoOfPiecesOrRCPAsInt);
		}

		public void TestER_GrossWeight()
		{
			RateLine.ER_GrossWeight = 10.3M;
			AssertEquals(10.3M, RateLine.ER_GrossWeight);
		}

		public void TestGrossWeight_DecimalPlaces()
		{
			RateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Pounds;
			RateLine.ER_GrossWeight = 1.23M;

			AssertEquals("ER_GrossWeight should show 1 decimal", 1, RateLine.GetNumberOfDecimals(RateLine.ER_GrossWeightInfo));
			AssertEquals("ER_GrossWeight should equal 1.3", 1.3M, RateLine.ER_GrossWeight);

			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			RateLine.ER_GrossWeight = 1.23M;

			AssertEquals("ER_GrossWeight should show 2 decimals", 2, RateLine.GetNumberOfDecimals(RateLine.ER_GrossWeightInfo));
			AssertEquals("ER_GrossWeight should equal 1.3", 1.23M, RateLine.ER_GrossWeight);
		}

		public void TestTotalGrossWeight_DecimalPlaces()
		{
			AWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Pounds;
			AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.23M;
			AWBHeader.AWBRateLines[1].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.23M;
			AssertEquals("EH_TotalGrossWeight should show 1 decimal", 1, AWBHeader.TotalGrossWeightDecimalPlaces);
			AssertEquals("EH_TotalGrossWeight should equal 2.6", 2.6M, AWBHeader.EH_TotalGrossWeight);

			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.23M;
			AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.23M;

			AssertEquals("EH_TotalGrossWeight should show 2 decimals", 2, AWBHeader.TotalGrossWeightDecimalPlaces);
			AssertEquals("EH_TotalGrossWeight should equal 1.23", 2.53M, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestGetIndexOfLastRateLineWithDecimalPlaceWithAndWithoutMaster()
		{
			AWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Pounds;
			AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.23M;
			AWBHeader.AWBRateLines[1].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.23M;
			AssertEquals("EH_TotalGrossWeight should show 1 decimal", 1, AWBHeader.TotalGrossWeightDecimalPlaces);
			AssertEquals("EH_TotalGrossWeight should equal 2.6", 2.6M, AWBHeader.EH_TotalGrossWeight);

			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.23M;
			AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.23M;

			foreach (ExportAWBRateLine awbHeaderAwbRateLine in AWBHeader.AWBRateLines)
			{
				AssertNotEquals("Master should be different than null", null, awbHeaderAwbRateLine.Master);
			}

			AssertEquals("EH_TotalGrossWeight should show 2 decimals", 2, AWBHeader.TotalGrossWeightDecimalPlaces);
			AssertEquals("EH_TotalGrossWeight should equal 1.23", 2.53M, AWBHeader.EH_TotalGrossWeight);

			foreach (ExportAWBRateLine awbHeaderAwbRateLine in AWBHeader.AWBRateLines)
			{
				awbHeaderAwbRateLine.ER_EH = ZGuid.Empty;

				AssertEquals("Master should be null", null, awbHeaderAwbRateLine.Master);
			}

			AssertEquals("ER_GrossWeight should show 2 decimals", 2, AWBHeader.TotalGrossWeightDecimalPlaces);
			AssertEquals("EH_TotalGrossWeight should equal 1.23", 2.53M, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestER_ChargeableWeight()
		{
			RateLine.ER_ChargeableWeight = 10.32M;
			AssertEquals(10.5M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.2M;
			AssertEquals(10.5M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.26M;
			AssertEquals(10.5M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.7M;
			AssertEquals(11M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.75M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.8M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.9M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			ZGuid previousHeaderPK = RateLine.ER_EH;
			RateLine.ER_EH = ZGuid.Empty;
			RateLine.ER_ChargeableWeight = 10.9M;
			AssertEquals(10.9M, RateLine.ER_ChargeableWeight);

			RateLine.ER_EH = previousHeaderPK;
			RateLine.ER_ChargeableWeight = 10.9M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			RateLine.ER_ChargeableWeight = 10.0M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.32M;
			AssertEquals(11M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.7M;
			AssertEquals(11M, RateLine.ER_ChargeableWeight);

			collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			RateLine.ER_ChargeableWeight = 10.0M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.32M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.7M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);
		}

		public void TestER_RateClass()
		{
			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			AssertEquals(Constants.AWB.RateClass.RatePerKilogram, RateLine.ER_RateClass);

			//test that changine ER_RateClass causes a refresh
			RateLine.ER_ChargeableWeight = 10.5M;
			RateLine.ER_Total = 152M;
			AssertEquals(14.48M, RateLine.ER_RateChargeOrDiscount);
			RateLine.ER_RateClass = Constants.AWB.RateClass.MinimumCharge;
			AssertEquals(14.48M, RateLine.ER_Total);
		}

		public void TestER_RateChargeOrDiscount()
		{
			AWBHeader.EH_Currency = "AUD";

			RateLine.ER_RateChargeOrDiscount = 10.3M;
			AssertEquals(10.3M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			RateLine.ER_ChargeableWeight = 10.5M;
			RateLine.ER_Total = 152M;
			AssertEquals(14.48M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;
			RateLine.ER_CommodityItemNumber = "M150";
			RateLine.ER_RateChargeOrDiscount = 99.5M;
			AssertEquals(99.5M, RateLine.ER_Total);

			RateLine.ER_CommodityItemNumber = "N150";
			RateLine.ER_ChargeableWeight = 10.5M;
			AssertEquals(1044.75M, RateLine.ER_Total);

			AWBHeader.EH_Currency = "JPY";

			RateLine.ER_RateChargeOrDiscount = 10.3M;
			AssertEquals(10.3M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			RateLine.ER_ChargeableWeight = 10.5M;
			RateLine.ER_Total = 152M;
			AssertEquals(14.0M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;
			RateLine.ER_CommodityItemNumber = "M150";
			RateLine.ER_RateChargeOrDiscount = 99.5M;
			AssertEquals(100.0M, RateLine.ER_Total);

			RateLine.ER_CommodityItemNumber = "N150";
			RateLine.ER_ChargeableWeight = 10.5M;
			AssertEquals(1045.0M, RateLine.ER_Total);
		}

		public void TestER_Total()
		{
			AWBHeader.EH_Currency = "AUD";

			RateLine.ER_ChargeableWeight = 10.31M;
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_RateChargeOrDiscount = 9.43M;

			RateLine.ER_RateClass = Constants.AWB.RateClass.MinimumCharge;
			AssertEquals(9.43M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals(94.3M, RateLine.ER_Total);

			RateLine.ER_RateClass = "";
			AssertEquals(0M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.QuantityRate;
			AssertEquals(99.02M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			AssertEquals(99.02M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;
			RateLine.ER_CommodityItemNumber = "M150";
			RateLine.ER_Total = 66.6M;
			AssertEquals(66.6M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_CommodityItemNumber = "N150";
			RateLine.ER_Total = 66.6M;
			AssertEquals(6.34M, RateLine.ER_RateChargeOrDiscount);

			AWBHeader.EH_Currency = "JPY";

			RateLine.ER_ChargeableWeight = 10.31M;
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_RateChargeOrDiscount = 9.43M;

			RateLine.ER_RateClass = Constants.AWB.RateClass.MinimumCharge;
			AssertEquals(9.0M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals(94.0M, RateLine.ER_Total);

			RateLine.ER_RateClass = "";
			AssertEquals(0M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.BasicCharge;
			RateLine.ER_ChargeableWeight = 11.6M;
			RateLine.ER_RateChargeOrDiscount = 16M;
			AssertEquals(16m, RateLine.ER_Total);
		}

		public void TestCurrencyDecimals()
		{
			AWBHeader.EH_Currency = "AUD";
			AssertEquals("Currency decimals", 2, RateLine.CurrencyDecimals);

			AWBHeader.EH_Currency = "JPY";
			AssertEquals("Currency decimals", 0, RateLine.CurrencyDecimals);

			AWBHeader.EH_Currency = "XXX";
			AssertEquals("Default currency decimals", 2, RateLine.CurrencyDecimals);
		}

		public void TestShouldPropertiesBeReadOnly()
		{
			AWBHeader.Parent.IsAWBValuesOverriddenProperty = false;
			AWBHeader.EH_AreRateLinesOverridden = false;
			AWBHeader.SetReadOnlyIncludingChildren(true);
			CombineAssertions(() =>
			{
				Assert("ER_NoOfPiecesOrRCPInfo", RateLine.ER_NoOfPiecesOrRCPInfo.ReadOnly);
				Assert("ER_GrossWeightInfo", RateLine.ER_GrossWeightInfo.ReadOnly);
				Assert("ER_WeightInLBsOrKGsInfo", RateLine.ER_WeightInLBsOrKGsInfo.ReadOnly);
				Assert("ER_CommodityItemNumberInfo", RateLine.ER_CommodityItemNumberInfo.ReadOnly);
				Assert("ER_ChargeableWeightInfo", RateLine.ER_ChargeableWeightInfo.ReadOnly);
				Assert("ER_RateChargeOrDiscountInfo", RateLine.ER_RateChargeOrDiscountInfo.ReadOnly);
				Assert("ER_TotalInfo", RateLine.ER_TotalInfo.ReadOnly);
			});

			AWBHeader.EH_AreRateLinesOverridden = true;
			AWBHeader.SetReadOnlyIncludingChildren(true);
			CombineAssertions(() =>
			{
				Assert("ER_NoOfPiecesOrRCPInfo", !RateLine.ER_NoOfPiecesOrRCPInfo.ReadOnly);
				Assert("ER_GrossWeightInfo", !RateLine.ER_GrossWeightInfo.ReadOnly);
				Assert("ER_WeightInLBsOrKGsInfo", !RateLine.ER_WeightInLBsOrKGsInfo.ReadOnly);
				Assert("ER_CommodityItemNumberInfo", !RateLine.ER_CommodityItemNumberInfo.ReadOnly);
				Assert("ER_ChargeableWeightInfo", !RateLine.ER_ChargeableWeightInfo.ReadOnly);
				Assert("ER_RateChargeOrDiscountInfo", !RateLine.ER_RateChargeOrDiscountInfo.ReadOnly);
				Assert("ER_TotalInfo", !RateLine.ER_TotalInfo.ReadOnly);
			});

			AWBHeader.Parent.IsAWBValuesOverriddenProperty = true;
			RateLine.SetReadOnlyIncludingChildren(false);
			CombineAssertions(() =>
			{
				Assert("ER_NoOfPiecesOrRCPInfo", !RateLine.ER_NoOfPiecesOrRCPInfo.ReadOnly);
				Assert("ER_GrossWeightInfo", !RateLine.ER_GrossWeightInfo.ReadOnly);
				Assert("ER_WeightInLBsOrKGsInfo", !RateLine.ER_WeightInLBsOrKGsInfo.ReadOnly);
				Assert("ER_CommodityItemNumberInfo", !RateLine.ER_CommodityItemNumberInfo.ReadOnly);
				Assert("ER_ChargeableWeightInfo", !RateLine.ER_ChargeableWeightInfo.ReadOnly);
				Assert("ER_RateChargeOrDiscountInfo", !RateLine.ER_RateChargeOrDiscountInfo.ReadOnly);
				Assert("ER_TotalInfo", !RateLine.ER_TotalInfo.ReadOnly);
			});
		}

		public void TestRateUQList()
		{
			AssertEquals(OLookUpEditType.AWBRateUQ, RateLine.RateUQList.LookupEditType);
		}

		public virtual void TestRateClassList()
		{
			AssertEquals(OLookUpEditType.AWBRateClass, RateLine.RateClassList.LookupEditType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("House Air Waybill Freight Breakdown", RateLine.HumanReadableName);

			ConsolExportAWBHeader consolAWBHeader = Factory.New<ConsolExportAWBHeader>();
			ExportAWBRateLine consolAWBRateLine = consolAWBHeader.AWBRateLines.AddNew();
			AssertEquals("Master Air Waybill Freight Breakdown", consolAWBRateLine.HumanReadableName);

			ExportAWBHeader mockAWBHeader = Factory.New<MockExportAWBHeader>();
			ExportAWBRateLine aWBRateLineFromMockAWBHeader = mockAWBHeader.AWBRateLines.AddNew();
			AssertEquals("Not a shipment or consol Air Waybill", "Air Waybill Freight Breakdown", aWBRateLineFromMockAWBHeader.HumanReadableName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ExportAWBRateLine>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var header = factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			header.ForceSavingByFactory = true;

			var result = (ExportAWBRateLine)factory.New(GetExpectedBusinessObjectType());
			result.FillWithValidTestData();
			result.ER_EH = header.PK;

			return result;
		}

		ExportAWBRateLineTestClass RateLine;
		protected ShipmentExportAWBHeader AWBHeader;

		protected override void SetUp()
		{
			RateLine = Factory.New<ExportAWBRateLineTestClass>();
			AWBHeader = Factory.New<ShipmentExportAWBHeader>();
			ExportAWBRateLineCollection rateLineCollection = new ExportAWBRateLineCollection(AWBHeader);
			rateLineCollection.Add(RateLine);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AWBHeader.EH_ParentID = shipment.PK;
			shipment.JS_OverrideWaybillDefaults = ZBool.True;

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Weight.Pounds;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public class ExportAWBRateLineTestClass : ExportAWBRateLine
		{
			public ExportAWBRateLineTestClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new int CurrencyDecimals
			{
				get { return base.CurrencyDecimals; }
			}
		}

		#endregion
	}
}
