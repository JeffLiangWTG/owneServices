using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	sealed class MenuBuilderTests : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ZForm(header))
			{
				AssertEquals("TW Manifest", new MenuBuilder(header, form).MenuCaption.EnglishText);
			}
		}

		public void TestBuildMenu()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = TWManifestTypes.Codes.MAN;
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			Assert(!header.AMA_RN_NKCountryInfo.HasMessageErrors());

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(2, menu.MenuItems.Count);
					AssertEquals("Send to Customs", menu.MenuItems[0].Text);
					AssertEquals("Send to Customs (By Bag)", menu.MenuItems[1].Text);
				});
			}
		}

		public void TestSendToCustomsMenuItem()
		{
			var confirmSaveMsg = "You need to save first. Would you like to save now and proceed?";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MAN";
			header.AMA_RL_NKPortOfLoading = "TWCMJ";
			header.AMA_RL_NKPortOfDischarge = "TWCMJ";
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				var menuItem = menu.MenuItems.FindByText("Send to Customs", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(confirmSaveMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				AssertEquals(confirmSaveMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuCaption = ZString.Empty;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((manifestForm) =>
				{
					menuCaption = ((MessageSendingObjectParent)((MessageSendingForm)manifestForm).DataSource).MenuCaption;
				});

				menuItem.PerformClick();
				AssertEquals("Send to Customs", menuCaption);
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(1, header.Messages.Count);
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
				AssertType<MessageSendingForm>(lastFormShown);
				lastFormShown.Dispose();
			}
		}

		public void TestSendToCustomsByBagMenuItem()
		{
			var confirmSaveMsg = "You need to save first. Would you like to save now and proceed?";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MAN";
			header.AMA_RL_NKPortOfLoading = "TWCMJ";
			header.AMA_RL_NKPortOfDischarge = "TWCMJ";
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			bill1.BagNumber = "B1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "456";
			bill2.BagNumber = "B2";

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				var menuItem = menu.MenuItems.FindByText("Send to Customs (By Bag)", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(confirmSaveMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				AssertEquals(confirmSaveMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuCaption = ZString.Empty;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((manifestForm) =>
				{
					menuCaption = ((MessageSendingObjectParent)((ByBagMessageSendingForm)manifestForm).DataSource).MenuCaption;
				});

				menuItem.PerformClick();
				AssertEquals("Send to Customs (By Bag)", menuCaption);
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(2, header.Messages.Count);
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
				AssertType<ByBagMessageSendingForm>(lastFormShown);
				lastFormShown.Dispose();
			}
		}

		public void TestCheckVATNumberBeforeSendingMessage()
		{
			var messageText = "A valid entry number is missing. The message cannot be sent. To generate entry number, a valid TW-VAT is required for Organization Proxy. To create a valid TW-VAT for Organization Proxy, visit Maintain > User Admin > Companies > Company Info. > Organization Proxy > Details > Config > Registration Numbers / Codes.";
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrg1.PK;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = TWManifestTypes.Codes.MAN;
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				var menuItem = menu.MenuItems.FindByText("Send to Customs", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Should have an error.", messageText, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("The message type is Error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrg2.PK;
				menuItem.PerformClick();
				AssertNullOrEmpty("No error if the proxy has a valid VAT Number.", UnitTestUserNotification.Instance.LastMessage.Text);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrg.PK;
		}
	}
}
