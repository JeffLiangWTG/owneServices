using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(BizObjectPopupFinder.ModulePopup))]
	public class TestModulePopup : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BizObjectPopupFinder.ModulePopup(new DummyFilterGridModule());
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}
	}
}
