using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommercialInvoiceLineWorkflowDescriptor))]
	sealed class CommercialInvoiceLineWorkflowDescriptorBaseOnlyTest : CommercialInvoiceLineWorkflowDescriptorAbstractTest<CommercialInvoiceLineWorkflowDescriptor>
	{
		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals(string.Empty, WorkflowDescriptor.MilestoneTemplateHintCaption);
		}
	}
}
