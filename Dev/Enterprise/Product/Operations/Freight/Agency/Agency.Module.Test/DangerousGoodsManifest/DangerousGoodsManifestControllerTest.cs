using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(DangerousGoodsManifestController))]
	internal class DangerousGoodsManifestControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn_VoyageIsDeleted_ReturnNull()
		{
			var controller = new DangerousGoodsManifestControllerForTesting();
			var voyage = Factory.New<JobVoyage>();
			voyage.Delete();
			using (var plugin = controller.GetPlugIn(voyage))
			{
				AssertNull(plugin);
			}
		}

		public void TestGetPlugIn_VoyageIsNotSea_ReturnNull()
		{
			var controller = new DangerousGoodsManifestControllerForTesting();
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			using (var plugin = controller.GetPlugIn(voyage))
			{
				AssertNull(plugin);
			}
		}

		public void TestGetPlugIn_VoyageIsSea_ReturnPlugIn()
		{
			var controller = new DangerousGoodsManifestControllerForTesting();
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			using (var plugin = controller.GetPlugIn(voyage))
			{
				AssertNotNull(plugin);
			}
		}

		public void TestSecurityCheckpoints()
		{
			var voyage = Factory.New<JobVoyage>();
			var controller = new AgencyAllocationController();

			AssertEquals(controller.GetCheckPointForView(voyage), Env.Security.SailingScheduleAllocationView);
			AssertEquals(controller.GetCheckPointForEdit(voyage), Env.Security.SailingScheduleAllocationEdit);
			AssertEquals(controller.GetCheckPointForNew(voyage), Env.Security.SailingScheduleAllocationEdit);
			AssertEquals(controller.GetCheckPointForDelete(voyage), Env.Security.None);
		}

		#region

		class DangerousGoodsManifestControllerForTesting : DangerousGoodsManifestController
		{
			internal new ZPlugIn GetPlugIn(IBusiness businessEntity)
			{
				return base.GetPlugIn(businessEntity);
			}
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyDangerousGoodsManifest;
		}

		#endregion
	}
}
