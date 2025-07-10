using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsTransitDispatchConsignmentController))]
	public class WhsTransitDispatchConsignmentControllerTest : WhsTransitControllerTest<WhsTransitDispatchConsignmentController, WhsItemDispatchConsignment>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsTransitDispatchConsignment;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsTransitDispatchConsignment;

		public void TestGetForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse1 = Helper.CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK, jobID: "1");
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse2.PK, jobID: "2");

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = GetNewController.ShowEditForm(dcn1))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form);
				AssertEquals(typeof(DispatchConsignmentForm), form.GetType());
			}

			using (var form = GetNewController.ShowEditForm(dcn2))
			{
				AssertEquals(
					"Should not show the form",
					"The DCN could not be found. Please check that you are logged into the DCN's Warehouse Branch and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShowFormForNewEntity()
		{
			using (var form = GetNewController.ShowFormForNewEntity(Factory.NewWithValidTestData<WhsItemDispatchConsignment>()))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Transit Dispatch Consignment on the CargoWise Desktop, please go to the Transit Warehouse Management Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}
	}
}
