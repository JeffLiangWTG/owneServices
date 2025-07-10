using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementProcessTask))]
	public class GteGateMovementProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var transportationUnit = Factory.NewWithValidTestData<GteGateMovement>();
			return transportationUnit.WorkflowItems.AddNew();
		}
	}
}
