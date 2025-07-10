using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SenderFormCreateEmailUserControlTest : TestCaseWithFactory
	{
		public void TestOpenAddressBook()
		{
			ISendEmailSource source = Factory.New<OrgHeader>();
			var email = new HtmlEmailWithAttachment(source);
			using (var form = new ZForm(email))
			{
				var emailUserControl = new HtmlEmailUserControl();
				form.Controls.Add(emailUserControl);
				form.Show();

				emailUserControl.ToButton.PerformClick();
				AssertEquals(typeof(RecipientSelectionForm), ZFormModaliser.LastFormShownForTest.GetType());

				if (ZFormModaliser.LastFormShownForTest != null)
				{
					((ZForm)ZFormModaliser.LastFormShownForTest).Dispose();
				}
			}
		}

		[RequiresSTA]
		public void TestInsertCurrentUserEmailAddressIntoTextBoxOnKeyPress()
		{
			using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly("test@test.com"))
			{
				ISendEmailSource source = Factory.New<OrgHeader>();
				var email = new HtmlEmailWithAttachment(source);
				using (var form = new ZForm(email))
				{
					var emailUserControl = new HtmlEmailUserControl();
					form.Controls.Add(emailUserControl);
					form.Show();
					Application.DoEvents();
					Message msg = new Message();
					var toTextBox = (ZTextBox)form.Controls.Find("ToTextBox", true)[0];
					var ccTextBox = (ZTextBox)form.Controls.Find("CcTextBox", true)[0];
					var bccTextBox = (ZTextBox)form.Controls.Find("BccTextBox", true)[0];

					var processCmdKey = typeof(ZTextBox).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic);
					object[] args = new object[] { msg, Keys.Control | Keys.E };

					processCmdKey.Invoke(toTextBox, args);
					AssertEquals("test@test.com", toTextBox.Text);
					AssertEquals("", ccTextBox.Text);
					AssertEquals("", bccTextBox.Text);

					ccTextBox.Focus();
					processCmdKey.Invoke(ccTextBox, args);
					AssertEquals("test@test.com", toTextBox.Text);
					AssertEquals("test@test.com", ccTextBox.Text);
					AssertEquals("", bccTextBox.Text);

					bccTextBox.Focus();
					processCmdKey.Invoke(bccTextBox, args);
					AssertEquals("test@test.com", toTextBox.Text);
					AssertEquals("test@test.com", ccTextBox.Text);
					AssertEquals("test@test.com", bccTextBox.Text);
				}
			}
		}

		public void TestEmailAddressShouldBeManuallyEntered()
		{
			using (var control = new HtmlEmailUserControlForTest())
			{
				AssertEquals(true, control.ToTextBox_Exposed.Enabled);
				AssertEquals(true, control.CcTextBox_Exposed.Enabled);
				AssertEquals(true, control.BccTextBox_Exposed.Enabled);
			}
		}

		class HtmlEmailUserControlForTest : HtmlEmailUserControl
		{
			public HtmlEmailUserControlForTest() : base()
			{
			}

			public ZTextBox ToTextBox_Exposed => ToTextBox;
			public ZTextBox CcTextBox_Exposed => CcTextBox;
			public ZTextBox BccTextBox_Exposed => BccTextBox;
		}
	}
}
