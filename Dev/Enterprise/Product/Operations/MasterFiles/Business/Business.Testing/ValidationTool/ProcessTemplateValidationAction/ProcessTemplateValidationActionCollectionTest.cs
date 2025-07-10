using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTemplateValidationActionCollection))]
	sealed class ProcessTemplateValidationActionCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTemplateValidationActionCollection>
	{
		public void TestP0A_P0_WorkflowTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			var processTemplateValidationAction = templateValidation.ProcessTemplateValidationActions.AddNew();
			AssertEquals(template.PK, processTemplateValidationAction.P0A_P0_WorkflowTemplate);
		}

		protected override ProcessTemplateValidationActionCollection GetCollectionToTest()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			return new ProcessTemplateValidationActionCollection(template);
		}
	}
}
