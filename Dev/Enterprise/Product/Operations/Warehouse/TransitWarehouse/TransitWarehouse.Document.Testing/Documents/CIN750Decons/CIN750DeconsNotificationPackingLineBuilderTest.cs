using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750DeconsNotificationPackingLineBuilderTest : DocPackingLineBuilderTest
	{
		#region TestBuildDocPackingLines

		public void TestBuildDocPackingLines_HasPackline_UnloadedToOVP()
		{
			var data = TestBuildDocPackingLinesSetup();

			var packline = Helper.CreatePackageState(data.rcn, 5, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Arrived, data.rtu, data.dcn, unitType: "PKL", weight: 20.123m);
			packline.Package.KP_GoodsDescription = "SHOES";
			var ovpPackageState = Helper.CreateOverpackPackage("OVP-1", data.rcn, null, TransitWarehouseStatuses.Codes.Arrived, rcn: data.rcn, dcn: data.dcn);

			Helper.PackPackageIntoHandlingUnit(ovpPackageState, packline, ZDateTimeOffset.Now, "LWC", ovpPackageState);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=5|PTP=AWB|RFN=EDIDATRC0000001|WGT=20.123");

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();
			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());
			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 5, 20.123, "SHOES", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 5, 20.123, "SHOES", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_HasPackline_UnloadedToHU() => TestBuildDocPackingLines_HasPackline_UnloadedToHUCore(false);

		public void TestBuildDocPackingLines_HasPackline_UnloadedToHU_ViaSkipScanMode() => TestBuildDocPackingLines_HasPackline_UnloadedToHUCore(true);

		void TestBuildDocPackingLines_HasPackline_UnloadedToHUCore(bool skipScanMode)
		{
			var data = TestBuildDocPackingLinesSetup();

			var packline = Helper.CreatePackageState(data.rcn, 5, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Arrived, data.rtu, data.dcn, unitType: "PKL", weight: 20.123m);
			packline.Package.KP_GoodsDescription = "SHOES";
			packline.WPS_ReceivedAs = skipScanMode ? TransitWarehouseReceiveAs.Codes.SkipScanMode : TransitWarehouseReceiveAs.Codes.ScannedIn;
			var hu = Helper.CreateHandlingUnitPackage("HU-1", Helper.CreatePackageHandlingUnit(), null);
			hu.WPS_WW_Warehouse = packline.WPS_WW_Warehouse;

			Helper.PackPackageIntoHandlingUnit(hu, packline, ZDateTimeOffset.Now, "LWC", hu);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=5|PTP=AWB|RFN=EDIDATRC0000001|WGT=20.123");

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();
			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 5, 20.123, "SHOES", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 5, 20.123, "SHOES", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_HasNoOVP()
		{
			var data = TestBuildDocPackingLinesSetup();

			var packageState1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "P0001", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState1, 1.1, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var packageState2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "P0001", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState2, 2.2, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);

			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 0.3m, receiveUnit: data.rtu);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=3.3");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-0.3");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=3");

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 1.1 + 2.2, "SHOES", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 2, 1.1 + 2.2, "SHOES", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_AdjustedOutPackageState()
		{
			var data = TestBuildDocPackingLines_StandalonePackageState_Setup();

			var outerPackageState2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "Outer-2", TransitWarehouseStatuses.Codes.AdjustedOut, data.rtu, data.dcn);
			SetPackageStateDetails(outerPackageState2, 333.45, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var innerPackageState3 = Helper.CreatePackageState(data.rcn, 1, "PKG", "INNER-3", TransitWarehouseStatuses.Codes.AdjustedOut, data.rtu, data.dcn);
			SetPackageStateDetails(innerPackageState3, 13.3, "SHOES", TransitWarehouseReceiveAs.Codes.Adhoc);
			Helper.PackPackageIntoHandlingUnit(outerPackageState2, innerPackageState3, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);
			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 123.3, "BOOKS", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 2, 23.3 + 3.3, "BOOKS", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");

			AssertEquals("line.TemporaryStorageDeclaration should be TSD1", "TSD1", fromGoods.TemporaryStorageDeclaration);
			AssertEquals("line.TemporaryStorageDeclaration should be TSD1", "TSD1", toGoods.TemporaryStorageDeclaration);
		}

		public void TestBuildDocPackingLines_StandalonePackageState()
		{
			var data = TestBuildDocPackingLines_StandalonePackageState_Setup();

			Helper.CreateAdditionalReference(data.dcn, "MAB-2", AdditionalReferenceTypes.Codes.MasterBill);

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();

			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 123.3, "BOOKS", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 2, 23.3 + 3.3, "BOOKS", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");

			AssertEquals("line.TemporaryStorageDeclaration should be TSD1", "TSD1", fromGoods.TemporaryStorageDeclaration);
			AssertEquals("line.TemporaryStorageDeclaration should be TSD1", "TSD1", toGoods.TemporaryStorageDeclaration);
		}

		(WhsItemReceiveConsignment rcn, WhsItemDispatchConsignment dcn, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, Environment.Business.WhsWarehouse warehouse) TestBuildDocPackingLines_StandalonePackageState_Setup()
		{
			var data = TestBuildDocPackingLinesSetup();

			Helper.CreateCustomsAdditionalReference(data.rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			Helper.CreateCustomsAdditionalReference(data.rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var outerPackageState = Helper.CreatePackageState(data.rcn, 1, "PKG", "Outer-1", TransitWarehouseStatuses.Codes.AdjustedOut, data.rtu, data.dcn);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=123.3");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-0.3");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=3");
			SetPackageStateDetails(outerPackageState, 123.3, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var innerPackageState1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "INNER-1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(innerPackageState1, 23.3, "BOOKS", TransitWarehouseReceiveAs.Codes.Adhoc);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState1, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var innerPackageState2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "INNER-2", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(innerPackageState2, 3.3, "BOOKS", TransitWarehouseReceiveAs.Codes.Adhoc);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState2, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			return data;
		}

		public void TestBuildDocPackingLines_RemoveHypen()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_JobID = "DC0000001";
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_JobID = "RC0000001";
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			dcn.WDC_HouseBillNumber = "HSB-1";

			var outerPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "Outer-1", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll);
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=1|PTP=AWB|RFN=EDIDATRC0000001|WGT=1");
			SetPackageStateDetails(outerPackageState, 1, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			Factory.Save();

			var notification = new CIN750DeconsNotificationBuilder(dcn).Build();
			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();

			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 1, "BOOKS", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 1, 1, "BOOKS", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_RCNHasNoAWB()
		{
			var data = TestBuildDocPackingLinesSetup();
			var rcn = Helper.CreateReceiveConsignment("RCN2", data.warehouse.PK);

			var outerPackageState = Helper.CreateOverpackPackage("OVP-1", data.dcn, data.rtu, rcn: rcn, dcn: data.dcn);
			SetPackageStateDetails(outerPackageState, 4.4, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var innerPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(innerPackageState, 3.3, "SHOES", TransitWarehouseReceiveAs.Codes.PackedPackage);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(0, fromToGoods.Count());
		}

		public void TestBuildDocPackingLines_OverpackBreakDown_DCNHasNoHSBOrMAB() => TestBuildDocPackingLines_OverpackBreakDown(false);

		public void TestBuildDocPackingLines_OverpackBreakDown_DCNHasHSB() => TestBuildDocPackingLines_OverpackBreakDown(true);

		public void TestBuildDocPackingLines_OverpackBreakDown(bool createDCNHouseBill)
		{
			var data = TestBuildDocPackingLinesSetup(createDCNHouseBill);

			var outerPackageState = Helper.CreateOverpackPackage("OVP-1", data.rcn, data.rtu, rcn: data.rcn);
			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 0.4m, receiveUnit: data.rtu);
			SetPackageStateDetails(outerPackageState, 4.4, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=4.4");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-0.4");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=4");

			var innerPackageState = Helper.CreatePackageState(data.rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(innerPackageState, 3.3, "SHOES", TransitWarehouseReceiveAs.Codes.PackedPackage);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());
			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 4.4, "SHOES", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 1, 3.3, "SHOES", CIN750RefTypes.Codes.HouseAirWaybill, createDCNHouseBill ? "HSB1" : string.Empty);
		}

		public void TestBuildDocPackingLines_AdhocOverpackBreakDown()
		{
			var data = TestBuildDocPackingLinesSetup(true);

			var outerPackageState = Helper.CreateOverpackPackage("OVP-1", data.dcn, data.rtu, dcn: data.dcn);
			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 0.4m, receiveUnit: data.rtu);
			SetPackageStateDetails(outerPackageState, 4.4, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);
			outerPackageState.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			outerPackageState.WPS_AdjustedOut = "BDS";

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=4.4");

			var innerPackageState = Helper.CreatePackageState(data.rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(innerPackageState, 4.4, "SHOES", TransitWarehouseReceiveAs.Codes.PackedPackage);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			Factory.Save();

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());
			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 4.4, "SHOES", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 1, 4.4, "SHOES", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_OverpackBreakDown_RCNHasNoAWBOrSameAWB()
		{
			var data = TestBuildDocPackingLinesSetup();
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=6|PTP=AWB|RFN=EDIDATRC0000001|WGT=42.000");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-7.000");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=7.000");

			var packageState1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PLT, "P1", TransitWarehouseStatuses.Codes.AdjustedOut, data.rtu, adjustedOut: "OTH", weight: 2);
			var packageStateInner1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BOX, "P-Inner-1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 3);
			var packageStateInner2 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BOX, "P-Inner-2", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 4);
			var packageStateInner3 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BOX, "P-Inner-3", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 5);
			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 7, receiveUnit: data.rtu);
			Helper.PackPackageIntoHandlingUnit(packageState1, packageStateInner1, ZDateTimeOffset.Now, "TST", ZDateTimeOffset.Now);
			Helper.PackPackageIntoHandlingUnit(packageState1, packageStateInner2, ZDateTimeOffset.Now, "TST", ZDateTimeOffset.Now);
			Helper.PackPackageIntoHandlingUnit(packageState1, packageStateInner3, ZDateTimeOffset.Now, "TST", ZDateTimeOffset.Now);

			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BAG, "P2", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 6);
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BAG, "P3", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 7);
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BAG, "P4", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 8);
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BAG, "P5", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 9);
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.BAG, "P6", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll, weight: 10);

			packageState1.Package.KP_GoodsDescription = "GOODSDESC";

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());
			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 2, "GOODSDESC", CIN750RefTypes.Codes.Reference, "EDIDATRCN1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 3, 12, ZString.Empty, CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_RCNAndDCNHaveSameAWB()
		{
			var data = TestBuildDocPackingLinesSetup();
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var outerPackageState = Helper.CreateOverpackPackage("OVP-1", data.rcn, data.rtu, rcn: data.rcn);
			SetPackageStateDetails(outerPackageState, 4.4, "SHOES", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var innerPackageState = Helper.CreatePackageState(data.rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, data.rtu, data.dcn);
			SetPackageStateDetails(innerPackageState, 3.3, "SHOES", TransitWarehouseReceiveAs.Codes.PackedPackage);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(0, fromToGoods.Count());
		}

		public void TestBuildDocPackingLines_DCNHasShipmentDescription()
		{
			var data = TestBuildDocPackingLines_StandalonePackageState_Setup();
			Helper.CreateAdditionalReference(data.rcn, "shipment description", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription);

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 123.3, "shipment description", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "MAB1");
		}

		public void TestBuildDocPackingLines_UseOVPGoodsDescription()
		{
			var data = TestBuildDocPackingLines_StandalonePackageState_Setup();
			var outerOVPPackageState = Helper.CreateOverpackPackage("OVP", data.dcn, data.rtu, TransitWarehouseStatuses.Codes.Departed, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, dll: data.dll);
			SetPackageStateDetails(outerOVPPackageState, 7.7, "SHOES");

			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 0.7m, receiveUnit: data.rtu);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=7.7");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-0.7");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=7");

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();

			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 2, 131.0, "SHOES", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "MAB1");
		}

		public void TestBuildDocPackingLines_InMessageHasNotBeenSent()
		{
			var data = TestBuildDocPackingLinesSetup();

			var packageState = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState, 2.2, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();
			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(0, fromToGoods.Count());
		}

		public void TestBuildDocPackingLines_InQuantityIsLessThanReported()
		{
			var data = TestBuildDocPackingLinesSetup();

			var packageState1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState1, 2.2, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var packageState2 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState2, 3.3, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 0.2m, receiveUnit: data.rtu);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=2.2");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-0.2");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=2");

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();
			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 4.0, "BOOKS", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 2, 2.2 + 3.3, "BOOKS", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		public void TestBuildDocPackingLines_DeconsMessageHasNotBeenSent()
		{
			var data = TestBuildDocPackingLinesSetup();

			var packageState1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState1, 2.2, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var packageState2 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, data.rtu, data.dcn, data.dtu, data.dll);
			SetPackageStateDetails(packageState2, 3.3, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 0.5m, receiveUnit: data.rtu);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=3|PTP=AWB|RFN=EDIDATRCN1|WGT=10.5");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRCN1|WGT=-0.5");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRCN1|WGT=5");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750DeconsNotification_From|OTY=1|PTP=AWB|RFN=EDIDATRCN1|WGT=2.2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=1|PTP=HWB|RFN=EDIDATDCN1|WGT=2.2");

			var notification = new CIN750DeconsNotificationBuilder(data.dcn).Build();
			Factory.Save();

			var fromToGoods = new CIN750DeconsNotificationPackingLineBuilder(notification).BuildDocPackingLines();
			AssertEquals(1, fromToGoods.Count());

			var fromGoods = fromToGoods.Single().Item1;
			AssertGoodsDetails(fromGoods, 1, 3.3, "BOOKS", CIN750RefTypes.Codes.MasterAirWaybill, "MAB1");

			var toGoods = fromToGoods.Single().Item2;
			AssertGoodsDetails(toGoods, 1, 3.3, "BOOKS", CIN750RefTypes.Codes.HouseAirWaybill, "HSB1");
		}

		(WhsItemReceiveConsignment rcn, WhsItemDispatchConsignment dcn, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, Environment.Business.WhsWarehouse warehouse) TestBuildDocPackingLinesSetup(bool createDCNHouseBill = true)
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_JobID = "DC0000001";
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_JobID = "RC0000001";
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			if (createDCNHouseBill)
			{
				dcn.WDC_HouseBillNumber = "HSB1";
			}

			return (rcn, dcn, rtu, dtu, dll, warehouse);
		}

		#endregion
	}
}
