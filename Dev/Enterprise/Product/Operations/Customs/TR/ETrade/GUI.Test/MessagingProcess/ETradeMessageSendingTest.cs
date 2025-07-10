using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.TR.ETrade.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI.MessagingProcess.Testing
{
	public class ETradeMessageSendingTest : TestCaseWithFactory
	{
		public void TestSendMessageInvalidUserDetails()
		{
			SetupCurrentUser(false);
			var header = SetupHeader();

			Factory.Save();

			var result = SendMessage(header, System.Windows.Forms.DialogResult.OK);
			CombineAssertions(() =>
			{
				AssertEquals("Sending Succeeded", false, result.Success);
				AssertEquals("Notifications", @"Your staff profile requires an email address as this is needed for messaging.
Your customs credentials are marked as invalid, please update your customs credentials.
A valid certificate serial number is required on your staff record on the Brokerage tab", result.Notifications.CreateSummaryNotification(true).Message);
				AssertEquals("Message Count", 0, result.EDIMessages?.Count ?? 0);
			});
		}

		public void TestSendMessageBOValidation()
		{
			SetupCurrentUser(true);
			var header = SetupHeader();

			Factory.Save();

			var result = SendMessage(header, System.Windows.Forms.DialogResult.OK);
			CombineAssertions(() =>
			{
				AssertEquals("Sending Succeeded", false, result.Success);
				AssertContains("Notifications", MessageSendingValidation.MessageErrorsExistHeaderText, result.PreviousNotifications.CreateSummaryNotification(false).Message);
				AssertEquals("Message Count", 0, result.EDIMessages?.Count ?? 0);
			});
		}

		public void TestSendMessageSuccess()
		{
			SetupCurrentUser(true);
			var header = SetupHeader();

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			var result = SendMessage(header, System.Windows.Forms.DialogResult.OK);
			header.Reload();
			CombineAssertions(() =>
			{
				AssertEquals("Sending Succeeded", true, result.Success);
				AssertEquals("Notifications", "1 Message(s) queued for sending", result.Notifications.CreateSummaryNotification(false).Message);
				AssertEquals("Message Count", 1, result.EDIMessages.Count);
				AssertEquals("Msgs on header", 1, header.Messages.Count);
			});
		}

		public void TestSendMessageSigningCancelled()
		{
			SetupCurrentUser(true);
			var header = SetupHeader();

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			var result = SendMessage(header, System.Windows.Forms.DialogResult.Cancel);
			CombineAssertions(() =>
			{
				AssertEquals("Sending aborted", false, result.Success);
				AssertEquals("Notifications", "Message signing canceled", result.Notifications.CreateSummaryNotification(false).Message);
				AssertEquals("Message Count", 1, result.EDIMessages.Count);
				AssertEquals("Msg Deleted", true, result.EDIMessages[0].IsDeleted);
			});
		}

		ActionResult SendMessage(AsycudaManifestHeader header, System.Windows.Forms.DialogResult userAction)
		{
			using (var mainForm = new ZForm(header))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = userAction;

				var providerFactory = new TRCustomsMessagingProviderFactory(ETradeCustomsMessagingProvider.New, TRMessageTypes.Codes.TRE, TRMessageSignerForTest.New());

				return TRCustomsMessagingGui.SendMessages(header, providerFactory, mainForm);
			}
		}
		void SetupCurrentUser(bool valid)
		{
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			GlbStaff.CurrentUser.GS_Code = "CZH";
			currentUser.GP_UserID = "20201224104";
			currentUser.GP_PasswordType = PasswordTypesList.Codes.TRK;
			currentUser.GP_GC = Env.CurrentBranch.Company.PK;
			currentUser.GP_GS = currentUser.PK;
			currentUser.CurrentDecryptedPassword = "12345678";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.PINCode = "123546";

			if (valid)
			{
				GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
				currentUser.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
				currentUser.GP_StatusReason = "";
				currentUser.TR_Chipset = "EKART";
				currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			}
			else
			{
				GlbStaff.CurrentUser.GS_EmailAddress = "";
				currentUser.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				currentUser.GP_StatusReason = "Break validation";
				currentUser.TR_Chipset = "";
				currentUser.GP_CertificateSerialNumber = "Invalid";
			}
			Factory.Save();
		}

		AsycudaManifestHeader SetupHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_JobReference = "MAN0000442";
			header.AMA_ManifestType = "ATAİTH";
			return header;
		}
	}
}
