using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementBookingProcessTask))]
	public class GteGateMovementBookingProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var transportationUnit = Factory.NewWithValidTestData<GteGateMovementBooking>();
			return transportationUnit.WorkflowItems.AddNew();
		}
	}
}
