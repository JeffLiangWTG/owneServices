using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitProcessTask))]
	public class CYDTransportationUnitProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			return transportationUnit.WorkflowItems.AddNew();
		}
	}
}
