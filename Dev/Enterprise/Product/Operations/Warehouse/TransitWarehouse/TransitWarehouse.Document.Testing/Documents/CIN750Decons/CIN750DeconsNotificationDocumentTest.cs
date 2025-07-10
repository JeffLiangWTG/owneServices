using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750DeconsNotificationDocumentTest : CIN750NotificationDocumentTest<WhsItemDispatchConsignment>
	{
		protected override WhsItemDispatchConsignment CreateSource()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-2|MST=CIN750InNotification|OTY=1|PTP=AWB|RFN=EDIDATRC0000001|WGT=123.3");

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			dcn.WDC_HouseBillNumber = "HSB1";

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var outerPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "Outer-1", TransitWarehouseStatuses.Codes.AdjustedOut, rtu, dcn);
			SetPackageStateDetails(outerPackageState, 123.3, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);

			var innerPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "INNER-1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			SetPackageStateDetails(innerPackageState1, 23.3, "BOOKS", TransitWarehouseReceiveAs.Codes.Adhoc);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState1, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var innerPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "INNER-2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			SetPackageStateDetails(innerPackageState2, 3.3, "BOOKS", TransitWarehouseReceiveAs.Codes.Adhoc);
			Helper.PackPackageIntoHandlingUnit(outerPackageState, innerPackageState2, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			var ctoAddress = Factory.NewWithValidTestData<OrgAddress>();
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			var cinCodeForCTO = ctoJobDocAddress.Address.Header.CustomsCodes.AddNew();
			cinCodeForCTO.OK_CodeType = OrgCusCode.FranceCodeTypes.CIN;
			cinCodeForCTO.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			cinCodeForCTO.OK_CustomsRegNo = "C002";

			return dcn;
		}

		protected void SetPackageStateDetails(WhsItemPackageState packageState, ZDecimal weight, ZString description, string receiveAs = "")
		{
			packageState.WPS_ReceivedAs = string.IsNullOrEmpty(receiveAs) ? TransitWarehouseReceiveAs.Codes.ScannedIn : receiveAs;
			packageState.Package.KP_Weight = weight;
			packageState.Package.KP_GoodsDescription = description;
		}

		public override ZGuid GetDocumentID() => TransitDocDataConstants.TransitDocTemplateIDs.CIN750DeconsFromDCNTemplateID;

		protected override ZString CreateContent() =>
@"[3,4] Reference Type
[3,16] Reference Value
[3,28] WAREHOUSE-DECONS (CIN 750)
[4,4] HWB - House Air Waybill
[4,16] HSB1
[5,4] Enterprise Code
[5,16] Movement Time
[6,4] EDIDAT
[6,16] 01-Dec-23 01:02:03
[7,4] From Goods Reference Type
[7,28] From Goods Reference Value
[8,4] AWB - Master Air Waybill
[8,28] MAB1
[9,4] From Goods PNTS Temporary Storage Number(s)
[10,4] TSD1
[11,4] From Goods Description
[12,4] BOOKS
[13,4] From Gross Weight
[13,28] From Gross Quantity
[14,4] 123.3 KG
[14,28] 1
[15,4] To Goods Reference Type
[15,28] To Goods Reference Value
[16,4] HWB - House Air Waybill
[16,28] HSB1
[17,4] To Goods PNTS Temporary Storage Number(s)
[18,4] TSD1
[19,4] To Goods Description
[20,4] BOOKS
[21,4] To Gross Weight
[21,28] To Gross Quantity
[22,4] 26.6 KG
[22,28] 2
[24,4] CFS/TWH Warehouse
[25,4] HEADER
[26,4] #1
[30,4] CFS/TWH CIN Code
[31,4] NOTCIN
[34,42] Created By";
	}
}
