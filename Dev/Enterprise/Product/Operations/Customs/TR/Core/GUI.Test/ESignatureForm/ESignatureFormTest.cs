using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ESignatureForm))]
	public class ESignatureFormTest : ZFormBasherTest
	{
		public void TestClickSendMessageWithoutTRBPassword()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null)))
			{
				signForm.Show();

				var signButton = signForm.Controls.Find("SignButton", true).Single() as ZButton;
				AssertNotNull("Sign Button", signButton);
				signButton.Enabled = true;
				signButton.PerformClick();
				AssertEquals("Notify message text, user should create broker information on the staff", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please edit your staff record to add the broker in the Brokerage tab"));
			}
		}

		public void TestClickSendMessageWithoutChipset()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var externalPasswordInfo = CreateExternalPassword(staff, "", "02b9572b9cad7250a906b3");
				var messageSign = new MessageSendAcknowledgeAndSign(message, externalPasswordInfo);

				using (var signForm = new ESignatureForm(messageSign))
				{
					signForm.Show();

					var signButton = signForm.Controls.Find("SignButton", true).Single() as ZButton;
					AssertNotNull("Sign Button", signButton);
					signButton.Enabled = true;
					signButton.PerformClick();
					AssertEquals("Notify message text, Missed Chip-set", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Make sure fill Chip-set"));
				}
			}
		}

		public void TestClickSendMessageWithInvalidCertificateSerialNumber()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var externalPasswordInfo = CreateExternalPassword(staff, "EKART", "abc");
				var messageSign = new MessageSendAcknowledgeAndSign(message, externalPasswordInfo);

				using (var signForm = new ESignatureForm(messageSign))
				{
					signForm.Show();

					var signButton = signForm.Controls.Find("SignButton", true).Single() as ZButton;
					AssertNotNull("Sign Button", signButton);
					signButton.Enabled = true;
					signButton.PerformClick();

					CombineAssertions("Send Message With Invalid CertificateSerial Number and valid Chip-set", () =>
					{
						AssertEquals("Notify message text, user should create broker information on the staff", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please edit your staff record to add the broker in the Brokerage tab"));
						AssertEquals("Notify message text, Missed Chip-set", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Make sure fill Chip-set"));
						AssertEquals("Notify message text, Invalid certificate serial number", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Invalid certificate serial number"));
					});
				}
			}
		}
		public void TestClickSendMessageWithValidCertificateSerialNumber()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var externalPasswordInfo = CreateExternalPassword(staff, "EKART", "02b9572b9cad7250a906b3");
				var messageSign = new MessageSendAcknowledgeAndSign(message, externalPasswordInfo);

				using (var signForm = new ESignatureForm(messageSign))
				{
					signForm.Show();

					var signButton = signForm.Controls.Find("SignButton", true).Single() as ZButton;
					AssertNotNull("Sign Button", signButton);
					signButton.PerformClick();

					CombineAssertions("Send Message With Valid CertificateSerial Number and Chip-set", () =>
					{
						AssertEquals("Notify message text, user should create broker information on the staff", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please edit your staff record to add the broker in the Brokerage tab"));
						AssertEquals("Notify message text, Missed Chip-set", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Make sure fill Chip-set"));
						AssertEquals("Notify message text, Invalid certificate serial number", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Invalid certificate serial number"));
					});
				}
			}
		}

		public void TestSignButtonEnabledOrNotWithPINCodeLength()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null)))
			{
				signForm.Show();

				var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
				AssertNotNull("PinCodeTextBox", pinCodeTextBox);

				var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
				AssertNotNull("Sign Button", signButton);

				pinCodeTextBox.Text = "12345";

				AssertEquals("Sign button should be enabled", true, signButton.Enabled);

				pinCodeTextBox.Text = "123";

				AssertEquals("Sign button should be disabled", false, signButton.Enabled);
			}
		}

		public void TestUpdateSignButtonLabelAndPinCodeTextBoxVisibilityForWithoutSign()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var item = TRCustomsDataRegistry.Instance.SendTRNCTSMessageWithoutSign;

				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.TRN)))
				{
					signForm.Show();

					var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
					AssertNotNull("PinCodeTextBox", pinCodeTextBox);

					var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
					AssertNotNull("Sign Button", signButton);

					CombineAssertions(() =>
					{
						AssertEquals("Sign button should be enabled", true, signButton.Enabled);
						AssertEquals("Sign button text should be Send", "Send", signButton.Text);
						AssertEquals("Pin Code TextBox should be disable", false, pinCodeTextBox.Enabled);
					});
				}

				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.TR5)))
				{
					signForm.Show();

					var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
					AssertNotNull("PinCodeTextBox", pinCodeTextBox);

					var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
					AssertNotNull("Sign Button", signButton);

					CombineAssertions(() =>
					{
						AssertEquals("Sign button should be enabled", true, signButton.Enabled);
						AssertEquals("Sign button text should be Send", "Send", signButton.Text);
						AssertEquals("Pin Code TextBox should be disable", false, pinCodeTextBox.Enabled);
					});
				}
			}
		}

		public void TestUpdateSignButtonLabelAndPinCodeTextBoxVisibilityForSign()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var item = TRCustomsDataRegistry.Instance.SendTRNCTSMessageWithoutSign;

				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null)))
				{
					signForm.Show();

					var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
					AssertNotNull("PinCodeTextBox", pinCodeTextBox);

					var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
					AssertNotNull("Sign Button", signButton);

					AssertEquals("[PreCondition]: Sign button should be disabled without pincode", false, signButton.Enabled);
					pinCodeTextBox.Text = "12345";

					CombineAssertions(() =>
					{
						AssertEquals("Sign button should be enabled", true, signButton.Enabled);
						AssertEquals("Sign button text should be Sign", "Sign", signButton.Text);
						AssertEquals("Pin Code TextBox should be enable", true, pinCodeTextBox.Enabled);
					});
				}
			}
		}

		public void TestSignUIShouldHaveSignButtonWhenRegistryValueFalse()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.TRN)))
			{
				signForm.Show();

				var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
				AssertNotNull("PinCodeTextBox", pinCodeTextBox);

				var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
				AssertNotNull("Sign Button", signButton);

				AssertEquals("[PreCondition]: Sign button should be disabled without pincode", false, signButton.Enabled);
				pinCodeTextBox.Text = "12345";

				CombineAssertions(() =>
				{
					AssertEquals("Sign button should be enabled", true, signButton.Enabled);
					AssertEquals("Sign button text should be Sign", "Sign", signButton.Text);
					AssertEquals("Pin Code TextBox should be enable", true, pinCodeTextBox.Enabled);
				});
			}

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.TR5)))
			{
				signForm.Show();

				var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
				AssertNotNull("PinCodeTextBox", pinCodeTextBox);

				var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
				AssertNotNull("Sign Button", signButton);

				AssertEquals("[PreCondition]: Sign button should be disabled without pincode", false, signButton.Enabled);
				pinCodeTextBox.Text = "12345";

				CombineAssertions(() =>
				{
					AssertEquals("Sign button should be enabled", true, signButton.Enabled);
					AssertEquals("Sign button text should be Sign", "Sign", signButton.Text);
					AssertEquals("Pin Code TextBox should be enable", true, pinCodeTextBox.Enabled);
				});
			}
		}

		public void TestSignButtonAndPinCodeShouldBeDisabledForExportUnion()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.EUT)))
				{
					signForm.Show();

					var pinCodeTextBox = signForm.Controls.Find("PinCodeTextBox", true).SingleOrDefault() as ZTextBox;
					AssertNotNull("PinCodeTextBox", pinCodeTextBox);

					var signButton = signForm.Controls.Find("SignButton", true).SingleOrDefault() as ZButton;
					AssertNotNull("Sign Button", signButton);

					CombineAssertions(() =>
					{
						AssertEquals("Sign button should be enabled", true, signButton.Enabled);
						AssertEquals("Sign button text should be Send", "Send", signButton.Text);
						AssertEquals("Pin Code TextBox should be disable", false, pinCodeTextBox.Enabled);
					});
				}
			}
		}

		public void TestSignNCTSP5HasNationalXMLUI()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.TRN)))
				{
					signForm.Show();

					var nationalXMLContentsTextBox = signForm.Controls.Find("NationalXMLContentsTextBox", true).SingleOrDefault() as ZTextBox;
					CombineAssertions(("NCTS P4 message"), () =>
					{
						AssertNotNull("NationalXMLContentsTextBox", nationalXMLContentsTextBox);
						AssertEquals("National xml visibility", false, nationalXMLContentsTextBox.Visible);
					});
				}

				using (var signForm = new ESignatureForm(new MessageSendAcknowledgeAndSign(message, null, TRMessageTypes.Codes.TR5)))
				{
					signForm.Show();

					var nationalXMLContentsTextBox = signForm.Controls.Find("NationalXMLContentsTextBox", true).SingleOrDefault() as ZTextBox;
					CombineAssertions(("NCTS P5 message"), () =>
					{
						AssertNotNull("NationalXMLContentsTextBox", nationalXMLContentsTextBox);
						AssertEquals("NationalXMLContentsTextBox contents", "National XML", nationalXMLContentsTextBox.Text);
						AssertEquals("National xml visibility", true, nationalXMLContentsTextBox.Visible);
					});
				}
			}
		}

		MasterFiles.Business.GlbStaff staff;
		protected override void SetUp()
		{
			staff = Factory.New<MasterFiles.Business.GlbStaff>();
			staff.GS_Code = "TSD";
			staff.GS_FullName = "TR Testing User";

			base.SetUp();
		}

		protected override Form GetFormToBashCore()
		{
			var externalPasswordInfo = CreateExternalPassword(staff, "EKART", "02b9572b9cad7250a906b3");
			return new ESignatureForm(new MessageSendAcknowledgeAndSign(message, externalPasswordInfo));
		}

		GlbExternalPassword_TR CreateExternalPassword(MasterFiles.Business.GlbStaff staff, string chipset, string certificateSerialNumber)
		{
			var result = TRGlbStaffWrapper.Get(staff).TRBPassword;
			result.GP_UserID = "1234";
			result.CurrentDecryptedPassword = "xxx";
			result.GP_CertificateAuthority = "TÜBİTAK";
			result.TR_Chipset = chipset;
			result.GP_CertificateSerialNumber = certificateSerialNumber;
			return result;
		}

		readonly ZString message = "<Body><Test>MessageToSign</Test></Body>";
	}
}


