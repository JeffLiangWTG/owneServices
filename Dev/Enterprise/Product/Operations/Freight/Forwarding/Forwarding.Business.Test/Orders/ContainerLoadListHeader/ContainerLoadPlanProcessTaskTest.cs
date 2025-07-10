using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadPlanProcessTask))]
	sealed class ContainerLoadPlanProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Factory.NewWithValidTestData<CFSContainerLoadList>();
			return booking.WorkflowItems.AddNew();
		}

		#endregion
	}
}

