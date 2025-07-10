using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiry))]
	sealed class SalesEnquiryWorkflowProviderTest : WorkflowProviderTest<SalesEnquiry, SalesEnquiryProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return new SalesEnquiryWorkflowDescriptor().Code; }
		}
	}
}
