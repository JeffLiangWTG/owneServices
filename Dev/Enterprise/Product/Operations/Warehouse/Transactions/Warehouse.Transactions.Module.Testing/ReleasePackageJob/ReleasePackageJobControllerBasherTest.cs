using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ReleasePackageJobController))]
	public class ReleasePackageJobControllerBasherTest : ZControllerBasherTest
	{
		public void TestShowEditForm_OpensPackingTab()
		{
			using (var form = (ReleaseEntryForm)GetEditFormToShow())
			{
				AssertEquals("MainTabPage", form.GetMainTabControl().SelectedTab.Name);
				AssertEquals("PackingTabPage", form.GetFinaliseOrderTabControl().SelectedTab.Name);
			}
		}

		public void TestShowViewForm_OpensPackingTab()
		{
			using (var form = (ReleaseEntryForm)Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("MainTabPage", form.GetMainTabControl().SelectedTab.Name);
				AssertEquals("PackingTabPage", form.GetFinaliseOrderTabControl().SelectedTab.Name);
			}
		}

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsReleaseView, Controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.WhsReleaseEdit, Controller.GetCheckPointForEdit(null));

			AssertExceptionThrown<ControllerShowNewFormNotSupportedException>(() => Controller.GetCheckPointForNew(null));
			AssertExceptionThrown<ControllerShowDeleteFormNotSupportedException>(() => Controller.GetCheckPointForDelete(null));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsReleasePackageJob;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			Factory.Save();

			return packageJob;
		}
	}
}
