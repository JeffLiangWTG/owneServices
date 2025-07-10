using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(RefCurrencyForm))]
	sealed class RefCurrencyFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new RefCurrencyForm(Factory.New<RefCurrency>());

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefCurrencyForm)GetFormToBashCore())
			{
				AssertNotNull("RefCurrencyForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
