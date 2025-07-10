using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	sealed class TransitDocDataObjectProviderTest : TestCaseWithFactory
	{
		#region TestGetDocDataObject_FromNull

		public void TestGetDocDataObject_FromNull()
		{
			var provider = new TransitDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "TestTitle"
			};

			var dataObject = provider.GetDocDataObject(null, TransitDocDataContext.CIN750WarehouseIn, parameters);

			AssertNull($"expected null for null parent and '{TransitDocDataContext.CIN750WarehouseIn}' data context", dataObject);
		}

		#endregion

		#region TestGetDocDataObject_FromRCN

		object GetDocDataObjectFromRCN(string contextType)
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var provider = new TransitDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "TestTitle"
			};

			return provider.GetDocDataObject(rcn, contextType, parameters);
		}

		public void TestGetInDocObject_FromRCN()
		{
			var dataObject = GetDocDataObjectFromRCN(TransitDocDataContext.CIN750WarehouseIn);
			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CIN 750 IN data object", typeof(CIN750InNotification), dataObject.GetType());
		}

		public void TestGetCorDocObject_FromRCN()
		{
			var dataObject = GetDocDataObjectFromRCN(TransitDocDataContext.CIN750WarehouseCor);
			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CIN 750 COR data object", typeof(CIN750CorNotification), dataObject.GetType());
		}

		#endregion

		#region TestGetDocDataObject_FromDCN

		public void TestGetDocDataObject_FromDCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			var provider = new TransitDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "TestTitle"
			};

			var dataObject = provider.GetDocDataObject(dcn, TransitDocDataContext.CIN750WarehouseDecons, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CIN 750 Decons data object", typeof(CIN750DeconsNotification), dataObject.GetType());

			dataObject = provider.GetDocDataObject(dcn, TransitDocDataContext.CIN750WarehouseCons, parameters);
			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CIN 750 Cons data object", typeof(CIN750ConsNotification), dataObject.GetType());
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}

	sealed class DummyDocDataObjectParameters : IDocDataObjectParameters
	{
		public string DocumentTitle { get; set; }
		public string DataStoreName { get; set; }
		public object Data { get; set; }
		public IStmALogProvider LogProvider { get; set; }
	}
}
