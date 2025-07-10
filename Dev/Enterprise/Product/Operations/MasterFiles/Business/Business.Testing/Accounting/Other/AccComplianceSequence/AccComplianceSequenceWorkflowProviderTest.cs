using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequence))]
	sealed class AccComplianceSequenceWorkflowProviderTest : WorkflowProviderTest<AccComplianceSequence, AccComplianceSequenceProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return new AccComplianceSequenceWorkflowDescriptor().Code; }
		}

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => nameof(AccComplianceSequence.XD_Description);
	}
}
