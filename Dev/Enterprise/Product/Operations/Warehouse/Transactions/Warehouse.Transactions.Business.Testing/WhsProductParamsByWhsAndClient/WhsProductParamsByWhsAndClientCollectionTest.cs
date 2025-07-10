using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsProductParamsByWhsAndClientCollection))]
	class WhsProductParamsByWhsAndClientCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsProductParamsByWhsAndClientCollection>
	{
		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChildDoesNotThrowException()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddNew().OU_OH = ZGuid.Empty;
			var collection = new WhsProductParamsByWhsAndClientCollection(part, Factory);
			collection.AddNew();
		}

		public void TestSetDefaultsForNewChild()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P1");
			var collection = new WhsProductParamsByWhsAndClientCollection(part, Factory);

			var o = collection.AddNew();
			AssertEquals(1, collection.Count);
			AssertEquals(client.PK, o.W3_OH);
		}

		public void TestFindWhsProductParamsByWhsAndClient()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P1");
			var collection = new WhsProductParamsByWhsAndClientCollection(part, Factory);

			var paramsByWhsAndClient = collection.AddNew();
			var whs = Factory.New<WhsWarehouse>();
			paramsByWhsAndClient.W3_WW = whs.PK;
			paramsByWhsAndClient.W3_OH = client.PK;

			AssertEquals(paramsByWhsAndClient, collection.FindWhsProductParamsByWhsAndClient(client.OH_Code, whs.PK));
			AssertNull(collection.FindWhsProductParamsByWhsAndClient("Some", whs.PK));
		}

		public void TestIWhsProductParamsByWhsAndClientCollection_FindWhsProductParamsByWhsAndClients()
		{
			var client1 = Helper.CreateClient();
			var client2 = Helper.CreateClient();
			var client3 = Helper.CreateClient();
			var part = Helper.CreateProduct(client1, "P1");
			Helper.CreateProductClientRelationShip(client2, part);
			Helper.CreateProductClientRelationShip(client3, part);
			var collection = new WhsProductParamsByWhsAndClientCollection(part, Factory);

			var whs = Factory.New<WhsWarehouse>();
			var paramsByWhsAndClient1 = collection.AddNew();
			paramsByWhsAndClient1.W3_WW = whs.PK;
			paramsByWhsAndClient1.W3_OH = client1.PK;

			var paramsByWhsAndClient2 = collection.AddNew();
			paramsByWhsAndClient2.W3_WW = whs.PK;
			paramsByWhsAndClient2.W3_OH = client2.PK;

			var paramsByWhsAndClient3 = collection.AddNew();
			paramsByWhsAndClient3.W3_WW = whs.PK;
			paramsByWhsAndClient3.W3_OH = client3.PK;

			AssertEquals(paramsByWhsAndClient1, collection.FindWhsProductParamsByWhsAndClient(client1.PK, whs.PK));
			AssertNull(collection.FindWhsProductParamsByWhsAndClient(ZGuid.BrettsGuid, whs.PK));

			AssertContainsExactElementsInAnyOrder(new[] { paramsByWhsAndClient3 }, collection.FindWhsProductParamsByWhsAndClients(new[] { client3.PK }, whs.PK));
			AssertContainsExactElementsInAnyOrder(new[] { paramsByWhsAndClient1, paramsByWhsAndClient2 }, collection.FindWhsProductParamsByWhsAndClients(new[] { client1.PK, client2.PK }, whs.PK));
			AssertContainsExactElementsInAnyOrder(new[] { paramsByWhsAndClient1, paramsByWhsAndClient2, paramsByWhsAndClient3 }, collection.FindWhsProductParamsByWhsAndClients(new[] { client1.PK, client2.PK, client3.PK }, whs.PK));
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), collection.FindWhsProductParamsByWhsAndClients(new[] { ZGuid.BrettsGuid }, whs.PK));

			var iCollection = (IWhsProductParamsByWhsAndClientCollection)collection;
			AssertEquals(paramsByWhsAndClient1, iCollection.FindWhsProductParamsByWhsAndClient(client1.PK, whs.PK));
			AssertNull(iCollection.FindWhsProductParamsByWhsAndClient(ZGuid.BrettsGuid, whs.PK));

			AssertContainsExactElementsInAnyOrder(new[] { paramsByWhsAndClient3 }, iCollection.FindWhsProductParamsByWhsAndClients(new[] { client3.PK }, whs.PK));
			AssertContainsExactElementsInAnyOrder(new[] { paramsByWhsAndClient1, paramsByWhsAndClient2 }, iCollection.FindWhsProductParamsByWhsAndClients(new[] { client1.PK, client2.PK }, whs.PK));
			AssertContainsExactElementsInAnyOrder(new[] { paramsByWhsAndClient1, paramsByWhsAndClient2, paramsByWhsAndClient3 }, iCollection.FindWhsProductParamsByWhsAndClients(new[] { client1.PK, client2.PK, client3.PK }, whs.PK));
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), iCollection.FindWhsProductParamsByWhsAndClients(new[] { ZGuid.BrettsGuid }, whs.PK));
		}

		public void TestSortByBOMLocation()
		{
			TestSortByBOMLocationCore(isInwardProcessing: false);
		}

		public void TestSortByInwardsProcessingBOMLocation()
		{
			TestSortByBOMLocationCore(isInwardProcessing: true);
		}

		void TestSortByBOMLocationCore(bool isInwardProcessing)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;

			var area1 = Helper.CreateArea(whs1, "X");
			var row1 = Helper.CreateRowAndGenerateLocations(whs1, "X", 10);

			var whs2 = Helper.CreateWarehouse("Whs2");
			var area2 = Helper.CreateArea(whs2, "X");
			var row2 = Helper.CreateRowAndGenerateLocations(whs2, "X", 10);

			var whs3 = Helper.CreateWarehouse("Whs3");
			var area3 = Helper.CreateArea(whs3, "X");
			var row3 = Helper.CreateRowAndGenerateLocations(whs3, "X", 10);

			if (isInwardProcessing)
			{
				whs1.WW_IsVirtualWarehouse = true;
				whs2.WW_IsVirtualWarehouse = true;
				whs3.WW_IsVirtualWarehouse = true;

				area1.WA_AreaType = AreaTypes.Codes.InwardProcessing;
				area2.WA_AreaType = AreaTypes.Codes.InwardProcessing;
				area3.WA_AreaType = AreaTypes.Codes.InwardProcessing;

				row1.Locations.ForEach(l =>
				{
					l.WLV_WA_PutawayArea = area1.PK;
					l.WLV_WA_PickingArea = area1.PK;
				});

				row2.Locations.ForEach(l =>
				{
					l.WLV_WA_PutawayArea = area2.PK;
					l.WLV_WA_PickingArea = area2.PK;
				});

				row3.Locations.ForEach(l =>
				{
					l.WLV_WA_PutawayArea = area3.PK;
					l.WLV_WA_PickingArea = area3.PK;
				});
			}

			Factory.Save();

			var locationColumn = isInwardProcessing
				? nameof(WhsProductParamsByWhsAndClient.W3_WL_InwardsProcessingStagingLocationBOM)
				: nameof(WhsProductParamsByWhsAndClient.W3_WL_StagingLocationBOM);

			var client = data.Org1;
			var collection = new WhsProductParamsByWhsAndClientCollection(data.Part1, Factory);
			var param1 = collection.AddNew();
			param1.W3_WW = whs1.PK;
			param1.W3_OH = client.PK;
			param1[locationColumn] = whs1.FindLocation("X-1").PK;

			var param2 = collection.AddNew();
			param2.W3_WW = whs2.PK;
			param2.W3_OH = client.PK;
			param2[locationColumn] = whs2.FindLocation("X-2").PK;

			var param3 = collection.AddNew();
			param3.W3_WW = whs3.PK;
			param3.W3_OH = client.PK;
			param3[locationColumn] = whs3.FindLocation("X-10").PK;

			WhsLocation GetLocation(WhsProductParamsByWhsAndClient x) => isInwardProcessing ? x.InwardProcessingStagingLocationBOM : x.StagingLocationBOM;

			collection.ApplySort(locationColumn, ListSortDirection.Ascending);
			AssertEquals("X-1", GetLocation(collection[0]).ToLocationString());
			AssertEquals("X-2", GetLocation(collection[1]).ToLocationString());
			AssertEquals("X-10", GetLocation(collection[2]).ToLocationString());

			collection.ApplySort(locationColumn, ListSortDirection.Descending);
			AssertEquals("X-10", GetLocation(collection[0]).ToLocationString());
			AssertEquals("X-2", GetLocation(collection[1]).ToLocationString());
			AssertEquals("X-1", GetLocation(collection[2]).ToLocationString());
		}

		protected override WhsProductParamsByWhsAndClientCollection GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new WhsProductParamsByWhsAndClientCollection(part, Factory);
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;
	}
}
