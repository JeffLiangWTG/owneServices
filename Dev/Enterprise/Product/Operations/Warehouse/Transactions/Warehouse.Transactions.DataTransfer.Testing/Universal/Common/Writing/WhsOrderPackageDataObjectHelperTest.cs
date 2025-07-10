using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsOrderPackageDataObjectHelperTest : WhsTestCaseWithFactory
	{
		public void TestGetWhsOrderPackageDataObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order, "CONT123", "20GP", false, false);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");

			var orderDataObjectForPackage = WhsOrderPackageDataObjectHelper.GetWhsOrderPackageDataObject(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package)), order, ExcludeElement.None);
			AssertEquals(nameof(orderDataObjectForPackage.Order.ClientReference), "CLIENT REF", orderDataObjectForPackage.Order.ClientReference);
			AssertEquals(nameof(orderDataObjectForPackage.Order.TransportReference), "TRANSPORT REF", orderDataObjectForPackage.Order.TransportReference);

			var warehouseClientAddress = orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertEquals("Warehouse Client Organisation should be correct.", data.Org1.OH_Code, warehouseClientAddress.OrganizationCode);

			var warehouseAddress = orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorPickupDeliveryAddress));
			AssertEquals("Warehouse Address should be correct.", data.Whs1.WarehouseAddress.OA_Code, warehouseAddress.AddressShortCode);
			AssertNull("Customs Warehouse Address should not be present.", orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CustomsWarehouseAddress)));
			AssertNull("Carrier Booking Agent Address should not be present.", orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CarrierBookingAgent)));
		}

		public void TestGetWhsOrderPackageDataObject_OrderLineDictionary()
		{
			var warehouseHelper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var order = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = warehouseHelper.CreateClient("RTUS").PK;
			var orderLine1 = warehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = warehouseHelper.CreateWhsOrderLine(order, data.Part1, 15m);
			warehouseHelper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("BOX");
			var releaseLine1 = orderLine1.ReleaseLines[0];
			package.Pack(releaseLine1, 10m);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			package.Pack(releaseLine2, 15m);

			var orderLineDictionary = new Dictionary<ZGuid, ZInt>();
			WhsOrderPackageDataObjectHelper.GetWhsOrderPackageDataObject(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package)), order, ExcludeElement.None, orderLineDictionary);
			AssertEquals(2, orderLineDictionary.Count);
			Assert(orderLineDictionary.ContainsKey(releaseLine1.PK));
			Assert(orderLineDictionary.ContainsKey(releaseLine2.PK));
		}
	}
}
