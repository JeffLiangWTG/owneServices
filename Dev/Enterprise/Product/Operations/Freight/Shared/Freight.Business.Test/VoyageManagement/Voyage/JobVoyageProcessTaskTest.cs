using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobVoyageProcessTask))]
	sealed class JobVoyageProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			var task = (JobVoyageProcessTask)((IWorkflowProvider)voyage).WorkflowItems.AddNew();

			AssertEquals(ControllerIDs.JobAirSailing, task.ParentControllerID);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			task = (JobVoyageProcessTask)((IWorkflowProvider)voyage).WorkflowItems.AddNew();

			AssertEquals(ControllerIDs.JobRailSailing, task.ParentControllerID);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			task = (JobVoyageProcessTask)((IWorkflowProvider)voyage).WorkflowItems.AddNew();

			AssertEquals(ControllerIDs.JobRoadSailing, task.ParentControllerID);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			task = (JobVoyageProcessTask)((IWorkflowProvider)voyage).WorkflowItems.AddNew();

			AssertEquals(ControllerIDs.JobSeaVoyage, task.ParentControllerID);
		}

		public void TestParent()
		{
			IWorkflowProvider voyage = Factory.New<JobVoyage>();
			var task = (JobVoyageProcessTask)voyage.WorkflowItems.AddNew();
			AssertEquals(voyage, task.Parent);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			IWorkflowProvider voyage = Factory.New<JobVoyage>();
			return voyage.WorkflowItems.AddNew();
		}
	}
}
