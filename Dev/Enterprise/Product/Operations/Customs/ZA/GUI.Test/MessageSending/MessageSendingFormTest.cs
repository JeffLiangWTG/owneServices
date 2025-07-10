using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes;
using JobDeclarationMessageSendingObjectParent = Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent;
using LineMerger = Enterprise.Customs.ZA.Business.LineMerger;
using UniversalReferenceConstants = Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration));
		}

		public void TestDeclarationTypeColumn()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
					Assert(grid.Columns.Contains(MessageSendingObject.Schema.DeclarationType));
				}
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
					Assert(grid.Columns.Contains(MessageSendingObject.Schema.DeclarationType));
				}
			}
		}

		public void TestSendToCustomsMenuItem()
		{
			var testMenu = new EDIMenuForTest();
			AssertEquals(false, testMenu.SendToCustomsMenuItem.Visible);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
			testMenu.SendToCustomsMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			CombineAssertions(() =>
			{
				ZDecimal rate1 = new ZDecimal(1.36);
				ZDecimal rate2 = new ZDecimal(1.2);
				RefCurrency currency = Factory.New<RefCurrency>();
				currency.RX_Code = "ZZZ";
				RefExchangeRate rate = Factory.New<RefExchangeRate>();
				rate.RE_ExRateType = "CUS";
				rate.RE_StartDate = ZDateTime.Today.AddDays(-1);
				rate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
				rate.RE_SellRate = rate1;
				rate.RE_RX_NKExCurrency = currency.Code;
				declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = _11;
				var testInvHeader = declaration.Invoices.AddNew();
				testInvHeader.JZ_RX_NKInvoice_Currency = currency.Code;
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + _00;
				Factory.Save();
				var merger = new LineMerger(declaration);
				merger.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				testMenu.SendToCustomsMenuItem.PerformClick();
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					(form.Controls.Find("SendButton", true)[0] as ZButton).PerformClick();
					AssertNotContains("Contains No Exchange Error 01", "Exchange Rate: This exchange rate was set when the currency was set.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				rate.RE_SellRate = rate2;
				Factory.Save();
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					(form.Controls.Find("SendButton", true)[0] as ZButton).PerformClick();
					AssertNotContains("Contains Exchange Error", "Exchange Rate: This exchange rate was set when the currency was set.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				rate.RE_SellRate = rate1;
				Factory.Save();
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					(form.Controls.Find("SendButton", true)[0] as ZButton).PerformClick();
					AssertNotContains("Contains No Exchange Error 02", "Exchange Rate: This exchange rate was set when the currency was set.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestStopSendingWithMessageErrorIfUserDoesnotHaveSecurityRight()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = _11;
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInst.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + _00;
			Factory.Save();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			Factory.Save();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Packages = 0;
			entry.CH_BGMReference = "Test Reference Number";
			CombineAssertions(() =>
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					(form.Controls.Find("SendButton", true)[0] as ZButton).PerformClick();
					AssertEquals("1 Last was Error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
					var message = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertContains("1 Assert SecurityCheckPoint Notification", "There are message errors on this job and you don't have security rights to send with message errors.", message);
					AssertContains("1 Assert SecurityCheckPoint Notification 2", "Please fix these errors before sending any messages:", message);
					AssertContains("1 Assert Package MessageError", "Packages: value cannot be zero.", message);
					AssertNotContains("1 Assert Question Notification", "It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", message);
					AssertNotContains("1 Assert Question Notification", "Do you want to send the message(s) despite these errors?", message);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					(form.Controls.Find("SendButton", true)[0] as ZButton).PerformClick();
					AssertEquals("2 Last should not be error", false, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("2 Last should be Question", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					var message = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertNotContains("2 Assert SecurityCheckPoint Notification", "There are message errors on this job and you don't have security rights to send with message errors.", message);
					AssertNotContains("2 Assert SecurityCheckPoint Notification 2", "Please fix these errors before sending any messages:", message);
					AssertContains("2 Assert Package MessageError", "Packages: value cannot be zero.", message);
					AssertContains("2 Assert Question Notification", "It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", message);
					AssertContains("2 Assert Question Notification", "Do you want to send the message(s) despite these errors?", message);
				}
			});
		}

		public void TestStopSendingWithMessageErrorIfBondAmountExceedBondGuaranteeValue()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "BHR";
			testOrg2.OH_Code = "REM";
			testOrg1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "BHR", Core.Constants.CountryCodes.SouthAfrica);
			testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "REM", Core.Constants.CountryCodes.SouthAfrica);
			var customsCode1 = testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "2000", Core.Constants.CountryCodes.SouthAfrica);
			var customsCode2 = testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "1000", Core.Constants.CountryCodes.SouthAfrica);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = _11;
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInstruction.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + _00;
			Factory.Save();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			Factory.Save();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Packages = 0;
			entry.CH_BGMReference = "Test Reference Number";
			var entryLine1 = entry.MergedLines[0];
			var addInfo1ForLine1 = entryLine1.AdditionalInformationCodes.AddNew();
			addInfo1ForLine1.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
			addInfo1ForLine1.CY_Data = "3000";
			entryLine1.AdditionalInformationCodes.Add(addInfo1ForLine1);
			testInstruction.CEI_OH_BondHolder = testOrg1.PK;
			testInstruction.CEI_OH_Carrier = testOrg2.PK;
			Factory.Save();
			CombineAssertions(() =>
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				using (var form = new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration)))
				{
					form.Show();
					var sendButton = (form.Controls.Find("SendButton", true)[0] as ZButton);
					sendButton.PerformClick();
					AssertContains("Bond amount from lines exceed Bond Guarantee Value for BHR, customs will likely reject this entry.", UnitTestUserNotification.Instance.LastMessage.Text);
					addInfo1ForLine1.CY_Data = "2000";
					Factory.Save();
					sendButton.PerformClick();
					AssertNotContains("Bond amount from lines exceed Bond Guarantee Value for BHR, customs will likely reject this entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestCheckIsOKToSend_ZAMessagingPOC()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var wrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			using (var form = new MessageSendingFormForTest(wrapper))
			{
				form.Show();
				wrapper.IsMessagingPOC = true;
				AssertEquals("Can send", true, form.CheckIsOKToSend_Exposed());

				wrapper.IsMessagingPOC = false;
				AssertEquals("Cant send", false, form.CheckIsOKToSend_Exposed());
			}
		}

		class MessageSendingFormForTest : MessageSendingForm
		{
			public MessageSendingFormForTest(JobDeclarationMessageSendingObjectParent declarationWrapper) : base(declarationWrapper) { }
			public bool CheckIsOKToSend_Exposed() => base.CheckIsOKToSend();
		}
	}
}
