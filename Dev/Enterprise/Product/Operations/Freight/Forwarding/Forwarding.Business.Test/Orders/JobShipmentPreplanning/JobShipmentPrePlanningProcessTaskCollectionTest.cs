using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobShipmentPrePlanningProcessTaskCollection))]
	sealed class JobShipmentPrePlanningProcessTaskCollectionTest : RoutingSupportProcessTaskCollectionTest<JobShipmentPrePlanningProcessTaskCollection>
	{
		public void TestSupportsContactAndAddress()
		{
			AssertEquals("Pre Advice Task DOES NOT support contacts and addresses", false, ((IWorkflowProvider)PreAdvice).WorkflowItems.SupportsContactAndAddress);
		}

		public void TestOriginCountry()
		{
			PreAdvice.EF_RL_NKPortLoad = "MYPKG";
			AssertEquals("MY", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			PreAdvice.EF_RL_NKPortDisch = "USORD";
			AssertEquals("US", Collection.DestinationCountry);
		}

		#region Implementation

		protected override JobShipmentPrePlanningProcessTaskCollection GetCollectionToTestCore()
		{
			return new JobShipmentPrePlanningProcessTaskCollection(PreAdvice);
		}

		JobShipmentPreplanning PreAdvice
		{
			get
			{
				if (preAdvice == null)
				{
					preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
				}
				return preAdvice;
			}
		}

		JobShipmentPreplanning preAdvice;

		#endregion
	}
}
