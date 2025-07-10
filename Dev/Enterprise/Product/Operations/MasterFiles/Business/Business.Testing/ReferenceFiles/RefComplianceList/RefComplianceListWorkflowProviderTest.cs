using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefComplianceList))]
	public class RefComplianceListWorkflowProviderTest : WorkflowProviderTest<RefComplianceList, RefComplianceListProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.RefComplianceListWorkflowDescriptorCode; }
		}
	}
}
