using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemPackageStateProcessTask))]
	public class WhsItemPackageStateProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var packageState = Factory.NewWithValidTestData<WhsItemPackageState>();
			return packageState.WorkflowItems.AddNew();
		}
	}
}
