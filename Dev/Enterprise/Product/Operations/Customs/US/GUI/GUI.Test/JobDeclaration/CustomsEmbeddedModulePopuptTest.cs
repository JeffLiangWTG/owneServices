using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(CustomsEmbeddedModulePopup))]
	sealed class CustomsEmbeddedModulePopuptTest : ZArchitecture.GUI.Internal.Testing.EmbeddModulePopupBasherTest
	{
		protected override Form GetFormToBashCore() => new CustomsEmbeddedModulePopup(new DummyFilterGridModule());
	}
}
