using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobShipmentPreplanning))]
	sealed class JobShipmentPreplanningWorkflowProviderTest : WorkflowProviderTest<JobShipmentPreplanning, JobShipmentPrePlanningProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.JobShipmentPreplanningWorkflowDescriptorCode; }
		}

		public void TestGetTemplateSelectionCriteria_ForBuyer()
		{
			Preplanning.Factory.Save();
			AssertGetTemplateFilterCriteria(Preplanning.BuyerPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		JobShipmentPreplanning Preplanning
		{
			get { return BusinessObject; }
		}
	}
}
