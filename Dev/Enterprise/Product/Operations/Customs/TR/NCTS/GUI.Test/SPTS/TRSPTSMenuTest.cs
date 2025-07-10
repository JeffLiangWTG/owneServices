using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class TRSPTSMenuTest : TestCaseWithFactory
	{
		public void TestSptsMenuItemsVisibiliy()
		{
			var header = Factory.New<SPTSHeader>();
			using (var menu = new TRSPTSMenu(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.MenuItems.FindByText("Send for SPTS Declaration").Visible);
			}
		}

		public void TestSendButton_Click()
		{
			var sptsHeader = Factory.New<SPTSHeader>();
			var departureMovement = sptsHeader.MovementHeader;
			departureMovement.BM_InlandTransportMode = "SEA";
			departureMovement.BM_DestinationPortCode = "06666";

			using (var form = new ZForm(sptsHeader))
			using (var menu = new TRSPTSMenu(sptsHeader))
			{
				form.Show();
				form.Menu.MenuItems.Add(menu);
				var sendButton = menu.MenuItems.FindByText("Send for SPTS Declaration");
				AssertNotNull(sendButton);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation message
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; // Message signing form

				CombineAssertions(() =>
				{
					sendButton.PerformClick();

					AssertEquals("Save message", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
					AssertContains("Review first line", "Please review the following notifications:", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertContains("Customs rejection message", "It is likely that your message(s) will be rejected by Customs", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertContains("Message error details", "Voyage No: You have not entered a Voyage No.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertEquals("Signing canceled", "Message signing canceled", UnitTestUserNotification.Instance.PreviousMessages[0].Text);

					AssertEquals("Signing Form", typeof(ESignatureForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				});

				using (sptsHeader.GetValidationSuspender())
				{
					sptsHeader.RegistrationNumber = "XXXXXX";
					sptsHeader.RegistrationDate = ZDateTime.Now;
					sptsHeader.BH_MessageStatus = "XXX";
					Factory.Save();
					CombineAssertions(() =>
					{
						sendButton.PerformClick();
						AssertEquals("Registered SPSTS errors", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("You are sending a SPTS Declaration which has a Registration No"));
					});
				}
			}
		}

		public void TestSPTSAmendSPTS()
		{
			var sptsHeader = Factory.New<SPTSHeader>();
			var departureMovement = sptsHeader.MovementHeader;
			departureMovement.BM_InlandTransportMode = "SEA";
			departureMovement.BM_DestinationPortCode = "06666";
			Factory.Save();

			using (var menu = new TRSPTSMenu(sptsHeader))
			using (var form = new ZForm(sptsHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.ShowPopupMenu();

				var amendMenu = menu.MenuItems.FindByText("Amend SPTS");
				AssertNull("Menu step available", amendMenu);

				using (sptsHeader.GetValidationSuspender())
				{
					sptsHeader.RegistrationNumber = "XXXXXX";
					sptsHeader.RegistrationDate = ZDateTime.Now;
					sptsHeader.BH_MessageStatus = "XXX";
					Factory.Save();

					menu.ShowPopupMenu();
					AssertEquals(true, menu.MenuItems.FindByText("Amend SPTS Declaration").Visible);

					amendMenu = menu.MenuItems.FindByText("Amend SPTS Declaration");
					amendMenu.PerformClick();
					AssertEquals("Do you want to amend the registered SPTS Declaration?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // YES, proceed with errors

					amendMenu.PerformClick();
					AssertEquals("The SPTS Declaration is amended and ready for sending to Customs.\r\nPlease Send the SPTS to Customs with Send for SPTS Declaration option", UnitTestUserNotification.Instance.LastMessage.Text);
					CombineAssertions("Amend Field", () =>
					{
						AssertEquals("RegistrationNumber", sptsHeader.RegistrationNumber, ZString.Empty);
						AssertEquals("RegistrationDate", sptsHeader.RegistrationDate, ZDateTime.Empty);
						AssertEquals("BH_MessageStatus", sptsHeader.BH_MessageStatus, ZString.Empty);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbStaff.CurrentUser.GS_EmailAddress = "bob@where.com";
			var password = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			password.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			password.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			password.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
		}
	}
}
