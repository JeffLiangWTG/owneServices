using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(AgencyAllocationController))]
	internal class AgencyAllocationControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		[ExpectNoExceptions()]
		public void TestGettingPlugInOnDeletedVoyage()
		{
			AgencyAllocationControllerForTesting controller = new AgencyAllocationControllerForTesting();
			JobVoyage testVoyage = Factory.New<JobVoyage>();
			testVoyage.Delete();
			controller.ExposedGetPlugIn(testVoyage);
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

		#region Implementation
		public class AgencyAllocationControllerForTesting : AgencyAllocationController
		{
			public ZPlugIn ExposedGetPlugIn(IBusiness businessEntity)
			{
				return GetPlugIn(businessEntity);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyAllocation;
		}
		#endregion
	}
}
