using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.Customs.TR.NCTS.Business.MessagingProcess;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.TR.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	public class TestTRNctsDepartureMovementMessagingMenuProvider : TestCaseWithFactory
	{
		public void TestNewNctsDepartureMovementMessagingMenuProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var nctsMovementForm = new TRNctsMovementForm(header))
			{
				AssertType(typeof(TRNctsDepartureMovementMessagingMenuProvider), new TRNctsDepartureMovementMessagingMenuProvider(header, nctsMovementForm));
			}
		}

		[TestDate(2023, 08, 08)]
		public void TestSendDepartureMessasgeClickCore()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			Factory.Save();

			using (var nctsMovementForm = new TRNctsMovementForm(nctsHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var messagingMenuProvider = new TRNctsDepartureMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
				var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();
				var depMenuItem = menuItems.FindByText("Send Departure Message");
				depMenuItem.PerformClick();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				depMenuItem.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

				var result = SendMessage(nctsHeader);
				nctsHeader.Reload();
				CombineAssertions(() =>
				{
					AssertEquals("Sending Succeeded", true, result.Success);
					AssertEquals("Notifications", "1 Message(s) queued for sending", result.Notifications.CreateSummaryNotification(false).Message);
					AssertEquals("Msgs on header", 1, nctsHeader.Messages.Count);
					var message = nctsHeader.Messages[0];
					AssertEquals("TRN", message.EM_MessageType);
				});
			}
		}

		ActionResult SendMessage(NctsHeader header)
		{
			using (var mainForm = new ZForm(header))
			{
				var providerFactory = new TRCustomsMessagingProviderFactory(NctsCustomsMessagingProvider.New, string.Empty, TRMessageSignerForTest.New());
				var success = TRCustomsMessagingGui.SendMessages(header, providerFactory, mainForm);

				return success;
			}
		}

		protected override void SetUp()
		{
			var currentUser = TRGlbStaffWrapper.Get(MasterFiles.Business.GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			MasterFiles.Business.GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";

			base.SetUp();
		}
	}

	class SignatureBuilderForTest : ISignatureBuilder
	{
		byte[] ISignatureBuilder.Sign(byte[] messageText, ISignatureAlgorithm signatureAlgorithm)
		{
			return ((ISignatureBuilder)this).Sign(messageText, signatureAlgorithm, DateTime.UtcNow);
		}

		byte[] ISignatureBuilder.Sign(byte[] messageText, ISignatureAlgorithm signatureAlgorithm, DateTime utcNow)
		{
			var msg = Encoding.UTF8.GetString(messageText);
			var algName = signatureAlgorithm.GetType().Name;

			var data = $"Signed with: {algName}\nMessage: {msg}";

			return Encoding.UTF8.GetBytes(data);
		}
	}

	class TRMessageSignerForTest : TRMessageSigner
	{
		public static new TRMessageSigner New() => new TRMessageSignerForTest(new SignatureBuilderForTest());
		public static TRMessageSigner New(ISignatureBuilder signatureBuilder) => new TRMessageSignerForTest(signatureBuilder ?? new SignatureBuilderForTest());
		protected TRMessageSignerForTest(ISignatureBuilder signatureBuilder) : base(signatureBuilder) { }
	}
}
