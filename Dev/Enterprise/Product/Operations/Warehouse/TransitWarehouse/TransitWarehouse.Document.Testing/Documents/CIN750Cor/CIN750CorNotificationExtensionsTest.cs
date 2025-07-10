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
	public class CIN750CorNotificationExtensionsTest : CIN750NotificationExtensionsTest<CIN750CorNotificationExtensions, CIN750CorNotification>
	{
		protected override CIN750CorNotificationExtensions GetExtensions(bool succeed = true)
		{
			var rcn = Consignment as WhsItemReceiveConsignment;
			var notification = new CIN750CorNotificationBuilder(rcn).Build();
			notification.MessageID = "b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118";

			if (!succeed)
			{
				notification.Goods.First().AmountQuantity = -100;
			}

			var dynamicData = new Mock<IDynamicData>();
			dynamicData.Setup(d => d.Value).Returns(notification);
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(TransitDocDataContext.CIN750WarehouseCor);
			document.Setup(d => d.Data).Returns(dynamicData.Object);

			var documentInstruction = new Mock<IMessageInstructions>();
			return new CIN750CorNotificationExtensions(document.Object, documentInstruction.Object);
		}

		WhsItemReceiveConsignment CreateRCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 800, weightUQ: "G", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;
			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8");
			Factory.Save();

			return rcn;
		}

		protected override IStmNoteParent Consignment => receiveConsignment ?? (receiveConsignment = CreateRCN());
		WhsItemReceiveConsignment receiveConsignment;

		protected override ZString ExpectedCINMessageNote_Succeed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type    Result     Ref Type    Ref Code           Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Correction      Succeed    REF         EDIDATRC0000001    EDIDAT             Header               C001
Goods Details:
    Quantity    Weight       PNTS    Description
    -5          -8.800 KG    -       Test Description

";

		protected override ZString ExpectedCINMessageNote_Failed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Failed reason: Cannot Send CIN 750 Cor Notification because the reported Packages Quantity adjustment (-100) exceeds the Quantity of In Notification (5).
CIN 750 Notification:
    Message Type    Result    Ref Type    Ref Code           Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Correction      Failed    REF         EDIDATRC0000001    EDIDAT             Header               C001
Goods Details:
    Quantity    Weight       PNTS    Description
    -100        -8.800 KG    -       Test Description

";
	}
}
