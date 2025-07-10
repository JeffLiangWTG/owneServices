using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(ProductStyleController))]
	class ProductStyleControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestID

		public void TestID()
		{
			var controller = new ProductStyleController();
			AssertEquals(ControllerIDs.WhsConfigProductStyle, controller.ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var controller = new ProductStyleController();
			AssertEquals(ModuleIDs.WhsConfigProductStyle, controller.ModuleID);
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new ProductStyleController();
			AssertEquals(typeof(WhsProductStyle), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region TestSecurityCheckpoints

		public void TestSecurityCheckpoints()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			var controller = new ProductStyleController();
			AssertEquals(Env.Security.WhsConfigProductStyleDelete, controller.GetCheckPointForDelete(productStyle));
			AssertEquals(Env.Security.WhsConfigProductStyleEdit, controller.GetCheckPointForEdit(productStyle));
			AssertEquals(Env.Security.WhsConfigProductStyleNew, controller.GetCheckPointForNew(productStyle));
			AssertEquals(Env.Security.WhsConfigProductStyleView, controller.GetCheckPointForView(productStyle));
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigProductStyle;
		}

		#endregion
	}
}
