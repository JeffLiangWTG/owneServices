using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsTransitReceiveConsignmentController))]
	public class WhsTransitReceiveConsignmentControllerTest : WhsTransitControllerTest<WhsTransitReceiveConsignmentController, WhsItemReceiveConsignment>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsTransitReceiveConsignment;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsTransitReceiveConsignment;

		public void TestGetForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse1 = Helper.CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse1.PK, jobID: "1");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse2.PK, jobID: "2");

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = GetNewController.ShowEditForm(rcn1))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form);
				AssertEquals(typeof(ReceiveConsignmentForm), form.GetType());
			}

			using (var form = GetNewController.ShowEditForm(rcn2))
			{
				AssertEquals(
					"Should not show the form",
					"The RCN could not be found. Please check that you are logged into the RCN's Warehouse Branch and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShowFormForNewEntity()
		{
			using (var form = GetNewController.ShowFormForNewEntity(Factory.NewWithValidTestData<WhsItemReceiveConsignment>()))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Transit Receive Consignment on the CargoWise Desktop, please go to the Transit Warehouse Management Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}
	}
}
