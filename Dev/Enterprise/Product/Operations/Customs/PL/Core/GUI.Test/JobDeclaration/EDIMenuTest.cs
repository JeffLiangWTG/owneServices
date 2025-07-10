using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EDIMenuTest : TestCaseWithFactory
{
	public void TestGenerateEntriesMenuItem()
	{
		CombineAssertions(() =>
		{
			using (var menu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				menu.Declaration = declaration;

				var sendToCustomsMenuItem = menu.MenuItems.FindByText(SendCustomsDeclarationMenuItem);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				AssertEquals("GenerateEntriesMenuItem should be hidden", false, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals("SendToCustomsMenuItem should be hidden", false, sendToCustomsMenuItem.Visible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();
				AssertEquals("GenerateEntriesMenuItem should be visible", true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals("SendToCustomsMenuItem should be visible", true, sendToCustomsMenuItem.Visible);

				AssertNotNull("Send To Customs menu item should exist", sendToCustomsMenuItem);
				AssertNotNull("Customs Declaration menu item should exist", sendToCustomsMenuItem.MenuItems.FindByText(CustomsDeclarationMenuItem));
				AssertNotNull("Retrospective Quota Request menu item should exist", sendToCustomsMenuItem.MenuItems.FindByText(RetrospectiveQuotaRequestMenuItem));
			}
		});
	}

	public void TestRetrospectiveQuotaRequestMenuItemVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		using (var menu = new EDIMenu())
		{
			menu.Declaration = declaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			menu.RefreshMenu();
			var sendToCustomsMenuItem = menu.MenuItems.FindByText(SendCustomsDeclarationMenuItem);
			var retrospectiveQuotaRequestMenuItem = sendToCustomsMenuItem.MenuItems.FindByText(RetrospectiveQuotaRequestMenuItem);
			AssertEquals("Retrospective Quota Request menu item should not be visible", false, retrospectiveQuotaRequestMenuItem.Visible);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.JI_ConcessionOrder = "asd";
			menu.RefreshMenu();
			AssertEquals("Retrospective Quota Request menu item should be visible", true, retrospectiveQuotaRequestMenuItem.Visible);

			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			menu.RefreshMenu();
			AssertEquals("Retrospective Quota Request menu item should not be visible", false, retrospectiveQuotaRequestMenuItem.Visible);
		}
	}

	public void TestRetrospectiveQuotaRequestMenuItemVisibility_Issue01624766()
	{
		using (var menu = new EDIMenu())
		{
			menu.Declaration = null;

			AssertNoExceptionThrown("Should not throw Null Reference Exception when Declaration is null", () => menu.RefreshMenu());
		}
	}

	public void TestPreSendMessageErrors_Error()
	{
		const string preSendValidationErorrMessage = "Declaration lacks entries.";
		const string sendSuccessfullyMessage = "Message sent successfully";
		using var form = GetForm(isImport: false);
		var declaration = form.Declaration;
		Factory.Save();

		CombineAssertions(() =>
		{
			ClickSendToCustoms(form, CustomsDeclarationMenuItem);
			AssertContains("Contains pre-sending validation errors", preSendValidationErorrMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Has Error Notification", UnitTestUserNotification.Instance.LastMessage.WasError);

			declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
			Factory.Save();
			ClickSendToCustoms(form, CustomsDeclarationMenuItem);
			AssertEquals("Send successfully", sendSuccessfullyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestPreSendMessageErrors_Warning()
	{
		const string preSendValidationErorrMessage = "PUESC certificate chain is not valid:";
		const string sendSuccessfullyMessage = "Message sent successfully";
		using var form = GetForm(isImport: false);
		var declaration = form.Declaration;
		declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
		Factory.Save();

		CombineAssertions(() =>
		{
			ClickSendToCustoms(form, CustomsDeclarationMenuItem);

			AssertEquals("Send successfully", sendSuccessfullyMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			var penultimateMessage = UnitTestUserNotification.Instance.PreviousMessages.Skip(1).FirstOrDefault();
			AssertContains("Contains pre-sending validation errors", preSendValidationErorrMessage, penultimateMessage?.Text);
			Assert("Has Warning Notification", penultimateMessage?.WasWarning ?? false);
		});
	}

	public void TestRetrospectiveQuotaRequestSendToCustomsSerializedXmlMessage()
	{
		using (var form = GetForm(isImport: true))
		{
			var declaration = form.Declaration;
			declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
			Factory.Save();

			ClickSendToCustoms(form, RetrospectiveQuotaRequestMenuItem);

			CombineAssertions(() =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				var ediMessage = declaration.CustomsEntryHeaders[0].Messages[0];
				AssertEquals($"Generated message should not have MessageText for the time being - todo", ZString.Empty, ediMessage.EM_MessageText);
			});
		}
	}

	public void TestImportCustomsDeclarationSendToCustomsSerializedXmlMessage()
	{
		using (var form = GetForm(isImport: true))
		{
			var declaration = form.Declaration;
			declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
			Factory.Save();

			ClickSendToCustoms(form, CustomsDeclarationMenuItem);

			CombineAssertions(() =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				var ediMessage = declaration.CustomsEntryHeaders[0].Messages[0];
				AssertEquals($"Generated message should have empty MessageText", ZString.Empty, ediMessage.EM_MessageText);
			});
		}
	}

	public void TestExportCustomsDeclarationSendToCustomsSerializedXmlMessage()
	{
		using (var form = GetForm(isImport: false))
		{
			var declaration = form.Declaration;
			declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
			Factory.Save();

			ClickSendToCustoms(form, CustomsDeclarationMenuItem);

			CombineAssertions(() =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				var ediMessage = (EDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
				AssertEquals($"Generated message should have empty MessageText", ZString.Empty, ediMessage.EM_MessageText);
			});
		}
	}

	public void TestSendToCustomsAttachmentsMessage_NoEdocs()
	{
		using (var form = GetForm(isImport: true))
		{
			var declaration = (JobDeclaration)form.Declaration;
			declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
			Factory.Save();

			ClickSendToCustoms(form, CustomsDeclarationMenuItem);

			CombineAssertions(() =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No eDocs were added so no Attachment Messages were generated", 0, declaration.AttachmentMessages.Count);
			});
		}
	}

	public void TestSendToCustomsAttachmentsMessage_EDocsExist()
	{
		using (var form = GetForm(isImport: true))
		{
			var declaration = (JobDeclaration)form.Declaration;
			declaration.DoMerge(new Customs.GUI.SendsMessagesToCustomsGUI());
			Factory.Save();

			ClickSendToCustoms(form, CustomsDeclarationMenuItem, generateAttachments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Only 1 attachment Message should be generated at a time", 1, declaration.AttachmentMessages.Count);
			});
		}
	}

	public void TestSingleLineEntryMenu_Click()
	{
		using (var ediMenu = new EDIMenu())
		{
			ediMenu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			CombineAssertions(() =>
			{
				var singleLineMenuItem = ediMenu.MenuItems.FindByText(EU.GUI.Testing.EDIMenuTest.SingleLineEntryMenuItemCaption);
				singleLineMenuItem.PerformClick();
				AssertType<SingleLineEntryForm>("Export", ZFormModaliser.LastFormShownDialogForTest);

				ediMenu.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				singleLineMenuItem = ediMenu.MenuItems.FindByText(EU.GUI.Testing.EDIMenuTest.SingleLineEntryMenuItemCaption);
				singleLineMenuItem.PerformClick();
				AssertType<ImportSingleLineEntryForm>("Import", ZFormModaliser.LastFormShownDialogForTest);
			});
		}
	}

	static void ClickSendToCustoms(JobDeclarationFormTestClass form, string menuItemName, bool generateAttachments = false)
	{
		var menu = form.EDIMenu;
		var sendToCustomsMenuItem = menu.MenuItems.FindByText(SendCustomsDeclarationMenuItem);
		var searchedMenuItem = sendToCustomsMenuItem.MenuItems.FindByText(menuItemName);
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
		{
			var messageSendingForm = (MessageSendingForm)obj;
			var businessEntity = messageSendingForm.BusinessEntity;
			var action = businessEntity.SendingObjectsCollection[0];
			action.ShouldSend = true;

			if (generateAttachments)
			{
				var customsDeclaration = (CustomsDeclarationMessageSendingObjectParent)businessEntity;
				var declaration = customsDeclaration.ParentDeclaration;
				var pdfFile = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "pdfFile.pdf", Core.Constants.RefDocTypes.DocumentOfOrigin);
				var eDoc = customsDeclaration.EDocs.AddNew();
				eDoc.EDoc = pdfFile.UniqueKey;
			}
		});
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		searchedMenuItem.PerformClick();
	}

	JobDeclarationFormTestClass GetForm(bool isImport, bool setupCredentials = true
		, bool setupMergeableInvoiceLine = true, bool setupPhoneNumber = true)
	{
		if (setupCredentials)
		{
			var certificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
			certificate.GP_MailBoxID = "ABCXYZ";
			certificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			certificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			certificate.GP_ExpiryDate = ZDateTime.Now.AddDays(7);
			certificate.Factory.Save();
		}

		if (setupPhoneNumber)
		{
			var user = GlbStaff.CurrentUser;
			user.GS_WorkPhone = "123";
			user.GS_PublishWorkPhone = true;
		}

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = isImport ? MessageTypeList.Codes.Import : MessageTypeList.Codes.Export;
		if (setupMergeableInvoiceLine)
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
		}

		var form = new JobDeclarationFormTestClass(declaration);
		var menu = form.EDIMenu;
		menu.Declaration = declaration;
		return form;
	}

	public static string SendCustomsDeclarationMenuItem => EDIMenu.Constants.SendCustomsDeclaration.ToString();
	public static string CustomsDeclarationMenuItem => EDIMenu.Constants.CustomsDeclaration.ToString();
	public static string RetrospectiveQuotaRequestMenuItem = EDIMenu.Constants.RetrospectiveQuotaRequest.ToString();

	class JobDeclarationFormTestClass : Customs.GUI.Testing.BaseJobDeclarationFormTestClass
	{
		public JobDeclarationFormTestClass(JobDeclaration dec) : base(dec) { }

		protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();
	}
}
