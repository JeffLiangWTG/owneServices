using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgCreditorGroupForm))]
	sealed class OrgCreditorGroupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OrgCreditorGroupForm(Factory.New<OrgCreditorGroup>());
		}

		public void TestFixedCheckBoxDNMCaption()
		{
			using (var form = GetFormToBash() as OrgCreditorGroupForm)
			{
				var control = form.Controls.Find("FixedCheckBoxDNM", true);
				AssertEquals(1, control.Length);
				var fixedCheckBoxDNMCheckBox = control[0] as ZCheckBox;
				AssertEquals("This invoice hold option is mandatory and cannot be deselected. This is the default invoice hold option on all claims, unless another default is nominated for this creditor group at ‘Default Invoice Hold Option on New Claims’.", fixedCheckBoxDNMCheckBox.CaptionResourceString.FullDescription);
				AssertEquals("DNM - Do not match any transactions while claim is open", fixedCheckBoxDNMCheckBox.CaptionResourceString.Caption);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (OrgCreditorGroupForm)GetFormToBashCore())
			{
				AssertNotNull("Creditor groups form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
