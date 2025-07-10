using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderProcessTask))]
	public class CYDAdHocServiceOrderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var advice = Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
			return advice.WorkflowItems.AddNew();
		}
	}
}
