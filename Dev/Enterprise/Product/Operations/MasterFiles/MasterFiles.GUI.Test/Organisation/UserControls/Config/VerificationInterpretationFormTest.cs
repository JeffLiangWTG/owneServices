using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(VerificationInterpretationForm))]
	sealed class VerificationInterpretationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new VerificationInterpretationForm("TEST VERIFICATION");
		}
	}
}
