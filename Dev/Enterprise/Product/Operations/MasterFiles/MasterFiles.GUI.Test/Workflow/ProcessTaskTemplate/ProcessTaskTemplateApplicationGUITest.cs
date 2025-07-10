using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class ProcessTaskTemplateApplicationGUITest : TestCaseWithFactory
	{
		ProcessTask CreateTask(IWorkflowProvider provider)
		{
			var task = provider.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "task" + task.P9_Sequence;
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = @"""1""==""1""";
			return task;
		}

		ProcessTaskTemplate CreateTemplateWithTask()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			CreateTask(template);
			return template;
		}

		DummyWithWorkflow CreateJob() => Factory.NewWithValidTestData<DummyWithWorkflow>();

		void AssertTemplateApplied(string message, bool applied, IWorkflowProvider provider)
		{
			var fn = applied ? new Func<bool, bool>(f => f) : f => !f;
			Assert(message, fn(provider.WorkflowItems.Tasks.Count > 0));
		}

		public void TestFormScope_AppliesToBoundItem()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToForm(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Unworthy of template application", false, job);

			using (var bind = FakeBinding(job))
			{
				job.ApplyWorkflowTemplates();
				AssertTemplateApplied("It bind, so we find.", true, job);
			}
		}

		Form FakeBinding(DummyWithWorkflow job)
		{
			var form = new Form();
			var textbox = new ZTextBox();
			textbox.SetDataBinding(job, "Z0_Code");
			var bindo = textbox.DataBindings.OfType<KBinding>().First(n => n.PropertyName == "Text");
			form.Controls.Add(textbox);
			form.Show();
			Application.DoEvents();
			Assert(bindo.IsBinding);
			return form;
		}
	}
}
