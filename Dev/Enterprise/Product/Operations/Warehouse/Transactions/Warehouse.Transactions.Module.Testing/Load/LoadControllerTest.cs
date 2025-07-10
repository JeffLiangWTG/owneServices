using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(LoadController))]
	public class LoadControllerTest : WhsControllerBaseBasherTest
	{
		#region TestID

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsLoad, controller.ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsLoad, controller.ModuleID);
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsLoad), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region TestGetForm

		public void TestGetForm()
		{
			var load = GetBusinessObjectThatIsInTheDatabase();
			var controller = ZControllerFactory.Create(GetControllerID());
			using (var form = controller.ShowViewForm(load))
			{
				AssertEquals(typeof(LoadEntryForm), form.GetType());
			}
		}

		#endregion

		#region TestCheckPointForView

		public void TestCheckPointForView()
		{
			var load = Factory.New<WhsLoad>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForView(load));
		}

		#endregion

		#region TestCheckPointForEdit

		public void TestCheckPointForEdit()
		{
			var load = Factory.New<WhsLoad>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForEdit(load));
		}

		#endregion

		#region TestCheckPointForNew

		public void TestCheckPointForNew()
		{
			var load = Factory.New<WhsLoad>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForNew(load));
		}

		#endregion

		#region TestCheckPointForDelete

		public void TestCheckPointForDelete()
		{
			var load = Factory.New<WhsLoad>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForDelete(load));
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID() => ControllerIDs.WhsLoad;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var transportCompany = helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck = helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = helper.CreateWhsLoad(transportCompany, dockDoorLocation, "WL00000001", carrierServicelevel.PL_Code, truck);
			Factory.Save();

			return load;
		}

		#endregion
	}
}
