using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AuditMessageForm))]
	sealed class AuditMessageFormTest : ZFormBasherTest
	{
		public void TestAuditMessageForm()
		{
			using (var form = new AuditMessageForm())
			{
				form.Show();
				AssertContains("Set audit message", form.FormCaption);
				AssertNullOrEmpty(form.ReferenceText);
				form.ReferenceText = "AAA";
				AssertEquals("AAA", form.ReferenceText);
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
				form.ReferenceText = "BBB";
				AssertEquals("BBB", form.ReferenceText);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new AuditMessageForm();
		}
	}
}
