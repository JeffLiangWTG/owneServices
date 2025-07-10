using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefTimeZoneSetForm))]
	sealed class RefTimeZoneSetFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefTimeZoneSetForm(Factory.New<RefTimeZoneSet>());
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefTimeZoneSetForm)GetFormToBashCore())
			{
				AssertNotNull("RefTimeZoneSetForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
