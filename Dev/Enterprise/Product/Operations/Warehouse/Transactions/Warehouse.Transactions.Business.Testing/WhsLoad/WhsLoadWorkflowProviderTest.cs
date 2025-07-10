using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoad))]
	public class WhsLoadWorkflowProviderTest : WorkflowProviderTest<WhsLoad, WhsLoadProcessTaskCollection>
	{
		#region ExpectedWorkflowType

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.WhsLoadWorkflowDescriptorCode;

		#endregion
	}
}
