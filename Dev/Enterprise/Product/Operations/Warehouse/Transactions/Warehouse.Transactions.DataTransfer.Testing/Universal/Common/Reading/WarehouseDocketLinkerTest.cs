using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WarehouseDocketLinkerTest : WhsUniversalTestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var shipmentWithDataContext = new UniversalShipment { DataContext = DataContextFactory.New() };

			var order = Factory.New<WhsOrder>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new WarehouseDocketLinker(order, null, Factory));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new WarehouseDocketLinker(null, shipmentWithDataContext, Factory));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new WarehouseDocketLinker(order, shipmentWithDataContext, null));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new WarehouseDocketLinker(order, new UniversalShipment(), Factory));
		}

		#endregion

		#region TestAppropriateMethodMustBeCalledWithCorrespondingConstructor

		public void TestAppropriateMethodMustBeCalledWithCorrespondingConstructor()
		{
			var shipmentWithDataContext = new UniversalShipment { DataContext = DataContextFactory.New() };
			var order = Factory.New<WhsOrder>();
			AssertExceptionThrown(typeof(InvalidOperationException), "IWarehouseDocketLinker.LinkDocket(IColumnIndexer, IEntityID, IXmlImportLogger) should only be used when this class is invoked through Spring.",
				() => ((IWarehouseDocketLinker)new WarehouseDocketLinker(order, shipmentWithDataContext, Factory)).LinkDocket(order, order.GetUniversalDataContextManager(), Logger));
		}

		#endregion

		#region TestLinkDocket_ForDocket

		public void TestLinkDocket_ForDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Description = "DUM123";
			Factory.SaveForTesting();

			var shipmentWithDataContext = new UniversalShipment { DataContext = DataContextFactory.New() };
			var dataContext = shipmentWithDataContext.DataContext;
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			var linker = new WarehouseDocketLinker(order, shipmentWithDataContext, Factory);
			linker.LinkDocket(null, Logger);
			AssertEquals("Cannot create link will no parent Data Source.", 0, order.GetRelatedParents().Count());

			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");
			linker.LinkDocket(dataContext.GetMatchingDataSource(DataContextType.DummyBusinessObject), Logger);
			AssertEquals("Cannot create link if parent Data Source Key is not in DB.", 0, order.GetRelatedParents().Count());

			var shipmentWithCorrectDataContext = new UniversalShipment { DataContext = DataContextFactory.New() };
			var dataContextWithCorrectDataSource = shipmentWithCorrectDataContext.DataContext;
			dataContextWithCorrectDataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContextWithCorrectDataSource.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			new WarehouseDocketLinker(order, shipmentWithCorrectDataContext, Factory).LinkDocket(dataContextWithCorrectDataSource.GetMatchingDataSource(DataContextType.DummyBusinessObject), Logger);
			AssertContainsExactElementsInAnyOrder(new[] { dummy.PK }, order.GetRelatedParents().Select(p => p.PK));
		}

		#endregion

		#region TestLinkDocket_ForParent

		public void TestLinkDocket_ForParent()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketID = "W001";

			var shipment = (BusinessObject)Factory.BOFactory.New<Forwarding.IForwardingShipment>();
			Factory.SaveForTesting();
			var shipmentCode = ((ICodeDescription)shipment).Code;

			var topLevelDataObject = new UniversalShipment { DataContext = DataContextFactory.New() };
			var dataContext = topLevelDataObject.DataContext;
			dataContext.AddDataSource(DataContextType.WarehouseOrder, "W001");
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			IWarehouseDocketLinker linker = new WarehouseDocketLinker(DataContextType.WarehouseOrder, topLevelDataObject, Factory);
			linker.LinkDocket(shipment, shipment.GetUniversalDataContextManager(), Logger);
			AssertEquals("Should only create link during an Internal Import.", 0, order.GetRelatedParents().Count());

			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			linker.LinkDocket(null, shipment.GetUniversalDataContextManager(), Logger);
			AssertEquals("Cannot create link will null parent.", 0, order.GetRelatedParents().Count());

			var topLevelDataObjectWithNoDataSource = new UniversalShipment { DataContext = DataContextFactory.New() };
			var dataContextWithNoDataSource = topLevelDataObjectWithNoDataSource.DataContext;
			dataContextWithNoDataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			((IWarehouseDocketLinker)new WarehouseDocketLinker(DataContextType.WarehouseOrder, topLevelDataObjectWithNoDataSource, Factory)).LinkDocket(shipment, shipment.GetUniversalDataContextManager(), Logger);
			AssertEquals("Cannot create link if the DataSource is not present.", 0, order.GetRelatedParents().Count());

			dataContextWithNoDataSource.AddDataSource(DataContextType.WarehouseOrder, "W002");
			((IWarehouseDocketLinker)new WarehouseDocketLinker(DataContextType.WarehouseOrder, topLevelDataObjectWithNoDataSource, Factory)).LinkDocket(shipment, shipment.GetUniversalDataContextManager(), Logger);
			AssertEquals("Cannot create link if the DataSource Key does not exist in DB.", 0, order.GetRelatedParents().Count());

			// all data is correct, should link the parent correctly.
			linker.LinkDocket(shipment, shipment.GetUniversalDataContextManager(), Logger);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.PK }, (System.Collections.IEnumerable)order.GetRelatedParents().Select(p => p.PK));

			var pivotQuery = new ZQuery();
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_WD_Docket, order.PK);
			AssertEquals("Should only have created one pivot.", 1, Factory.Load<WhsDocketJobPivot>(pivotQuery).Length);

			// all data is correct, should link the parent correctly.
			linker.LinkDocket(shipment, shipment.GetUniversalDataContextManager(), Logger);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.PK }, (System.Collections.IEnumerable)order.GetRelatedParents().Select(p => p.PK));
			AssertEquals("Should only have created one pivot, not create another one.", 1, Factory.Load<WhsDocketJobPivot>(pivotQuery).Length);
			AssertEquals("Linked Entities are only logged on Factory Save.", 0, Logger.LinkedEntities.Count());

			Factory.SaveForTesting();
			AssertContainsExactElementsInAnyOrder(new[] { $"Parent:ForwardingShipment-{shipmentCode}|Child:WarehouseOrder-W001" }, Logger.LinkedEntities.Select(e => e.ToString()));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = null;
		}
		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}