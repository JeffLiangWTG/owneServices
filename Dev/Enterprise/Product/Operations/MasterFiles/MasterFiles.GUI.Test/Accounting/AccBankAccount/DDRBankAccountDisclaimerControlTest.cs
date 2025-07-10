using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class DDRBankAccountDisclaimerControlTest : TestCaseWithFactory
	{
		public void TestDisclaimerMessageAndButtons()
		{
			using (var form = new ZForm())
			using (var userControl = new DDRBankAccountDisclaimerControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var disclaimerMessageLabel = form.Controls.Find("DisclaimerMessageLabel", true)[0] as ZLabel;
				AssertNotNull(disclaimerMessageLabel);
				AssertEquals(@"You have selected an ABA file format for this bank account.
In order for your DDR file to be accepted by the bank, please ensure you have recorded the ""User ID No"" and that you have set the ""Abbreviation"" to the approved code for the financial institution.", disclaimerMessageLabel.Text);

				var okButton = form.Controls.Find("OKButton", true)[0] as ZButton;
				AssertNotNull(okButton);
				AssertEquals("DialogResult must be Cancel, otherwise clicking this button will not close the form.", DialogResult.Cancel, okButton.DialogResult);
			}
		}

		public void TestDialogDefaultContext()
		{
			var context = DDRBankAccountDisclaimerControl.DialogDefaultContext;
			Assert(context.ShowCheckboxOnly);
			AssertEquals("Do not show this message again", context.CheckBoxCaption.Caption);
			AssertEquals("Save DDR Account", context.Caption);
			AssertEquals(ZMessageBoxIcon.Information, context.Icon);
		}
	}
}
