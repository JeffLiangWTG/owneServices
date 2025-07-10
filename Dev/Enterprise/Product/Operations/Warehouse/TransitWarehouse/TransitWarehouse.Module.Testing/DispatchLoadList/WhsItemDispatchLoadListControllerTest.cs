using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListController))]
	public class WhsItemDispatchLoadListControllerTest : WhsTransitControllerTest<WhsItemDispatchLoadListController, WhsItemDispatchLoadList>
	{
		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemDispatchLoadList;

		protected override ControllerID GetControllerID() => ControllerIDs.WhsItemDispatchLoadList;

		public void TestGetForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse1 = Helper.CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");

			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse2.PK);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = GetNewController.ShowEditForm(dll1))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form);
				AssertEquals(typeof(DispatchLoadListForm), form.GetType());
			}

			using (var form = GetNewController.ShowEditForm(dll2))
			{
				AssertEquals(
					"Should not show the form",
					"The DLL could not be found. Please check that you are logged into the DLL's Warehouse Branch and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShowFormForNewEntity()
		{
			using (var form = GetNewController.ShowFormForNewEntity(Factory.NewWithValidTestData<WhsItemDispatchLoadList>()))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Transit Dispatch Load List on the CargoWise Desktop, please go to the Transit Warehouse Management Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}
	}
}
