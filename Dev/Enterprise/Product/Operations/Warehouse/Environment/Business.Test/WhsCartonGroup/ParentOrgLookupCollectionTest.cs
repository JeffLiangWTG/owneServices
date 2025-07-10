using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business
{
	[TestedType(typeof(ParentOrgLookupCollection))]
	class ParentOrgLookupCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		#region TestAdditionalFilter

		public void TestAdditionalFilter()
		{
			var invalidOrg = Helper.CreateClient("1");
			var carrier = Helper.CreateClient("2");
			var whsClient = Helper.CreateClient("3");
			var consignee = Helper.CreateClient("4");
			var alreadyAttached = Helper.CreateClient("5");
			var whs = Helper.CreateWarehouse("TEST"); // Creates an Org

			invalidOrg.OH_IsWarehouseClient = false;
			whsClient.OH_IsWarehouseClient = true;
			carrier.OH_IsShippingProvider = true;
			consignee.OH_IsConsignee = true;
			alreadyAttached.MiscServ.OM_WCG_CartonGroup = Helper.CreateWhsCartonGroup("1", "1").PK;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals("Precondtion - collection should not be loaded yet.", 0, collection.Count);

			collection.Load();
			AssertCollectionContains(carrier, collection);
			AssertCollectionContains(whsClient, collection);
			AssertCollectionContains(consignee, collection);
			AssertCollectionContains(whs.WarehouseAddress.Header, collection);

			AssertCollectionNotContains(invalidOrg, collection);
			AssertCollectionNotContains(alreadyAttached, collection);
		}

		#endregion

		#region TestAddNotificationWhenAdditionalFilterNotMet

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var invalidOrg = Helper.CreateClient("1");
			var carrier = Helper.CreateClient("2");
			var whsClient = Helper.CreateClient("3");
			var consignee = Helper.CreateClient("4");
			var whsOrg = Helper.CreateWarehouse("TEST").WarehouseAddress.Header;

			invalidOrg.OH_IsWarehouseClient = false;
			carrier.OH_IsShippingProvider = true;
			consignee.OH_IsConsignee = true;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals("An Organization selected from here must have an Organization type of Carrier, Warehouse Client, Consignee or be linked to a Warehouse record.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(invalidOrg));

			AssertNotEquals("An Organization selected from here must have an Organization type of Carrier, Warehouse Client, Consignee or be linked to a Warehouse record.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(carrier));
			AssertNotEquals("An Organization selected from here must have an Organization type of Carrier, Warehouse Client, Consignee or be linked to a Warehouse record.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(whsClient));
			AssertNotEquals("An Organization selected from here must have an Organization type of Carrier, Warehouse Client, Consignee or be linked to a Warehouse record.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(consignee));
			AssertNotEquals("An Organization selected from here must have an Organization type of Carrier, Warehouse Client, Consignee or be linked to a Warehouse record.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(whsOrg));

			carrier.MiscServ.OM_WCG_CartonGroup = Helper.CreateWhsCartonGroup("1", "1").PK;
			AssertEquals("This Organization is already linked to Carton Group 1.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(carrier));
		}

		#endregion

		#region TestSetFilterBusinessObjectDefaults

		public void TestSetFilterBusinessObjectDefaults()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property7"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "AndJoinCondition"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "OrJoinCondition"));

			AssertEquals(true, collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"].Value);
			AssertEquals(true, collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"].Value);
			AssertEquals(true, collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property7"].Value);
			AssertEquals(false, collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "AndJoinCondition"].Value);
			AssertEquals(true, collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "OrJoinCondition"].Value);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ParentOrgLookupCollection(Factory);
		}

		#endregion
	}
}
