using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsDynamicPickFaceViewCollection))]
	class WhsDynamicPickFaceViewCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsDynamicPickFaceViewCollection>
	{
		public void TestConstructors()
		{
			var collection = new WhsDynamicPickFaceViewCollection(Factory);
			AssertNotNull(collection);

			var mockRefreshable = new Mock<IModuleGridCollectionRefreshable>();
			mockRefreshable.Setup(m => m.GetTableNamesToMonitor()).Returns(new[] { WhsProductParamsByWhsAndClientSchema.Constants.TableName });
			var collectionRefreshable = new WhsDynamicPickFaceViewCollection(Factory, mockRefreshable.Object);
			AssertNotNull(collectionRefreshable);
		}

		public void TestCollection_ShouldSortLocationByNumber()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var whs = helper.CreateWarehouse("W1", "A", 10, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA10 = whs.FindLocation("A-10");
			helper.CreateDynamicPF(whs, locationA1);
			Factory.Save();

			locationA2.WLV_WLT_LocationType = locationA1.WLV_WLT_LocationType;
			locationA2.WLV_WA_PickingArea = locationA1.WLV_WA_PickingArea;
			locationA10.WLV_WLT_LocationType = locationA1.WLV_WLT_LocationType;
			locationA10.WLV_WA_PickingArea = locationA1.WLV_WA_PickingArea;
			Factory.Save();

			var collection = new WhsDynamicPickFaceViewCollection(Factory);
			AssertEquals("Precondition: 3 dynamic locations", 3, collection.Count);

			collection.ApplySort(WhsDynamicPickFaceViewSchema.Constants.WDP_LocationString, ListSortDirection.Ascending);
			AssertContainsExactElementsInExactOrder("Assert locations should be sorted by number not by string (A-2 comes before A-10)",
				new string[] { "A-1", "A-2", "A-10" }, collection.Select(x => x.WDP_LocationString));
		}

		public void TestCollection_OtherFieldsUseDefaultComparer()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var whs = helper.CreateWarehouse("W1", "A", 10, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA10 = whs.FindLocation("A-10");
			var area1 = helper.CreateDynamicPF(whs, locationA1, "D1");
			var area2 = helper.CreateDynamicPF(whs, locationA2, "D2");
			var area3 = helper.CreateDynamicPF(whs, locationA10, "D3");
			Factory.Save();

			var client = helper.CreateClient("CLIENT");
			var product1 = helper.CreateProduct(client, "P1");
			var product2 = helper.CreateProduct(client, "P2");

			var params1 = Factory.New<IWhsProductParamsByWhsAndClient>();
			params1.W3_OH = client.PK;
			params1.W3_OP = product1.PK;
			params1.W3_WA_DynamicPickFaceArea = area1.PK;
			params1.W3_WW = area1.WA_WW_Whs;

			var params2 = Factory.New<IWhsProductParamsByWhsAndClient>();
			params2.W3_OH = client.PK;
			params2.W3_OP = product2.PK;
			params2.W3_WA_DynamicPickFaceArea = area2.PK;
			params2.W3_WW = area1.WA_WW_Whs;

			Factory.Save();

			var collection = new WhsDynamicPickFaceViewCollection(Factory);
			AssertEquals("Precondition: 3 dynamic locations", 3, collection.Count);

			collection.ApplySort(WhsDynamicPickFaceViewSchema.Constants.WDP_ProductCode, ListSortDirection.Ascending);
			AssertContainsExactElementsInExactOrder("Other columns should fallback to default Comparer",
				new string[] { "", "P1", "P2" }, collection.Select(x => x.WDP_ProductCode));
		}
	}
}
