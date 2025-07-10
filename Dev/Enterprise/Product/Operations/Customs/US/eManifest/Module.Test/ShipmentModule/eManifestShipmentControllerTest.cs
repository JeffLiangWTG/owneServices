using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestShipmentController))]
	sealed class eManifestShipmentControllerTest : ZControllerBasherTest
	{
		public void TestOverrides()
		{
			AssertEquals("ID", ControllerIDs.Customs.US.eManifestShipment, Controller.ID);
			AssertEquals("ModuleID", ModuleIDs.Customs.US.eManifestShipment, Controller.ModuleID);
			AssertEquals("TypeOfTopLevelBusinessObject", typeof(Shipment), Controller.TypeOfTopLevelBusinessObject);
			AssertEquals("CheckPointForView", Env.Security.USeManifestView, Controller.GetCheckPointForView(null));
			AssertEquals("CheckPointForNew", Env.Security.USeManifestNew, Controller.GetCheckPointForNew(null));
			AssertEquals("CheckPointForEdit", Env.Security.USeManifestEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals("CheckPointForDelete", Env.Security.USeManifestEdit, Controller.GetCheckPointForDelete(null));
		}

		public override void TestNewForm()
		{
			Assert("Not applicable", true);
		}

		public override void TestEditForm()
		{
			Assert("Not applicable", true);
		}

		public override void TestViewForm()
		{
			Assert("Not applicable", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Not applicable", true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.eManifestShipment;
	}
}
