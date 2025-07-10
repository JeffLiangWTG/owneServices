using System.Windows.Forms;
using NUnit.Framework;
using static Enterprise.Customs.GUI.MultiJobDeclarationForm;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(EmbeddedModulePopupWithNoButtonPanel))]
	sealed class EmbeddedModulePopupWithNoButtonPanelAndNoModalityBasherTest : ZArchitecture.GUI.Internal.Testing.EmbeddModulePopupBasherTest
	{
		protected override Form GetFormToBashCore() => new EmbeddedModulePopupWithNoButtonPanel(new ZArchitecture.Modules.Testing.DummyFilterGridModule());
	}
}
