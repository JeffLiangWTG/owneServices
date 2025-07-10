using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleMovementProcessTask))]
	public class GteVehicleMovementProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			return vehicleMovement.WorkflowItems.AddNew();
		}
	}
}
