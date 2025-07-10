using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ContainerBatchSummary))]
	public class ContainerBatchSummaryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContainerBatchSummary(ContainersForTest);
		}

		protected ContainerBatchSummary BatchSummaryForTest
		{
			get
			{
				if (batchSummaryForTest == null)
				{
					batchSummaryForTest = GetNewBusinessObject() as ContainerBatchSummary;
				}
				return batchSummaryForTest;
			}
		}
		ContainerBatchSummary batchSummaryForTest;

		protected TrackingContainerStandaloneCollection ContainersForTest
		{
			get
			{
				if (containersForTest == null)
				{
					containersForTest = new TrackingContainerStandaloneCollection(Factory);
				}
				return containersForTest;
			}
		}
		TrackingContainerStandaloneCollection containersForTest;
	}
}
