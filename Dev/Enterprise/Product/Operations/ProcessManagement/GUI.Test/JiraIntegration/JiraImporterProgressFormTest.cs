using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(JiraImporterProgressForm))]
	class JiraImporterProgressFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return JiraImporterProgressForm.CreateNonfunctional_ForBasherTestOnly();
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
