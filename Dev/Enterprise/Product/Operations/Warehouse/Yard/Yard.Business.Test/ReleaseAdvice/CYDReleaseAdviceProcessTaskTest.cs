using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceProcessTask))]
	public class CYDReleaseAdviceProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var releaseAdvice = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			return releaseAdvice.WorkflowItems.AddNew();
		}
	}
}
