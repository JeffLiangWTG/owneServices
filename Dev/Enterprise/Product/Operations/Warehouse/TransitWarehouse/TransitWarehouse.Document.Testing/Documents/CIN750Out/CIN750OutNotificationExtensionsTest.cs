using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750OutNotificationExtensionsTest : CIN750NotificationExtensionsTest<CIN750OutNotificationExtensions, CIN750OutNotification>
	{
		protected override CIN750OutNotificationExtensions GetExtensions(bool succeed = true)
		{
			var dcn = Consignment as WhsItemDispatchConsignment;
			if (!succeed)
			{
				var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery()).Where(a => a.XV_Name == "SourceType");
				foreach (var addOnValue in addOnValues)
				{
					addOnValue.XV_Name = "NotSourceType";
				}
				Factory.Save();
			}

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 5,
				PackageWeightReadyToOut = 8.8m
			};
			var notification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			notification.MessageID = "b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118";

			var dynamicData = new Mock<IDynamicData>();
			dynamicData.Setup(d => d.Value).Returns(notification);
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(TransitDocDataContext.CIN750WarehouseOut);
			document.Setup(d => d.Data).Returns(dynamicData.Object);

			var documentInstruction = new Mock<IMessageInstructions>();
			return new CIN750OutNotificationExtensions(document.Object, documentInstruction.Object);
		}

		WhsItemDispatchConsignment CreateDCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			var crn = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			crn.PopulateAddOnValue("SourceType", "STR", "T1");

			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(dcn, DocAddressTypes.Codes.DepartureCTOAddress, ctoAddress);
			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HB1";

			var packageState = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			packageState.Package.KP_GoodsDescription = "ppp";

			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			Factory.Save();

			return dcn;
		}

		protected override IStmNoteParent Consignment => dispatchConsignment ?? (dispatchConsignment = CreateDCN());
		WhsItemDispatchConsignment dispatchConsignment;

		protected override ZString ExpectedCINMessageNote_Succeed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: b6cebf1d-a3d9-4ff7-9aa0-50e5d1692118 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type    Result     Ref Type    Ref Code    Enterprise Code    Customs Status    To CTO    To CIN code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Out             Succeed    HWB         HB1         EDIDAT             C                 WHTEST    C002           Header               C001
Goods Details:
    Quantity    Weight      Description    PNTS
    5           8.800 KG    ppp            TSD1

";

		protected override ZString ExpectedCINMessageNote_Failed => @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Failed reason: Customs Documents is required.
CIN 750 Notification:
    Message Type    Result    Ref Type    Ref Code    Enterprise Code    Customs Status    To CTO    To CIN code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Out             Failed    HWB         HB1         EDIDAT             C                 WHTEST    C002           Header               C001
Goods Details:
    Quantity    Weight      Description    PNTS
    5           8.800 KG    ppp            TSD1

";
	}
}
