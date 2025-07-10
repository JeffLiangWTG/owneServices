using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ModulePopupWithMultiSelect))]
	sealed class ModulePopupWithMultiSelectTest : ZArchitecture.GUI.Internal.Testing.EmbeddModulePopupBasherTest
	{
		protected override Form GetFormToBashCore() => new ModulePopupWithMultiSelect(new DummyFilterGridModule());
	}
}
