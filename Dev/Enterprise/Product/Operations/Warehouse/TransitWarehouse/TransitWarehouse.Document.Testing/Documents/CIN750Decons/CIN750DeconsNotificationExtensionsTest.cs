using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750DeconsNotificationExtensionsTest : CIN750NotificationExtensionsTest<CIN750DeconsNotificationExtensions, CIN750DeconsNotification>
	{
		protected override CIN750DeconsNotificationExtensions GetExtensions(bool succeed = true)
		{
			var dcn = Consignment as WhsItemDispatchConsignment;
			var notification = new CIN750DeconsNotificationBuilder(dcn).Build();
			notification.MessageID = "b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118";

			if (!succeed)
			{
				notification.GoodsPairs.First().Item1.AmountQuantity = 100;
			}

			var dynamicData = new Mock<IDynamicData>();
			dynamicData.Setup(d => d.Value).Returns(notification);
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(TransitDocDataContext.CIN750WarehouseDecons);
			document.Setup(d => d.Data).Returns(dynamicData.Object);

			var documentInstruction = new Mock<IMessageInstructions>();
			return new CIN750DeconsNotificationExtensions(document.Object, documentInstruction.Object);
		}

		WhsItemDispatchConsignment CreateDCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			dcn.WDC_HouseBillNumber = "HSB1";
			var packageState = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll);
			packageState.Package.KP_Weight = 2.3;
			packageState.Package.KP_GoodsDescription = "Test Description";
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=AWB|RFN=MAB-1|WGT=2.3");

			return dcn;
		}

		protected override IStmNoteParent Consignment => dispatchConsignment ?? (dispatchConsignment = CreateDCN());
		WhsItemDispatchConsignment dispatchConsignment;

		protected override ZString ExpectedCINMessageNote_Succeed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type       Result     Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Deconsolidation    Succeed    HWB         HSB1        EDIDAT             Header               NOTCIN
From Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    AWB         MAB1        -
To Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    HWB         HSB1        -

";
		protected override ZString ExpectedCINMessageNote_Failed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Failed reason: Cannot Send CIN 750 Deconsolidation Notification because from goods amount (100) is greater than original from goods amount (1).
CIN 750 Notification:
    Message Type       Result    Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Deconsolidation    Failed    HWB         HSB1        EDIDAT             Header               NOTCIN
From Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    100         2.300 KG    Test Description    AWB         MAB1        -
To Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    HWB         HSB1        -

";
	}
}
