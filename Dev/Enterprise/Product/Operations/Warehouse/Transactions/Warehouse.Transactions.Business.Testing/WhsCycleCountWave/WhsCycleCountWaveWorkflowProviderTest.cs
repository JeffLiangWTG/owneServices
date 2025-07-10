using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountWave))]
	public class WhsCycleCountWaveWorkflowProviderTest : WorkflowProviderTest<WhsCycleCountWave, WhsCycleCountWaveProcessTaskCollection>
	{
		#region ExpectedWorkflowType

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.WhsCycleCountWaveWorkflowDescriptorCode;

		#endregion
	}
}
