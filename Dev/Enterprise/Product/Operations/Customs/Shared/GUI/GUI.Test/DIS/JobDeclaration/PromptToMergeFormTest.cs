using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(PromptToMergeForm))]
	sealed class PromptToMergeFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new PromptToMergeForm();
	}
}
