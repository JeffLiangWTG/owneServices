using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupHeaderProcessTask))]
	public class CYDPickupHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var pickupHeader = Factory.NewWithValidTestData<CYDPickupHeader>();
			return pickupHeader.WorkflowItems.AddNew();
		}
	}
}
