using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateProcessTaskLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActionConditionList()
		{
			ProcessTaskTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			var expectedList = new EventReferenceConditionList();
			var expectedElementsAsString = expectedList.ElementsAsString;

			AssertEquals(expectedList.Count, ProcessTask.TriggerConditions.Lookups.TriggerConditionList.Count);
			AssertEquals("With a valid WorkflowType", expectedElementsAsString, ProcessTask.TriggerConditions.Lookups.TriggerConditionList.ElementsAsString);
		}

		public void TestLineTriggerTypesGetter_ReturnTypesFromTemplateProcessType()
		{
			var sampleTypes = new[]
			{
				TriggerLineTypes.Codes.ForwardingShipment
			};

			DummyWorkflowDescriptor.Instance.SupportedTriggerLineTypes = sampleTypes;

			var template = Factory.New<ProcessTaskTemplate>();
			var trigger = (TemplateProcessTask)template.WorkflowItems.Triggers.AddNew();

			template.P0_ProcessType = ZString.Empty;
			var lookup = new TemplateProcessTaskLookups(trigger);
			var actualTypes = lookup.LineTriggerTypes.ToList<CodeDescriptionPair>().Select(p => p.Code);
			AssertContainsExactElementsInAnyOrder("Types", System.Array.Empty<string>(), actualTypes);

			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			lookup = new TemplateProcessTaskLookups(trigger);
			actualTypes = lookup.LineTriggerTypes.ToList<CodeDescriptionPair>().Select(p => p.Code);
			AssertContainsExactElementsInAnyOrder("Types", sampleTypes
				.Append(ProcessTasksLookups.TaskLineTriggerCode)
				.Append(ProcessTasksLookups.ExceptionLineTriggerCode),
				actualTypes);
		}

		#region Implementation

		TemplateProcessTask ProcessTask
		{
			get
			{
				if (processTask == null)
				{
					processTask = (TemplateProcessTask)ProcessTaskTemplate.WorkflowItems.AddNew();
				}
				return processTask;
			}
		}
		TemplateProcessTask processTask;

		ProcessTaskTemplate ProcessTaskTemplate
		{
			get
			{
				if (processTaskTemplate == null)
				{
					processTaskTemplate = Factory.New<ProcessTaskTemplate>();
				}
				return processTaskTemplate;
			}
		}
		ProcessTaskTemplate processTaskTemplate;

		#endregion
	}
}
