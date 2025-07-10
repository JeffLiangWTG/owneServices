using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyPortMessagesControllerTest : BaseAgencyTest
	{
		#region TestGettingPlugInOnDeletedVoyage
		public void TestGettingPlugInOnDeletedVoyage()
		{
			Voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			Voyage.Delete();
			using (ZPlugIn plugin = Controller.ExposedGetPlugIn(Voyage))
			{
				AssertNull(plugin);
			}
		}

		#endregion
		#region TestGetPlugIn_SeaVoyage
		public void TestGetPlugIn_SeaVoyage()
		{
			Voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			using (ZPlugIn plugin = Controller.ExposedGetPlugIn(Voyage))
			{
				AssertNotNull(plugin);
			}
		}

		#endregion
		#region TestGetPlugIn_NonSeaVoyage
		public void TestGetPlugIn_NonSeaVoyage()
		{
			Voyage.JV_AirSeaRoad = Constants.TransportModes.Road;
			using (ZPlugIn plugin = Controller.ExposedGetPlugIn(Voyage))
			{
				AssertNull(plugin);
			}
		}

		#endregion
		#region Implementation
		#region Voyage
		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;
		#endregion
		#region Controller
		AgencyPortMessagesControllerForTesting Controller
		{
			get
			{
				return controller ?? (controller = new AgencyPortMessagesControllerForTesting());
			}
		}

		AgencyPortMessagesControllerForTesting controller;
		#endregion
		#region AgencyPortMessagesControllerForTesting
		public class AgencyPortMessagesControllerForTesting : AgencyPortMessagesController
		{
			public ZPlugIn ExposedGetPlugIn(IBusiness businessEntity)
			{
				return GetPlugIn(businessEntity);
			}
		}

		#endregion
		public void TestSecurityCheckpoints()
		{
			var voyage = Factory.New<JobVoyage>();
			var controller = new AgencyAllocationController();
			AssertEquals(controller.GetCheckPointForView(voyage), Env.Security.SailingScheduleAllocationView);
			AssertEquals(controller.GetCheckPointForEdit(voyage), Env.Security.SailingScheduleAllocationEdit);
			AssertEquals(controller.GetCheckPointForNew(voyage), Env.Security.SailingScheduleAllocationEdit);
			AssertEquals(controller.GetCheckPointForDelete(voyage), Env.Security.None);
		}
		#endregion
	}
}
