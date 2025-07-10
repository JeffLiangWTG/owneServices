using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReceiveAdviceProcessTask))]
	public class CYDReceiveAdviceProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var advice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			return advice.WorkflowItems.AddNew();
		}
	}
}
