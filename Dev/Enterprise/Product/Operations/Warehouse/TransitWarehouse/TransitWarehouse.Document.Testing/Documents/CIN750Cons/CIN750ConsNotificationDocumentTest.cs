using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750ConsNotificationDocumentTest : CIN750NotificationDocumentTest<WhsItemDispatchConsignment>
	{
		protected override WhsItemDispatchConsignment CreateSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HSB1";

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var ovpPackcageState = Helper.CreateOverpackPackage("OVP-1", rcn, null, rcn: rcn, dcn: dcn, dtu: dtu, status: TransitWarehouseStatuses.Codes.Departed, dll: dll);
			SetPackageStateDetails(ovpPackcageState, 123.23, "BOOKS");
			var innerPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			SetPackageStateDetails(innerPackageState1, 23.23, "BOOKS", "SCN");
			var innerPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			SetPackageStateDetails(innerPackageState2, 3.23, "BOOKS", "SCN");

			Helper.PackPackageIntoHandlingUnit(ovpPackcageState, innerPackageState1, ZDateTimeOffset.Now, "LWC", ovpPackcageState);
			Helper.PackPackageIntoHandlingUnit(ovpPackcageState, innerPackageState2, ZDateTimeOffset.Now, "LWC", ovpPackcageState);
			Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRCN1|WGT=26.460");

			Factory.Save();
			return dcn;
		}

		protected void SetPackageStateDetails(WhsItemPackageState packageState, ZDecimal weight, ZString description, string receiveAs = "")
		{
			packageState.WPS_ReceivedAs = receiveAs;
			packageState.Package.KP_Weight = weight;
			packageState.Package.KP_GoodsDescription = description;
		}

		public override ZGuid GetDocumentID() => TransitDocDataConstants.TransitDocTemplateIDs.CIN750ConsFromDCNTemplateID;

		protected override ZString CreateContent() =>
@"[3,4] Reference Type
[3,16] Reference Value
[3,28] WAREHOUSE-CONS (CIN 750)
[4,4] HWB - House Air Waybill
[4,16] HSB1
[5,4] Enterprise Code
[5,16] Movement Time
[6,4] EDIDAT
[6,16] 01-Dec-23 01:02:03
[7,4] From Goods Reference Type
[7,28] From Goods Reference Value
[8,4] REF - Reference Number
[8,28] EDIDATRCN1
[9,4] From Goods PNTS Temporary Storage Number(s)
[11,4] From Goods Description
[12,4] BOOKS
[13,4] From Gross Weight
[13,28] From Gross Quantity
[14,4] 26.460 KG
[14,28] 2
[15,4] To Goods Reference Type
[15,28] To Goods Reference Value
[16,4] HWB - House Air Waybill
[16,28] HSB1
[17,4] To Goods PNTS Temporary Storage Number(s)
[19,4] To Goods Description
[20,4] BOOKS
[21,4] To Gross Weight
[21,28] To Gross Quantity
[22,4] 123.23 KG
[22,28] 1
[23,4] CFS/TWH Warehouse
[24,4] HEADER
[25,4] #1
[29,4] CFS/TWH CIN Code
[30,4] NOTCIN
[33,42] Created By

";
	}
}
