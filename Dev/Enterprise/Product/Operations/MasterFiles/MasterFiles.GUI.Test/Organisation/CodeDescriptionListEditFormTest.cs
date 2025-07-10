using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionListEditForm))]
	sealed class CodeDescriptionListEditFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CodeDescriptionListEditForm("Test");
		}
	}
}
