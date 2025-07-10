using System.Windows.Forms;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingImportInitiatorForm))]
	sealed class NettingImportInitiatorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NettingImportInitiatorForm(new NettingImportInitiator(Factory));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "activityTrailTextBox" || base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
