using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgDebtorGroupForm))]
	sealed class OrgDebtorGroupFormTest : ZFormBasherTest
	{
		public void TestAuditPluginIsAdded()
		{
			using (var form = (OrgDebtorGroupForm)GetFormToBashCore())
			{
				AssertNotNull("Debtor groups form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new OrgDebtorGroupForm(Factory.New<OrgDebtorGroup>());
		}
	}
}
