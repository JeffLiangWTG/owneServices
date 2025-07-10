using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Freight.Business
{
	[TestedType(typeof(SelectFromCollectionHelper.SelectorEmbeddedModulePopup))]
	sealed class SelectorEmbeddedModulePopupBasherTest : ZArchitecture.GUI.Internal.Testing.EmbeddModulePopupBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var module = new ZArchitecture.Modules.Testing.DummyFilterGridModule();
			var helper = new SelectFromCollectionHelper(new CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection(Factory), module);
			var form = new SelectFromCollectionHelper.SelectorEmbeddedModulePopup(helper, module);
			form.Disposed += (sender, args) => module.Dispose();
			return form;
		}
	}
}
