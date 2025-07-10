using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitController))]
	public class WhsItemReceiveTransportationUnitControllerTest : WhsTransitControllerTest<WhsItemReceiveTransportationUnitController, WhsItemReceiveTransportationUnit>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsItemReceiveTransportationUnit;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemReceiveTransportationUnit;

		public void TestGetForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse1 = Helper.CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var row1 = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock", 2, 2);
			var location1 = row1.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var row2 = Helper.CreateRowAndGenerateLocations(warehouse2, "Dock", 2, 2);
			var location2 = row2.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse1.PK, location1.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse2.PK, location2.PK);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = GetNewController.ShowEditForm(rtu1))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form);
				AssertEquals(typeof(ReceiveTransportationUnitForm), form.GetType());
			}

			using (var form = GetNewController.ShowEditForm(rtu2))
			{
				AssertEquals(
					"Should not show the form",
					"The RTU could not be found. Please check that you are logged into the RTU's Warehouse Branch and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShowFormForNewEntity()
		{
			using (var form = GetNewController.ShowFormForNewEntity(Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>()))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Transit Receive Transportation Unit on the CargoWise Desktop, please go to the Transit Warehouse Management Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}
	}
}
