using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestTRMainfestSendManifest()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			using (manifest.GetValidationSuspender())
			{
				manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				manifest.Bills.AddNew();
				manifest.AMA_RL_NKPortOfLoading = "EGALY";
				manifest.AMA_RL_NKPortOfDischarge = "TRIST";
				manifest.AMA_CustomsOffice = "TR000400";
				Factory.Save();
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TRFOZBY, Core.Constants.CountryCodes.Turkey, ZDateTime.Now.AddDays(-10), true))
				using (var menu = new AsycudaMenuForTest(manifest))
				using (var form = new ZForm(manifest))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var sendMenu = menu.MenuItems.FindByText("Send &Manifest");
					sendMenu.PerformClick();
					AssertEquals("Dialog Name", "ESignatureForm", ZFormModaliser.LastFormShownDialogForTest.Name);
				}
			}
		}

		public void TestDocumentsMenuForEMANIF()
		{
			manifest.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
			Factory.Save();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var documentsMenu = form.Menu.MenuItems.FindByText("&Documents");
				AssertNotNull("DocMenu Not Found", documentsMenu);
				AssertEquals("Documents menu should be shown", true, documentsMenu.Visible);
				documentsMenu.PerformClick();
				var menu = documentsMenu.MenuItems.FindByText("TR Manifest Bills for EMANIF");
				AssertNotNull("Menu should exist", menu);
				AssertEquals("Menu should be visible", true, menu.Visible);
			}
		}

		public void TestDocumentsMenuForVARONC()
		{
			manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
			Factory.Save();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var documentsMenu = form.Menu.MenuItems.FindByText("&Documents");
				AssertNotNull("DocMenu Not Found", documentsMenu);
				AssertEquals("Documents menu should be shown", true, documentsMenu.Visible);
				documentsMenu.PerformClick();
				var menu1 = documentsMenu.MenuItems.FindByText("TR Manifest Bills");
				AssertNotNull("Menu should exist", menu1);
				AssertEquals("Menu should be visible", true, menu1.Visible);
				var menu2 = documentsMenu.MenuItems.FindByText("TR Manifest Bills Lines");
				AssertNotNull("Menu should exist", menu2);
				AssertEquals("Menu should be visible", true, menu2.Visible);
			}
		}

		public void TestWarnUserWhenMessageSendManifest()
		{
			using (manifest.GetValidationSuspender())
			{
				manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				manifest.Bills.AddNew();
				manifest.AMA_RL_NKPortOfLoading = "EGALY";
				manifest.AMA_RL_NKPortOfDischarge = "TRIST";
				manifest.AMA_CustomsOffice = "TR000400";
				Factory.Save();
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TRFOZBY, Core.Constants.CountryCodes.Turkey, ZDateTime.Now.AddDays(-10), true))
				using (var menu = new AsycudaMenuForTest(manifest))
				using (var form = new ZForm(manifest))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var sendMenu = menu.MenuItems.FindByText("Send &Manifest");
					sendMenu.PerformClick();
					AssertNotContains("The status is wait for response message, if you send again, the message will be rejected.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Your staff profile requires an email address as this is needed for messaging", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Your customs credentials are marked as invalid, please update your customs credentials.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("A valid certificate serial number is required on your staff record on the Brokerage tab", UnitTestUserNotification.Instance.LastMessage.Text);
					GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
					var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
					currentUser.GP_UserID = "1234";
					currentUser.CurrentDecryptedPassword = "xxx";
					currentUser.GP_CertificateAuthority = "TÜBİTAK";
					currentUser.TR_Chipset = "EKART";
					currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
					sendMenu.PerformClick();
					AssertNotContains("Your staff profile requires an email address as this is needed for messaging", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("Your customs credentials are marked as invalid, please update your customs credentials.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("A valid certificate serial number is required on your staff record on the Brokerage tab", UnitTestUserNotification.Instance.LastMessage.Text);
					manifest.AMA_MessageStatus = "AWA";
					Factory.Save();
					sendMenu.PerformClick();
					AssertContains("The status is wait for response message, if you send again, the message will be rejected.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				}
			}
		}

		public void TestTRManifestAmendManifest()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			using (manifest.GetValidationSuspender())
			{
				manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				manifest.Bills.AddNew();
				manifest.AMA_RL_NKPortOfLoading = "EGALY";
				manifest.AMA_RL_NKPortOfDischarge = "TRIST";
				manifest.AMA_CustomsOffice = "TR000400";
				Factory.Save();
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TRFOZBY, Core.Constants.CountryCodes.Turkey, ZDateTime.Now.AddDays(-10), true))
				using (var menu = new AsycudaMenuForTest(manifest))
				using (var form = new ZForm(manifest))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var amendMenu = menu.MenuItems.FindByText("Amend Manifest");
					AssertNull("Menu step available", amendMenu);
					manifest.RegistrationNumber = "XXXXXX";
					manifest.RegistrationDate = ZDateTime.Now;
					manifest.AMA_MessageStatus = "XXX";
					Factory.Save();
					menu.OnPopup(EventArgs.Empty);
					amendMenu = menu.MenuItems.FindByText("Amend Manifest");
					AssertNotNull("Menu step available", amendMenu);
					amendMenu.PerformClick();
					AssertEquals("Do you want to amend the registered manifest?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // YES, proceed with errors
					amendMenu.PerformClick();
					AssertEquals("The Manifest is amended and ready for sending to the Customs.\r\nPlease Send the Manifest to Customs with Send Manifest option", UnitTestUserNotification.Instance.LastMessage.Text);
					CombineAssertions("Amend Field", () =>
					{
						AssertEquals("RegistrationNumber", manifest.RegistrationNumber, ZString.Empty);
						AssertEquals("RegistrationDate", manifest.RegistrationDate, ZDateTime.Empty);
						AssertEquals("AMA_MessageStatus", manifest.AMA_MessageStatus, ZString.Empty);
						AssertEquals("AMA_CustomsStatus", manifest.AMA_CustomsStatus, "AMD");
					});
				}
			}
		}

		public void TestOnlyMessagingProcessNotificationsDisplayed()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(manifest))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes); // Save
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No); // Validation message

				var menuItem = menu.MenuItems.FindByText("Send Manifest");
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Save message", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertContains("Review first line", "Please review the following notifications:", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertContains("Customs rejection message", "It is likely that your message(s) will be rejected by Customs", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertContains("Message error details", "Manifest Type: At least one bill (shipment) is required.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				});
			}
		}

		public void TestDocumentsMenuForTRBillsStickerPrint()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var documentsMenu = form.Menu.MenuItems.FindByText("&Documents");
				AssertNotNull("DocMenu Not Found", documentsMenu);
				AssertEquals("Documents menu should be shown", true, documentsMenu.Visible);
				documentsMenu.PerformClick();
				var menu1 = documentsMenu.MenuItems.FindByText("TR Bills Sticker Print");
				AssertNotNull("Menu should exist", menu1);
				AssertEquals("Menu should be visible", true, menu1.Visible);
			}
		}

		public void TestDocumentsMenuForTRBillsSmallStickerPrint()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var documentsMenu = form.Menu.MenuItems.FindByText("&Documents");
				CombineAssertions(() =>
				{
					AssertNotNull("DocMenu Not Found", documentsMenu);
					AssertEquals("Documents menu should be shown", true, documentsMenu.Visible);
					documentsMenu.PerformClick();
					var menu1 = documentsMenu.MenuItems.FindByText("TR Bills Small Sticker Print");
					AssertNotNull("Menu item should exist", menu1);
					AssertEquals("Menu item should be visible", true, menu1.Visible);
				});
			}
		}

		public void TestRegisteredTRMainfestNotSentByAnotherUser()
		{
			var user1 = Factory.New<GlbStaff>();
			user1.GS_Code = "LPK";
			user1.GS_LoginName = "LPK";
			user1.GS_FullName = "Ilker Pakten";
			user1.GS_EmailAddress = "ilker.pakten@wisetechglobal.com";

			var user2 = Factory.New<GlbStaff>();
			user2.GS_Code = "YK";
			user2.GS_LoginName = "YK";
			user2.GS_FullName = "Yusuf Kamhi";
			user2.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";

			GlbStaff.CurrentUser.GS_Code = "LPK";
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";

			manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
			manifest.AMA_GS_NKCustomsAgent = "YK";
			manifest.Bills.AddNew();
			manifest.AMA_RL_NKPortOfLoading = "EGALY";
			manifest.AMA_RL_NKPortOfDischarge = "TRIST";
			manifest.AMA_CustomsOffice = "TR000400";
			Factory.Save();

			CombineAssertions("Send Message Check", () =>
			{
				AssertSendMessage("Send | Error", ZBool.True, "Send &Manifest", ZString.Empty, ZDateTime.Empty, ZString.Empty);
				AssertSendMessage("Amend | Error", ZBool.True, "Amend Manifest", "XXXXXX", ZDateTime.Now, "XXX");

				GlbStaff.CurrentUser.GS_Code = "YK";
				AssertSendMessage("Send | Not Error", ZBool.False, "Send &Manifest", ZString.Empty, ZDateTime.Empty, ZString.Empty);
				AssertSendMessage("Amend | Not Error", ZBool.False, "Amend Manifest", "XXXXXX", ZDateTime.Now, "XXX");
			});

			void AssertSendMessage(ZString testCaseMessage, ZBool isTestErrorType, ZString menuItem, ZString registrationNumber, ZDateTime registrationDate, ZString messageStatus)
			{
				manifest.RegistrationNumber = registrationNumber;
				manifest.RegistrationDate = registrationDate;
				manifest.AMA_MessageStatus = messageStatus;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TRFOZBY, Core.Constants.CountryCodes.Turkey, ZDateTime.Now.AddDays(-10), true))
				using (var menu = new AsycudaMenuForTest(manifest))
				using (var form = new ZForm(manifest))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var sendMenu = menu.MenuItems.FindByText(menuItem);
					sendMenu.PerformClick();
					if (isTestErrorType)
					{
						AssertContains(testCaseMessage, "The Global Manifest is declared by", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNotContains(testCaseMessage, "The Global Manifest is declared by", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR000400", "Customs Office", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(10));
			Factory.Save();
			manifest = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
		}

		ASYCUDA.Business.AsycudaManifestHeader manifest;
	}
}
