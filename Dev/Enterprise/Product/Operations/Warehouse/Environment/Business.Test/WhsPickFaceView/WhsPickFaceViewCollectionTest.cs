using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPickFaceViewCollection))]
	class WhsPickFaceViewCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsPickFaceViewCollection>
	{
		public void TestConstructors()
		{
			var pickFaceViewCollection1 = new WhsPickFaceViewCollection(Factory);
			AssertNotNull(pickFaceViewCollection1);

			var mockPickFaceRefreshable = new Mock<IModuleGridCollectionRefreshable>();
			mockPickFaceRefreshable.Setup(m => m.GetTableNamesToMonitor()).Returns(new[] { WhsPickFaceSchema.Constants.TableName });
			var pickFaceViewCollection2 = new WhsPickFaceViewCollection(Factory, mockPickFaceRefreshable.Object);
			AssertNotNull(pickFaceViewCollection2);
		}

		public void TestLocationSortedProperly()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var helper = new WhsTestHelperFunctionsEnv(Factory);
			helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-1"));
			helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-2"));
			helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-3"));
			helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-10"));
			Factory.Save();

			var pickFaceViewCollection = new WhsPickFaceViewCollection(Factory);

			pickFaceViewCollection.ApplySort(WhsPickFaceView.Schema.WPV_WL, ListSortDirection.Ascending);
			AssertEquals("A-1", pickFaceViewCollection[0].Location.ToLocationString());
			AssertEquals("A-2", pickFaceViewCollection[1].Location.ToLocationString());
			AssertEquals("A-3", pickFaceViewCollection[2].Location.ToLocationString());
			AssertEquals("A-10", pickFaceViewCollection[3].Location.ToLocationString());

			pickFaceViewCollection.ApplySort(WhsPickFaceView.Schema.WPV_WL, ListSortDirection.Descending);
			AssertEquals("A-10", pickFaceViewCollection[0].Location.ToLocationString());
			AssertEquals("A-3", pickFaceViewCollection[1].Location.ToLocationString());
			AssertEquals("A-2", pickFaceViewCollection[2].Location.ToLocationString());
			AssertEquals("A-1", pickFaceViewCollection[3].Location.ToLocationString());
		}
	}
}
