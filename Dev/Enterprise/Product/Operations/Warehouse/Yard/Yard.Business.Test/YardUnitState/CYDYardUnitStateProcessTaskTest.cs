using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitStateProcessTask))]
	public class CYDYardUnitStateProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			return yardUnitState.WorkflowItems.AddNew();
		}
	}
}

