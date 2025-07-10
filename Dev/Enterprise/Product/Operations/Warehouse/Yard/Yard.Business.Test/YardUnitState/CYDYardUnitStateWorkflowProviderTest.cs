using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitState))]
	public class CYDYardUnitStateWorkflowProviderTest : WorkflowProviderTest<CYDYardUnitState, CYDYardUnitStateProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDYardUnitStateWorkflowDescriptorCode;
	}
}
