using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDDeliveryHeaderProcessTask))]
	public class CYDDeliveryHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var deliveryHeader = Factory.NewWithValidTestData<CYDDeliveryHeader>();
			return deliveryHeader.WorkflowItems.AddNew();
		}
	}
}
