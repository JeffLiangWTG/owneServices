using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(AdditionalCodesForm))]
	sealed class AdditionalCodesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new AdditionalCodesForm();

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => control.Name == "SealNumbersGrid";
	}
}
