using System.Linq;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public sealed class CIN750InNotificationBuilderTest : CIN750NotificationBuilderTest<CIN750InNotificationBuilder, WhsItemReceiveConsignment, CIN750InNotification>
	{
		public void TestBuild_RCNWithMasterBill()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			Factory.Save();

			var inNotification = new CIN750InNotificationBuilder(rcn).Build();
			AssertNotNull(inNotification);

			AssertEquals("DeclaredInWarehouse", "WH1Address", inNotification.DeclaredInWarehouse.AddressLine1);
			AssertEquals("DeclaredInWarehouseCIN", "C001", inNotification.DeclaredInWarehouseCIN);
			AssertEquals("RefType", "AWB", inNotification.RefType.Code);
			AssertEquals("RefCode", "MAB1", inNotification.RefCode);
			AssertEquals("FromCTO", "CTOAddress", inNotification.FromCTO.AddressLine1);
			AssertEquals("FromCTOCIN", "C002", inNotification.FromCTOCIN);
			AssertEquals("Custom Status Code", "C", inNotification.CustomsStatus.Code);
			AssertEquals("Custom Status Description", "Community", inNotification.CustomsStatus.Description);

			AssertEquals("Goods Count", 1, inNotification.Goods.Count);
			AssertEquals("Goods Amount", 5, inNotification.Goods.Single().AmountQuantity);
			AssertEquals("Goods Weight", 8.8m, inNotification.Goods.Single().AmountWeight);
			AssertEquals("Goods Description", "Test Description", inNotification.Goods.Single().Description);
			AssertEquals("Goods Accompany Document Ref", "CEN1", inNotification.Goods.Single().AccompanyDocumentRef);
			AssertEquals("Goods Accompany Document Ref", "T1", inNotification.Goods.Single().AccompanyDocumentType);
			AssertEquals("Goods Temporary Storage Declaration", "TST1", inNotification.Goods.Single().TemporaryStorageDeclaration);
		}

		public void TestBuild_NOTCIN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			Factory.Save();

			var inNotification = new CIN750InNotificationBuilder(rcn).Build();
			AssertNotNull(inNotification);

			AssertEquals("DeclaredInWarehouseCIN", "NOTCIN", inNotification.DeclaredInWarehouseCIN);
			AssertEquals("FromCTOCIN", "NOTCIN", inNotification.FromCTOCIN);
		}

		public void TestBuild_RCNWithoutMasterBill()
		{
			var rcn = CreateGeneralRCN();

			Factory.Save();

			var inNotification = new CIN750InNotificationBuilder(rcn).Build();
			AssertNotNull(inNotification);

			AssertEquals("RefType", "REF", inNotification.RefType.Code);
			AssertEquals("RefCode", "EDIDATRC0000001", inNotification.RefCode);
		}

		protected override void TestRemoveHypenCore()
		{
			var rcn = CreateGeneralRCN();
			Factory.Save();

			rcn.WRC_ConsignmentID = "RC0000001-1";

			var inNotification = new CIN750InNotificationBuilder(rcn).Build();
			AssertNotNull(inNotification);

			AssertEquals("RefType", "REF", inNotification.RefType.Code);
			AssertEquals("RefCode", "EDIDATRC00000011", inNotification.RefCode);
		}

		WhsItemReceiveConsignment CreateGeneralRCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);
			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			return rcn;
		}

		protected override CIN750InNotification GetGeneralNotification()
		{
			var rcn = CreateGeneralRCN();
			return new CIN750InNotificationBuilder(rcn).Build();
		}
	}
}
