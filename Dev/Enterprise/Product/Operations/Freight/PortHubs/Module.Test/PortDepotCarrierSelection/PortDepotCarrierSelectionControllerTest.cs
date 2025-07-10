using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.GUI;
using Enterprise.Freight.PortHubs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortDepotCarrierSelectionController))]
	public class PortDepotCarrierControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PortDepotCarrierSelection;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new PortDepotCarrierSelectionController();
			CombineAssertions(() =>
			{
				AssertEquals(Env.Security.PortDepotSelectionView, controller.GetCheckPointForNew(null));
				AssertEquals(Env.Security.PortDepotSelectionView, controller.GetCheckPointForView(null));
				AssertEquals(Env.Security.PortDepotSelectionModify, controller.GetCheckPointForDelete(null));
				AssertEquals(Env.Security.PortDepotSelectionModify, controller.GetCheckPointForEdit(null));
			});
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(PortHubSelection), Controller.TypeOfTopLevelBusinessObject);
		}

		public void TestGetForm()
		{
			var selection = Factory.NewWithValidTestData<PortHubSelection>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			selection.TY_OA_DepotAddress = orgAddress.PK;

			using (var form = Controller.ShowFormForNewEntity(selection))
			{
				AssertType<PortDepotCarrierSelectionDetailsForm>(form);
			}
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var selection = Factory.NewWithValidTestData<PortHubSelection>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			selection.TY_OA_DepotAddress = orgAddress.PK;

			Factory.Save();

			return selection;
		}

		#endregion
	}
}
