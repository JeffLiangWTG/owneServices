using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750CorNotificationDocumentTest : CIN750NotificationDocumentTest<WhsItemReceiveConsignment>
	{
		protected override WhsItemReceiveConsignment CreateSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);
			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState.Package.KP_GoodsDescription = "Test Description";
			packageState.WPS_AdjustedOut = "OTH";

			Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=4");
			Factory.Save();
			return rcn;
		}

		public override ZGuid GetDocumentID() => TransitDocDataConstants.TransitDocTemplateIDs.CIN750CorFromRCNTemplateID;

		protected override ZString CreateContent() =>
			@"[3,4] Reference Type
[3,16] Reference Value
[3,27] WAREHOUSE-COR (CIN 750)
[4,4] AWB - Master Air Waybill
[4,16] MAB1
[5,4] Enterprise Code
[5,16] Movement Time
[6,4] EDIDAT
[6,16] 01-Dec-23 01:02:03
[7,4] Goods PNTS Temporary Storage Number(s)
[8,4] TST1
[9,4] Goods Description
[10,4] Test Description
[11,4] Gross Weight
[11,28] Package Quantity
[12,4] -2 KG
[12,28] -1
[13,4] CFS/TWH Warehouse
[14,4] HEADER
[15,4] WH1ADDRESS
[19,4] CFS/TWH CIN Code
[20,4] C001
[24,42] Created By";
	}
}
