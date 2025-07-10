using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsPickUniversalShipmentDataObjectWriterTest : WhsTestCaseWithFactory
	{
		public void TestGetDataObject_Pick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order, "CONT123", "20GP", false, false);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "PKG1";
			Factory.Save();

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick))).GetDataObject(pick);

			var dataSourceCollection = pickDataObject.DataContext.DataSourceCollection;
			AssertEquals("Order is added as a data source.", 1, dataSourceCollection.Count());

			var dataSource = dataSourceCollection.Single();
			AssertEquals("Order is added as a data source.", nameof(DataContextType.WarehouseOrder), dataSource.Type);
			AssertEquals("Order is added as a data source.", order.WD_DocketID, dataSource.Key);

			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick has 1 order.", 1, subShipmentCollection.Count);
			var subShipment = subShipmentCollection.Single();

			var packingLines = subShipment.PackingLineCollection;
			AssertEquals("Order has 1 package.", 1, packingLines.Count);

			var packLine = packingLines.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG1");
			AssertEquals("ItemNo should set to the sequence of the package.", (short)1,  packLine.ItemNo);
		}

		public void TestGetDataObject_Pick_MultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order, "CONT123", "20GP", false, false);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("BOX");
			package1.KP_PackageID = "PKG1";
			var package2 = order.PackageJob.Packages.AddNew("BOX");
			package2.KP_PackageID = "PKG2";
			Factory.Save();

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick))).GetDataObject(pick);

			var dataSourceCollection = pickDataObject.DataContext.DataSourceCollection;
			AssertEquals("Order is added as a data source.", 1, dataSourceCollection.Count());

			var dataSource = dataSourceCollection.Single();
			AssertEquals("Order is added as a data source.", nameof(DataContextType.WarehouseOrder), dataSource.Type);
			AssertEquals("Order is added as a data source.", order.WD_DocketID, dataSource.Key);

			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick has 1 order.", 1, subShipmentCollection.Count);
			var subShipment = subShipmentCollection.Single();
			AssertDataContextForSubshipmentIsPopulated(subShipment);

			var packingLines = subShipment.PackingLineCollection;
			AssertEquals("Order has 2 packages.", 2, packingLines.Count);

			var packLine1 = packingLines.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG1");
			AssertEquals("ItemNo should set to the sequence of the package.", (short)1,  packLine1.ItemNo);

			var packLine2 = packingLines.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG2");
			AssertEquals("ItemNo should set to the sequence of the package.", (short)2,  packLine2.ItemNo);
		}

		public void TestGetDataObject_Pick_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = Helper.CreateClient("RTUS");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order1.CarrierBookingAgentDocAddress.OrganisationPK = client.PK;
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order1, "CONT123", "20GP", false, false);
			order1.WD_CustomerReference = "CLIENT REF";
			order1.WD_TransportReference = "TRANSPORT REF";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			order2.CarrierBookingAgentDocAddress.OrganisationPK = client.PK;
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order2, "CONT123", "20GP", false, false);
			order2.WD_CustomerReference = "CLIENT REF";
			order2.WD_TransportReference = "TRANSPORT REF";

			var pick = Helper.CreatePickNew(order1, order2);
			var package1 = order1.PackageJob.Packages.AddNew("BOX");
			package1.KP_PackageID = "PKG1";
			var package2 = order2.PackageJob.Packages.AddNew("BOX");
			package2.KP_PackageID = "PKG2";
			Factory.Save();

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick))).GetDataObject(pick);

			var dataSourceCollection = pickDataObject.DataContext.DataSourceCollection;
			AssertEquals(2, dataSourceCollection.Count());

			var dataSource1 = dataSourceCollection.Single(dts => dts.Key.Value == order1.WD_DocketID);
			AssertEquals("Order1 is a data source.", nameof(DataContextType.WarehouseOrder), dataSource1.Type);
			var dataSource2 = dataSourceCollection.Single(dts => dts.Key.Value == order2.WD_DocketID);
			AssertEquals("Order2 is a data source.", nameof(DataContextType.WarehouseOrder), dataSource2.Type);

			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick data object has 2 sub shipments - order1 and order2.", 2, subShipmentCollection.Count);

			var subShipment1 = subShipmentCollection.Single(subShipment => subShipment.DataContext.DataSourceCollection.Single().Key.Value == order1.WD_DocketID);
			AssertDataContextForSubshipmentIsPopulated(subShipment1);
			var packingLines1 = subShipment1.PackingLineCollection;
			AssertEquals("Order1 has 1 package.", 1, packingLines1.Count);
			var packLineSubShipment1 = packingLines1.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG1");

			var subShipment2 = subShipmentCollection.Single(subShipment => subShipment.DataContext.DataSourceCollection.Single().Key.Value == order2.WD_DocketID);
			AssertDataContextForSubshipmentIsPopulated(subShipment2);
			var packingLines2 = subShipment2.PackingLineCollection;
			AssertEquals("Order2 has 1 package.", 1, packingLines2.Count);
			var packLineSubShipment2 = packingLines2.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG2");
		}

		public void TestGetDataObject_Pick_MultipleOrders_WithMultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = Helper.CreateClient("RTUS");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order1.CarrierBookingAgentDocAddress.OrganisationPK = client.PK;
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order1, "CONT123", "20GP", false, false);
			order1.WD_CustomerReference = "CLIENT REF";
			order1.WD_TransportReference = "TRANSPORT REF";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			order2.CarrierBookingAgentDocAddress.OrganisationPK = client.PK;
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order2, "CONT123", "20GP", false, false);
			order2.WD_CustomerReference = "CLIENT REF";
			order2.WD_TransportReference = "TRANSPORT REF";

			var pick = Helper.CreatePickNew(order1, order2);
			var package1 = order1.PackageJob.Packages.AddNew("BOX");
			package1.KP_PackageID = "PKG1";
			var package2 = order1.PackageJob.Packages.AddNew("BOX");
			package2.KP_PackageID = "PKG2";
			var package3 = order2.PackageJob.Packages.AddNew("BOX");
			package3.KP_PackageID = "PKG3";
			var package4 = order2.PackageJob.Packages.AddNew("BOX");
			package4.KP_PackageID = "PKG4";
			Factory.Save();

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick))).GetDataObject(pick);

			var dataSourceCollection = pickDataObject.DataContext.DataSourceCollection;
			AssertEquals(2, dataSourceCollection.Count());

			var dataSource1 = dataSourceCollection.Single(dts => dts.Key.Value == order1.WD_DocketID);
			AssertEquals("Order1 is a data source.", nameof(DataContextType.WarehouseOrder), dataSource1.Type);
			var dataSource2 = dataSourceCollection.Single(dts => dts.Key.Value == order2.WD_DocketID);
			AssertEquals("Order2 is a data source.", nameof(DataContextType.WarehouseOrder), dataSource2.Type);

			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick data object has 2 sub shipments - order1 and order2.", 2, subShipmentCollection.Count);

			var subShipment1 = subShipmentCollection.Single(subShipment => subShipment.DataContext.DataSourceCollection.Single().Key.Value == order1.WD_DocketID);
			AssertDataContextForSubshipmentIsPopulated(subShipment1);
			var packingLines1 = subShipment1.PackingLineCollection;
			AssertEquals("Order1 has 2 packages.", 2, packingLines1.Count);
			var packLineSubShipment1 = packingLines1.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG1");
			var packLineSubShipment2 = packingLines1.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG2");

			var subShipment2 = subShipmentCollection.Single(subShipment => subShipment.DataContext.DataSourceCollection.Single().Key.Value == order2.WD_DocketID);
			AssertDataContextForSubshipmentIsPopulated(subShipment2);
			var packingLines2 = subShipment2.PackingLineCollection;
			AssertEquals("Order2 has 2 package.", 2, packingLines2.Count);
			var packLineSubShipment3 = packingLines2.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG3");
			var packLineSubShipment4 = packingLines2.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG4");
		}

		public void TestGetDataObject_Pick_WithItemNumbersDictionary()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = Helper.CreateClient("RTUS");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order1.CarrierBookingAgentDocAddress.OrganisationPK = client.PK;
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order1, "CONT123", "20GP", false, false);
			order1.WD_CustomerReference = "CLIENT REF";
			order1.WD_TransportReference = "TRANSPORT REF";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			order2.CarrierBookingAgentDocAddress.OrganisationPK = client.PK;
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order2, "CONT123", "20GP", false, false);
			order2.WD_CustomerReference = "CLIENT REF";
			order2.WD_TransportReference = "TRANSPORT REF";

			var pick = Helper.CreatePickNew(order1, order2);
			var package1 = order1.PackageJob.Packages.AddNew("BOX");
			package1.KP_PackageID = "PKG1";
			var package2 = order2.PackageJob.Packages.AddNew("BOX");
			package2.KP_PackageID = "PKG2";
			Factory.Save();

			var order1ToPackages = new WhsOrderToPackageItemNumbers(order1);
			order1ToPackages.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(package1, 2));
			var order2ToPackages = new WhsOrderToPackageItemNumbers(order2);
			order2ToPackages.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(package2, 1));

			var ordersDictionary = new List<WhsOrderToPackageItemNumbers>();
			ordersDictionary.Add(order1ToPackages);
			ordersDictionary.Add(order2ToPackages);

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick)), ordersDictionary).GetDataObject(pick);

			var dataSourceCollection = pickDataObject.DataContext.DataSourceCollection;
			AssertEquals(2, dataSourceCollection.Count());

			var dataSource1 = dataSourceCollection.Single(dts => dts.Key.Value == order1.WD_DocketID);
			AssertEquals("Order1 is a data source.", nameof(DataContextType.WarehouseOrder), dataSource1.Type);
			var dataSource2 = dataSourceCollection.Single(dts => dts.Key.Value == order2.WD_DocketID);
			AssertEquals("Order2 is a data source.", nameof(DataContextType.WarehouseOrder), dataSource2.Type);

			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick data object has 2 sub shipments - order1 and order2.", 2, subShipmentCollection.Count);

			var subShipment1 = subShipmentCollection.Single(subShipment => subShipment.DataContext.DataSourceCollection.Single().Key.Value == order1.WD_DocketID);
			AssertDataContextForSubshipmentIsPopulated(subShipment1);
			var packingLines1 = subShipment1.PackingLineCollection;
			AssertEquals("Order1 has 1 package.", 1, packingLines1.Count);
			var packLineSubShipment1 = packingLines1.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG1");
			AssertEquals("ItemNo is correct based on item numbers dictionary.", (ZShort)2, packLineSubShipment1.ItemNo);

			var subShipment2 = subShipmentCollection.Single(subShipment => subShipment.DataContext.DataSourceCollection.Single().Key.Value == order2.WD_DocketID);
			AssertDataContextForSubshipmentIsPopulated(subShipment2);
			var packingLines2 = subShipment2.PackingLineCollection;
			AssertEquals("Order2 has 1 package.", 1, packingLines2.Count);
			var packLineSubShipment2 = packingLines2.Single(packingLine => packingLine.ReferenceNumber.Value == "PKG2");
			AssertEquals("ItemNo is correct based on item numbers dictionary.", (ZShort)1, packLineSubShipment2.ItemNo);
		}

		public void TestGetDataObject_Pick_WithOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsDocketContainer(order, "CONT123", "20GP", false, false);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "PKG1";
			Factory.Save();

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick))).GetDataObject(pick);
			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick has 1 order.", 1, subShipmentCollection.Count);
			var orderDataObjectForPackage = subShipmentCollection.Single();
			AssertNotNull(nameof(orderDataObjectForPackage.Order.OrderLineCollection), orderDataObjectForPackage.Order.OrderLineCollection);

			var orderLineDataObject = orderDataObjectForPackage.Order.OrderLineCollection.Single();
			AssertEquals("OrderLine details are correct.", data.Part1.OP_PartNum, orderLineDataObject.Product.Code);
			AssertEquals("OrderLine details are correct.", 10m, orderLineDataObject.OrderedQty);
		}

		public void TestGetDataObject_Pick_WithOrderLines_WithDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine = Helper.CreateWhsDocketContainer(order, "CONT123", "20GP", false, false);
			order.WD_CustomerReference = "CLIENT REF";
			order.WD_TransportReference = "TRANSPORT REF";
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "PKG1";

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
			Factory.Save();

			var pickDataObject = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pick))).GetDataObject(pick);
			var subShipmentCollection = pickDataObject.SubShipmentCollection;
			AssertEquals("Pick has 1 order.", 1, subShipmentCollection.Count);
			var orderDataObjectForPackage = subShipmentCollection.Single();
			var orderLineDataObject = orderDataObjectForPackage.Order.OrderLineCollection.Single();
			var undgDataObject = orderLineDataObject.UNDGCollection[0];
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

		#region Test Helpers

		static void AssertDataContextForSubshipmentIsPopulated(ITopLevelDataObject subshipment)
		{
			var ids = subshipment.DataContext.GetEnterpriseServerAndCompanyIDs();
			AssertNotNullOrEmpty("Company code should not be null or empty.", ids.CompanyCode);
			AssertNotNullOrEmpty("Enterprise ID should not be null or empty.", ids.EnterpriseID);
			AssertNotNullOrEmpty("Server ID should not be null or empty.", ids.ServerID);
		}

		#endregion
	}
}
