using System.Linq;
using System.Windows.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(ProcessCompanyLinkRuleForm))]
	class ProcessCompanyLinkRuleFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ProcessCompanyLinkRuleForm(Factory.New<ProcessCompanyLinkRule>());

		public void TestMacroField()
		{
			using (var form = new ProcessCompanyLinkRuleForm(Factory.New<ProcessCompanyLinkRule>()))
			{
				var textBox = form.FindAll<ZTextBox>().Single(t => t.Name == "macroTextBox");
				AssertEquals(true, textBox.SupportsMacroTemplates);
			}
		}
	}
}
