using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListProcessTask))]
	sealed class ContainerLoadListProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Factory.NewWithValidTestData<CYContainerLoadList>();
			return booking.WorkflowItems.AddNew();
		}

		#endregion
	}
}
