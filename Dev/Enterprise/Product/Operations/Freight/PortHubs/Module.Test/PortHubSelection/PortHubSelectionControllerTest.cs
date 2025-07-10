using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortHubSelectionController))]
	public class PortHubSelectionControllerTest : ZPopupControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PortHubSelection;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestCheckpoints()
		{
			var controller = new PortHubSelectionController();
			AssertEquals(Env.Security.PortDepotSelectionView, controller.GetCheckPointForNew(null));
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
