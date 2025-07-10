using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdHocServiceJobController))]
	public class WhsAdHocControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsAdHocServiceJob, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsAdHocServiceJob, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsAdHocServiceJob), controller.TypeOfTopLevelBusinessObject);
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsAdHocServiceJob;
		}

		#endregion
	}
}
