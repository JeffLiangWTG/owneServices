using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitController))]
	public class WhsItemDispatchTransportationUnitControllerTest : WhsTransitControllerTest<WhsItemDispatchTransportationUnitController, WhsItemDispatchTransportationUnit>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsItemDispatchTransportationUnit;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemDispatchTransportationUnit;

		public void TestGetForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse1 = Helper.CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");

			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse1.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse2.PK);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = GetNewController.ShowEditForm(dtu1))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form);
				AssertEquals(typeof(DispatchTransportationUnitForm), form.GetType());
			}

			using (var form = GetNewController.ShowEditForm(dtu2))
			{
				AssertEquals(
					"Should not show the form",
					"The DTU could not be found. Please check that you are logged into the DTU's Warehouse Branch and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShowFormForNewEntity()
		{
			using (var form = GetNewController.ShowFormForNewEntity(Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>()))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Transit Dispatch Transportation Unit on the CargoWise Desktop, please go to the Transit Warehouse Management Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}
	}
}
