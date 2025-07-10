using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(MatchingTemplateForm))]
	sealed class MatchingTemplateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new MatchingTemplateForm(Factory.NewWithValidTestData<DummyWithWorkflow>().WorkflowItems.Tasks);

		[RequiresSTA]
		public void TestDoubleClickGrid_ShouldOpenMatchingTemplate()
		{
			var template1 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", name: "template1");
			var template2 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", name: "template2");
			var template3 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "BBB", name: "template3");

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.SubType1 = "AAA";

			using (var form = new MatchingTemplateForm(job.WorkflowItems.Tasks))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(2, form.TasksGrid.List.Count);

				form.TasksGrid.ListManager.Position = 0;
				AssertEquals("First template in the list should be the most specific template (ordering by Sequence by default)", template2.P0_Name, ((MatchingTemplateViewLine)form.TasksGrid.ListManager.GetCurrent()).Template.P0_Name);

				form.TasksGrid_MouseDoubleClick_ForTest();
				Application.DoEvents();

				AssertTemplateFormShown(template2);

				form.TasksGrid.ListManager.Position = 1;
				AssertEquals("Second template in the list should be next most specific template (ordering by Sequence by default)", template1.P0_Name, ((MatchingTemplateViewLine)form.TasksGrid.ListManager.GetCurrent()).Template.P0_Name);

				form.TasksGrid_MouseDoubleClick_ForTest();
				Application.DoEvents();

				AssertTemplateFormShown(template1);

				void AssertTemplateFormShown(ProcessTaskTemplate selectedTemplate)
				{
					using (var templateForm = Application.OpenForms.OfType<ProcessTaskTemplateForm>().SingleOrDefault())
					{
						AssertNotNull(templateForm);
						AssertEquals(selectedTemplate.P0_Name, templateForm.BusinessEntity.P0_Name);
					}
				}
			}
		}
	}
}
