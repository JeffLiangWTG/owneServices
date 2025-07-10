using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceHeader))]
	public class AccDraftInvoiceHeaderWorkflowProviderTest : WorkflowProviderTest<AccDraftInvoiceHeader, AccDraftInvoiceHeaderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.AccDraftInvoiceCode;

		public void TestTemplateIsAppliedToCorrectClients()
		{
			var clientWithTemplate = Factory.NewWithValidTestData<OrgHeader>();
			var clientWithoutTemplate = Factory.NewWithValidTestData<OrgHeader>();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;
			template.P0_OH_Client = clientWithTemplate.PK;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var invocie1 = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			invocie1.AIH_OH_Creditor = clientWithTemplate.PK;

			var invoice2 = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			invoice2.AIH_OH_Creditor = clientWithoutTemplate.PK;

			Factory.Save();

			AssertEquals("Template tasks created when client matches", 1, invocie1.WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT created when client doesn't match", 0, invoice2.WorkflowItems.Milestones.Count);
		}
	}
}
