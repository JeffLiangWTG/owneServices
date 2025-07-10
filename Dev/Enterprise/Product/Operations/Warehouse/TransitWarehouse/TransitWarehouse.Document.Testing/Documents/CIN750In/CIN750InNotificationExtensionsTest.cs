using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750InNotificationExtensionsTest : CIN750NotificationExtensionsTest<CIN750InNotificationExtensions, CIN750InNotification>
	{
		protected override CIN750InNotificationExtensions GetExtensions(bool succeed = true)
		{
			var rcn = Consignment as WhsItemReceiveConsignment;
			var notification = new CIN750InNotificationBuilder(rcn).Build();
			notification.MessageID = "b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118";

			if (!succeed)
			{
				notification.Goods.First().AmountQuantity = 100;
			}

			var dynamicData = new Mock<IDynamicData>();
			dynamicData.Setup(d => d.Value).Returns(notification);
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(TransitDocDataContext.CIN750WarehouseIn);
			document.Setup(d => d.Data).Returns(dynamicData.Object);

			var documentInstruction = new Mock<IMessageInstructions>();
			return new CIN750InNotificationExtensions(document.Object, documentInstruction.Object);
		}

		WhsItemReceiveConsignment CreateRCN()
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

			Factory.Save();

			return rcn;
		}

		protected override IStmNoteParent Consignment => receiveConsignment ?? (receiveConsignment = CreateRCN());
		WhsItemReceiveConsignment receiveConsignment;

		protected override ZString ExpectedCINMessageNote_Succeed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type    Result     Ref Type    Ref Code           Enterprise Code    Customs Status    From CTO    CTO CIN Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    In              Succeed    REF         EDIDATRC0000001    EDIDAT             C                 WHTEST      C002            Header               C001
Goods Details:
    Quantity    Weight      Doc Type    Doc Ref    TSD     Description
    5           8.800 KG    T1          CEN1       TST1    Test Description

";

		protected override ZString ExpectedCINMessageNote_Failed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Failed reason: Cannot Send CIN 750 In Notification because Amount Quantity (100) is greater than the Quantity of Received Packages that haven't been reported (5).
CIN 750 Notification:
    Message Type    Result    Ref Type    Ref Code           Enterprise Code    Customs Status    From CTO    CTO CIN Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    In              Failed    REF         EDIDATRC0000001    EDIDAT             C                 WHTEST      C002            Header               C001
Goods Details:
    Quantity    Weight      Doc Type    Doc Ref    TSD     Description
    100         8.800 KG    T1          CEN1       TST1    Test Description

";
	}
}
