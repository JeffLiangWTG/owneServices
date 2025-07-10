using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefAirlineForm))]
	sealed class RefAirlineFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefAirlineForm(Factory.New<RefAirline>());
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefAirlineForm)GetFormToBashCore())
			{
				AssertNotNull("RefAirlineForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
