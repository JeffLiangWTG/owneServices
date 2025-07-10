using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateConditionsViewModelLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTemplateCondition2List_WithMcrEnabled_ContainsMcr()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(lookups.TemplateCondition2List.ContainsCode(ProcessTasksLookups.MacroCondition));
			}
		}

		public void TestTemplateCondition2List_WithMcrDisabled_DoesntContainMcr()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!lookups.TemplateCondition2List.ContainsCode(ProcessTasksLookups.MacroCondition));
			}
		}

		public void TestTemplateCondition1List()
		{
			AssertEquals("With an empty WorkflowType", 0, lookups.TemplateCondition1List.Count);

			var shipmentCond1List = new JobShipmentWorkflowCondition1CodeList();

			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals(shipmentCond1List.Count, lookups.TemplateCondition1List.Count);
			AssertEquals("With a valid WorkflowType", shipmentCond1List.ElementsAsString, lookups.TemplateCondition1List.ElementsAsString);

			template.P0_ProcessType = "XXX";
			AssertEquals("With an unknown WorkflowType", 0, lookups.TemplateCondition1List.Count);
		}

		public void TestTemplateCondition2List()
		{
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("With a valid WorkflowType", typeof(JobShipmentWorkflowCondition2CodeList), lookups.TemplateCondition2List.GetType());

			template.P0_ProcessType = "XXX";
			AssertEquals("With an unknown WorkflowType", 1, lookups.TemplateCondition2List.Count);
			AssertEquals("With an unknown WorkflowType", ProcessTasksLookups.UserDefinedCondition, lookups.TemplateCondition2List[0].Code);
		}

		public void TestTemplateCondition2ValueList()
		{
			AssertEquals(0, lookups.TemplateCondition2ValueList.Count);

			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			trigger.TemplateCondition2 = JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster;
			AssertEquals("Values list empty", 0, lookups.TemplateCondition2ValueList.Count);

			trigger.TemplateCondition2 = JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType;
			Assert("Values list not empty", lookups.TemplateCondition2ValueList.Count > 0);
		}

		ProcessTaskTemplate template;
		ITemplateTrigger trigger;
		TemplateConditionsViewModelLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();

			template = Factory.New<ProcessTaskTemplate>();
			trigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			var viewModel = new TemplateConditionsViewModel(trigger, template);
			lookups = viewModel.Lookups;
		}
	}
}
