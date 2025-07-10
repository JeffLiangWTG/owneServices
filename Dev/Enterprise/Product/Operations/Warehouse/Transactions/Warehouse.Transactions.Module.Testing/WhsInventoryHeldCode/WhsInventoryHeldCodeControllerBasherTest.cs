using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeController))]
	class WhsInventoryHeldCodeControllerBasherTest : ZControllerBasherTest
	{
		#region TestID

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsInventoryHeldCodes, controller.ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsInventoryHeldCodes, controller.ModuleID);
		}

		#endregion

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsConfigInventoryHeldCodeDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsConfigInventoryHeldCodeEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WhsConfigInventoryHeldCodeNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsConfigInventoryHeldCodeView, Controller.GetCheckPointForView(null));
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsInventoryHeldCode), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsInventoryHeldCodes;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var heldCode = Factory.New<WhsInventoryHeldCode>();
			heldCode.WHC_Code = "ABC";
			heldCode.WHC_Description = "abc";
			Factory.Save();
			return heldCode;
		}

		#endregion
	}
}
