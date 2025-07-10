using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750ConsNotificationExtensionsTest : CIN750NotificationExtensionsTest<CIN750ConsNotificationExtensions, CIN750ConsNotification>
	{
		protected override CIN750ConsNotificationExtensions GetExtensions(bool succeed = true)
		{
			var dcn = Consignment as WhsItemDispatchConsignment;

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(dcn, historyManager.AdditionalDataForCons).Build();
			notification.MessageID = "b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118";

			if (!succeed)
			{
				notification.FromGoods.First().AmountQuantity = 100;
			}

			var dynamicData = new Mock<IDynamicData>();
			dynamicData.Setup(d => d.Value).Returns(notification);
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(TransitDocDataContext.CIN750WarehouseDecons);
			document.Setup(d => d.Data).Returns(dynamicData.Object);

			var documentInstruction = new Mock<IMessageInstructions>();
			return new CIN750ConsNotificationExtensions(document.Object, documentInstruction.Object);
		}

		WhsItemDispatchConsignment CreateDCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=13");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll, weight: 10);
			packageState1.Package.KP_GoodsDescription = "DESC 1";
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll, weight: 3);

			Factory.Save();

			return dcn;
		}

		protected override IStmNoteParent Consignment => dispatchConsignment ?? (dispatchConsignment = CreateDCN());
		WhsItemDispatchConsignment dispatchConsignment;

		protected override ZString ExpectedCINMessageNote_Succeed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type     Result     Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Consolidation    Succeed    HWB         HSB1        EDIDAT             Header               C001
From Goods Details:
    Quantity    Weight       Description    Ref Type    Ref Code           PNTS
    2           13.000 KG    DESC 1         REF         EDIDATRC0000001    -
To Goods Details:
    Quantity    Weight       Description    Ref Type    Ref Code    PNTS
    2           13.000 KG    DESC 1         HWB         HSB1        -

";

		protected override ZString ExpectedCINMessageNote_Failed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Failed reason: House Bill or Reference quantity or weight cannot be greater than DCN quantity or weight.
CIN 750 Notification:
    Message Type     Result    Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Consolidation    Failed    HWB         HSB1        EDIDAT             Header               C001
From Goods Details:
    Quantity    Weight       Description    Ref Type    Ref Code           PNTS
    100         13.000 KG    DESC 1         REF         EDIDATRC0000001    -
To Goods Details:
    Quantity    Weight       Description    Ref Type    Ref Code    PNTS
    2           13.000 KG    DESC 1         HWB         HSB1        -

";
	}
}
