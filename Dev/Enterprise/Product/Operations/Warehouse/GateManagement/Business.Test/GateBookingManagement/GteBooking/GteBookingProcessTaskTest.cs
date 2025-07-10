using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteBookingProcessTask))]
	public class GteBookingProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			return booking.WorkflowItems.AddNew();
		}
	}
}
