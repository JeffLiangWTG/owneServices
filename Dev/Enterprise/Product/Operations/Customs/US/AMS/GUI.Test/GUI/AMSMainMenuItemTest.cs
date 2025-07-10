using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	class AMSMainMenuItemTest : TestCaseWithFactory
	{
		public void TestDeclareAMS_NoExceptionThrownWhenValidationSuspendedCountIsZero()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Bills.AddNew();
			Factory.Save();
			using (var form = new USAMSForm(header))
			{
				var sendMenuItem = form.Menu.MenuItems.FindByText("Amendment Manifest", true);
				var orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_Code = "1";
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.UnitedStates);
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_OH_OrgProxy = orgProxy.PK;
				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Factory.Saving += delegate
				{
					header.Factory.ResumeValidation();
				};
				AssertNoExceptionThrown("ResumeValidation called 1 too many times", () => sendMenuItem.PerformClick());
			}
		}

		public void TestDeclareAMS_FormSaving_ValidationSuspended()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Bills.AddNew();
			Factory.Save();
			using (var form = new USAMSForm(header))
			{
				var sendMenuItem = form.Menu.MenuItems.FindByText("Send Manifest", true);
				var orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_Code = "1";
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.UnitedStates);
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_OH_OrgProxy = orgProxy.PK;
				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Factory.Saving += delegate
				{
					Assert("Validation should be suspended on header during form saving", header.Factory.IsValidationSuspended);
				};
				sendMenuItem.PerformClick();
			}
		}

		public void TestMenuItems()
		{
			var header = Factory.New<CusInBondHeader>();
			var amsMenuItem = new AMSMainMenuItem(header);
			AssertNotNull(amsMenuItem.MenuItems.FindByText("Send Manifest"));
			AssertNotNull(amsMenuItem.MenuItems.FindByText("Amendment Manifest"));
			var vesselMenuItem = amsMenuItem.MenuItems.FindByText("Vessel");
			AssertNotNull(vesselMenuItem);
			AssertNotNull(vesselMenuItem.MenuItems.FindByText("Send Arrival Messages"));
			AssertNotNull(vesselMenuItem.MenuItems.FindByText("Send Departure Messages"));
			AssertNotNull(vesselMenuItem.MenuItems.FindByText("Change in the Estimated Date of Arrival"));
			var pptMenuItem = amsMenuItem.MenuItems.FindByText("Permit To Transfer");
			AssertNotNull(pptMenuItem);
			AssertNotNull(pptMenuItem.MenuItems.FindByText("Send Permit To Transfer Messages"));
			AssertNotNull(pptMenuItem.MenuItems.FindByText("Cancel Permits To Transfer By Bill Of Lading"));
			var inbondMenuItem = amsMenuItem.MenuItems.FindByText("In-Bond");
			AssertNotNull(inbondMenuItem);
			AssertNotNull(inbondMenuItem.MenuItems.FindByText("Send Subsequent Original"));
			AssertNotNull(inbondMenuItem.MenuItems.FindByText("Send Subsequent Amendment"));
			AssertNotNull(inbondMenuItem.MenuItems.FindByText("Send Diversion"));
			AssertNotNull(inbondMenuItem.MenuItems.FindByText("Send Arrival"));
			AssertNotNull(inbondMenuItem.MenuItems.FindByText("Send Exportation"));
			AssertNotNull(inbondMenuItem.MenuItems.FindByText("Send Transfer Of Liability"));
		}

		public void TestAMSMenuItemOnStandAlone()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			using (var form = new USAMSForm(header))
			{
				var sendMenuItem = form.Menu.MenuItems.FindByText("Send Manifest", true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(AMSMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertEquals(AMSMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenuItem.PerformClick();
				AssertEquals(MessageSender.Constants.Message.NoOrgProxySCACNotification(header), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, header.IsInDatabase);
				var orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_Code = "1";
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.UnitedStates);
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_OH_OrgProxy = orgProxy.PK;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendWithMessageErrorSecurityCheckPoint()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.Creating)))
			{
				var messageAction = form.BusinessEntity;
				form.Show();
				var bottomPanel = (ZPanel)form.Controls["BottomPanel"];
				var sendButton = (ZButton)bottomPanel.Controls["SendButton"];
				AssertNotNull(sendButton);
				Env.Security.USAMSSendWithMessageErrors.IsAllowed = false;
				bill.B0_MasterBillNumber = "MD321";
				bill.B0_HouseBillNumber = "HB321";
				var orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_Code = "1";
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.UnitedStates);
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_OH_OrgProxy = orgProxy.PK;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Factory.Save();
				sendButton.PerformClick();
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(0, moveHeader.Messages.Count);
				AssertContains("There are message errors on this job and you don't have security rights to send with message errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, moveHeader.Messages.Count);
				Env.Security.USAMSSendWithMessageErrors.IsAllowed = true;
				bill.B0_MasterBillNumber = "MD322";
				bill.B0_HouseBillNumber = "HB323";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();
				Factory.Save();
				AssertContains("Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Integration.Customs.US.USAMS.IUSAMSMessageSender sender = new MessageSender();
				var result = sender.SendManifest(header.PK);
				AssertEquals(1, moveHeader.Messages.Count);
				var message = moveHeader.Messages[0];
				AssertEquals("Yes", message.EM_SendWithMessageErrorsFormatted);
			}
		}

		public void TestAMSMenuItemUpdateMessagesAfterSendingMessages()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.Creating)))
			{
				var messageAction = form.BusinessEntity;
				form.Show();
				var bottomPanel = (ZPanel)form.Controls["BottomPanel"];
				var sendButton = (ZButton)bottomPanel.Controls["SendButton"];
				AssertNotNull(sendButton);
				Env.Security.USAMSSendWithMessageErrors.IsAllowed = true;
				bill.B0_MasterBillNumber = "MD322";
				bill.B0_HouseBillNumber = "HB323";
				var orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_Code = "1";
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.UnitedStates);
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_OH_OrgProxy = orgProxy.PK;
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Integration.Customs.US.USAMS.IUSAMSMessageSender sender = new MessageSender();
				bill.Messages.Reload(true);
				AssertEquals(0, bill.Messages.Count);
				var result = sender.SendManifest(header.PK);
				bill.Messages.Reload(true);
				AssertEquals(1, bill.Messages.Count);
			}
		}

		public void TestSendManifest_AMS_ShouldShowLoadingDataProgressForm()
		{
			AssertProgressFormShown("Send Manifest", "Loading data", DialogResult.Cancel);
		}

		public void TestSendManifest_AMS_ShouldShowSendingManifestOriginalProgressForm()
		{
			AssertProgressFormShown("Send Manifest", "Sending Manifest Original", DialogResult.OK);
		}

		public void TestAmmendManifest_AMS_ShouldShowLoadingDataProgressForm()
		{
			AssertProgressFormShown("Amendment Manifest", "Loading data", DialogResult.Cancel);
		}

		public void TestAmmendManifest_AMS_ShouldShowSendingManifestAmmendmentsProgressForm()
		{
			AssertProgressFormShown("Amendment Manifest", "Sending Manifest Amendments", DialogResult.OK);
		}

		void AssertProgressFormShown(string menuItem, string caption, DialogResult result)
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.Bills.AddNew();
			Factory.Save();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var sendManifestMenuItem = form.Menu.MenuItems.FindByText(menuItem, true);
				AssertNotNull(sendManifestMenuItem);
				var orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_Code = "1";
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.UnitedStates);
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_OH_OrgProxy = orgProxy.PK;
				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = result;
				sendManifestMenuItem.PerformClick();
				var progressForm = ZFormModaliser.LastFormShownForTest as ProgressForm;
				AssertNotNull(progressForm);
				AssertProgressFormProperties(progressForm, caption);
			}
		}

		void AssertProgressFormProperties(ProgressForm progressForm, string caption)
		{
			CombineAssertions(() =>
			{
				Assert("cancel button should not be shown", !progressForm.ShowCancelButton);
				Assert("progress bar should not be shown", !progressForm.ShowProgressBar);
				AssertEquals(caption, progressForm.CaptionResourceString.Caption);
			});
		}
	}
}
