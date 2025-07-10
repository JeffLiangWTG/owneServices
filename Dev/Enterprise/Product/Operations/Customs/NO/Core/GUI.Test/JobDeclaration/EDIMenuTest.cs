using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(EDIMenu))]
sealed class EDIMenuTest : TestCaseWithFactory
{
	public void TestSendMessageMenu()
	{
		using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BTH", }))
		using (var testMenu = new EDIMenuForTest())
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not visible, no declaration attached", expected: false, testMenu.SendToCustomsMenuItem.Visible);
				AssertNoExceptionThrown("RefreshMenu without Declaration", () => testMenu.RefreshMenu());

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals("App code set to BLT - should be visible", expected: true, testMenu.SendToCustomsMenuItem.Visible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				testMenu.RefreshMenu();
				AssertEquals("App code set to ITF - should not be visible", expected: false, testMenu.SendToCustomsMenuItem.Visible);
			});
		}
	}

	public void TestDisplayGenerateEntriesMenuOption()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);

		var inv = declaration.Invoices.AddNew();
		inv.InvoiceLines.AddNew();

		Factory.Save();

		using (var menu = new EDIMenu())
		{
			menu.Declaration = declaration;
			menu.RefreshMenu();

			Assert("Generate Entries option should be visible in NO", menu.GenerateEntriesMenuItem.Visible);
		}
	}

	public void TestSendToCustoms_ShouldCheckCredit()
	{
		using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BTH", }))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.Importer.MiscServ.OM_ARCreditLimit = -1;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingFormWithValidationDetails)obj;
				var action = (MessageSendingObject)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.ShouldSend = true;
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			const string notSentMessage = "Submit message with credit restriction canceled.";
			const string sentMessage = "Create CUSDEC message queued for sending.";

			var testCases = new[]
			{
				(isCheckPass: false, expectedMessageCount: 0, expectedMessageText: notSentMessage),
				(isCheckPass: true, expectedMessageCount: 1, expectedMessageText: sentMessage),
			};

			foreach (var (isCheckPass, expectedMessageCount, expectedMessageText) in testCases)
			{
				using var form = new JobDeclarationForm(declaration);
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

				using (CheckCreditTestHelper.WithCreditCheck(Factory, declaration, isCheckPass))
				{
					AssertEquals($"Credit check passed: {isCheckPass}, Precondition", 0, entryHeader.Messages.Count);

					sendToCustomsMenu.PerformClick();

					CombineAssertions($"Credit check passed: {isCheckPass}", () =>
					{
						AssertEquals("Last message after sending", expectedMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Messages count after sending", expectedMessageCount, entryHeader.Messages.Count);
					});
				}
			}
		}
	}
}

sealed class EDIMenuForTest : EDIMenu
{
	public ZMenuItem SendToCustomsMenuItem => base.sendToCustomsMenuItem;
}
