using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class EPaymentBankAccountDisclaimerControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestDisclaimerMessageAndButtons()
		{
			using (var form = new ZForm())
			using (var userControl = new EPaymentBankAccountDisclaimerControl(EPaymentProviderCodes.Codes.OFX))
			{
				form.Controls.Add(userControl);
				form.Show();

				var disclaimerMessageLabel = form.Controls.Find("DisclaimerMessageLabel", true)[0] as ZLabel;
				AssertNotNull(disclaimerMessageLabel);
				AssertEquals(@"Before you create an E-payment Account, you must first register your organization for an OFX account. Each user on your E-payment Account should be an authorized user on your OFX account. Click “Manage Account Users” to add users to your E-payment account.

Once authorized, users will be able to request FX quotes and book FX transactions with OFX from the Payment Processing module.

Please note that CargoWise provides messaging and information exchange only. FX quotes are issued and FX transactions are executed by OFX, a third party provider.

Click “Learn More” for more information about OFX and CargoWise Global Integrated Payments.", disclaimerMessageLabel.Text);

				var learnMoreButton = form.Controls.Find("LearnMoreButton", true)[0] as ZButton;
				AssertNotNull(learnMoreButton);
				var productMarketingWebURL = AccountingMasterFilesRegistry.Instance.EPaymentProductMarketingWebURL.Value;
				WebUrlLauncher.ClearLastUrlLaunched();
				learnMoreButton.PerformClick();
				AssertEquals(productMarketingWebURL, WebUrlLauncher.LastUrlLaunched);

				var okButton = form.Controls.Find("OKButton", true)[0] as ZButton;
				AssertNotNull(okButton);
				AssertEquals("DialogResult must be Cancel, otherwise clicking this button will not close the form.", DialogResult.Cancel, okButton.DialogResult);
			}
		}

		public void TestDialogDefaultContext()
		{
			var context = EPaymentBankAccountDisclaimerControl.DialogDefaultContext;
			Assert(context.ShowCheckboxOnly);
			AssertEquals("Do not show this message again", context.CheckBoxCaption.Caption);
			AssertEquals("Save E-Payment Account", context.Caption);
			AssertEquals(ZMessageBoxIcon.Information, context.Icon);
		}
	}
}
