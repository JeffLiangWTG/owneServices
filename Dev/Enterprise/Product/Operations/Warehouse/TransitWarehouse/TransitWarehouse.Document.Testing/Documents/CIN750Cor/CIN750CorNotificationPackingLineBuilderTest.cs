using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	class CIN750CorNotificationPackingLineBuilderTest : DocPackingLineBuilderTest
	{
		public void TestPopulatePackingLines_BaseProperty()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			packageState2.WPS_AdjustedOut = "OTH";
			packageState2.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState2.WPS_ReceivedAs = TransitWarehouseReceiveAs.Codes.PackedPackline;

			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			packageState3.WPS_AdjustedOut = "OTH";
			packageState3.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState3.Package.KP_GoodsDescription = "A";

			var overpack = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);
			overpack.WPS_AdjustedOut = "OTH";
			overpack.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			var childPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -2, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -(ZDecimal)5, corNotification.Goods.First().AmountWeight);
			AssertEquals("Description", "A", corNotification.Goods.First().Description);
			AssertEquals("TemporaryStorageDeclaration", "123", corNotification.Goods.First().TemporaryStorageDeclaration);
		}

		public void TestPopulatePackingLines_ConvertUQ()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1, weightUQ: Core.Constants.Weight.Hectograms);
			packageState1.WPS_AdjustedOut = "OTH";
			packageState1.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1, weightUQ: Core.Constants.Weight.Tonnes);
			packageState2.WPS_AdjustedOut = "OTH";
			packageState2.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState2.Package.KP_GoodsDescription = "A";
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -2, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -8.8m, corNotification.Goods.First().AmountWeight);
			AssertEquals("Description", "A", corNotification.Goods.First().Description);
			AssertEquals("TemporaryStorageDeclaration", "123", corNotification.Goods.First().TemporaryStorageDeclaration);
		}

		public void TestPopulatePackingLines_PKLWasUnloadedToOVPThenAdjustedOut()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var packline = Helper.CreatePackageState(rcn, 5, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, unitType: "PKL", weight: 8.8m);
			packline.Package.KP_GoodsDescription = "SHOES";
			var ovpPackageState = Helper.CreateOverpackPackage("OVP-1", rcn, null, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn);
			ovpPackageState.Package.KP_GoodsDescription = "A";

			Helper.PackPackageIntoHandlingUnit(ovpPackageState, packline, ZDateTimeOffset.Now, "LWC", ovpPackageState);

			ovpPackageState.WPS_AdjustedOut = "OTH";
			ovpPackageState.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;

			packline.WPS_AdjustedOut = "OTH";
			packline.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;

			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -5, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -8.8m, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_PKLWasUnloadedToHUThenAdjustedOut() => TestPopulatePackingLines_PKLWasUnloadedToHUThenAdjustedOutCore(false);

		public void TestPopulatePackingLines_PKLWasUnloadedToHUViaSkipScanModeThenAdjustedOut() => TestPopulatePackingLines_PKLWasUnloadedToHUThenAdjustedOutCore(true);

		void TestPopulatePackingLines_PKLWasUnloadedToHUThenAdjustedOutCore(bool skipScanMode)
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);
			var packline = Helper.CreatePackageState(rcn, 5, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, unitType: "PKL", weight: 20.123m);
			packline.WPS_ReceivedAs = skipScanMode ? TransitWarehouseReceiveAs.Codes.SkipScanMode : TransitWarehouseReceiveAs.Codes.ScannedIn;
			packline.Package.KP_GoodsDescription = "SHOES";
			var hu = Helper.CreateHandlingUnitPackage("HU-1", Helper.CreatePackageHandlingUnit(), null);
			hu.WPS_WW_Warehouse = packline.WPS_WW_Warehouse;
			hu.Package.KP_GoodsDescription = "A";

			Helper.PackPackageIntoHandlingUnit(hu, packline, ZDateTimeOffset.Now, "LWC", hu);

			hu.WPS_AdjustedOut = "OTH";	
			hu.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;

			packline.WPS_AdjustedOut = "OTH";
			packline.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;

			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -5, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -8.8m, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_OVPWasScanIn()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var overpack = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);
			overpack.WPS_AdjustedOut = "OTH";
			overpack.WPS_ReceivedAs = TransitWarehouseReceiveAs.Codes.ScannedIn;
			overpack.Package.KP_GoodsDescription = "A";
			overpack.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage1, ZDateTimeOffset.Now, "AAA", overpack);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage2, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -1, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -(ZDecimal)3, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_OVPWasAdjustedOutByBreakDown()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var overpack = Helper.CreateOverpackPackage("OVP1", rcn, null, rcn: rcn);
			overpack.WPS_AdjustedOut = "BDS";
			overpack.WPS_ReceivedAs = TransitWarehouseReceiveAs.Codes.Default;
			overpack.Package.KP_GoodsDescription = "A";
			overpack.WPS_UnloadedNotYetProcessedTime = ZDateTimeOffset.Empty;
			overpack.WPS_UnloadedTime = ZDateTimeOffset.Empty;
			overpack.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 4.4m);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage1, ZDateTimeOffset.Now, "AAA", overpack);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 4.4m);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage2, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", 0, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", (ZDecimal)0, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_OVPContainsInnerOVP()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var overpack = Helper.CreateOverpackPackage("OVP1", rcn, null, rcn: rcn);
			overpack.WPS_AdjustedOut = "OTH";
			overpack.Package.KP_GoodsDescription = "A";
			overpack.WPS_ReceivedAs = TransitWarehouseReceiveAs.Codes.Default;
			overpack.WPS_UnloadedNotYetProcessedTime = ZDateTimeOffset.Empty;
			overpack.WPS_UnloadedTime = ZDateTimeOffset.Empty;
			overpack.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;

			var childPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1);
			childPackage.WPS_AdjustedOut = "OTH";
			childPackage.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage, ZDateTimeOffset.Now, "AAA", overpack);

			var innerOVP = Helper.CreateOverpackPackage("OVP2", rcn, null, rcn: rcn);
			innerOVP.WPS_AdjustedOut = "OTH";
			innerOVP.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			innerOVP.WPS_UnloadedNotYetProcessedTime = ZDateTimeOffset.Empty;
			innerOVP.WPS_UnloadedTime = ZDateTimeOffset.Empty;
			Helper.PackPackageIntoHandlingUnit(overpack, innerOVP, ZDateTimeOffset.Now, "AAA", overpack);

			var childPackageInSub = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1);
			childPackageInSub.WPS_AdjustedOut = "OTH";
			childPackageInSub.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			Helper.PackPackageIntoHandlingUnit(innerOVP, childPackageInSub, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -2, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -(ZDecimal)2, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_HUContainsOVPAndOVPContainsInnerOVP()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var hu = Helper.CreateHandlingUnitPackage("HU", Helper.CreatePackageHandlingUnit(), rtu);

			var overpack = Helper.CreateOverpackPackage("OVP1", rcn, null, rcn: rcn);
			overpack.WPS_AdjustedOut = "OTH";
			overpack.Package.KP_GoodsDescription = "A";
			overpack.WPS_ReceivedAs = TransitWarehouseReceiveAs.Codes.Default;
			overpack.WPS_UnloadedNotYetProcessedTime = ZDateTimeOffset.Empty;
			overpack.WPS_UnloadedTime = ZDateTimeOffset.Empty;
			overpack.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			Helper.PackPackageIntoHandlingUnit(hu, overpack, ZDateTimeOffset.Now, "AAA", hu);

			var childPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1);
			childPackage.WPS_AdjustedOut = "OTH";
			childPackage.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage, ZDateTimeOffset.Now, "AAA", hu);

			var innerOVP = Helper.CreateOverpackPackage("OVP2", rcn, null, rcn: rcn);
			innerOVP.WPS_AdjustedOut = "OTH";
			innerOVP.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			innerOVP.WPS_UnloadedNotYetProcessedTime = ZDateTimeOffset.Empty;
			innerOVP.WPS_UnloadedTime = ZDateTimeOffset.Empty;
			Helper.PackPackageIntoHandlingUnit(overpack, innerOVP, ZDateTimeOffset.Now, "AAA", hu);

			var childPackageInInnerOVP = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1);
			childPackageInInnerOVP.WPS_AdjustedOut = "OTH";
			childPackageInInnerOVP.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			Helper.PackPackageIntoHandlingUnit(innerOVP, childPackageInInnerOVP, ZDateTimeOffset.Now, "AAA", hu);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", -2, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -(ZDecimal)2, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_RCNHasShipmentDescription()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(rcn, "shipment description", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription);
			Helper.CreateGoverningReference(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			packageState2.WPS_AdjustedOut = "OTH";
			packageState2.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState2.WPS_ReceivedAs = TransitWarehouseReceiveAs.Codes.PackedPackline;

			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			packageState3.WPS_AdjustedOut = "OTH";
			packageState3.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState3.Package.KP_GoodsDescription = "A";

			var overpack = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);
			overpack.WPS_AdjustedOut = "OTH";
			overpack.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			var childPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("goods description should be shipment description", "shipment description", corNotification.Goods.First().Description);
		}

		public void TestPopulatePackingLines_WeightNoChanges()
		{
			var rcn = CreateGeneralRCN();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 8, receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 1.764m, weightUQ: "LB", receiveUnit: rtu);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", 0, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -(ZDecimal)0m, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_WeightDecreased()
		{
			var rcn = CreateGeneralRCN();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 1.764m, weightUQ: "LB", receiveUnit: rtu);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", 0, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", -(ZDecimal)6m, corNotification.Goods.First().AmountWeight);
		}

		public void TestPopulatePackingLines_WeightIncreased()
		{
			var rcn = CreateGeneralRCN();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 12, receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 1.764m, weightUQ: "LB", receiveUnit: rtu);
			Factory.Save();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertEquals("AmountQuantity", 0, corNotification.Goods.First().AmountQuantity);
			AssertEquals("AmountWeight", (ZDecimal)4m, corNotification.Goods.First().AmountWeight);
		}

		WhsItemReceiveConsignment CreateGeneralRCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8");
			return rcn;
		}
	}
}
