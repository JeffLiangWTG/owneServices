namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class ViewPackagesCFSInfoControllerTest : WhsTransitTestCaseWithFactory
	{
		#region TestCFSAddressSelected

		public void TestCFSAddressSelected()
		{
			var dummyParent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse1 = Helper.CreateTRWWarehouse("TR1");
			var transitWarehouse2 = Helper.CreateTRWWarehouse("TR2");
			var viewPackagesManager = new ViewPackagesManager(Factory, dummyParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo1 = new ViewPackagesCFSInfo(Factory, transitWarehouse1.WarehouseAddress.PK, controller);
			var cfsInfo2 = new ViewPackagesCFSInfo(Factory, transitWarehouse2.WarehouseAddress.PK, controller);
			controller.CFSInfoList.Add(cfsInfo1);
			controller.CFSInfoList.Add(cfsInfo2);
			var selectCalledOnInfo1 = false;
			var deselectCalledOnInfo1 = false;
			cfsInfo1.SelectedEvent += (s, e) => selectCalledOnInfo1 = true;
			cfsInfo1.DeselectedEvent += (s, e) => deselectCalledOnInfo1 = true;

			var deselectCalledOnInfo2 = false;
			var selectCalledOnInfo2 = false;
			cfsInfo2.SelectedEvent += (s, e) => selectCalledOnInfo2 = true;
			cfsInfo2.DeselectedEvent += (s, e) => deselectCalledOnInfo2 = true;

			var newWarehouseOverrideCalled = false;
			controller.CurrentWarehouseChanged += (s, e) => newWarehouseOverrideCalled = true;

			AssertContainsExactElementsInAnyOrder(new[] { cfsInfo1, cfsInfo2 }, controller.CFSInfoList);

			AssertNull("Precondition", viewPackagesManager.Warehouse);
			controller.CFSAddressSelected(cfsInfo2);
			Assert(newWarehouseOverrideCalled);
			Assert(!selectCalledOnInfo1);
			Assert(deselectCalledOnInfo1);
			Assert(selectCalledOnInfo2);
			Assert(!deselectCalledOnInfo2);
			AssertEquals(transitWarehouse2, viewPackagesManager.Warehouse);

			controller.CFSAddressSelected(cfsInfo1);
			AssertEquals(transitWarehouse1, viewPackagesManager.Warehouse);
		}

		#endregion
	}
}
