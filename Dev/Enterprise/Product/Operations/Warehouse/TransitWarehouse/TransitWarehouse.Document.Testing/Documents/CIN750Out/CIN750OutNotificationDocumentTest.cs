using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750OutNotificationDocumentTest : CIN750NotificationDocumentTest<WhsItemDispatchConsignment>
	{
		protected override WhsItemDispatchConsignment CreateSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address1";
			warehouse.WarehouseAddress.Address2 = "WH1Address2";
			warehouse.WarehouseAddress.City = "City-01";
			warehouse.WarehouseAddress.State = "State-01";
			warehouse.WarehouseAddress.Postcode = "Code-01";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			var crn = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			crn.PopulateAddOnValue("SourceType", "STR", "T1");

			var ctoHeader = Helper.CreateClient("Company-002");
			Helper.AddOrgCode(ctoHeader.MainAddress, OrgCusCode.FranceCodeTypes.CIN, "C002");
			ctoHeader.MainAddress.Address1 = "ctoAddress1";
			ctoHeader.MainAddress.Address2 = "ctoAddress2";
			ctoHeader.MainAddress.City = "City-02";
			ctoHeader.MainAddress.State = "State-02";
			ctoHeader.MainAddress.Postcode = "Code-02";
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(dcn, DocAddressTypes.Codes.DepartureCTOAddress, ctoHeader.MainAddress);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll);
			packageState.Package.KP_GoodsDescription = "ppp";

			return dcn;
		}

		public override ZGuid GetDocumentID() => TransitDocDataConstants.TransitDocTemplateIDs.CIN750OutFromDCNTemplateID;

		protected override ZString CreateContent() =>
@"[3,4] Reference Type
[3,16] Reference Value
[3,27] WAREHOUSE-Out (CIN 750)
[4,4] REF - Reference Number
[4,16] EDIDATRC0000001
[5,4] Enterprise Code
[5,16] Movement Time
[6,4] EDIDAT
[6,16] 01-Dec-23 01:02:03
[7,4] Customs Status
[8,4] I - Import
[9,4] Goods PNTS Temporary Storage Number(s)
[10,4] TSD1
[11,4] Goods Description
[12,4] ppp
[13,4] Gross Weight
[13,28] Package Quantity
[14,4] 0 KG
[14,28] 0
[15,4] Customs Document Types
[15,28] Customs Document Ref 
[16,4] T1
[16,28] CRN1
[17,4] To CTO/Terminal
[17,28] CFS/TWH Warehouse
[18,4] COMPANY-002
[18,28] HEADER
[19,4] CTOADDRESS1
[19,28] WH1ADDRESS1
[20,4] CTOADDRESS2
[20,28] WH1ADDRESS2
[21,4] CITY-02
[21,16] STATE-02
[21,28] CITY-01
[21,40] STATE-01
[22,4] AUSTRALIA
[22,16] Code-02
[22,40] Code-01
[23,4] To CTO/Terminal CIN Code
[23,28] CFS/TWH CIN Code
[24,4] C002
[24,28] C001
[28,42] Created By";
	}
}
