using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestsSubclassesOf(typeof(HandlingUnitController))]
	public abstract class HandlingUnitControllerTest<TController, TBusinessObject> : ZControllerBasherTest
	where TController : HandlingUnitController, new()
	where TBusinessObject : BusinessObject
	{
		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ExpectedModuleID, GetNewController.ModuleID);
		}

		protected abstract ModuleIdentifier ExpectedModuleID { get; }

		protected abstract SecurityCheckpoint checkpoint { get; }

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

			AssertEquals(checkpoint, controller.GetCheckPointForDelete(transport));
			AssertEquals(checkpoint, controller.GetCheckPointForEdit(transport));
			AssertEquals(checkpoint, controller.GetCheckPointForNew(transport));
			AssertEquals(checkpoint, controller.GetCheckPointForView(transport));
		}

		#endregion

		public TController GetNewController => new TController();
	}
}
