using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsOrderPackageDataObjectWriterTest : PackageParentExporterTestCase<WhsOrder>
	{
		#region TestGetDataObject_PkgPackage

		public void TestGetDataObject_PkgPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = WarehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order.CarrierBookingAgentDocAddress.OrganisationPK = WarehouseHelper.CreateClient("RTUS").PK;
			WarehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			WarehouseHelper.CreateWhsDocketContainer(order, "CONT123", "20GP", false, false);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			var pick = WarehouseHelper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");

			var orderDataObjectForPackage = new WhsOrderPackageDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package))).GetDataObject(package);
			AssertEquals(nameof(orderDataObjectForPackage.Order.ClientReference), "CLIENT REF", orderDataObjectForPackage.Order.ClientReference);
			AssertEquals(nameof(orderDataObjectForPackage.Order.TransportReference), "TRANSPORT REF", orderDataObjectForPackage.Order.TransportReference);

			var warehouseClientAddress = orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertEquals("Warehouse Client Organisation should be correct.", data.Org1.OH_Code, warehouseClientAddress.OrganizationCode);
			AssertNotNull(nameof(orderDataObjectForPackage.Order.OrderLineCollection), orderDataObjectForPackage.Order.OrderLineCollection);
			AssertNull(nameof(orderDataObjectForPackage.ContainerCollection), orderDataObjectForPackage.ContainerCollection);
			AssertNull(nameof(orderDataObjectForPackage.PackingLineCollection), orderDataObjectForPackage.PackingLineCollection);
		}

		public void TestGetDataObject_PkgPackage_WithOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = WarehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order.CarrierBookingAgentDocAddress.OrganisationPK = WarehouseHelper.CreateClient("RTUS").PK;
			WarehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			WarehouseHelper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");

			var orderDataObjectForPackage = new WhsOrderPackageDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package))).GetDataObject(package);
			AssertEquals(nameof(orderDataObjectForPackage.Order.ClientReference), "CLIENT REF", orderDataObjectForPackage.Order.ClientReference);
			AssertEquals(nameof(orderDataObjectForPackage.Order.TransportReference), "TRANSPORT REF", orderDataObjectForPackage.Order.TransportReference);

			var warehouseClientAddress = orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertEquals("Warehouse Client Organisation should be correct.", data.Org1.OH_Code, warehouseClientAddress.OrganizationCode);
			var orderLineDataObject = orderDataObjectForPackage.Order.OrderLineCollection.Single();
			AssertEquals("OrderLine details are correct.", data.Part1.OP_PartNum, orderLineDataObject.Product.Code);
			AssertEquals("OrderLine details are correct.", 10m, orderLineDataObject.OrderedQty);
		}

		public void TestGetDataObject_PkgPackage_WithOrderLines_WithDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = WarehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order.CarrierBookingAgentDocAddress.OrganisationPK = WarehouseHelper.CreateClient("RTUS").PK;
			var orderLine = WarehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";

			var contact = orderLine.Docket.Client.Contacts.AddNew();
			contact.OC_ContactName = "JohnSmith";
			contact.OC_Phone = "123456789";

			var undg1 = data.Part1.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3000";
			substance.DG_Variant = "c";
			substance.DG_FlashPoint = "100 C";
			substance.DG_Class = "Clas";
			substance.DG_PG = "Gr1";
			substance.DG_PSN = "Name1";
			substance.DG_TechName = "T";
			substance.DG_MP = "Y";
			substance.DG_SubLabel1 = "TEST";
			substance.DG_SubLabel2 = "AAA";

			undg1.DI_OC_DGContact = contact.PK;
			undg1.DI_DG = substance.PK;
			undg1.DI_DGFlashPoint = 0.1m;
			undg1.DI_DGVolume = 1m;
			undg1.DI_DGWeight = 2m;
			undg1.DI_MPMarinePollutant = "Y";
			undg1.DI_UnitOfWeight = "kg";
			undg1.DI_UnitOfVolume = "m3";
			undg1.DI_TechnicalName = "Tech1";
			undg1.DI_IsLimitedQuantity = true;

			WarehouseHelper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");

			var orderDataObjectForPackage = new WhsOrderPackageDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package))).GetDataObject(package);
			AssertEquals(nameof(orderDataObjectForPackage.Order.ClientReference), "CLIENT REF", orderDataObjectForPackage.Order.ClientReference);
			AssertEquals(nameof(orderDataObjectForPackage.Order.TransportReference), "TRANSPORT REF", orderDataObjectForPackage.Order.TransportReference);

			var warehouseClientAddress = orderDataObjectForPackage.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertEquals("Warehouse Client Organisation should be correct.", data.Org1.OH_Code, warehouseClientAddress.OrganizationCode);
			var orderLineDataObject = orderDataObjectForPackage.Order.OrderLineCollection.Single();
			var undgDataObject = orderLineDataObject.UNDGCollection.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Contact.FullName", "JohnSmith", undgDataObject.Contact.FullName);
				AssertEquals("Contact.Phone", "123456789", undgDataObject.Contact.Phone);
				AssertEquals("FlashPoint", "0.1", undgDataObject.FlashPoint);
				AssertEquals("IMOClass", "Clas", undgDataObject.IMOClass);
				AssertEquals("MarinePollutant.Code", "Y", undgDataObject.MarinePollutant.Code);
				AssertEquals("PackedInLimitedQuantity", true, undgDataObject.PackedInLimitedQuantity);
				AssertEquals("PackingGroup", "Gr1", undgDataObject.PackingGroup);
				AssertEquals("ProperShippingName", "Name1", undgDataObject.ProperShippingName);
				AssertEquals("TechicalName", "Tech1", undgDataObject.TechicalName);
				AssertEquals("UNDGCode", "3000c", undgDataObject.UNDGCode);
				AssertEquals("Volume", 1m, undgDataObject.Volume);
				AssertEquals("Weight", 2m, undgDataObject.Weight);
				AssertEquals("WeightUQ", "kg", undgDataObject.WeightUQ.Code);
				AssertEquals("VolumeUQ", "m3", undgDataObject.VolumeUQ.Code);
				AssertEquals("SubLabel1", "TEST", undgDataObject.SubLabel1);
				AssertEquals("SubLabel2", "AAA", undgDataObject.SubLabel2);
			});
		}

		#endregion

		#region IOrderLineDictionaryProvider

		public void TestWhsOrderPackageDataObjectWriterIsIOrderLineDictionaryProvider()
		{
			var orderPackageDataObjectWriter = new WhsOrderPackageDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<PkgPackage>())));
			Assert(orderPackageDataObjectWriter is IOrderLineDictionaryProvider);
		}

		public void TestWhsOrderPackageDataObjectWriter_OrderLineDictionary()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = WarehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var order = WarehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = WarehouseHelper.CreateClient("RTUS").PK;
			var orderLine1 = WarehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = WarehouseHelper.CreateWhsOrderLine(order, data.Part1, 15m);
			WarehouseHelper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("BOX");
			var releaseLine1 = orderLine1.ReleaseLines[0];
			package.Pack(releaseLine1, 10m);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			package.Pack(releaseLine2, 15m);

			var orderPackageDataObjectWriter = new WhsOrderPackageDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package)));
			orderPackageDataObjectWriter.GetDataObject(package);
			var orderLineDictionary = ((IOrderLineDictionaryProvider)orderPackageDataObjectWriter).GetOrderLineDictionary();
			AssertEquals(2, orderLineDictionary.Count);
			Assert(orderLineDictionary.ContainsKey(releaseLine1.PK));
			Assert(orderLineDictionary.ContainsKey(releaseLine2.PK));

			AssertNoExceptionThrown(() => orderPackageDataObjectWriter.GetDataObject(package));
			AssertEquals(2, orderLineDictionary.Count);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions WarehouseHelper => warehouseHelper ?? (warehouseHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions warehouseHelper;

		protected override WhsOrder GetPackingParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return new WhsTestHelperFunctions(Factory).CreateWhsOrder(data.Org1, data.Whs1);
		}

		protected override ParentJobType ParentJobType => ParentJobType.WarehouseOrder;

		protected override Type ParentWriterType => typeof(WhsOrderPackageDataObjectWriter);

		#endregion
	}
}
