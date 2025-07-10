using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class HighVolumeLowValueShipmentIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestHVLVShipmentRating_ShippedInMultipleShipments()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			SetupBasicRatesForClient(client, RatingConstants.RateCategory.LCL, RateMode.LCL);

			#region Shipment Setup

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var otherShipment = CreateForwardingShipment(TransportModes.Sea, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m);
			otherShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();

			var item1 = consignment.Items.AddNew();
			item1.HVI_ActualWeight = 50m;
			item1.HVI_ActualVolume = 0.05;

			var item2 = consignment.Items.AddNew();
			item2.HVI_ActualWeight = 200m;
			item2.HVI_ActualVolume = 0.2;

			var item3 = consignment.Items.AddNew();
			item3.HVI_ActualWeight = 100m;
			item3.HVI_ActualVolume = 0.1;
			item3.HVI_JS_LoadedOnShipment = otherShipment.PK;

			#endregion

			Factory.Save();

			var expectedResult1 = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 500m,
					RevenueCalculationDescription = $@"FRT
{item1.HumanReadableName}
	100.00 AUD		50 Kilogram(s) @ AUD 2.00/KG
{item2.HumanReadableName}
	400.00 AUD		200 Kilogram(s) @ AUD 2.00/KG"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 0.75m,
					RevenueCalculationDescription = $@"WAR
{item1.HumanReadableName}
	0.15 AUD		0.05 Cubic Meter(s) @ AUD 3.00/M3
{item2.HumanReadableName}
	0.60 AUD		0.2 Cubic Meter(s) @ AUD 3.00/M3"
				}
			};

			var expectedResult2 = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200m,
					RevenueCalculationDescription = $@"FRT
{item3.HumanReadableName}
	200.00 AUD		100 Kilogram(s) @ AUD 2.00/KG"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 0.30m,
					RevenueCalculationDescription = $@"WAR
{item3.HumanReadableName}
	0.30 AUD		0.1 Cubic Meter(s) @ AUD 3.00/M3"
				}
			};

			AutorateAndAssert("There should be charges since the consignment is manifested on this shipment", expectedResult1, shipment, client);
			AutorateAndAssert("There should be charges since the item3 is loaded on this shipment", expectedResult2, otherShipment, client);
		}

		public void TestHVLVShipmentRating_SeaLCL()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			SetupBasicRatesForClient(client, RatingConstants.RateCategory.LCL, RateMode.LCL);

			#region Shipment Setup

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ActualWeight = 3m;
			item1_1.HVI_ActualVolume = 0.004;

			var consignment2 = header.Consignments.AddNew();

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ActualWeight = 5m;
			item2_1.HVI_ActualVolume = 0.002;

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 18m,
					RevenueCalculationDescription = $@"FRT
{item1_1.HumanReadableName}
	8.00 AUD		4 Kilogram(s) @ AUD 2.00/KG
{item2_1.HumanReadableName}
	10.00 AUD		5 Kilogram(s) @ AUD 2.00/KG"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 0.03m,
					RevenueCalculationDescription = $@"WAR
{item1_1.HumanReadableName}
	0.01 AUD		0.004 Cubic Meter(s) @ AUD 3.00/M3
{item2_1.HumanReadableName}
	0.02 AUD		0.005 Cubic Meter(s) @ AUD 3.00/M3"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestHVLVShipmentRating_AirLSE()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			SetupBasicRatesForClient(client, RatingConstants.RateCategory.AIR, RateMode.LSE);

			#region Shipment Setup

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ActualWeight = 3m;
			item1_1.HVI_ActualVolume = 0.004;

			var consignment2 = header.Consignments.AddNew();

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ActualWeight = 5m;
			item2_1.HVI_ActualVolume = 0.002;

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 16m,
					RevenueCalculationDescription = $@"FRT
{item1_1.HumanReadableName}
	6.00 AUD		3 Kilogram(s) @ AUD 2.00/KG
{item2_1.HumanReadableName}
	10.00 AUD		5 Kilogram(s) @ AUD 2.00/KG"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 0.14m,
					RevenueCalculationDescription = $@"WAR
{item1_1.HumanReadableName}
	0.05 AUD		0.018 Cubic Meter(s) @ AUD 3.00/M3
{item2_1.HumanReadableName}
	0.09 AUD		0.03 Cubic Meter(s) @ AUD 3.00/M3"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestHVLVShipmentRating_UnitCalculation()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			#region Setup Rates

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");

			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var rateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, PkgUnit.Bag);

			var item1 = rateLine.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.MIN;
			item1.TM_RelevantValue = 10;

			var item2 = rateLine.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Minus;
			item2.TM_Break = 1;
			item2.TM_RelevantValue = 15;
			item2.TM_FlatAmount = 100;

			var item3 = rateLine.RateLineItems.AddNew();
			item3.TM_Type = Calculator.Items.Operator.Plus;
			item3.TM_Break = 2;
			item3.TM_RelevantValue = 20;
			item3.TM_FlatAmount = 100;

			#endregion

			#region Setup Shipment

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ItemId = "1_1";
			item1_1.HVI_ActualWeight = 1.5m;
			item1_1.HVI_ActualVolume = 0.002;
			item1_1.HVI_F3_NKPackType = PkgUnit.Bag;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ItemId = "1_2";
			item1_2.HVI_ActualWeight = 1.5m;
			item1_2.HVI_ActualVolume = 0.002;
			item1_2.HVI_F3_NKPackType = PkgUnit.Bag;

			var consignment2 = header.Consignments.AddNew();

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ItemId = "2_1";
			item2_1.HVI_ActualWeight = 5m;
			item2_1.HVI_ActualVolume = 0.002;
			item2_1.HVI_F3_NKPackType = PkgUnit.Bag;

			var consignment3 = header.Consignments.AddNew();

			var item3_1 = consignment3.Items.AddNew();
			item3_1.HVI_ItemId = "3_1";
			item3_1.HVI_ActualWeight = 7m;
			item3_1.HVI_ActualVolume = 0.001;
			item3_1.HVI_F3_NKPackType = PkgUnit.Box;

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 445m,
					RevenueCalculationDescription = $@"FRT
{item1_1.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Bag(s) @ AUD 15.00/Bag
{item1_2.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Bag(s) @ AUD 15.00/Bag
{item2_1.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Bag(s) @ AUD 15.00/Bag
{item3_1.HumanReadableName}
	100.00 AUD		Base Rate AUD 100.00"
				}
			};

			AutorateAndAssert("First consignment has 2 bags (plus rate), second has 1 (minus rate), third has none (base rate)", expected, shipment, client);
		}

		public void TestHVLVShipmentRating_PackageCalculation()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			#region Setup Rates 

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");

			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var rateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.PK);

			var item1 = rateLine.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.MIN;
			item1.TM_RelevantValue = 10;

			var item2 = rateLine.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Minus;
			item2.TM_Break = 1;
			item2.TM_RelevantValue = 15;
			item2.TM_FlatAmount = 100;

			var item3 = rateLine.RateLineItems.AddNew();
			item3.TM_Type = Calculator.Items.Operator.Plus;
			item3.TM_Break = 2;
			item3.TM_RelevantValue = 20;
			item3.TM_FlatAmount = 100;

			#endregion

			#region Setup Shipment

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m, 0, consol);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ActualWeight = 1.5m;
			item1_1.HVI_ActualVolume = 0.002;
			item1_1.HVI_F3_NKPackType = PkgUnit.Bag;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ActualWeight = 1.5m;
			item1_2.HVI_ActualVolume = 0.002;
			item1_2.HVI_F3_NKPackType = PkgUnit.Bag;

			var consignment2 = header.Consignments.AddNew();

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ActualWeight = 5m;
			item2_1.HVI_ActualVolume = 0.002;
			item2_1.HVI_F3_NKPackType = PkgUnit.Bag;

			var consignment3 = header.Consignments.AddNew();

			var item3_1 = consignment3.Items.AddNew();
			item3_1.HVI_ActualWeight = 7m;
			item3_1.HVI_ActualVolume = 0.001;
			item3_1.HVI_F3_NKPackType = PkgUnit.Bag;

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 460m,
					RevenueCalculationDescription = $@"FRT
{item1_1.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Package(s) @ AUD 15.00/Package
{item1_2.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Package(s) @ AUD 15.00/Package
{item2_1.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Package(s) @ AUD 15.00/Package
{item3_1.HumanReadableName}
	115.00 AUD		Base Rate AUD 100.00 + 1 Package(s) @ AUD 15.00/Package"
				}
			};

			AutorateAndAssert("First consignment has 2 packages (plus rate), consignment 1 and 2 have 1 package (minus rate)", expected, shipment, client);
		}

		public void TestCMB_PackageUnit_BreakPerWeight()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			#region Setup Rates

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var frtLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, PkgUnit.Bag);
			frtLine.UseOnlyActualWeightMeasure = true;

			var frtItem1 = frtLine.RateLineItems.AddNew();
			frtItem1.TM_Type = Calculator.Items.Operator.Minus;
			frtItem1.TM_Break = 150;
			frtItem1.TM_RelevantValue = 15;
			frtItem1.TM_BreakWeightVolume = "KG";

			var frtItem2 = frtLine.RateLineItems.AddNew();
			frtItem2.TM_Type = Calculator.Items.Operator.Plus;
			frtItem2.TM_Break = 150;
			frtItem2.TM_RelevantValue = 20;

			var bafLine = rateEntry.AddRateLine("BAF", CombinedCalculator.Code, "PK");
			bafLine.UseOnlyActualWeightMeasure = true;

			var bafItem1 = bafLine.RateLineItems.AddNew();
			bafItem1.TM_Type = Calculator.Items.Operator.Minus;
			bafItem1.TM_Break = 150;
			bafItem1.TM_RelevantValue = 15;
			bafItem1.TM_BreakWeightVolume = "KG";

			var bafItem2 = bafLine.RateLineItems.AddNew();
			bafItem2.TM_Type = Calculator.Items.Operator.Plus;
			bafItem2.TM_Break = 150;
			bafItem2.TM_RelevantValue = 20;

			#endregion

			#region Setup Shipment

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m, 0, consol);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ActualWeight = 100m;
			item1_1.HVI_ActualVolume = 1;
			item1_1.HVI_F3_NKPackType = PkgUnit.Bag;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ActualWeight = 200m;
			item1_2.HVI_ActualVolume = 1;
			item1_2.HVI_F3_NKPackType = PkgUnit.Box;

			var consignment2 = header.Consignments.AddNew();

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ActualWeight = 50m;
			item2_1.HVI_ActualVolume = 1;
			item2_1.HVI_F3_NKPackType = PkgUnit.Bag;

			var consignment3 = header.Consignments.AddNew();

			var item3_1 = consignment3.Items.AddNew();
			item3_1.HVI_ActualWeight = 300m;
			item3_1.HVI_ActualVolume = 1;
			item3_1.HVI_F3_NKPackType = PkgUnit.Pallet;

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
					RevenueCalculationDescription = $@"FRT
{item1_1.HumanReadableName}
	15.00 AUD		1 Bag(s) @ AUD 15.00/Bag
{item2_1.HumanReadableName}
	15.00 AUD		1 Bag(s) @ AUD 15.00/Bag"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 70m,
					RevenueCalculationDescription = $@"BAF
{item1_1.HumanReadableName}
	15.00 AUD		1 Package(s) @ AUD 15.00/Package
{item1_2.HumanReadableName}
	20.00 AUD		1 Package(s) @ AUD 20.00/Package
{item2_1.HumanReadableName}
	15.00 AUD		1 Package(s) @ AUD 15.00/Package
{item3_1.HumanReadableName}
	20.00 AUD		1 Package(s) @ AUD 20.00/Package"
				}
			};

			AutorateAndAssert("First consignment has 2 packages (plus rate), consignment 1 and 2 have 1 package (minus rate)", expected, shipment, client);
		}

		public void TestCTG_PackageUnit_BreakPerWeight()
		{
			var client = Helper.NewOrgHeader("ORGTSTSYD", "AUSYD");
			var consignee = Helper.NewOrgHeader("ORGTSTHKG", "HKHKG");

			#region Setup Rates

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "HKHKG");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var frtLine = rateEntry.AddRateLine("FRT", CartageCalculator.Code, "KG");
			frtLine.ConversionFactor = new ConversionFactor(5000m, Volume.CubicCentimeters, Weight.Kilograms);

			var calculator = frtLine.GetCalculator<CartageCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;

			var frtLineItem1 = frtLine.RateLineItems.AddNew();
			frtLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			frtLineItem1.TM_Break = 1m;
			frtLineItem1.TM_RelevantValue = 10m;
			frtLineItem1.TM_BreakWeightVolume = "KG";

			var frtLineItem2 = frtLine.RateLineItems.AddNew();
			frtLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			frtLineItem2.TM_Break = 1m;
			frtLineItem2.TM_RelevantValue = 11m;

			var frtLineItem3 = frtLine.RateLineItems.AddNew();
			frtLineItem3.TM_Type = Calculator.Items.Operator.Plus;
			frtLineItem3.TM_Break = 1.1m;
			frtLineItem3.TM_RelevantValue = 11.1m;

			var frtLineItem4 = frtLine.RateLineItems.AddNew();
			frtLineItem4.TM_Type = Calculator.Items.Operator.Plus;
			frtLineItem4.TM_Break = 1.2m; // 8 x 17 x 44.5 cm / 5000 = 1.2104 kg > 1.2
			frtLineItem4.TM_RelevantValue = 11.2m; // Targeted/Expected Item

			var frtLineItem5 = frtLine.RateLineItems.AddNew();
			frtLineItem5.TM_Type = Calculator.Items.Operator.Plus;
			frtLineItem5.TM_Break = 1.3m;
			frtLineItem5.TM_RelevantValue = 11.3m;

			var bafLine = rateEntry.AddRateLine("BAF", CartageCalculator.Code, "PK");
			bafLine.ConversionFactor = new ConversionFactor(5000m, Volume.CubicCentimeters, Weight.Kilograms);

			calculator = bafLine.GetCalculator<CartageCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;

			var bafLineItem1 = bafLine.RateLineItems.AddNew();
			bafLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			bafLineItem1.TM_Break = 0.1m;
			bafLineItem1.TM_RelevantValue = 4.6m;
			bafLineItem1.TM_BreakWeightVolume = "KG";

			var bafLineItem2 = bafLine.RateLineItems.AddNew();
			bafLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			bafLineItem2.TM_Break = 0.1m;
			bafLineItem2.TM_RelevantValue = 5.3m;

			var bafLineItem3 = bafLine.RateLineItems.AddNew();
			bafLineItem3.TM_Type = Calculator.Items.Operator.Plus;
			bafLineItem3.TM_Break = 1.0m;
			bafLineItem3.TM_RelevantValue = 12m;

			var bafLineItem4 = bafLine.RateLineItems.AddNew();
			bafLineItem4.TM_Type = Calculator.Items.Operator.Plus;
			bafLineItem4.TM_Break = 1.1m;
			bafLineItem4.TM_RelevantValue = 12.1m;

			var bafLineItem5 = bafLine.RateLineItems.AddNew();
			bafLineItem5.TM_Type = Calculator.Items.Operator.Plus;
			bafLineItem5.TM_Break = 1.2m; // 8 x 17 x 44.5 cm / 5000 = 1.2104 kg > 1.2
			bafLineItem5.TM_RelevantValue = 12.2m; // Targeted/Expected Item

			var bafLineItem6 = bafLine.RateLineItems.AddNew();
			bafLineItem6.TM_Type = Calculator.Items.Operator.Plus;
			bafLineItem6.TM_Break = 1.3m;
			bafLineItem6.TM_RelevantValue = 12.3m;

			#endregion

			#region Setup Shipment

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "HKHKG", 0.3m, 0.006m, consol);
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_ActualChargeable = 1m;
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = PkgUnit.Package;
			shipment.JS_TotalPackageCount = 2;
			shipment.JS_F3_NKTotalCountPackType = PkgUnit.Piece;
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();

			var item = consignment1.Items.AddNew();
			item.HVI_ManifestedWeight = 0.009m;
			item.HVI_ActualWeight = 0.280m;
			item.HVI_ManifestedVolume = 0.006m;
			item.HVI_ActualVolume = 0.006m;
			item.HVI_Height = 8m;
			item.HVI_Width = 17m;
			item.HVI_Length = 44.5m;
			item.HVI_UnitOfDimension = Length.Centimetres;

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 13.44m,
					RevenueCalculationDescription = $@"FRT
{item.HumanReadableName}
	13.44 AUD		1.2 Kilogram(s) @ AUD 11.20/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 12.20m,
					RevenueCalculationDescription = $@"BAF
{item.HumanReadableName}
	12.20 AUD		1 Package(s) @ AUD 12.20/Package"
				}
			};

			AutorateAndAssert("Line unit takes priority to find conversion factors", expected, shipment, client, autorateRevenue: true, autorateCosts: false);
		}

		public void TestHVLVShipmentRating_PrioritiseActualValuesWhenNonEmpty()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);
			SetupBasicRatesForClient(client, RatingConstants.RateCategory.AIR, RateMode.LSE);

			#region Setup Shipment

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();

			var item1 = consignment.Items.AddNew();
			item1.HVI_ManifestedWeight = 50m;
			item1.HVI_ManifestedVolume = 0.3m;

			var item2 = consignment.Items.AddNew();
			item2.HVI_ManifestedWeight = 50m;
			item2.HVI_ManifestedVolume = 0.3m;

			#endregion

			Factory.Save();

			var manifestedExpected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m,
					RevenueCalculationDescription = $@"FRT
{item1.HumanReadableName}
	100.00 AUD		50 Kilogram(s) @ AUD 2.00/KG
{item2.HumanReadableName}
	100.00 AUD		50 Kilogram(s) @ AUD 2.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSSellAmt = 1.8m,
					RevenueCalculationDescription = $@"WAR
{item1.HumanReadableName}
	0.90 AUD		0.3 Cubic Meter(s) @ AUD 3.00/M3
{item2.HumanReadableName}
	0.90 AUD		0.3 Cubic Meter(s) @ AUD 3.00/M3"
				}
			};

			AutorateAndAssert("Pre-Condition - use only manifested values since no actual values", manifestedExpected, shipment, client);

			item1.HVI_ActualWeight = 100m;
			item1.HVI_ActualVolume = 0.04m;

			Factory.Save();

			var actualExpected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m,
					RevenueCalculationDescription = $@"FRT
{item1.HumanReadableName}
	200.00 AUD		100 Kilogram(s) @ AUD 2.00/KG
{item2.HumanReadableName}
	100.00 AUD		50 Kilogram(s) @ AUD 2.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSSellAmt = 2.7m,
					RevenueCalculationDescription = $@"WAR
{item1.HumanReadableName}
	1.80 AUD		0.6 Cubic Meter(s) @ AUD 3.00/M3
{item2.HumanReadableName}
	0.90 AUD		0.3 Cubic Meter(s) @ AUD 3.00/M3"
				}
			};

			AutorateAndAssert("Should use the actual value when it is not 0, but should fallback to manifested otherwise", actualExpected, shipment, client);

			item1.HVI_ActualWeight = 0m;
			item1.HVI_ActualVolume = 0m;

			Factory.Save();

			AutorateAndAssert("Should now reset to the fallback", manifestedExpected, shipment, client);
		}

		public void TestHVLVShipmentRating_MatchByConsignmentLastMileCarrier()
		{
			var fromPostcode = Helper.CreateRefPostCode("2000");
			var toPostcode = Helper.CreateRefPostCode("2100");

			var carrier = Helper.NewOrgHeader();
			var provider = Helper.CreateRateTransportZoneSet(carrier, CountryCodes.Australia, zoneNames: new ZString[] { "Zone 1" });
			provider.Zones[0].CreateRateTransportZoneItemForTest(fromPostcode, toPostcode);

			var costing = Helper.NewCosting(carrier);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, TransportModes.Air, string.Empty, CountryCodes.Australia);

			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["DCART"], CartageZoneDistanceCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine1.GetCalculator<CartageZoneDistanceCalculator>().AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, provider.Zones[0].PK);

			var rateLine2 = rateEntry.AddRateLine(Helper.ChargeCodes["DDOC"], UnitCalculator.Code, PkgUnit.Package);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 15m;

			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);
			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUSYD", 1m);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.HVC_ConsigneePostcode = "2000";
			consignment1.HVC_OH_LastMileCarrier = carrier.PK;

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ManifestedWeight = 1m;
			item1.HVI_F3_NKPackType = PkgUnit.Bag;

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.HVC_ConsigneePostcode = "2001";
			consignment2.HVC_OH_LastMileCarrier = carrier.PK;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ManifestedWeight = 1m;
			item2.HVI_F3_NKPackType = PkgUnit.Package;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSCostAmt = 20m,
					CostCalculationDescription = $@"DCART
{item1.HumanReadableName}
	10.00 AUD		1 Kilogram(s) @ AUD 10.00/KG
{item2.HumanReadableName}
	10.00 AUD		1 Kilogram(s) @ AUD 10.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSCostAmt = 15m,
					CostCalculationDescription = $@"DDOC
{item1.HumanReadableName}
	0.00 AUD		0 Package(s) @ AUD 15.00/Package
{item2.HumanReadableName}
	15.00 AUD		1 Package(s) @ AUD 15.00/Package",
				},
			};

			AutorateAndAssert("Costing should be matched by consignment last mile carrier", expected, shipment, null);
		}

		#region Helpers

		void SetupBasicRatesForClient(OrgHeader client, string category, string mode)
		{
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(category, mode, "USLAX", "AUSYD");

			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, Weight.Kilograms);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 2m;

			var rateLine2 = rateEntry.AddRateLine("WAR", UnitCalculator.Code, Volume.CubicMetres);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 3m;
		}

		#endregion
	}
}
