using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(RefContainerStock))]
	internal class RefContainerStockWorkflowTest : WorkflowProviderTest<RefContainerStock, RefContainerStockProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.ContainerStockManagerWorkflowDescriptorCode;
			}
		}
	}
}
