using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestsSubclassesOf(typeof(WhsTransitController))]
	public abstract class WhsTransitControllerTest<TController, TBusinessObject> : ZControllerBasherTest
	where TController : WhsTransitController, new()
	where TBusinessObject : BusinessObject
	{
		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ExpectedModuleID, GetNewController.ModuleID);
		}

		protected abstract ModuleIdentifier ExpectedModuleID { get; }

		#endregion

		#region TestForms

		public override void TestViewForm()
		{
			Assert("Only Document Customisation is supported", true);
		}

		public override void TestNewForm()
		{
			Assert("Only Document Customisation is supported", true);
		}

		public override void TestEditForm()
		{
			Assert("Only Document Customisation is supported", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Only Document Customisation is supported", true);
		}

		#endregion

		#region TestSecurityCheckpoints

		public void TestSecurityCheckpoints()
		{
			var transport = Factory.New<TBusinessObject>();
			var controller = GetNewController;

			AssertEquals(Env.Security.TransitWarehouse, controller.GetCheckPointForDelete(transport));
			AssertEquals(Env.Security.TransitWarehouse, controller.GetCheckPointForEdit(transport));
			AssertEquals(Env.Security.TransitWarehouse, controller.GetCheckPointForNew(transport));
			AssertEquals(Env.Security.TransitWarehouse, controller.GetCheckPointForView(transport));
		}

		#endregion

		public TController GetNewController => new TController();

		public WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
