using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(JiraProjectSelectorForm))]
	class JiraProjectSelectorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new JiraProjectSelectorForm(Factory);
		}
	}
}
