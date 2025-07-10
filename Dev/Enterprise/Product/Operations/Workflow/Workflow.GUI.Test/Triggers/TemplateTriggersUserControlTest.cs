using System.Windows.Forms;
using Enterprise.Workflow.Business.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	class TemplateTriggersUserControlTest : WorkflowTestCase
	{
		[ExpectNoExceptions]
		public void TestShowControl_ShouldBindCorrectly()
		{
			var template = CreateTemplate(Factory);
			Factory.Save();

			using (var form = new ZForm(template))
			using (var control = new TemplateTriggersUserControl())
			{
				form.SetDataBinding(template.TemplateTriggers, string.Empty);

				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();
			}
		}
	}
}
