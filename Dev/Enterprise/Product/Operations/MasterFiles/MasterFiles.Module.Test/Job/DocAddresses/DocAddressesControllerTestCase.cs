using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(DocAddressesController))]
	sealed class DocAddressesControllerTestCase : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocAddresses;
		}

		#region Security checkpoints

		public void TestCheckPoints()
		{
			DocAddressesControllerForTest controller = new DocAddressesControllerForTest();

			string message = "Incorrect security checkpoint.";
			AssertEquals(message, Env.Security.Organisation, controller.CheckPointForDelete);
			AssertEquals(message, Env.Security.Organisation, controller.CheckPointForEdit);
			AssertEquals(message, Env.Security.Organisation, controller.CheckPointForNew);
			AssertEquals(message, Env.Security.Organisation, controller.CheckPointForView);
		}

		public void TestGetPlugIn()
		{
			using (ZPlugIn plugIn = new DocAddressesControllerForTest().GetPlugIn(null))
			{
				AssertEquals("Type of PlugIn", typeof(DocAddressesPlugIn), plugIn.GetType());
			}
		}

		#endregion

		class DocAddressesControllerForTest : DocAddressesController
		{
			internal new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
			internal new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;
			internal new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
			internal new SecurityCheckpoint CheckPointForView => base.CheckPointForView;

			internal new ZPlugIn GetPlugIn(IBusiness businessEntity) => base.GetPlugIn(businessEntity);
		}
	}
}
