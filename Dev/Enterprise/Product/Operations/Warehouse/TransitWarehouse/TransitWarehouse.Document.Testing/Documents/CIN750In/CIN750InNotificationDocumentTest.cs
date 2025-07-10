using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750InNotificationDocumentTest : CIN750NotificationDocumentTest<WhsItemReceiveConsignment>
	{
		protected override WhsItemReceiveConsignment CreateSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			return rcn;
		}

		public override ZGuid GetDocumentID() => TransitDocDataConstants.TransitDocTemplateIDs.CIN750InFromRCNTemplateID;

		protected override ZString CreateContent() =>
@"[3,4] Reference Type
[3,16] Reference Value
[3,27] WAREHOUSE-IN (CIN 750)
[4,4] REF - Reference Number
[4,16] EDIDATRC0000001
[5,4] Enterprise Code
[5,16] Movement Time
[6,4] EDIDAT
[6,16] 01-Dec-23 01:02:03
[7,4] Custom Status
[8,4] C - Community
[9,4] Accompanying Document Type
[9,28] Accompanying Document Ref
[10,4] T1
[10,28] CEN1
[11,4] Temporary Storage Declaration
[12,4] TST1
[13,4] Goods Description
[14,4] Test Description
[15,4] Gross Weight
[15,28] Package Quantity
[16,4] 8.8 KG
[16,28] 5
[17,4] From CTO/Terminal
[17,28] CFS/TWH Warehouse
[18,4] WHTEST
[18,28] HEADER
[19,4] CTOADDRESS
[19,28] WH1ADDRESS
[22,4] AUSTRALIA
[23,4] CTO/Terminal CIN Code
[23,28] CFS/TWH CIN Code
[24,4] C002
[24,28] C001
[28,42] Created By";
	}
}
