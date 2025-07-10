using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommercialInvoiceWorkflowDescriptor))]
	sealed class CommercialInvoiceWorkflowDescriptorBaseOnlyTest : CommercialInvoiceWorkflowDescriptorAbstractTest<BaseJobComInvoiceHeader, CommercialInvoiceWorkflowDescriptor>
	{
		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}
	}
}
