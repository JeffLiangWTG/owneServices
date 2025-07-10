using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTemplateTriggerConditionsViewModel))]
	class ProcessTemplateTriggerConditionsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		ProcessTemplateTriggerConditionsViewModel viewModel;

		protected override BusinessObject GetNewBusinessObject()
		{
			return viewModel;
		}

		protected override void SetUp()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			var trigger = WorkflowTestCase.CreateTrigger(template);

			viewModel = new ProcessTemplateTriggerConditionsViewModel(trigger);
		}

		#endregion
	}
}
