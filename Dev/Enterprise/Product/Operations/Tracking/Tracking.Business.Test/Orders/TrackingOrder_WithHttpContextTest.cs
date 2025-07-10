using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business
{
	[HttpContextEnabledTest]
	sealed class TrackingOrder_WithHttpContextTest : Freight.Forwarding.Orders.Business.Testing.OrderTest
	{
		#region TestLookups

		public void TestGetLookups()
		{
			Assert("Should return lookups of TrackingOrder", TestTrackingOrder.Lookups is TrackingOrderLookups);
		}

		#endregion TestLookups

		#region TestShipDec

		public void TestShipDec()
		{
			AssertEquals("No shipment attached", null, TestTrackingOrder.Shipment);
			AssertEquals("No declaration attached", null, TestTrackingOrder.Declaration);
			AssertEquals("No shipment or declaration attached", null, TestTrackingOrder.ShipOrDec);

			TestTrackingOrder.JD_JS = Shipment.PK;

			AssertSame("Shipment is attached", Shipment, TestTrackingOrder.Shipment);
			AssertEquals("No declaration attached", null, TestTrackingOrder.Declaration);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;

			AssertSame("Declaration is attached", Declaration, TestTrackingOrder.Declaration);
			AssertEquals("No shipment attached", null, TestTrackingOrder.Shipment);
		}

		#endregion TestShipDec

		#region Test Volume

		public void TestJD_UnitOfVolume()
		{
			TestTrackingOrder.JD_UnitOfVolume = "KG";
			AssertEquals("Unit of volume is taken from Planning tab", "KG", TestTrackingOrder.JD_UnitOfVolume);

			TestTrackingOrder.JD_JS = Shipment.PK;
			Shipment.JS_UnitOfVolume = "G";
			AssertEquals("Unit of volume is not taken from Shipment", "KG", TestTrackingOrder.JD_UnitOfVolume);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;
			Declaration.JE_TotalVolumeUnit = "L";
			AssertEquals("Unit of volume is not taken from Declaration", "KG", TestTrackingOrder.JD_UnitOfVolume);
		}

		public void TestVolume()
		{
			TestTrackingOrder.JD_ActualVolume = 100;
			AssertEquals("volume is taken from Planning tab", 100m, TestTrackingOrder.JD_ActualVolume);

			TestTrackingOrder.JD_JS = Shipment.PK;
			Shipment.JS_ActualVolume = 200;
			AssertEquals("volume is not taken from Shipment", 100m, TestTrackingOrder.JD_ActualVolume);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;
			Declaration.JE_TotalVolume = 300m;
			AssertEquals("volume is not taken from Declaration", 100m, TestTrackingOrder.JD_ActualVolume);
		}

		public void TestVolumeWithUnits()
		{
			TestTrackingOrder.JD_ActualVolume = 100;
			TestTrackingOrder.JD_UnitOfVolume = "M3";
			AssertEquals("100.000 M3", TestTrackingOrder.JD_ActualVolumeWithUnits);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			TestTrackingOrder.JD_TransportMode = Core.Constants.TransportModes.Sea;

			TestTrackingOrder.JD_ActualVolume = 12.128;
			TestTrackingOrder.JD_UnitOfVolume = "M3";
			AssertEquals("12.12 M3", TestTrackingOrder.JD_ActualVolumeWithUnits);
		}

		#endregion Test Volume

		#region Test Order Number And Split

		public void TestJD_OrderNumberAndSplit()
		{
			AssertEquals("Order Number Split is 0", (byte)0, TestTrackingOrder.JD_OrderNumberSplit);
			AssertEquals("ReadOnly is false", false, TestTrackingOrder.JD_OrderNumberInfo.ReadOnly);
			TestTrackingOrder.JD_OrderNumber = "10";
			AssertEquals("Order Number is assigned to 10", "10", TestTrackingOrder.JD_OrderNumber);
			AssertEquals("Order Number And Split is also 10", "10", TestTrackingOrder.JD_OrderNumberAndSplit);

			TestTrackingOrder.JD_OrderNumberSplit = 1;
			AssertEquals("Order Number Split is 1", (byte)1, TestTrackingOrder.JD_OrderNumberSplit);
			AssertEquals("ReadOnly is true", true, TestTrackingOrder.JD_OrderNumberInfo.ReadOnly);
			AssertEquals("OrderNumberAndSplit should be with split postfix", "10-1", TestTrackingOrder.JD_OrderNumberAndSplit);
		}

		public void TestJD_OrderNumberAndSplitAfterSplitting()
		{
			TestTrackingOrder.JD_OrderNumber = "10";
			OrgHeader org = TestTrackingOrder.Factory.LoadTop1<OrgHeader>(new ZQuery());
			TestTrackingOrder.BuyerPK = org.PK;
			TestTrackingOrder.SupplierPK = org.PK;
			OrderLine line = TestTrackingOrder.OrderLines.AddNew();

			AssertEquals("BaseOrder JD_OrderNumber should not be readonly", false, TestTrackingOrder.JD_OrderNumberInfo.ReadOnly);

			TrackingOrder split = (TrackingOrder)TestTrackingOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split Order Number Split is 1", (byte)1, split.JD_OrderNumberSplit);
			AssertEquals("Split Order JD_OrderNumber should be readonly", true, split.JD_OrderNumberInfo.ReadOnly);
			AssertEquals("Split Order JD_OrderNumberAndSplit should be readonly", true, split.JD_OrderNumberAndSplitInfo.ReadOnly);
			AssertEquals("OrderNumberAndSplit should be with split postfix", "10-1", split.JD_OrderNumberAndSplit);

			AssertEquals("BaseOrder JD_OrderNumberInfo should be readonly", true, TestTrackingOrder.JD_OrderNumberInfo.ReadOnly);
			AssertEquals("BaseOrder JD_OrderNumberAndSplit should be readonly", true, TestTrackingOrder.JD_OrderNumberAndSplitInfo.ReadOnly);
			AssertEquals("Order Number Split is 0", (byte)0, TestTrackingOrder.JD_OrderNumberSplit);
			AssertEquals("OrderNumberAndSplit should be without split postfix", "10", TestTrackingOrder.JD_OrderNumberAndSplit);
		}

		public void TestOrderSplitPreservesValuesForOverrides()
		{
			TrackingOrder order = (TrackingOrder)NewOrder();

			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "UAIEV";

			order.JD_RL_NKGoodsAvailableAt = "AUBNE";
			order.JD_RL_NKGoodsDeliveredTo = "UAKBP";
			order.JD_OH_SendingAgent = TestOrganisation1.PK;
			order.JD_OH_ReceivingAgent = TestOrganisation2.PK;
			order.JD_Waybill = "1234567";
			order.JD_MasterWaybill = "7654321";

			// Imitating scenario when user attaches the order to a shipment and split.
			// Attaching to shipment will make overrides in TrackingOrder to retrieve their values from shipment instead 
			order.JD_JS = Shipment.PK;
			TrackingOrder splitOrder = (TrackingOrder)order.SplitOrder(CreateOrderType.Split);

			// overrides on split order should match the values on intial order, not the ones retreived from the shipment
			AssertEquals(splitOrder.JD_RL_NKPortOfLoading, "AUSYD");
			AssertEquals(splitOrder.JD_RL_NKPortOfDischarge, "UAIEV");

			AssertEquals(splitOrder.JD_RL_NKGoodsAvailableAt, "AUBNE");
			AssertEquals(splitOrder.JD_RL_NKGoodsDeliveredTo, "UAKBP");

			AssertEquals(splitOrder.JD_OH_SendingAgent, TestOrganisation1.PK);
			AssertEquals(splitOrder.JD_OH_ReceivingAgent, TestOrganisation2.PK);

			AssertEquals(splitOrder.JD_Waybill, "1234567");
			AssertEquals(splitOrder.JD_MasterWaybill, "7654321");
		}

		#endregion Test Order Number

		#region Test Weight

		public void TestUnitOfWeight()
		{
			TestTrackingOrder.JD_UnitOfWeight = "KG";
			AssertEquals("Unit of weight is taken from Planning tab", "KG", TestTrackingOrder.JD_UnitOfWeight);

			TestTrackingOrder.JD_JS = Shipment.PK;
			Shipment.JS_UnitOfWeight = "G";
			AssertEquals("Unit of weight is not taken from Shipment", "KG", TestTrackingOrder.JD_UnitOfWeight);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;
			Declaration.JE_TotalWeightUnit = "L";
			AssertEquals("Unit of weight is not taken from Declaration", "KG", TestTrackingOrder.JD_UnitOfWeight);
		}

		public void TestWeight()
		{
			TestTrackingOrder.JD_ActualWeight = 100;
			AssertEquals("weight is taken from Planning tab", 100m, TestTrackingOrder.JD_ActualWeight);

			TestTrackingOrder.JD_JS = Shipment.PK;
			Shipment.JS_ActualWeight = 200;
			AssertEquals("weight is not taken from Shipment", 100m, TestTrackingOrder.JD_ActualWeight);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;
			Declaration.JE_TotalWeight = 300;
			AssertEquals("weight is not taken from Declaration", 100m, TestTrackingOrder.JD_ActualWeight);
		}

		public void TestWeightWithUnits()
		{
			TestTrackingOrder.JD_ActualWeight = 100;
			TestTrackingOrder.JD_UnitOfWeight = "KG";
			AssertEquals("100.000 KG", TestTrackingOrder.JD_ActualWeightWithUnits);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			TestTrackingOrder.JD_TransportMode = Core.Constants.TransportModes.Sea;

			TestTrackingOrder.JD_ActualWeight = 12.251;
			TestTrackingOrder.JD_UnitOfWeight = "KG";
			AssertEquals("12.26 KG", TestTrackingOrder.JD_ActualWeightWithUnits);
		}

		#endregion Test Weight

		#region Test Packs

		public void TestPacksType()
		{
			TestTrackingOrder.JD_F3_NKPackType = "KG";
			AssertEquals("Unit of weight is taken from Planning tab", "KG", TestTrackingOrder.JD_F3_NKPackType);

			TestTrackingOrder.JD_JS = Shipment.PK;
			Shipment.JS_F3_NKPackType = "G";
			AssertEquals("Unit of weight is not taken from Shipment", "KG", TestTrackingOrder.JD_F3_NKPackType);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;
			Declaration.JE_TotalNoOfPacksPackType = "L";
			AssertEquals("Unit of weight is not taken from Declaration", "KG", TestTrackingOrder.JD_F3_NKPackType);
		}

		public void TestPacks()
		{
			TestTrackingOrder.JD_Packs = 100;
			AssertEquals("weight is taken from Planning tab", 100, TestTrackingOrder.JD_Packs);

			TestTrackingOrder.JD_JS = Shipment.PK;
			Shipment.JS_OuterPacks = 200;
			AssertEquals("weight is not taken from Shipment", 100, TestTrackingOrder.JD_Packs);

			TestTrackingOrder.JD_JS = ZGuid.Empty;
			TestTrackingOrder.JD_JE = Declaration.PK;
			Declaration.JE_TotalNoOfPacks = 300;
			AssertEquals("weight is not taken from Declaration", 100, TestTrackingOrder.JD_Packs);
		}

		public void TestJD_PacksWithUnits()
		{
			TestTrackingOrder.JD_Packs = 100;
			TestTrackingOrder.JD_F3_NKPackType = "KG";
			AssertEquals("100 KG", TestTrackingOrder.JD_PacksWithUnits);
		}

		#endregion Test Packs

		#region Test HouseBill

		public void TestJD_WayBillShipment()
		{
			AssertEquals("ShipmentDeclaration should initially be null", null, TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_Waybill = "test";
			AssertEquals("HouseBill should be taken from base JD_WayBill for null ShipmentDeclarations", "test", TestTrackingOrder.JD_Waybill);

			Shipment.JS_HouseBill = "Shipment";
			Factory.Save();

			TestTrackingOrder.JD_JS = Shipment.PK;
			AssertEquals("HouseBill should be taken from Shipment", Shipment.JS_HouseBill, TestTrackingOrder.JD_Waybill);
		}

		public void TestJDWayBillDeclaration()
		{
			AssertEquals("ShipmentDeclaration should initially be null", null, TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_Waybill = "test";
			AssertEquals("HouseBill should be taken from base JD_WayBill for null ShipmentDeclarations", "test", TestTrackingOrder.JD_Waybill);

			Declaration.JE_HouseBill = "Declaration";
			Factory.Save();

			TestTrackingOrder.JD_JE = Declaration.PK;
			AssertEquals("HouseBill should be taken from Declaration", Declaration.JE_HouseBill, TestTrackingOrder.JD_Waybill);
		}

		#endregion Test HouseBill

		#region Locations

		public void TestJD_RL_NKPortOfLoadingWithNoConsols()
		{
			AssertNull("ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Port of Loading should be used", "AUSYD", TestTrackingOrder.JD_RL_NKPortOfLoading);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			TestTrackingOrder.JD_JS = shipment.PK;
			AssertEquals("Shipment has no load port defined", ZString.Empty, TestTrackingOrder.JD_RL_NKPortOfLoading);
		}

		public void TestJD_RL_NKPortOfDischargeWithNoConsols()
		{
			AssertNull("ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("Port of Discharge should be used", "AUSYD", TestTrackingOrder.JD_RL_NKPortOfDischarge);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			TestTrackingOrder.JD_JS = shipment.PK;

			AssertEquals("Shipment has no discharge port defined", ZString.Empty, TestTrackingOrder.JD_RL_NKPortOfDischarge);
		}

		public void TestJD_RL_NKPortOfLoadingWithShipment()
		{
			AssertNull("ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Port of Loading should be used", "AUSYD", TestTrackingOrder.JD_RL_NKPortOfLoading);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.Consols.AddNew().JK_RL_NKLoadPort = "AUMEL";
			Factory.Save();

			TestTrackingOrder.JD_JS = shipment.PK;

			AssertEquals("Destination port name should come from Shipment/Declaration", "AUMEL", TestTrackingOrder.JD_RL_NKPortOfLoading);
		}

		public void TestJD_RL_NKPortOfDischargeWithShipment()
		{
			AssertNull("ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("Port of Discharge should be used", "AUSYD", TestTrackingOrder.JD_RL_NKPortOfDischarge);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.Consols.AddNew().JK_RL_NKDischargePort = "AUMEL";
			Factory.Save();

			TestTrackingOrder.JD_JS = shipment.PK;

			AssertEquals("Destination port name should come from Shipment/Declaration", "AUMEL", TestTrackingOrder.JD_RL_NKPortOfDischarge);
		}

		public void TestJD_RL_NKGoodsAvailableAtWithShipment()
		{
			AssertNull("ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_RL_NKGoodsAvailableAt = "AUSYD";
			AssertEquals("Port of Discharge should be used", "AUSYD", TestTrackingOrder.JD_RL_NKGoodsAvailableAt);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RL_NKOrigin = "AUMEL";
			TestTrackingOrder.shipOrDec = shipment;

			AssertEquals("Destination port name should come from Shipment/Declaration", "AUMEL", TestTrackingOrder.JD_RL_NKGoodsAvailableAt);
		}

		public void TestJD_RL_NKGoodsDeliveredToWithShipment()
		{
			AssertNull("ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);
			TestTrackingOrder.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			AssertEquals("Port of Discharge should be used", "AUSYD", TestTrackingOrder.JD_RL_NKGoodsDeliveredTo);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RL_NKDestination = "AUMEL";
			TestTrackingOrder.shipOrDec = shipment;

			AssertEquals("Destination port name should come from Shipment/Declaration", "AUMEL", TestTrackingOrder.JD_RL_NKGoodsDeliveredTo);
		}

		#endregion

		#region TestShipmentOrDeclarationProperties

		public void TestActualVolumeAndWeightWhenOrderAttachedToShipmentOrDeclaration()
		{
			TestTrackingOrder.JD_ActualVolume = 0.25;
			TestTrackingOrder.JD_UnitOfVolume = "M3";

			TestTrackingOrder.JD_ActualWeight = 7.125;
			TestTrackingOrder.JD_UnitOfWeight = "KG";

			Factory.Save();

			AssertEquals("Volume", "0.250 M3", TestTrackingOrder.JD_ActualVolumeWithUnits);
			AssertEquals("Weight", "7.125 KG", TestTrackingOrder.JD_ActualWeightWithUnits);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_HouseBill = "W2";
			shipment.JS_RL_NKOrigin = "KRASA";
			shipment.JS_RL_NKDestination = "KRBAA";

			shipment.JS_ActualVolume = 1.314;
			shipment.JS_UnitOfVolume = "L";

			shipment.JS_ActualWeight = 0.123;
			shipment.JS_UnitOfWeight = "T";

			TestTrackingOrder.JD_JS = shipment.PK;

			Factory.Save();

			AssertEquals("Volume", "1.314 L", TestTrackingOrder.JD_ActualVolumeWithUnits);
			AssertEquals("Weight", "0.123 T", TestTrackingOrder.JD_ActualWeightWithUnits);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			TestTrackingOrder.JD_JE = declaration.PK;

			declaration.JE_TotalVolume = 1.234;
			declaration.JE_TotalVolumeUnit = "ML";

			declaration.JE_TotalWeight = 0.245;
			declaration.JE_TotalWeightUnit = "LB";

			Factory.Save();

			AssertEquals("Volume", "1.234 ML", TestTrackingOrder.JD_ActualVolumeWithUnits);
			AssertEquals("Weight", "0.245 LB", TestTrackingOrder.JD_ActualWeightWithUnits);
		}

		public void TestShipmentOrDeclarationOrPlannedOrderProperties()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				AssertNull("Precondition: ShipmentDeclaration should initially be null", TestTrackingOrder.ShipOrDec);

				TestTrackingOrder.JD_Waybill = "W1";
				TestTrackingOrder.JD_MasterWaybill = "MW1";

				TestTrackingOrder.JD_RL_NKPortOfLoading = "AUFRE";
				TestTrackingOrder.JD_RL_NKPortOfDischarge = "AUMEL";

				TestTrackingOrder.JD_RL_NKGoodsAvailableAt = "AUZBO";
				TestTrackingOrder.JD_RL_NKGoodsDeliveredTo = "AUSYD";

				TestTrackingOrder.JD_OH_SendingAgent = TestOrganisation1.PK;
				TestTrackingOrder.JD_OH_ReceivingAgent = TestOrganisation2.PK;

				TestTrackingOrder.JD_Packs = 10;
				TestTrackingOrder.JD_F3_NKPackType = "PLT";

				TestTrackingOrder.JD_ActualVolume = 0.25;
				TestTrackingOrder.JD_UnitOfVolume = "M3";

				TestTrackingOrder.JD_ActualWeight = 7.125;
				TestTrackingOrder.JD_UnitOfWeight = "KG";

				TestTrackingOrder.JD_ArrivalVoyage = "Voyage1";
				TestTrackingOrder.JD_RV_NKArrivalVessel = "Vessel1";

				AssertNull("Precondition: WebShipment should initially be null", TestTrackingOrder.ShipOrDec);

				AssertShipmentOrDeclarationOrPlannedOrderProperties("W1", "MW1", "AUFRE", "AUMEL", "AUZBO", "AUSYD",
																	TestOrganisation1.PK, TestOrganisation2.PK, "Voyage1", "Vessel1", "10 PLT", "0.250 M3", "7.125 KG", "10 PLT", "0.250 M3", "7.125 KG");

				TrackingShipment shipment = Factory.New<TrackingShipment>();
				shipment.JS_HouseBill = "W2";
				shipment.JS_RL_NKOrigin = "KRASA";
				shipment.JS_RL_NKDestination = "KRBAA";

				shipment.JS_OuterPacks = 11;
				shipment.JS_F3_NKPackType = "PKG";

				shipment.JS_ActualVolume = 1.314;
				shipment.JS_UnitOfVolume = "L";

				shipment.JS_ActualWeight = 0.123;
				shipment.JS_UnitOfWeight = "T";

				TrackingConsol consol = shipment.Consols.AddNew();
				consol.JK_RL_NKLoadPort = "KRINC";
				consol.JK_RL_NKDischargePort = "KRSEL";
				consol.JK_MasterBillNum = "MW2";
				consol.SetDefaultSendingForwarderAddress(TestOrganisation2);
				consol.SetDefaultReceivingForwarderAddress(TestOrganisation1);

				if (consol.Transports.Count == 0)
				{
					consol.Transports.AddNew();
				}

				consol.Transports[0].JW_VoyageFlight = "Voyage2";
				consol.Transports[0].JW_Vessel = "Vessel2";

				TestTrackingOrder.JD_JS = shipment.PK;
				Factory.Save();

				AssertShipmentOrDeclarationOrPlannedOrderProperties("W2", "MW2", "KRINC", "KRSEL", "KRASA", "KRBAA",
																	TestOrganisation2.PK, TestOrganisation1.PK, "Voyage2", "Vessel2", "10 PLT", "1.314 L", "0.123 T", "11 PKG", "1.314 L", "0.123 T");
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		void AssertShipmentOrDeclarationOrPlannedOrderProperties(
		ZString expectedHouseBill,
		ZString expectedMasterBill,
		ZString expectedPortOfLoading,
		ZString expectedPortOfDischarge,
		ZString expectedOrigin,
		ZString expectedDestination,
		ZGuid expectedSendingAgent,
		ZGuid expectedReceivingAgent,
		ZString expectedVoyage,
		ZString expectedVessel,
		ZString expectedPacks,
		ZString expectedVolume,
		ZString expectedWeight,
		ZString expectedShipDecPacks,
		ZString expectedShipDecVolume,
		ZString expectedShipDecWeight)
		{
			AssertEquals("House Bill", expectedHouseBill, TestTrackingOrder.JD_Waybill);
			AssertEquals("Master Bill", expectedMasterBill, TestTrackingOrder.JD_MasterWaybill);

			AssertEquals("Port of Loading", expectedPortOfLoading, TestTrackingOrder.JD_RL_NKPortOfLoading);
			AssertEquals("Port of Discharge", expectedPortOfDischarge, TestTrackingOrder.JD_RL_NKPortOfDischarge);

			AssertEquals("Origin", expectedOrigin, TestTrackingOrder.JD_RL_NKGoodsAvailableAt);
			AssertEquals("Destination", expectedDestination, TestTrackingOrder.JD_RL_NKGoodsDeliveredTo);

			AssertEquals("Sending Agent", expectedSendingAgent, TestTrackingOrder.JD_OH_SendingAgent);
			AssertEquals("Receiving Agent", expectedReceivingAgent, TestTrackingOrder.JD_OH_ReceivingAgent);

			AssertEquals("Voyage", expectedVoyage, TestTrackingOrder.MainVoyageWithSuppression);
			AssertEquals("Vessel", expectedVessel, TestTrackingOrder.MainVessel);

			AssertEquals("Packs", expectedPacks, TestTrackingOrder.JD_PacksWithUnits);
			AssertEquals("Weight", expectedWeight, TestTrackingOrder.JD_ActualWeightWithUnits);
			AssertEquals("Volume", expectedVolume, TestTrackingOrder.JD_ActualVolumeWithUnits);

			AssertEquals("Packs", expectedShipDecPacks, TestTrackingOrder.ShipDecPacksWithUnits);
			AssertEquals("Weight", expectedShipDecWeight, TestTrackingOrder.ShipDecActualWeightWithUnits);
			AssertEquals("Volume", expectedShipDecVolume, TestTrackingOrder.ShipDecActualVolumeWithUnits);
		}

		#endregion

		#region Organisation

		public void TestJD_OH_SendingAgent()
		{
			AssertNull("ShipmentDeclaration bizO should initially be null", TestTrackingOrder.ShipOrDec);

			TestTrackingOrder.JD_OH_SendingAgent = TestOrganisation1.PK;
			AssertEquals("Order's sending agent should be used", TestOrganisation1.PK, TestTrackingOrder.JD_OH_SendingAgent);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.Consols.AddNew().SetDefaultSendingForwarderAddress(TestOrganisation2);
			Factory.Save();
			TestTrackingOrder.shipOrDec = shipment;

			AssertEquals("Sending agent should come from shipment", TestOrganisation2.PK, TestTrackingOrder.JD_OH_SendingAgent);
		}

		public void TestJD_OH_ReceivingAgent()
		{
			AssertNull("ShipmentDeclaration bizO should initially be null", TestTrackingOrder.ShipOrDec);

			TestTrackingOrder.JD_OH_ReceivingAgent = TestOrganisation1.PK;
			AssertEquals("Order's receiving agent should be used", TestOrganisation1.PK, TestTrackingOrder.JD_OH_ReceivingAgent);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.Consols.AddNew().SetDefaultReceivingForwarderAddress(TestOrganisation2);
			Factory.Save();
			TestTrackingOrder.shipOrDec = shipment;

			AssertEquals("Receiving agent should come from shipment", TestOrganisation2.PK, TestTrackingOrder.JD_OH_ReceivingAgent);
		}

		#endregion

		public void TestOrderSendingAgent_EditableMilestones()
		{
			var contact = TestOrganisation1.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.SetHashedPassword("test");
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			WebEnv.AppInstance.SiteUser.Login(TestOrganisation1.OH_Code, contact.OC_Email, contact.PasswordForTesting);

			var milestoneEventUpdates = new OrderMilestoneEventUpdatesCollection
				{
					new OrderMilestoneEventUpdates
					{
						EventType = "AAA",
						IsSendingAgentUpdateAllowed = true,
					}
				};

			using (WebDataRegistry.Instance.OrderMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, milestoneEventUpdates))
			{
				TestTrackingOrder.JD_OH_SendingAgent = TestOrganisation1.PK;

				var shipment = Factory.New<TrackingShipment>();
				shipment.Consols.AddNew().SetDefaultSendingForwarderAddress(TestOrganisation2);
				var milestone = TestTrackingOrder.WorkflowItems.Milestones.AddNew();
				milestone.P9_Description = "Milestone";
				milestone.TriggerConditions.TriggerEventCode = "AAA";
				milestone.P9_IsPublished = true;
				TestTrackingOrder.shipOrDec = shipment;
				Factory.Save();

				AssertEquals(1, TestTrackingOrder.EditableMilestones.Count);
			}
		}

		public void TestOrderReceivingAgent_EditableMilestones()
		{
			var contact = TestOrganisation1.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var password = "test";
			contact.SetHashedPassword(password);
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			WebEnv.AppInstance.SiteUser.Login(TestOrganisation1.OH_Code, contact.OC_Email, password);

			var milestoneEventUpdates = new OrderMilestoneEventUpdatesCollection
				{
					new OrderMilestoneEventUpdates
					{
						EventType = "AAA",
						IsReceivingAgentUpdateAllowed = true,
					}
				};

			using (WebDataRegistry.Instance.OrderMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, milestoneEventUpdates))
			{
				TestTrackingOrder.JD_OH_ReceivingAgent = TestOrganisation1.PK;

				var shipment = Factory.New<TrackingShipment>();
				shipment.Consols.AddNew().SetDefaultReceivingForwarderAddress(TestOrganisation2);
				var milestone = TestTrackingOrder.WorkflowItems.Milestones.AddNew();
				milestone.P9_Description = "Milestone";
				milestone.TriggerConditions.TriggerEventCode = "AAA";
				milestone.P9_IsPublished = true;
				TestTrackingOrder.shipOrDec = shipment;
				Factory.Save();

				AssertEquals(1, TestTrackingOrder.EditableMilestones.Count);
			}
		}

		#region Custom Tracking Dates

		public void TestGetCustomTrackingDates()
		{
			Assert("Custom estimate date field names must start with JD_EstimateUserDate", AutoJobOrderHeader.Schema.JD_EstimateUserDate1.StartsWith("JD_EstimateUserDate"));
			Assert("Custom estimate date field names must start with JD_EstimateUserDate", AutoJobOrderHeader.Schema.JD_EstimateUserDate2.StartsWith("JD_EstimateUserDate"));
			Assert("Custom estimate date field names must start with JD_EstimateUserDate", AutoJobOrderHeader.Schema.JD_EstimateUserDate3.StartsWith("JD_EstimateUserDate"));
			Assert("Custom estimate date field names must start with JD_EstimateUserDate", AutoJobOrderHeader.Schema.JD_EstimateUserDate4.StartsWith("JD_EstimateUserDate"));

			Assert("Custom actual data field names must start with JD_ActualUserDate", AutoJobOrderHeader.Schema.JD_ActualUserDate1.StartsWith("JD_ActualUserDate"));
			Assert("Custom actual data field names must start with JD_ActualUserDate", AutoJobOrderHeader.Schema.JD_ActualUserDate2.StartsWith("JD_ActualUserDate"));
			Assert("Custom actual data field names must start with JD_ActualUserDate", AutoJobOrderHeader.Schema.JD_ActualUserDate3.StartsWith("JD_ActualUserDate"));
			Assert("Custom actual data field names must start with JD_ActualUserDate", AutoJobOrderHeader.Schema.JD_ActualUserDate4.StartsWith("JD_ActualUserDate"));

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test organization";
			testOrg.OH_Code = "TST";

			OrgCustomLabels customLabels1 = testOrg.CustomLabels.AddNew();
			customLabels1.OT_FieldName = "OrderHeader.UserTrackDate1";
			customLabels1.OT_Caption = "Test date 1";

			OrgCustomLabels customLabels2 = testOrg.CustomLabels.AddNew();
			customLabels2.OT_FieldName = "OrderHeader.UserTrackDate2";
			customLabels2.OT_Caption = "Test date 2";

			OrgCustomLabels customLabels3 = testOrg.CustomLabels.AddNew();
			customLabels3.OT_FieldName = "OrderHeader.UserTrackDate3";
			customLabels3.OT_Caption = "Test date 3";

			OrgCustomLabels customLabels4 = testOrg.CustomLabels.AddNew();
			customLabels4.OT_FieldName = "OrderHeader.UserTrackDate4";
			customLabels4.OT_Caption = "Test date 4";

			AssertEquals("There must be 4 custom labels (created for testing)", 4, testOrg.CustomLabels.Count);

			TrackingOrder testOrder = Factory.New<TrackingOrder>();
			testOrder.BuyerPK = testOrg.PK;
			CustomLabelInfoList orderCustomLabels = testOrder.GetAdditionalInformationFields();

			AssertEquals("All 8 custom labels must be retrieved.", 8, orderCustomLabels.Count);

			Assert("All custom labels created must be retrieved.", orderCustomLabels.Contains(AutoJobOrderHeader.Schema.JD_EstimateUserDate1));
			CustomLabelInfo orderCustomLabel1 = (CustomLabelInfo)orderCustomLabels.GetFieldByPropertyName(AutoJobOrderHeader.Schema.JD_EstimateUserDate1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", "Estimated " + customLabels1.OT_Caption, orderCustomLabel1.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", customLabels1.OT_FieldName, orderCustomLabel1.LabelName);

			Assert("All custom labels created must be retrieved.", orderCustomLabels.Contains(AutoJobOrderHeader.Schema.JD_ActualUserDate1));
			CustomLabelInfo orderCustomLabel2 = (CustomLabelInfo)orderCustomLabels.GetFieldByPropertyName(AutoJobOrderHeader.Schema.JD_ActualUserDate1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", "Actual " + customLabels1.OT_Caption, orderCustomLabel2.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", customLabels1.OT_FieldName, orderCustomLabel2.LabelName);
		}

		#endregion

		#region Additional Information Fields test

		public void TestGetAdditionalInformationFields()
		{
			TrackingOrder testOrder = Factory.New<TrackingOrder>();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test organization";
			testOrg.OH_Code = "TST";

			OrgCustomLabels orgCustomLabel3 = testOrg.CustomLabels.AddNew();
			orgCustomLabel3.OT_FieldName = "OrderHeader.CustomContact1";
			orgCustomLabel3.OT_Caption = "Custom Contact 1 Test";

			OrgCustomLabels orgCustomLabel4 = testOrg.CustomLabels.AddNew();
			orgCustomLabel4.OT_FieldName = "OrderHeader.CustomContact2";
			orgCustomLabel4.OT_Caption = "Custom Contact 2 Test";

			OrgCustomLabels orgCustomLabel5 = testOrg.CustomLabels.AddNew();
			orgCustomLabel5.OT_FieldName = "OrderHeader.CustomAttrib1";
			orgCustomLabel5.OT_Caption = "Custom Attribute 1 Test";

			OrgCustomLabels orgCustomLabel6 = testOrg.CustomLabels.AddNew();
			orgCustomLabel6.OT_FieldName = "OrderHeader.CustomFlag1";
			orgCustomLabel6.OT_Caption = "Custom Flag 1 Test";

			AssertEquals("There must be 4 custom labels (created for testing).", 4, testOrg.CustomLabels.Count);

			testOrder.BuyerPK = testOrg.PK;
			CustomLabelInfoList customLabels = testOrder.GetAdditionalInformationFields();

			AssertEquals("All 4 custom labels must be retrieved.", 4, customLabels.Count);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoJobOrderHeader.Schema.JD_CustomAttrib1));
			CustomLabelInfo customLabel1 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoJobOrderHeader.Schema.JD_CustomAttrib1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel5.OT_Caption, customLabel1.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel5.OT_FieldName, customLabel1.LabelName);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoJobOrderHeader.Schema.JD_CustomFlag1));
			CustomLabelInfo customLabel2 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoJobOrderHeader.Schema.JD_CustomFlag1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel6.OT_Caption, customLabel2.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel6.OT_FieldName, customLabel2.LabelName);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoJobOrderHeader.Schema.JD_FirstBuyerContact));
			CustomLabelInfo customLabel3 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoJobOrderHeader.Schema.JD_FirstBuyerContact);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel3.OT_Caption, customLabel3.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel3.OT_FieldName, customLabel3.LabelName);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoJobOrderHeader.Schema.JD_SecondBuyerContact));
			CustomLabelInfo customLabel4 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoJobOrderHeader.Schema.JD_SecondBuyerContact);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel4.OT_Caption, customLabel4.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel4.OT_FieldName, customLabel4.LabelName);
		}

		#endregion

		#region Setup

		TrackingOrder fTestTrackingOrder;
		TrackingOrder TestTrackingOrder
		{
			get
			{
				if (fTestTrackingOrder == null)
				{
					fTestTrackingOrder = (TrackingOrder)NewOrder();
				}
				return fTestTrackingOrder;
			}
		}

		TrackingShipment fShipment;
		TrackingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<TrackingShipment>();
				}
				return fShipment;
			}
		}

		BaseJobDeclaration fDeclaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}

				return fDeclaration;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			fTestTrackingOrder = null;
			fShipment = null;
			fDeclaration = null;
		}

		OrgHeader fTestOrganisation1;
		OrgHeader TestOrganisation1
		{
			get
			{
				if (fTestOrganisation1 == null)
				{
					fTestOrganisation1 = CreateNewOrg(Factory, "Org1");
				}

				return fTestOrganisation1;
			}
		}

		OrgHeader fTestOrganisation2;
		OrgHeader TestOrganisation2
		{
			get
			{
				if (fTestOrganisation2 == null)
				{
					fTestOrganisation2 = CreateNewOrg(Factory, "Org2");
				}

				return fTestOrganisation2;
			}
		}

		protected override Order NewOrder()
		{
			OrgHeader buyer = CreateNewOrg(Factory, "TBY");
			OrgHeader supplier = CreateNewOrg(Factory, "TSP");

			Order result = Factory.New<TrackingOrder>();
			result.BuyerPK = buyer.PK;
			result.SupplierPK = supplier.PK;
			result.JD_OrderNumber = "1234567890";

			return result;
		}

		#endregion
	}
}
