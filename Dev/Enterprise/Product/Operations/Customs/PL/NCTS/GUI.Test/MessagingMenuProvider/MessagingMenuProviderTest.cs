using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestGetProvider()
	{
		AssertType<MessagingMenuProvider>(Phase5MessagingMenuProvider.GetProvider(Factory.New<NctsHeader>()));
	}

	public void TestSendToCustomsCore()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		Factory.Save();

		using var form = new ZForm();
		var provider = new MessagingMenuProvider(nctsHeader);
		var formMenuItems = form.Menu.MenuItems;
		provider.RefreshMenu();
		formMenuItems.AddRange(provider.CreateMenuItems().ToArray());
		form.Show();

		var sendToCustomsMenuItem = formMenuItems.FindByText("Send to Customs");

		CombineAssertions(() =>
		{
			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddOKAnswer();

			sendToCustomsMenuItem.PerformClick();

			Assert("Error Notification - Declaration is not ready to be sent", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertNull("Send form is not shown - pre-sending validation did not pass", ZFormModaliser.LastFormShownDialogForTest);

			using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
			{
				var certificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
				certificate.GP_MailBoxID = "ABCXYZ";
				certificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				certificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				certificate.GP_ExpiryDate = ZDateTime.Now.AddDays(7);

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "Header";
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", "PL");
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddOKAnswer();

				sendToCustomsMenuItem.PerformClick();

				Assert("Warning Notification - Declaration is ready to be sent", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertType<MessageSendingForm>("Sending form is shown - pre-sending validation passed", ZFormModaliser.LastFormShownDialogForTest);
			}
		});
	}
}
