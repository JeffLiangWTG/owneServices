using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Freight.GUI
{
	[TestedType(typeof(VoyageIFindBox.VoyageModulePopup))]
	sealed class VoyageModulePopupBasherTest : ZArchitecture.GUI.Internal.Testing.EmbeddModulePopupBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var module = new ZArchitecture.Modules.Testing.DummyFilterGridModule();
			var form = new VoyageIFindBox.VoyageModulePopup(module, _ => { });
			form.Disposed += (sender, args) => module.Dispose();
			return form;
		}
	}
}
