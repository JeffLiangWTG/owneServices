using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750ConsNotificationPackingLineBuilderTest : DocPackingLineBuilderTest
	{
		public void TestBuild_RCNHasID_DCNHasHWB()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=10.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_RemoveHypen()
		{
			var data = PrepareData();
			data.rcn.WRC_ConsignmentID = data.rcn.WRC_ConsignmentID + "-1";
			data.dcn.WDC_HouseBillNumber = "HWB1-1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN11|WGT=10.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN11");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB11");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_HasCor_OnlyChangeWeight() => TestBuild_RCNHasID_DCNHasHWB_HasCor_Core(true);

		public void TestBuild_RCNHasID_DCNHasHWB_HasCor() => TestBuild_RCNHasID_DCNHasHWB_HasCor_Core(false);

		public void TestBuild_RCNHasID_DCNHasHWB_HasCor_Core(bool onlyChangeWeight)
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=10.000");
			if (onlyChangeWeight)
			{
				data.rcn.PackageStates.FirstOrDefault().Package.KP_Weight -= new ZDecimal(0.3);
				Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=0|PTP=REF|RFN=EDIDATRCN1|WGT=-0.3");
			}
			else
			{
				data.rcn.PackageStates.FirstOrDefault().WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
				data.rcn.PackageStates.FirstOrDefault().WPS_AdjustedOut = "ADJ";
				data.rcn.PackageStates.FirstOrDefault().WPS_WDH_TransitDispatchHeader = ZGuid.Empty;
				Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-1|PTP=REF|RFN=EDIDATRCN1|WGT=-5.000");
			}
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), onlyChangeWeight ? 2 : 1, onlyChangeWeight ? 9.700 : 5.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, onlyChangeWeight ? 2 : 1, onlyChangeWeight ? 9.700 : 5.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasID_OVP()
		{
			var data = PrepareData();
			var ovp = Helper.CreateOverpackPackage("OVP1", data.rcn, null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, dll: data.dll);
			SetPackageStateDetails(ovp, 10, "OVP DESC", "");
			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner2", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");
			Helper.PackPackageIntoHandlingUnit(ovp, inner1, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(ovp, inner2, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 4, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 3, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATDCN1");
		}

		public void TestBuild_RCNHasID_DCNHasMBL_PackOVPAfterHWBCons()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner2", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_To|OTY=4|PTP=HWB|RFN=EDIDATDCN1|WGT=20.000");

			var ovp = Helper.CreateOverpackPackage("OVP1", data.rcn, null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, dll: data.dll);
			SetPackageStateDetails(ovp, 10, "OVP DESC", "");
			Helper.PackPackageIntoHandlingUnit(ovp, inner1, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(ovp, inner2, ZDateTimeOffset.Now, "LDN", ovp);

			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 4, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
			AssertGoodsDetails(notification.ToGoods, 3, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHBL_PackOVPAfterHWBCons()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner2", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_To|OTY=4|PTP=HWB|RFN=EDIDATDCN1|WGT=20.000");

			var ovp = Helper.CreateOverpackPackage("OVP1", data.rcn, null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, dll: data.dll);
			SetPackageStateDetails(ovp, 10, "OVP DESC", "");
			Helper.PackPackageIntoHandlingUnit(ovp, inner1, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(ovp, inner2, ZDateTimeOffset.Now, "LDN", ovp);

			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 4, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
			AssertGoodsDetails(notification.ToGoods, 3, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasMBL_PackOVPAfterAWBCons()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner2", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");

			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_To|OTY=4|PTP=HWB|RFN=EDIDATDCN1|WGT=20.000");

			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_From|OTY=4|PTP=HWB|RFN=EDIDATDCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_To|OTY=4|PTP=AWB|RFN=EDIDATDCN1|WGT=20.000");

			var ovp = Helper.CreateOverpackPackage("OVP1", data.rcn, null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, dll: data.dll);
			SetPackageStateDetails(ovp, 10, "OVP DESC", "");
			Helper.PackPackageIntoHandlingUnit(ovp, inner1, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(ovp, inner2, ZDateTimeOffset.Now, "LDN", ovp);

			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 4, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB1");
			AssertGoodsDetails(notification.ToGoods, 3, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB1");
		}

		public void TestBuild_RCNHasID_DCNHasMBL_PackOVPBeforeHWBCons()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner2", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			var ovp = Helper.CreateOverpackPackage("OVP1", data.rcn, null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, dll: data.dll);
			SetPackageStateDetails(ovp, 10, "OVP DESC", "");
			Helper.PackPackageIntoHandlingUnit(ovp, inner1, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(ovp, inner2, ZDateTimeOffset.Now, "LDN", ovp);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=4|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_To|OTY=3|PTP=HWB|RFN=EDIDATDCN1|WGT=20.000");

			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 3, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
			AssertGoodsDetails(notification.ToGoods, 3, 20.000, "OVP DESC", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_HasConsHistory()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=10.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=1|PTP=REF|RFN=EDIDATRCN1|WGT=5.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=-|MST=CIN750ConsNotification_To|OTY=1|PTP=HWB|RFN=EDIDATDCN1|WGT=5.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 1, 5.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 1, 5.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_PackHU()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";

			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 3, "PKL", "", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll, unitType: "PKL");
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var hu = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, data.rtu, status: TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageStateDetails(hu, 10, "HU DESC");
			Helper.PackPackageIntoHandlingUnit(hu, inner1, ZDateTimeOffset.Now, "LDN", hu);
			Helper.PackPackageIntoHandlingUnit(hu, inner2, ZDateTimeOffset.Now, "LDN", hu);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=6|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 6, 20.000, "INNER2 DESC", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 6, 20.000, "INNER2 DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_PackULD()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";

			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 3, "PKL", "", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll, unitType: "PKL");
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var uld = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, data.rtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, unitType: "ULD");
			SetPackageStateDetails(uld, 10, "HU DESC");
			Helper.PackPackageIntoHandlingUnit(uld, inner1, ZDateTimeOffset.Now, "LDN", uld);
			Helper.PackPackageIntoHandlingUnit(uld, inner2, ZDateTimeOffset.Now, "LDN", uld);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=6|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 6, 20.000, "INNER2 DESC", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 6, 20.000, "INNER2 DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_PackHUIntoULD()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";

			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 3, "PKL", "", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll, unitType: "PKL");
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var hu = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, data.rtu, status: TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageStateDetails(hu, 10, "HU DESC");
			var uldHandlingUnit = Helper.CreatePackageHandlingUnit();
			var uld = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, data.rtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, unitType: "ULD");
			SetPackageStateDetails(uld, 10, "ULD DESC");

			Helper.PackPackageIntoHandlingUnit(hu, inner1, ZDateTimeOffset.Now, "LDN", uld);
			Helper.PackPackageIntoHandlingUnit(hu, inner2, ZDateTimeOffset.Now, "LDN", uld);
			Helper.PackPackageIntoHandlingUnit(uld, hu, ZDateTimeOffset.Now, "LDN", uld);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=6|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 6, 20.000, "INNER2 DESC", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 6, 20.000, "INNER2 DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasHWB_PackHUIntoOVP()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";

			var inner1 = Helper.CreatePackageState(data.rcn, 1, "PKG", "inner1", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll);
			SetPackageStateDetails(inner1, 5, "INNER1 DESC");
			var inner2 = Helper.CreatePackageState(data.rcn, 3, "PKL", "", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, dispatchConsignment: data.dcn, dispatchUnit: data.dtu, dispatchLoadList: data.dll, unitType: "PKL");
			SetPackageStateDetails(inner2, 5, "INNER2 DESC");

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var hu = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, data.rtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, rcn: data.rcn, dcn: data.dcn);
			SetPackageStateDetails(hu, 10, "HU DESC");
			var ovp = Helper.CreateOverpackPackage("OVP1", data.rcn, null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.FreightLoaded, dll: data.dll);
			SetPackageStateDetails(ovp, 10, "OVP DESC", "");

			Helper.PackPackageIntoHandlingUnit(hu, inner1, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(hu, inner2, ZDateTimeOffset.Now, "LDN", ovp);
			Helper.PackPackageIntoHandlingUnit(ovp, hu, ZDateTimeOffset.Now, "LDN", ovp);

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=6|PTP=REF|RFN=EDIDATRCN1|WGT=20.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 6, 20.000, "HU DESC", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 3, 20.000, "HU DESC", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasID_DCNHasAWB_HWBCons() => TestBuild_RCNHasID_DCNHasAWB_Core();

		public void TestBuild_RCNHasID_DCNHasAWB_AWBCons() => TestBuild_RCNHasID_DCNHasAWB_Core(true);

		void TestBuild_RCNHasID_DCNHasAWB_Core(bool isAWBCons = false)
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=10.000");
			if (isAWBCons)
			{
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=10.000");
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750ConsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDCN1|WGT=10.000");
			}
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1",
				refTypeCode: isAWBCons ? CIN750RefTypes.Codes.HouseAirWaybill : CIN750RefTypes.Codes.Reference,
				refCode: isAWBCons ? "HWB1" : "EDIDATRCN1");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1",
				refTypeCode: isAWBCons ? CIN750RefTypes.Codes.MasterAirWaybill : CIN750RefTypes.Codes.HouseAirWaybill,
				refCode: isAWBCons ? "AWB1" : "HWB1");
		}

		public void TestBuild_RCNHasHWB_DCNHasHWB()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			data.rcn.WRC_HouseBillNumber = "HWB2";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HWB2|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=HWB|RFN=EDIDATRC0000001|WGT=10.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB2");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		public void TestBuild_RCNHasHWB_DCNAndRCNHasSameHWB_DCNHasAWB()
		{
			var data = PrepareData();
			data.rcn.WRC_HouseBillNumber = "HWB1";
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=HWB|RFN=EDIDATRC0000001|WGT=10.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB1");
		}

		public void TestBuild_RCNHasHWB_DCNAndRCNHasSameHWB_DCNHasAWB_HasCor()
		{
			var data = PrepareData();
			data.rcn.WRC_HouseBillNumber = "HWB1";
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			data.rcn.PackageStates.FirstOrDefault().WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			data.rcn.PackageStates.FirstOrDefault().WPS_AdjustedOut = "ADJ";
			data.rcn.PackageStates.FirstOrDefault().WPS_WDH_TransitDispatchHeader = ZGuid.Empty;

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=HWB|RFN=EDIDATRC0000001|WGT=10.000");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-1|PTP=REF|RFN=EDIDATRC0000001|WGT=-5.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 1, 5.0, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
			AssertGoodsDetails(notification.ToGoods, 1, 5.0, "DESC 1", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB1");
		}

		public void TestBuild_RCNHasHWB_DCNAndRCNHasDifferentHWBs_DCNHasAWB_HWBCons() => TestBuild_RCNHasHWB_DCNHasDifferentHWB_DCNHasAWB_Core();

		public void TestBuild_RCNHasHWB_DCNAndRCNHasDifferentHWBs_DCNHasAWB_AWBCons() => TestBuild_RCNHasHWB_DCNHasDifferentHWB_DCNHasAWB_Core(true);

		void TestBuild_RCNHasHWB_DCNHasDifferentHWB_DCNHasAWB_Core(bool isAWBCons = false)
		{
			var data = PrepareData();
			data.rcn.WRC_HouseBillNumber = "HWB2";
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HWB2|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=HWB|RFN=EDIDATRCN1|WGT=10.000");
			if (isAWBCons)
			{
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB2|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=2|PTP=HWB|RFN=EDIDATDCN1|WGT=10.000");
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=-|MST=CIN750ConsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDCN1|WGT=10.000");
			}
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1",
				refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill,
				refCode: isAWBCons ? "HWB1" : "HWB2");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1",
				refTypeCode: isAWBCons ? CIN750RefTypes.Codes.MasterAirWaybill : CIN750RefTypes.Codes.HouseAirWaybill,
				refCode: isAWBCons ? "AWB1" : "HWB1");
		}

		public void TestBuild_RCNHasAWB_DCNHasSameHWB_RCNAndDCNHaveDifferentAWBs()
		{
			var data = PrepareData();
			data.rcn.WRC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.rcn, "AWB1", AdditionalReferenceTypes.Codes.MasterBill);
			data.dcn.WDC_HouseBillNumber = "HWB1";
			Helper.CreateAdditionalReference(data.dcn, "AWB2", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=RC0000001|MBL=AWB1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRCN1|WGT=10.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=RC0000001|MBL=AWB1|MST=CIN750DeconsNotification_From|OTY=2|PTP=AWB|RFN=EDIDATDCN1|WGT=10.000");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB1|JOB=DC0000001|MBL=AWB1|MST=CIN750DeconsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDCN1|WGT=10.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(), 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
			AssertGoodsDetails(notification.ToGoods, 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.MasterAirWaybill, refCode: "AWB2");
		}

		public void TestBuild_FromDifferentRCNs()
		{
			var data = PrepareData();
			data.dcn.WDC_HouseBillNumber = "HWB1";
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", data.warehouse.PK);
			rcn2.WRC_JobID = "RC0000002";
			var packageState3 = Helper.CreatePackageState(rcn2, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, data.rtu, data.dcn, data.dtu, data.dll, weight: 5);
			packageState3.Package.KP_GoodsDescription = "DESC 3";

			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=10.000");
			Helper.CreateStmALog(rcn2, EventCodes.MessageSent, "|HBL=-|JOB=RC0000002|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRCN2|WGT=5.000");
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			AssertEquals(2, notification.FromGoods.Count);
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(g => g.RefCode == "EDIDATRCN1"), 2, 10.000, "DESC 1", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN1");
			AssertGoodsDetails(notification.FromGoods.FirstOrDefault(g => g.RefCode == "EDIDATRCN2"), 1, 5.000, "DESC 3", refTypeCode: CIN750RefTypes.Codes.Reference, refCode: "EDIDATRCN2");
			AssertGoodsDetails(notification.ToGoods, 3, 15, "DESC 1", refTypeCode: CIN750RefTypes.Codes.HouseAirWaybill, refCode: "HWB1");
		}

		(WhsItemReceiveConsignment rcn, WhsItemDispatchConsignment dcn, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, Environment.Business.WhsWarehouse warehouse) PrepareData()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_JobID = "DC0000001";

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultLocation);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_JobID = "RC0000001";
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll, weight: 5);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll, weight: 5);
			packageState2.Package.KP_GoodsDescription = "DESC 2";
			return (rcn, dcn, rtu, dtu, dll, warehouse);
		}
	}
}
