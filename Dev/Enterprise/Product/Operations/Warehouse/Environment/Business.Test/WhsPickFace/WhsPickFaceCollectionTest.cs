using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPickFaceCollection))]
	class WhsPickFaceCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsPickFaceCollection>
	{
		#region TestContainsPickFace

		public void TestContainsPickFace()
		{
			CreatePickFaces();

			var collection = new WhsPickFaceCollection(Product, Factory);
			AssertEquals(true, collection.ContainsPickFace(Client, Whs.FindLocation("A-1-1")));
			AssertEquals(false, collection.ContainsPickFace(Client, Whs.FindLocation("A-1-2")));
			AssertEquals(true, collection.ContainsPickFace(Client, Whs.FindLocation("A-2-2")));
			AssertEquals(false, collection.ContainsPickFace(Client, Whs.FindLocation("A-2-1")));
			AssertEquals(true, collection.ContainsPickFace(Client, Whs.FindLocation("A-3-3")));
			AssertEquals(false, collection.ContainsPickFace(Client, Whs.FindLocation("A-3-2")));
		}

		#endregion

		#region TestFindByLocation

		public void TestFindByLocation()
		{
			CreatePickFaces();

			var collection = GetCollectionToTest();
			var location = Whs.FindLocation("A-1-1");
			var pickFace = collection.AddNew();
			pickFace.WF_WL = location.PK;

			AssertNotNull(collection.FindByLocation(location.PK));
			AssertNull(collection.FindByLocation(ZGuid.NewZGuid()));
		}

		public void TestFindByLocation_DifferentClients()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var pickfaceForClient1 = data.Whs1.FindLocation("A-1");
			var pickfaceForClient2 = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceForClient1);
			Helper.CreateProductPickFace(data.Part1, client2, pickfaceForClient2);

			var pickFaces = new WhsPickFaceCollection(data.Part1, Factory);
			AssertNotNull(pickFaces.FindByLocation(data.Org1.PK, pickfaceForClient1.PK));
			AssertNull(pickFaces.FindByLocation(client2.PK, pickfaceForClient1.PK));
			AssertNull(pickFaces.FindByLocation(data.Org1.PK, pickfaceForClient2.PK));
			AssertNotNull(pickFaces.FindByLocation(client2.PK, pickfaceForClient2.PK));
		}

		#endregion

		#region TestHasPickFace

		public void TestHasPickFace()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 1, 2);
			var differentWarehouse = Helper.CreateWarehouse("W2", "R1", 1, 1);
			var orgWithNoPickFaceProducts = Helper.CreateClient("Org2");
			Factory.Save();

			Helper.CreateProductClientRelationShip(orgWithNoPickFaceProducts, data.Part1);
			Helper.CreateProductClientRelationShip(orgWithNoPickFaceProducts, data.Part2);

			var productForPart1 = data.Part1;
			var productForPart2 = data.Part2;
			var pickFaceLocationForOrg1 = data.Whs1.FindLocation("A-1-1");
			var bulkLocationForOrg1 = data.Whs1.FindLocation("A-1-2");
			Helper.CreateProductPickFace(productForPart1, data.Org1, pickFaceLocationForOrg1);

			var part1Collection = new WhsPickFaceCollection(productForPart1, Factory);
			var part2Collection = new WhsPickFaceCollection(productForPart2, Factory);

			AssertEquals(true, part1Collection.HasPickFace(data.Org1, data.Part1, data.Whs1));
			AssertEquals("It shouldn't have a pick face since the warehouse doesn't contain a pick face for this product",
				false, part1Collection.HasPickFace(data.Org1, data.Part1, differentWarehouse));
			AssertEquals(false, part1Collection.HasPickFace(orgWithNoPickFaceProducts, data.Part1, data.Whs1));
			AssertEquals(false, part2Collection.HasPickFace(data.Org1, data.Part2, data.Whs1));
			AssertEquals(false, part2Collection.HasPickFace(orgWithNoPickFaceProducts, data.Part2, data.Whs1));

			AssertEquals(true, part1Collection.HasPickFace(data.Org1.PK, data.Part1.PK, data.Whs1.PK));
			AssertEquals("It shouldn't have a pick face since the warehouse doesn't contain a pick face for this product",
				false, part1Collection.HasPickFace(data.Org1.PK, data.Part1.PK, differentWarehouse.PK));
			AssertEquals(false, part1Collection.HasPickFace(orgWithNoPickFaceProducts.PK, data.Part1.PK, data.Whs1.PK));
			AssertEquals(false, part2Collection.HasPickFace(data.Org1.PK, data.Part2.PK, data.Whs1.PK));
			AssertEquals(false, part2Collection.HasPickFace(orgWithNoPickFaceProducts.PK, data.Part2.PK, data.Whs1.PK));
		}

		#endregion

		#region TestFindAllPickFaceLocations

		public void TestFindAllPickFaceLocations()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			var pickFaceLocationForDifferentClient = data.Whs1.FindLocation("A-1");
			var pickFaceLocationForDifferentPart = data.Whs1.FindLocation("A-2");
			var pickfaceLocation1 = data.Whs1.FindLocation("A-3");
			var pickfaceLocation2 = data.Whs1.FindLocation("A-4");
			var differentWarehouse = Helper.CreateWarehouse("W2", "R1", 1, 1);
			Factory.Save();

			var differentClientWithPickfaceProducts = Helper.CreateClient("O2");
			var clientWithNoPickFaceProducts = Helper.CreateClient("O3");
			Helper.CreateProductClientRelationShip(differentClientWithPickfaceProducts, data.Part1);
			Helper.CreateProductClientRelationShip(clientWithNoPickFaceProducts, data.Part1);

			var differentWarehousePickfaceLocation = differentWarehouse.DefaultLocation;
			Helper.CreateProductPickFace(data.Part1, data.Org1, differentWarehousePickfaceLocation);
			Helper.CreateProductPickFace(data.Part1, differentClientWithPickfaceProducts, pickFaceLocationForDifferentClient);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocationForDifferentPart);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation1);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation2);

			var productForPart1 = data.Part1;
			var productForPart2 = data.Part2;

			var part1Collection = new WhsPickFaceCollection(productForPart1, Factory);

			AssertContainsExactElementsInAnyOrder(new[] { pickfaceLocation1, pickfaceLocation2 },
				part1Collection.FindAllPickFaceLocations(data.Org1.PK, data.Part1.PK, data.Whs1.PK));
			AssertContainsExactElementsInAnyOrder(new[] { pickFaceLocationForDifferentPart },
				new WhsPickFaceCollection(productForPart2, Factory).FindAllPickFaceLocations(data.Org1.PK, data.Part2.PK, data.Whs1.PK));
			AssertContainsExactElementsInAnyOrder(new[] { pickFaceLocationForDifferentClient },
				part1Collection.FindAllPickFaceLocations(differentClientWithPickfaceProducts.PK, data.Part1.PK, data.Whs1.PK));
			AssertContainsExactElementsInAnyOrder(new[] { differentWarehousePickfaceLocation },
				part1Collection.FindAllPickFaceLocations(data.Org1.PK, data.Part1.PK, differentWarehouse.PK));
		}

		#endregion

		#region TestSetDefaultsForNewChildDoesNotThrowException

		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChildDoesNotThrowException()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddNew().OU_OH = ZGuid.Empty;
			WhsPickFaceCollection collection = new WhsPickFaceCollection(part, Factory);
			WhsPickFace face = collection.AddNew();
		}

		#endregion

		#region TestSetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			var client = Helper.CreateClient();
			var productWithZeroDecmalPlacesForStockKeepingUnit = Helper.CreateProduct(client, "P1");
			var productWithOneDecmalPlaceForStockKeepingUnit = Helper.CreateProduct(client, "P1", 1);

			var collectionForProductWithZeroDecmalPlacesForStockKeepingUnit =
				new WhsPickFaceCollection(productWithZeroDecmalPlacesForStockKeepingUnit, Factory);
			var faceForProductWithZeroDecmalPlacesForStockKeepingUnit = collectionForProductWithZeroDecmalPlacesForStockKeepingUnit.AddNew();
			AssertEquals(1, collectionForProductWithZeroDecmalPlacesForStockKeepingUnit.Count);
			AssertEquals(client.PK, faceForProductWithZeroDecmalPlacesForStockKeepingUnit.WF_OH_Client);
			AssertEquals(1m, faceForProductWithZeroDecmalPlacesForStockKeepingUnit.WF_ReplenishmentMultiple);

			var collectionForProductWithOneDecmalPlacesForStockKeepingUnit =
				new WhsPickFaceCollection(productWithOneDecmalPlaceForStockKeepingUnit, Factory);
			var faceForProductWithOneDecmalPlaceForStockKeepingUnit = collectionForProductWithOneDecmalPlacesForStockKeepingUnit.AddNew();
			AssertEquals(1, collectionForProductWithOneDecmalPlacesForStockKeepingUnit.Count);
			AssertEquals(client.PK, faceForProductWithOneDecmalPlaceForStockKeepingUnit.WF_OH_Client);
			AssertEquals(0.1m, faceForProductWithOneDecmalPlaceForStockKeepingUnit.WF_ReplenishmentMultiple);
		}

		#endregion

		#region

		public void TestPickFaceLocationSortedProperly_LocationString()
		{
			TestPickFaceLocationSortedProperlyCore(nameof(WhsPickFace.LocationString));
		}

		public void TestPickFaceLocationSortedProperly_WF_WL()
		{
			TestPickFaceLocationSortedProperlyCore(nameof(WhsPickFace.WF_WL));
		}

		void TestPickFaceLocationSortedProperlyCore(string locationPropertyToSort)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-1"));
			Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-2"));
			Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-3"));
			Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-10"));

			var pickFaceCollection = new WhsPickFaceCollection(data.Part1, Factory);

			pickFaceCollection.ApplySort(locationPropertyToSort, ListSortDirection.Ascending);
			AssertEquals("A-1", pickFaceCollection[0].Location.ToLocationString());
			AssertEquals("A-2", pickFaceCollection[1].Location.ToLocationString());
			AssertEquals("A-3", pickFaceCollection[2].Location.ToLocationString());
			AssertEquals("A-10", pickFaceCollection[3].Location.ToLocationString());

			pickFaceCollection.ApplySort(locationPropertyToSort, ListSortDirection.Descending);
			AssertEquals("A-10", pickFaceCollection[0].Location.ToLocationString());
			AssertEquals("A-3", pickFaceCollection[1].Location.ToLocationString());
			AssertEquals("A-2", pickFaceCollection[2].Location.ToLocationString());
			AssertEquals("A-1", pickFaceCollection[3].Location.ToLocationString());
		}

		#endregion

		#region Implementation

		void CreatePickFaces()
		{
			Whs = Helper.CreateWarehouse("1", "A", 3, 3);
			Client = Helper.CreateClient();
			DifferentClient = Helper.CreateClient("C2", "C2");
			Product = Helper.CreateProduct(Client, "P1");
			Helper.CreateProductClientRelationShip(DifferentClient, Product);
			Factory.Save();

			Helper.CreateProductPickFace(Product, Client, Whs.FindLocation("A-1-1"));
			Helper.CreateProductPickFace(Product, Client, Whs.FindLocation("A-2-2"));
			Helper.CreateProductPickFace(Product, Client, Whs.FindLocation("A-3-3"));
			Helper.CreateProductPickFace(Product, DifferentClient, Whs.FindLocation("A-1-1"));
		}

		protected override WhsPickFaceCollection GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new WhsPickFaceCollection(part, Factory);
		}

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		OrgHeader Client;
		OrgHeader DifferentClient;
		WhsWarehouse Whs;
		OrgSupplierPart Product;

		#endregion
	}
}
