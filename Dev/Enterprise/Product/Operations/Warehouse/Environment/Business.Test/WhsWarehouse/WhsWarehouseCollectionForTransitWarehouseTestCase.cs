using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsWarehouseCollectionForTransitWarehouse))]
	class WhsWarehouseCollectionForTransitWarehouseTestCase : WhsBusinessObjectCollectionTestCase
	{
		#region TestRelationshipAndAdditionalFilters

		public void TestRelationshipAndAdditionalFilters()
		{
			void AssertWhsInCollection(WhsWarehouse whs, BusinessObjectCollection whsCollection, bool contains)
			{
				AssertCollectionContains(
					$"Warehouse {(contains ? "not found" : "found")} in collection: {whs.WW_WarehouseName} with type {whs.WW_WarehouseType} and branch {whs.RelatedCompanyBranch?.GB_Code}",
					whs,
					whsCollection,
					contains);
			}

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = Helper.CreateGlbBranch("OTH");
			var whs1 = SetupWarehouse("Whs1", WarehouseTypes.Codes.Product, true);
			var whs11 = SetupWarehouse("Whs11", WarehouseTypes.Codes.Product, true, branch1);
			var whs12 = SetupWarehouse("Whs12", WarehouseTypes.Codes.Product, true, branch2);
			var whs2 = SetupWarehouse("Whs2", WarehouseTypes.Codes.ContainerYard, true);
			var whs21 = SetupWarehouse("Whs21", WarehouseTypes.Codes.ContainerYard, true, branch1);
			var whs22 = SetupWarehouse("Whs22", WarehouseTypes.Codes.ContainerYard, true, branch2);
			var whs3 = SetupWarehouse("Whs3", WarehouseTypes.Codes.FreeTradeZone, true);
			var whs31 = SetupWarehouse("Whs31", WarehouseTypes.Codes.FreeTradeZone, true, branch1);
			var whs32 = SetupWarehouse("Whs32", WarehouseTypes.Codes.FreeTradeZone, true, branch2);
			var whs4 = SetupWarehouse("Whs4", WarehouseTypes.Codes.Transit, true);
			var whs41 = SetupWarehouse("Whs41", WarehouseTypes.Codes.Transit, true, branch1);
			var whs42 = SetupWarehouse("Whs42", WarehouseTypes.Codes.Transit, true, branch2);
			var whs5 = SetupWarehouse("Whs5", WarehouseTypes.Codes.Transit, false);
			var whs51 = SetupWarehouse("Whs51", WarehouseTypes.Codes.Transit, false, branch1);
			var whs52 = SetupWarehouse("Whs52", WarehouseTypes.Codes.Transit, false, branch2);

			var collection = GetCollectionToTest();

			CombineAssertions("Load including additional filters. Only transit warehouses on current branch should be found", () =>
			{
				collection.Load();
				AssertWhsInCollection(whs1, collection, false);
				AssertWhsInCollection(whs11, collection, false);
				AssertWhsInCollection(whs12, collection, false);
				AssertWhsInCollection(whs2, collection, false);
				AssertWhsInCollection(whs21, collection, false);
				AssertWhsInCollection(whs22, collection, false);
				AssertWhsInCollection(whs3, collection, false);
				AssertWhsInCollection(whs31, collection, false);
				AssertWhsInCollection(whs32, collection, false);
				AssertWhsInCollection(whs4, collection, false);
				AssertWhsInCollection(whs41, collection, true);
				AssertWhsInCollection(whs42, collection, false);
				AssertWhsInCollection(whs5, collection, false);
				AssertWhsInCollection(whs51, collection, true);
				AssertWhsInCollection(whs52, collection, false);
			});

			CombineAssertions("Load ignoring additional filters to keep only the relationship filters. All transit warehouses should be found.", () =>
			{
				collection.Load(new ZQuery());
				AssertWhsInCollection(whs1, collection, false);
				AssertWhsInCollection(whs11, collection, false);
				AssertWhsInCollection(whs12, collection, false);
				AssertWhsInCollection(whs2, collection, false);
				AssertWhsInCollection(whs21, collection, false);
				AssertWhsInCollection(whs22, collection, false);
				AssertWhsInCollection(whs3, collection, false);
				AssertWhsInCollection(whs31, collection, false);
				AssertWhsInCollection(whs32, collection, false);
				AssertWhsInCollection(whs4, collection, true);
				AssertWhsInCollection(whs41, collection, true);
				AssertWhsInCollection(whs42, collection, true);
				AssertWhsInCollection(whs5, collection, true);
				AssertWhsInCollection(whs51, collection, true);
				AssertWhsInCollection(whs52, collection, true);
			});
		}

		#endregion

		#region TestAddNotificationWhenAdditionalFilterNotMet

		public void TestAddNotificationWhenAdditionalFilterNotMet_Branch()
		{
			var collection = GetCollectionToTest();
			var whs = SetupWarehouse("Whs", WarehouseTypes.Codes.Transit, true);
			AssertEquals("This Transit Warehouse cannot be chosen at this time, log into the correct branch to select this warehouse.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			AssertNotEquals("This Transit Warehouse cannot be chosen at this time, log into the correct branch to select this warehouse.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_GB_RelatedCompanyBranch = ZGuid.NewZGuid();
			AssertEquals("This Transit Warehouse cannot be chosen at this time, log into the correct branch to select this warehouse.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet_WarehouseType()
		{
			var collection = GetCollectionToTest();
			var whs = SetupWarehouse("Whs", WarehouseTypes.Codes.Transit, true, GlbBranch.CurrentBranch);
			AssertNotEquals("Only Transit Warehouses may be selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals("Only Transit Warehouses may be selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals("Only Transit Warehouses may be selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals("Only Transit Warehouses may be selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(whs));
		}

		#endregion

		#region Implementation

		WhsWarehouse SetupWarehouse(ZString name, ZString whsType, bool isActive, GlbBranch branch = null)
		{
			var whs = Helper.CreateWarehouse(name);
			whs.WW_WarehouseType = whsType;
			whs.WW_IsActive = isActive;
			if (branch != null)
			{
				whs.WW_GB_RelatedCompanyBranch = branch.PK;
			}
			return whs;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsWarehouseCollectionForTransitWarehouse(Factory);
		}

		#endregion
	}
}
