
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderHeaderProcessTask))]
	public class MNRWorkOrderHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var advice = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			return advice.WorkflowItems.AddNew();
		}
	}
}

