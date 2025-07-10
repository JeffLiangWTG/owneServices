
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemPackageState))]
	public class WhsItemPackageStateWorkflowProviderTest : WorkflowProviderTest<WhsItemPackageState, WhsItemPackageStateProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.TransitPackage;
	}
}