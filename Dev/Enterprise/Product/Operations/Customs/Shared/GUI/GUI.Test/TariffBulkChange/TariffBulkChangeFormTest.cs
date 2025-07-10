using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(TariffBulkChangeForm))]
	sealed class TariffBulkChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new TariffBulkChangeForm(new Business.TariffBulkChange(Factory), false);

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
