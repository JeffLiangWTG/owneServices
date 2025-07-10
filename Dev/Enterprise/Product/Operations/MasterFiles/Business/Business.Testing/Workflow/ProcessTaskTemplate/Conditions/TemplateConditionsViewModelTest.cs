using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TemplateConditionsViewModel))]
	class TemplateConditionsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region GetSampleContext

		public void TestGetSampleContext_WithShipmentProcess_CreatesInstanceOfType()
		{
			// Arrange
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			
			// Act
			var result = ((IAntlrMacroContextProvider)viewModel).GetSampleContext();

			// Assert
			AssertEquals(template.WorkflowDescriptor.WorkflowProviderType, result.ParentType);

			result.Dispose();
		}

		public void TestGetSampleContext_ContainsExpectedVariables()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

				// Act
				var result = ((IAntlrMacroContextProvider)viewModel).GetSampleContext();

				Assert(result.Variables.ContainsKey("env"));
				Assert(!result.Variables.ContainsKey("Event"));
				Assert(!result.Variables.ContainsKey("EventSource"));
				Assert(result.Variables.ContainsKey("WorkflowItem"));

				result.Dispose();
			}
		}

		#endregion

		#region TemplateCondition2Value

		public void TestTemplateCondition2Value()
		{
			var styleDecider = new DummyConditionValueStyleDecider();
			viewModel.ConditionValueStyleDecider = styleDecider;

			AssertEquals(true, viewModel.TemplateCondition2ValueInfo.ReadOnly);
			AssertEquals(nameof(FieldType.Text), viewModel.TemplateCondition2ValueFieldType);

			viewModel.TemplateCondition2 = "EEE";
			AssertEquals(true, viewModel.TemplateCondition2ValueInfo.ReadOnly);
			AssertEquals(nameof(FieldType.Text), viewModel.TemplateCondition2ValueFieldType);

			styleDecider.StyleToDecide = TemplateConditionValueStyle.DropDown;
			AssertEquals(false, viewModel.TemplateCondition2ValueInfo.ReadOnly);
			AssertEquals(nameof(FieldType.TextDropEdit), viewModel.TemplateCondition2ValueFieldType);

			viewModel.TemplateCondition2Value = "ABC";
			AssertEquals("ABC", viewModel.TemplateCondition2Value);
			AssertEquals(nameof(FieldType.TextDropEdit), viewModel.TemplateCondition2ValueFieldType);

			styleDecider.StyleToDecide = TemplateConditionValueStyle.Unused;
			viewModel.TemplateCondition2 = "XXX";
			AssertEquals(true, viewModel.TemplateCondition2ValueInfo.ReadOnly);
			AssertEquals(ZString.Empty, viewModel.TemplateCondition2Value);
			AssertEquals(nameof(FieldType.Text), viewModel.TemplateCondition2ValueFieldType);
		}

		public void TestTemplateCondition2Value_MaxLength()
		{
			AssertEquals(ProcessTasksSchema.P9_Condition2Value.MaxLength, viewModel.TemplateCondition2ValueInfo.MaxLength);
			AssertEquals(ProcessTemplateTriggerSchema.P9T_TemplateCondition2Value.MaxLength, viewModel.TemplateCondition2ValueInfo.MaxLength);

			viewModel.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			AssertEquals(int.MaxValue, viewModel.TemplateCondition2ValueInfo.MaxLength);

			viewModel.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			AssertEquals(int.MaxValue, viewModel.TemplateCondition2ValueInfo.MaxLength);
		}

		#endregion

		#region Condition2ValueFieldType

		public void TestCondition2ValueFieldType()
		{
			AssertEquals(nameof(FieldType.Text), viewModel.TemplateCondition2ValueFieldType);

			viewModel.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			AssertEquals(nameof(FieldType.TextMacro), viewModel.TemplateCondition2ValueFieldType);

			viewModel.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			AssertEquals(nameof(FieldType.AntlrMacro), viewModel.TemplateCondition2ValueFieldType);
		}

		#endregion

		#region Implementation

		ProcessTaskTemplate template;
		ITemplateTrigger trigger;
		TemplateConditionsViewModel viewModel;

		protected override void SetUp()
		{
			base.SetUp();

			template = Factory.New<ProcessTaskTemplate>();
			trigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			viewModel = new TemplateConditionsViewModel(trigger, template);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TemplateConditionsViewModel(trigger, template);
		}

		class DummyConditionValueStyleDecider : ITemplateConditionValueStyleDecider
		{
			internal TemplateConditionValueStyle StyleToDecide { get; set; }

			public TemplateConditionValueStyle Decide(ITemplateConditionalWorkflowItem workflowItem)
			{
				return StyleToDecide;
			}
		}

		#endregion
	}
}
