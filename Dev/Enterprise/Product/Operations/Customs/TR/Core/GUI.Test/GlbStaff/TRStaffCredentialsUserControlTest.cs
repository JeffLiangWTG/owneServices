using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI.GlbStaff;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(TRStaffCredentialsUserControl))]
	class TRStaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestPasswordStatusReasonShortLength()
		{
			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword_TR>();
			externalPassword.GP_StatusReason = "Short test string.";
			externalPassword.GP_CertificateSerialNumber = "2090";
			externalPassword.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			externalPassword.GP_GS = glbStaff.PK;
			externalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var statusReasonTextBox = form.Controls.Find("StatusReasonTextBox", true)[0] as ZTextBox;
				AssertEquals(true, statusReasonTextBox.Visible);
				statusReasonTextBox.SelectionLength = 0;
				statusReasonTextBox.SelectionStart = statusReasonTextBox.Text.Length;
				statusReasonTextBox.Focus();
				statusReasonTextBox.ScrollToCaret();
			}
		}

		public void TestPasswordStatusReasonLongLength()
		{
			var externalPassword2 = Factory.NewWithValidTestData<GlbExternalPassword_TR>();
			externalPassword2.GP_StatusReason = "Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Very long test string repeating.Last line.Last line.Last line.";
			externalPassword2.GP_CertificateSerialNumber = "2090";
			externalPassword2.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			externalPassword2.GP_GS = glbStaff.PK;
			externalPassword2.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var statusReasonTextBox = form.Controls.Find("StatusReasonTextBox", true)[0] as ZTextBox;
				AssertEquals(true, statusReasonTextBox.Visible);
				statusReasonTextBox.SelectionLength = 0;
				statusReasonTextBox.SelectionStart = statusReasonTextBox.Text.Length;
				statusReasonTextBox.Focus();
				statusReasonTextBox.ScrollToCaret();
			}
		}

		public void TestPasswordStatusReasonTextboxIsInVisible()
		{
			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword_TR>();
			externalPassword.GP_StatusReason = "has a text";
			externalPassword.GP_CertificateSerialNumber = "2090";
			externalPassword.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			externalPassword.GP_GS = glbStaff.PK;
			externalPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var statusReasonTextBox = form.Controls.Find("StatusReasonTextBox", true)[0] as ZTextBox;
				AssertEquals(false, statusReasonTextBox.Visible);
			}
		}

		public void TestPasswordStatusReasonTextboxIsVisible()
		{
			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword_TR>();
			externalPassword.GP_StatusReason = "some error code";
			externalPassword.GP_CertificateSerialNumber = "2090";
			externalPassword.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			externalPassword.GP_GS = glbStaff.PK;
			externalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var statusReasonTextBox = form.Controls.Find("StatusReasonTextBox", true)[0] as ZTextBox;
				AssertEquals(true, statusReasonTextBox.Visible);
			}
		}

		public void TestTRCustomsCertificateDefiningLinkLabel()
		{
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				var linkLabel = form.Controls.Find("TRCustomsCertificateDefiningLinkLabel", true)[0] as ZLinkLabel;
				AssertNotNull(linkLabel);
			}
		}

		public void TestChooseButton()
		{
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				var buttonChoose = form.Controls.Find("ChooseButton", true)[0] as ZButton;
				AssertEquals(true, buttonChoose.Visible);
			}
		}

		public void TestCertificateInfButton()
		{
			using (var form = GetFormToBash())
			{
				var control = new TRStaffCredentialsUserControl();
				form.Controls.Add(control);
				form.Show();
				var buttonChoose = form.Controls.Find("CertificateInfButton", true)[0] as ZButton;
				AssertEquals(true, buttonChoose.Visible);
			}
		}
	}
}
