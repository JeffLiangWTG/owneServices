using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZFormModaliser;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestSendCustomsDeclarationMenuItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			Factory.Save();
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				declaration.JE_MessageType = "A";
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendCustomsDeclaration, menu.SendCustomsDeclarationMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendImportCustomsDeclaration, menu.SendCustomsDeclarationMenuItem.Caption);

				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendImportCustomsDeclarationForAircraftParts, menu.SendImportCustomsDeclarationForAircraftPartsMenuItem.Caption);
				Assert("SendImportCustomsDeclarationForAircraftPartsMenuItem should be visible", menu.SendImportCustomsDeclarationForAircraftPartsMenuItem.Visible);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendExportCustomsDeclaration, menu.SendCustomsDeclarationMenuItem.Caption);
				Factory.Save();
				var menuItem = menu.SendCustomsDeclarationMenuItem;
				menuItem.PerformClick();
				AssertContains("You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage.Text);
				var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
				var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
				cusNum1.CE_ParentID = cusHead1.PK;
				cusNum1.CE_Category = "CUS";
				cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
				cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
				cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
				cusNum1.CE_EntryNum = "NO1";
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CusEntryInstruction;
				entryInstruction.CEI_CustomsOffice = "BB";
				entryInstruction.CEI_Style = "G1";
				entryInstruction.CEI_BoxNumber = "123";
				cusHead1.CH_CEI_Instruction = entryInstruction.PK;
				var extPassword1 = Factory.New<Business.GlbExternalPassword>();
				extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
				extPassword1.GP_GC = company.PK;
				extPassword1.GP_MailBoxID = "123-3";
				extPassword1.GP_UserID = "001";
				extPassword1.GP_GS = staff.PK;
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertEquals(typeof(AdditionalDocumentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotShouldWaitUntilResponded(declaration, cusHead1, menuItem, (form) =>
				{
					var testForm = form as AdditionalDocumentForm;
					var wrapper = testForm.BusinessEntity;
					var messageSendingObject = wrapper.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
					messageSendingObject.ShouldSend = true;
				});
			}

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				Assert("should hide Send CustomsDeclarationMenu when submit type is ITF", !menu.SendCustomsDeclarationMenuItem.Visible);
				Assert("should hide Send ControllingImportCustomsDeclaration when submit type is ITF", !menu.SendControllingImportCustomsDeclarationMenuItem.Visible);
				Assert("should hide Send GoodsExaminationApplication when submit type is ITF", !menu.SendGoodsExaminationApplicationMenuItem.Visible);
				Assert("should hide Send AdditionalDocumentMessage when submit type is ITF", !menu.SendAdditionalDocumentMessageMenuItem.Visible);
				Assert("should hide Send ImportCustomsDeclarationForAircraftParts when submit type is ITF", !menu.SendImportCustomsDeclarationForAircraftPartsMenuItem.Visible);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				Assert("should hide Send AdditionalDocumentMessage when submit type is ITF", !menu.SendAdditionalDocumentMessageMenuItem.Visible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.OnPopup(EventArgs.Empty);
				Assert("SendAdditionalDocumentMessageMenuItem should be visible when submit type is not ITF and message type is EXP", menu.SendAdditionalDocumentMessageMenuItem.Visible);
			}
		}

		void AssertNotShouldWaitUntilResponded(JobDeclaration declaration, Business.CusEntryHeader cusHeader, ZMenuItem menuItem, PreShowInvoker delegateToCall, bool shouldCheckEntryNumber = false)
		{
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_CustomsOffice = "BB";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			cusHeader.CH_CEI_Instruction = entryInstruction.PK;
			cusHeader.CH_Status = "AWC";
			cusHeader.EntryNumber = "55555";
			Factory.Save();
			if (shouldCheckEntryNumber)
			{
				cusHeader.EntryNumber = "";
				entryInstruction.CEI_Style = "";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegateToCall);
				menuItem.PerformClick();
				AssertContains("The entry does not have entry number.", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.EntryNumber = "AAA";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegateToCall);
				menuItem.PerformClick();
				AssertContains("The entry does not have entry number.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryInstruction.CEI_Style = "G1";
				cusHeader.EntryNumber = "BB  0912300002";
				Factory.Save();
				AssertEquals("BB  0912300002", cusHeader.EntryNumber);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegateToCall);
			menuItem.PerformClick();
			AssertContains("This job is waiting for a Customs response", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegateToCall);
			menuItem.PerformClick();
			AssertNotContains("You may not continue due to one or more critical problems", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("1 original message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2020, 07, 02)]
		public void TestSendMessageForQuarantineApplication()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			Factory.Save();
			var missingNX401ControllingMessageHeader = "You have not created a NX401 message. Please visit the ‘Licensing’ tab to create a NX401 message by selecting ‘NX401’ in the ‘Message Type’ column before proceeding.";
			var missingEntryNumber = "Please generate entries first.";
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				declaration.JE_MessageType = "A";
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendMessageForQuarantineApplication, menu.SendMessageForQuarantineApplicationMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendMessageForQuarantineApplication, menu.SendMessageForQuarantineApplicationMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendMessageForQuarantineApplication, menu.SendMessageForQuarantineApplicationMenuItem.Caption);
				var menuItem = menu.SendMessageForQuarantineApplicationMenuItem;
				Factory.Save();
				menuItem.PerformClick();
				AssertContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AddEntryHeader(declaration);
				declaration.EntryHeader.EntryNumber = "AAA";
				Factory.Save();
				menuItem.PerformClick();
				AssertNotContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(missingNX401ControllingMessageHeader, UnitTestUserNotification.Instance.LastMessage.Text);
				var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingMessageType = "NX401";
				messageHeader.TW1_ControllingAgency = "VP";
				messageHeader.PermitNumber = "001";
				messageHeader.TW1_RequestDescription = "AA";
				new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotContains(missingNX401ControllingMessageHeader, UnitTestUserNotification.Instance.LastMessage.Text);
				var validCredential = string.Format(CultureInfo.CurrentCulture, "Please create a valid Credential for {0} on {1} - Brokerage.", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				var continueSendCredential = string.Format(CultureInfo.CurrentCulture, "The credential (for {0} on {1}) you have selected is not valid. Sending this message will most likely result in an error. Do you still want to proceed with this action?", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				var extPassword = Factory.New<Business.GlbExternalPassword>();
				extPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
				extPassword.GP_GC = company.PK;
				extPassword.GP_MailBoxID = "123-3";
				extPassword.GP_UserID = "001";
				extPassword.GP_GS = staff.PK;
				extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.JE_ApplicationCode = "BLT";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				declaration.EntryHeader.EntryNumber = "";
				declaration.EntryNumber = "";
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuCaption = ZString.Empty;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					menuCaption = ((ControllingMessageSendingObjectParent)((ControllingMessageSendingForm)form).DataSource).MenuCaption;
				});
				menuItem.PerformClick();
				AssertEquals(menu.SendMessageForQuarantineApplicationMenuItem.Caption, menuCaption);

				AssertEquals("Last dialog form type = ControllingMessageSendingForm", true, ZFormModaliser.LastFormShownDialogForTest is ControllingMessageSendingForm);
				AssertEquals("BB  0912300001", declaration.EntryHeader.EntryNumber);
			}
		}

		void AddEntryHeader(JobDeclaration declaration)
		{
			var entry = declaration.ActiveEntryHeaders.AddNew();
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_BoxNumber = "123";
			entry.CH_CEI_Instruction = entryInstruction.PK;
		}

		public void TestSendFoodAndDrugImportApplication()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			Factory.Save();
			var missingNX601ControllingMessageHeader = "You have not created a NX601 message. Please visit the ‘Licensing’ tab to create a NX601 message by selecting ‘NX601’ in the ‘Message Type’ column before proceeding.";
			var missingEntryNumber = "Please generate entries first.";
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				MenuAssertion.AssertHasMenu(menu, EDIMenu.Constants.SendMessagesToNCATK, EDIMenu.Constants.SendFoodAndDrugImportApplication);
				declaration.JE_MessageType = "A";
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendFoodAndDrugImportApplication, menu.SendFoodAndDrugImportApplicationMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(false, menu.SendFoodAndDrugImportApplicationMenuItem.Visible);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(true, menu.SendFoodAndDrugImportApplicationMenuItem.Visible);
				AssertEquals(EDIMenu.Constants.SendFoodAndDrugImportApplication, menu.SendFoodAndDrugImportApplicationMenuItem.Caption);
				var menuItem = menu.SendFoodAndDrugImportApplicationMenuItem;
				Factory.Save();
				menuItem.PerformClick();
				AssertContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AddEntryHeader(declaration);
				declaration.EntryHeader.EntryNumber = "AAA";
				Factory.Save();
				menuItem.PerformClick();
				AssertNotContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(missingNX601ControllingMessageHeader, UnitTestUserNotification.Instance.LastMessage.Text);
				var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingAgency = "IF";
				messageHeader.TW1_ControllingMessageType = "NX601";
				messageHeader.PermitNumber = "001";
				messageHeader.TW1_RequestDescription = "AA";
				new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotContains(missingNX601ControllingMessageHeader, UnitTestUserNotification.Instance.LastMessage.Text);
				var validCredential = string.Format(CultureInfo.CurrentCulture, "Please create a valid Credential for {0} on {1} - Brokerage.", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				var continueSendCredential = string.Format(CultureInfo.CurrentCulture, "The credential (for {0} on {1}) you have selected is not valid. Sending this message will most likely result in an error. Do you still want to proceed with this action?", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				var extPassword = Factory.New<Business.GlbExternalPassword>();
				extPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
				extPassword.GP_GC = company.PK;
				extPassword.GP_MailBoxID = "123-3";
				extPassword.GP_UserID = "001";
				extPassword.GP_GS = staff.PK;
				extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.JE_ApplicationCode = "BLT";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuCaption = ZString.Empty;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					menuCaption = ((ControllingMessageSendingObjectParent)((ControllingMessageSendingForm)form).DataSource).MenuCaption;
				});
				menuItem.PerformClick();
				AssertEquals(menu.SendFoodAndDrugImportApplicationMenuItem.Caption, menuCaption);

				AssertEquals("Last dialog form type = ControllingMessageSendingForm", true, ZFormModaliser.LastFormShownDialogForTest is ControllingMessageSendingForm);
				AssertEquals("AAA", declaration.EntryHeader.EntryNumber);
				AssertEquals("", declaration.EntryNumber);
			}
		}

		[TestDate(2020, 07, 02)]
		public void TestSendApplicationMessageForCertificateOfOrigin()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			Factory.Save();
			var noX101 = "You have not created a X101 message. Please visit the ‘Licensing’ tab to create a X101 message by selecting ‘X101’ in the ‘Message Type’ column before proceeding.";
			var missingEntryNumber = "Please generate entries first.";
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				declaration.JE_MessageType = "A";
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendApplicationMessageForCertificateOfOrigin, menu.SendApplicationMessageForCertificateOfOriginMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendApplicationMessageForCertificateOfOrigin, menu.SendApplicationMessageForCertificateOfOriginMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendApplicationMessageForCertificateOfOrigin, menu.SendApplicationMessageForCertificateOfOriginMenuItem.Caption);
				var menuItem = menu.SendApplicationMessageForCertificateOfOriginMenuItem;
				Factory.Save();
				menuItem.PerformClick();
				AssertContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AddEntryHeader(declaration);
				Factory.Save();
				menuItem.PerformClick();
				AssertNotContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(noX101, UnitTestUserNotification.Instance.LastMessage.Text);
				var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingMessageType = "X101";
				messageHeader.PermitNumber = "001";
				messageHeader.TW1_RequestDescription = "AA";
				new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotContains(noX101, UnitTestUserNotification.Instance.LastMessage.Text);
				var validCredential = string.Format(CultureInfo.CurrentCulture, "Please create a valid Credential for {0} on {1} - Brokerage.", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				var continueSendCredential = string.Format(CultureInfo.CurrentCulture, "The credential (for {0} on {1}) you have selected is not valid. Sending this message will most likely result in an error. Do you still want to proceed with this action?", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				var extPassword1 = Factory.New<Business.GlbExternalPassword>();
				extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
				extPassword1.GP_GC = company.PK;
				extPassword1.GP_MailBoxID = "123-3";
				extPassword1.GP_UserID = "001";
				extPassword1.GP_GS = staff.PK;
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuCaption = ZString.Empty;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					menuCaption = ((ControllingMessageSendingObjectParent)((ControllingMessageSendingForm)form).DataSource).MenuCaption;
				});
				menuItem.PerformClick();
				AssertEquals(menu.SendApplicationMessageForCertificateOfOriginMenuItem.Caption, menuCaption);

				AssertEquals("Last dialog form type = ControllingMessageSendingForm", true, ZFormModaliser.LastFormShownDialogForTest is ControllingMessageSendingForm);
				AssertEquals("BB  0912300001", declaration.EntryHeader.EntryNumber);
			}
		}

		[TestDate(2020, 07, 02)]
		public void TestSendNX101NCATKMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			Factory.Save();
			var noNX101 = "You have not created a NX101 message. Please visit the ‘Licensing’ tab to create a NX101 message by selecting ‘NX101’ in the ‘Message Type’ column before proceeding.";
			var missingEntryNumber = "Please generate entries first.";
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				declaration.JE_MessageType = "A";
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendNX101NCATKMessage, menu.SendNX101NCATKMessageMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendNX101NCATKMessage, menu.SendNX101NCATKMessageMenuItem.Caption);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendNX101NCATKMessage, menu.SendNX101NCATKMessageMenuItem.Caption);
				var menuItem = menu.SendNX101NCATKMessageMenuItem;
				Factory.Save();
				menuItem.PerformClick();
				AssertContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AddEntryHeader(declaration);
				Factory.Save();
				menuItem.PerformClick();
				AssertNotContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(noNX101, UnitTestUserNotification.Instance.LastMessage.Text);
				var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingMessageType = "NX101";
				messageHeader.TW1_CertificateType = "01";
				new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotContains(noNX101, UnitTestUserNotification.Instance.LastMessage.Text);
				var validCredential = string.Format(CultureInfo.CurrentCulture, "Please create a valid Credential for {0} on {1} - Brokerage.", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				var continueSendCredential = string.Format(CultureInfo.CurrentCulture, "The credential (for {0} on {1}) you have selected is not valid. Sending this message will most likely result in an error. Do you still want to proceed with this action?", declaration.JE_CustomsProfile, declaration.JE_GS_NKCusAgent);
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				var extPassword1 = Factory.New<Business.GlbExternalPassword>();
				extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
				extPassword1.GP_GC = company.PK;
				extPassword1.GP_MailBoxID = "123-3";
				extPassword1.GP_UserID = "001";
				extPassword1.GP_GS = staff.PK;
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuCaption = ZString.Empty;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					menuCaption = ((ControllingMessageSendingObjectParent)((ControllingMessageSendingForm)form).DataSource).MenuCaption;
				});
				menuItem.PerformClick();
				AssertEquals(menu.SendNX101NCATKMessageMenuItem.Caption, menuCaption);

				AssertEquals("Last dialog form type = ControllingMessageSendingForm", true, ZFormModaliser.LastFormShownDialogForTest is ControllingMessageSendingForm);
				AssertEquals("BB  0912300001", declaration.EntryHeader.EntryNumber);
			}
		}

		public void TestGenerateEntriesMenu()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.MenuForTest.RefreshMenu();
				Assert(formForTest.MenuForTest.GenerateEntriesMenuItem.Visible);
			}
		}

		public void TestSendGoodsExaminationApplicationMenuItem()
		{
			var company = GlbCompany.CurrentCompany;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var extPassword1 = Factory.New<Business.GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				MenuAssertion.AssertHasMenu(menu, EDIMenu.Constants.SendGoodsExaminationApplication);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(new EventArgs());
				AssertEquals(false, menu.SendGoodsExaminationApplicationMenuItem.Visible);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(new EventArgs());
				AssertEquals(true, menu.SendGoodsExaminationApplicationMenuItem.Visible);
				var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
				cusHead1.CH_EntryStatus = "IEM";
				cusHead1.EntryNumber = "AAA";

				var menuCaption = ZString.Empty;
				AssertNotShouldWaitUntilResponded(declaration, cusHead1, menu.SendGoodsExaminationApplicationMenuItem, (form) =>
				{
					var testForm = form as MessageSendingForm;
					var wrapper = testForm.BusinessEntity;
					var messageSendingObject = wrapper.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
					messageSendingObject.ShouldSend = true;
					menuCaption = wrapper.MenuCaption;
				}, true);
				AssertEquals(menu.SendGoodsExaminationApplicationMenuItem.Caption, menuCaption);
			}
		}

		public void TestSendAdditionalDocumentMessageMenuItem()
		{
			var company = GlbCompany.CurrentCompany;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var extPassword1 = Factory.New<Business.GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var frm = new FormForTest(declaration))
			{
				frm.Show();
				var mockMenu = frm.MockMenu;
				var menu = frm.MenuForTest;
				menu.RefreshMenu();
				MenuAssertion.AssertHasMenu(menu, EDIMenu.Constants.SendAdditionalDocumentMessage);
				var cusHead1 = declaration.ActiveEntryHeaders.AddNew();

				var menuCaption = ZString.Empty;
				AssertNotShouldWaitUntilResponded(declaration, cusHead1, menu.SendAdditionalDocumentMessageMenuItem, (form) =>
				{
					var testForm = form as AdditionalDocumentForm;
					var wrapper = testForm.BusinessEntity;
					var messageSendingObject = wrapper.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
					messageSendingObject.ShouldSend = true;
					menuCaption = wrapper.MenuCaption;
				}, true);
				AssertEquals(menu.SendAdditionalDocumentMessageMenuItem.Caption, menuCaption);
			}
		}

		public void TestSendApplicationMessageForCertificateOfOriginMenuItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var frm = new FormForTest(declaration))
			{
				frm.Show();
				var mockMenu = frm.MockMenu;
				var menu = frm.MenuForTest;
				menu.RefreshMenu();
				MenuAssertion.AssertHasMenu(menu, EDIMenu.Constants.SendMessagesToNCATK);
				MenuAssertion.AssertHasMenu(menu.SendMessagesToNCATKMenuItem, EDIMenu.Constants.SendApplicationMessageForCertificateOfOrigin);
			}
		}

		public void TestNX101MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX101Messages, false, true);
		}

		public void TestNX201_01MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX201_01Messages, TWCustomsDataRegistry.Instance.EnableNX201_01, true, true);
		}

		public void TestNX201_07MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX201_07Messages, TWCustomsDataRegistry.Instance.EnableNX201_07, true, false);
		}

		public void TestNX301MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX301Messages, TWCustomsDataRegistry.Instance.EnableNX301, true, false);
		}

		public void TestNX301_AXMenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX301_AXMessages, TWCustomsDataRegistry.Instance.EnableNX301_AX, true, false);
		}

		public void TestNX301_DNMenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX301_DNMessages, TWCustomsDataRegistry.Instance.EnableNX301_DN, true, false);
		}

		public void TestNX401MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX401Messages, TWCustomsDataRegistry.Instance.EnableNX401, true, true);
		}

		public void TestNX601MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX601Messages, TWCustomsDataRegistry.Instance.EnableNX601, true, false);
		}

		public void TestNX603MenuItemVisibility()
		{
			TestNXMMenuItemVisibility(EDIMenu.Constants.SendNX603Messages, TWCustomsDataRegistry.Instance.EnableNX603, true, false);
		}

		void TestNXMMenuItemVisibility(string menuName, BooleanRegistryItem registryItem, bool displayWhenImport, bool displayWhenExport)
		{
			TWCustomsDataRegistry.Instance.EnableNX601.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var menu = formForTest.MenuForTest;
				var nxmMenuItem = menu.SendLicensingMessagesMenuItem.MenuItems.FindByText(menuName, false);

				using (registryItem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
					menu.RefreshMenu();
					menu.OnPopup(EventArgs.Empty);
					CombineAssertions(() =>
					{
						AssertEquals("SendLicensingMessages visiblity when JE_MessageType is Import", displayWhenImport, menu.SendLicensingMessagesMenuItem.Visible);
						AssertEquals($"{menuName} visiblity when {registryItem.Name} is 'yes' and JE_MessageType is Import", displayWhenImport, nxmMenuItem.Visible);

						declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
						menu.RefreshMenu();
						menu.OnPopup(EventArgs.Empty);
						AssertEquals($"{menuName} visiblity when {registryItem.Name} is 'yes' and JE_MessageType is Export", displayWhenExport, nxmMenuItem.Visible);
					});
				}
			}
		}

		void TestNXMMenuItemVisibility(string menuName, bool displayWhenImport, bool displayWhenExport)
		{
			TWCustomsDataRegistry.Instance.EnableNX201_01.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TWCustomsDataRegistry.Instance.EnableNX301.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TWCustomsDataRegistry.Instance.EnableNX301_AX.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TWCustomsDataRegistry.Instance.EnableNX301_DN.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TWCustomsDataRegistry.Instance.EnableNX401.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TWCustomsDataRegistry.Instance.EnableNX601.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TWCustomsDataRegistry.Instance.EnableNX603.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var formForTest = new FormForTest(declaration))
			using (TWCustomsDataRegistry.Instance.EnableNX201_07.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				formForTest.Show();
				var menu = formForTest.MenuForTest;
				var nxmMenuItem = menu.SendLicensingMessagesMenuItem.MenuItems.FindByText(menuName, false);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				menu.OnPopup(EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals("SendLicensingMessages visiblity when JE_MessageType is Import", displayWhenImport, menu.SendLicensingMessagesMenuItem.Visible);

					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					menu.RefreshMenu();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("SendLicensingMessages visiblity when JE_MessageType is Export", displayWhenExport, menu.SendLicensingMessagesMenuItem.Visible);
				});
			}
		}

		public void TestSendNX101Message()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			Factory.Save();
			var missingNX101ControllingMessageHeader = "You have not created a NX101 message. Please visit the ‘Licensing’ tab to create a NX101 message by selecting ‘NX101’ in the ‘Message Type’ column before proceeding.";
			var missingEntryNumber = "Please generate entries first.";
			using (var formForTest = new FormForTest(declaration))
			{
				formForTest.Show();
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				MenuAssertion.AssertHasMenu(menu, EDIMenu.Constants.SendLicensingMessages, EDIMenu.Constants.SendNX101Messages);
				var menuItem = menu.SendNX101MessageMenuItem;
				Factory.Save();
				menuItem.PerformClick();
				AssertContains(missingNX101ControllingMessageHeader, UnitTestUserNotification.Instance.LastMessage.Text);

				var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingAgency = "FT";
				messageHeader.TW1_ControllingMessageType = "NX101";
				new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotContains(missingNX101ControllingMessageHeader, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);

				AddEntryHeader(declaration);
				declaration.EntryHeader.EntryNumber = "AAA";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotContains(missingEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendLicensingMessageWhenAutoGenerateEntriesIfRequired()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			using var formForTest = new FormForTest(declaration);
			formForTest.Show();
			var mockMenu = formForTest.MockMenu;
			var menu = formForTest.MenuForTest;
			menu.RefreshMenu();
			var menuItem = menu.SendNX401MessageMenuItem;
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menuItem.PerformClick();
			AssertContains("Generate Entry(Merge) successful, please save and send again.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendControllingImportCustomsDeclarationMenuItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var cusHeader = declaration.ActiveEntryHeaders.AddNew();
			cusHeader.CH_CEI_Instruction = entryInstruction.PK;
			using (var frm = new FormForTest(declaration))
			{
				frm.Show();
				var mockMenu = frm.MockMenu;
				var menu = frm.MenuForTest;
				menu.RefreshMenu();
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.OnPopup(EventArgs.Empty);
				MenuAssertion.AssertHasMenu(menu, EDIMenu.Constants.SendControllingImportCustomsDeclaration);
				AssertEquals(EDIMenu.Constants.SendControllingImportCustomsDeclaration, menu.SendControllingImportCustomsDeclarationMenuItem.Caption);
				Assert(menu.SendControllingImportCustomsDeclarationMenuItem.Visible);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(EDIMenu.Constants.SendExportCustomsDeclaration, menu.SendCustomsDeclarationMenuItem.Caption);
				Assert(!menu.SendControllingImportCustomsDeclarationMenuItem.Visible);
			}
		}

		public void TestCreatePackingListMenuItemShouldBeAvailable()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
				AssertEquals("&Brokerage", testMenu.Text);
				Assert(testMenu.MenuItems.FindByText("Create Packing List").Visible);
			}

			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
				AssertEquals("&Brokerage", testMenu.Text);
				Assert("Should be hidden.", !testMenu.MenuItems.FindByText("Create Packing List").Visible);
			}
		}

		class FormForTest : JobDeclarationForm
		{
			public FormForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore()
			{
				return MenuForTest;
			}

			public Mock<EDIMenu> MockMenu
			{
				get
				{
					if (mockMenu == null)
					{
						mockMenu = new Mock<EDIMenu>();
						mockMenu.CallBase = true;
					}

					return mockMenu;
				}
			}

			Mock<EDIMenu> mockMenu;
			public EDIMenu MenuForTest
			{
				get
				{
					return MockMenu.Object;
				}
			}
		}
	}
}
