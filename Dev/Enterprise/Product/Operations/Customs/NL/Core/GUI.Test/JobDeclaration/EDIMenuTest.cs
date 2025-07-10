using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.NL.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.NL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EDIMenuTest : TestCaseWithFactory
{
	public void TestSendToCustoms_AMD()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		Factory.Save();
		var entryHeader = declaration.CustomsEntryHeaders[0];
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
		entryHeader.CH_EntryStatus = "300";

		using (var form = new JobDeclarationForm(declaration))
		{
			using (CheckCreditTestHelper.WithCreditCheck(Factory, declaration, true))
			{
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == EDIMenu.Constants.SendToCustomsCaption);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;
					var parent = (JobDeclarationMessageSendingObjectParent)dialog.MessageSendingObjectParent;
					var action = parent.SendingObjectsCollection[0];
					action.MessageType = ExportSendMessageTypes.Codes.AMD;
					action.ShouldSend = true;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendToCustomsMenu?.PerformClick();
				AssertEquals("Last message after sending", "The message(s) have been sent to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);

				var msg = entryHeader.Messages[1];
				AssertEquals("Messages count after sending", 2, entryHeader.Messages.Count);
				AssertEquals("AMD", msg.EM_MessageSubType);
			}
		}
	}

	public void TestSendToCustoms_EXT() => DoSendToCustoms(ExportSendMessageTypes.Codes.EXT);

	public void TestSendToCustoms_DEC() => DoSendToCustoms(ExportSendMessageTypes.Codes.DEC);

	public void TestSendToCustoms_DEC_Import() => DoSendToCustoms(ImportSendMessageTypes.Codes.DEC, "IMP");

	public void TestSendToCustoms_PRE() => DoSendToCustoms(ExportSendMessageTypes.Codes.PRE);

	public void TestSendToCustoms_PRE_Import() => DoSendToCustoms(ImportSendMessageTypes.Codes.PRE, "IMP");

	public void TestSendToCustoms_CRE() => DoSendToCustoms(ExportSendMessageTypes.Codes.CRE);

	public void TestSendToCustoms_SUP() => DoSendToCustoms(ExportSendMessageTypes.Codes.SUP);

	public void TestSendToCustoms_CAN() => DoSendToCustoms(ExportSendMessageTypes.Codes.CAN);

	public void TestSendToCustoms_FBK() => DoSendToCustoms(ExportSendMessageTypes.Codes.FBK);

	public void DoSendToCustoms(string messageType, string declarationType = "EXP")
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = declarationType;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		Factory.Save();

		using (var form = new JobDeclarationForm(declaration))
		{
			using (CheckCreditTestHelper.WithCreditCheck(Factory, declaration, true))
			{
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == EDIMenu.Constants.SendToCustomsCaption);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;
					var parent = (JobDeclarationMessageSendingObjectParent)dialog.MessageSendingObjectParent;
					var action = parent.SendingObjectsCollection[0];
					action.MessageType = messageType;
					action.ShouldSend = true;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendToCustomsMenu?.PerformClick();

				var entryHeader = declaration.CustomsEntryHeaders[0];
				var msg = entryHeader.Messages[0];
				AssertEquals("Last message after sending", "The message(s) have been sent to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Messages count after sending", 1, entryHeader.Messages.Count);
				AssertEquals(messageType, msg.EM_MessageSubType);
			}
		}
	}

	public void TestSendToCustomsForExportMenuVisible()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		var sendToCustomsMenu = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption);
		AssertEquals("Menu 'Submit to customs' should be visible", true, sendToCustomsMenu.Visible);
		AssertEquals("Menu 'Submit to customs' should not have any child menu-items", 0, sendToCustomsMenu.MenuItems.Count);

		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		menu.RefreshMenu();
		sendToCustomsMenu = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption);
		AssertEquals("Menu 'Send to customs' should not be visible", false, sendToCustomsMenu.Visible);
	}

	public void TestSendToCustomsForImportMenuVisible()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		var sendToCustomsMenu = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption);
		AssertEquals("Menu 'Send to customs' should be visible", true, sendToCustomsMenu.Visible);
		AssertEquals("Menu 'Send to customs' should have 2 child menu-items", 2, sendToCustomsMenu.MenuItems.Count);
		var declarationsMenuItem = sendToCustomsMenu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsDeclarationsCaption);
		AssertEquals("Child menu-item 'Declarations' should be visible", true, declarationsMenuItem.Visible);

		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		menu.RefreshMenu();
		sendToCustomsMenu = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption);
		AssertEquals("Menu 'Send to customs' should not be visible", false, sendToCustomsMenu.Visible);
	}

	public void TestMenuItemsVisibleOnRefreshMenu()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		var sendToCustomsMenu = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption);
		var declarationsMenuItem = sendToCustomsMenu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsDeclarationsCaption);
		var serviceMessagesMenuItem = sendToCustomsMenu.MenuItems.FindByText(EDIMenu.Constants.ServiceMessagesCaption);

		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		menu.RefreshMenu();
		CombineAssertions("Import + ITF", () =>
		{
			AssertEquals("Menu 'Send to customs' should not be visible", false, sendToCustomsMenu.Visible);
			AssertEquals("Menu 'Declaration' should always be visible (but only available in menu on import declarations)", true, declarationsMenuItem.Visible);
			AssertEquals("Menu 'Service messages' should always be visible (but only available in menu on import declarations)", true, serviceMessagesMenuItem.Visible);
		});

		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		menu.RefreshMenu();
		CombineAssertions("Import + BLT - after ITF", () =>
		{
			AssertEquals("Menu 'Send to customs' should be visible", true, sendToCustomsMenu.Visible);
			AssertEquals("Menu 'Declaration' should always be visible (but only available in menu on import declarations)", true, declarationsMenuItem.Visible);
			AssertEquals("Menu 'Service messages' should always be visible (but only available in menu on import declarations)", true, serviceMessagesMenuItem.Visible);
		});

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		menu.RefreshMenu();
		CombineAssertions("Export + BLT, changed from IMP + BLT", () =>
		{
			AssertEquals("Menu 'Send to customs' should be visible", true, sendToCustomsMenu.Visible);
			AssertEquals("Menu 'Send to customs' should not have menu-items", 0, sendToCustomsMenu.MenuItems.Count);
			AssertEquals("Menu 'Declaration' should always be visible (but only available in menu on import declarations)", true, declarationsMenuItem.Visible);
			AssertEquals("Menu 'Service messages' should always be visible (but only available in menu on import declarations)", true, serviceMessagesMenuItem.Visible);
		});
	}

	public void TestServicesMessagesMenuVisible()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		var sendToCustomsMenu = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption);
		var servicesMessagesMenu = sendToCustomsMenu.MenuItems.FindByText(EDIMenu.Constants.ServiceMessagesCaption);
		AssertEquals("Menu 'Service messages' should be visible", true, servicesMessagesMenu.Visible);
		AssertEquals("Menu 'Service messages' should have child menu-items", true, servicesMessagesMenu.IsParent);
		var dutyGuaranteeMenuItem = servicesMessagesMenu.MenuItems.FindByText("Duty guarantee amount check");
		AssertEquals("Child menu-item 'Duty guarantee amount check' should be visible", true, dutyGuaranteeMenuItem.Visible);
		var guaranteeMenuItem = servicesMessagesMenu.MenuItems.FindByText("Check Guarantee amount left");
		AssertEquals("Child menu-item 'Check Guarantee amount left' should be visible", true, guaranteeMenuItem.Visible);
		var vatArticle23MenuItem = servicesMessagesMenu.MenuItems.FindByText("VAT (Article 23) deferment check");
		AssertEquals("Child menu-item 'VAT (Article 23) deferment check' should be visible", true, vatArticle23MenuItem.Visible);

		AssertEquals("There should be 3 child menu-items for service messages", 3, servicesMessagesMenu.MenuItems.Count);
	}

	public void TestSendVatDeferment()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		var vatDefermentMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems
												 .FindByText(EDIMenu.Constants.ServiceMessagesCaption).MenuItems
												 .FindByText("VAT (Article 23) deferment check");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		vatDefermentMenuItem.PerformClick();
		Assert("Message should be shown 'Message sent successfully'.", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Message sent successfully"));
		AssertEquals(typeof(CheckVatDefermentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
	}

	public void TestSendVatDeferment_Cancel()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		var vatDefermentMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems.
												 FindByText(EDIMenu.Constants.ServiceMessagesCaption).MenuItems.
												 FindByText("VAT (Article 23) deferment check");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		vatDefermentMenuItem.PerformClick();
		AssertEquals(typeof(CheckVatDefermentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		AssertNull("No message should be shown when user cancelled the 'Check Vat Deferment'-form.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSendMenuItem_Cancel()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};
		var sendToCustomsMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems.FindByText("Declarations");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		sendToCustomsMenuItem.PerformClick();
		AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		AssertNull("No message should be shown when user cancelled the 'Send'-form.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSendMenuItem()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;

		Customs.Business.CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entry.MergedLines.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		Factory.Save();

		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		menu.RefreshMenu();
		var sendToCustomsMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems.FindByText("Declarations");
		declaration.Factory.Save();
		sendToCustomsMenuItem.PerformClick();
		AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
	}

	public void TestSendGuaranteeAmount()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		var guaranteeAmountMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems
													 .FindByText(EDIMenu.Constants.ServiceMessagesCaption).MenuItems
													 .FindByText(EDIMenu.Constants.GuaranteeAmountCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		guaranteeAmountMenuItem.PerformClick();
		Assert("Message should be shown 'Message sent successfully'.", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Message sent successfully"));
		AssertEquals(typeof(CheckGuaranteeAmountForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
	}

	public void TestSendGuaranteeAmount_Cancel()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var menu = new EDIMenu
		{
			Declaration = dec
		};
		var guaranteeAmountMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems.
												 FindByText(EDIMenu.Constants.ServiceMessagesCaption).MenuItems.
												 FindByText("Check Guarantee amount left");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		guaranteeAmountMenuItem.PerformClick();
		AssertEquals(typeof(CheckGuaranteeAmountForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		AssertNull("No message should be shown when user cancelled the 'Check Customs Guarantee Amount'-form.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSendToCustoms_ShouldCheckCredit()
	{
		var expDeclaration = PrepareDeclaration(JobMessageTypeList.Codes.Export);
		expDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
		Factory.Save();

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(form =>
		{
			var messageSendingForm = (MessageSendingFormWithValidationDetails)form;
			var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		});
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		const string notSentMessage = "Unable to submit message due to Denied Party Screening cancellation.";
		const string sentMessage = "The message(s) have been sent to Customs.";

		var testCases = new[]
		{
			(jobDeclaration: expDeclaration, isCheckPass: false, expectedMessageCount: 0, expectedMessageText: notSentMessage),
			(jobDeclaration: expDeclaration, isCheckPass: true, expectedMessageCount: 1, expectedMessageText: sentMessage),
		};
		foreach (var (jobDeclaration, isCheckPass, expectedMessageCount, expectedMessageText) in testCases)
		{
			jobDeclaration.DPSFreightMovementRestricted = !isCheckPass;
			if (!isCheckPass)
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
			}
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == EDIMenu.Constants.SendToCustomsCaption);
				menu.Declaration = jobDeclaration;
				AssertEquals(GlbCompany.CurrentCompany.PK, jobDeclaration.CompanyPK);
				menu.RefreshMenu();

				CombineAssertions($"MessageType: {jobDeclaration.JE_MessageType}, Credit check passed: {isCheckPass}", () =>
				{
					using (CheckCreditTestHelper.WithCreditCheck(Factory, jobDeclaration, isCheckPass))
					{
						Factory.Save();
						AssertEquals("Precondition", 0, entryHeader.Messages.Count);
						sendToCustomsMenu?.PerformClick();

						AssertEquals("Last message after sending", expectedMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Messages count after sending", expectedMessageCount, entryHeader.Messages.Count);
					}
				});
			}
		}
	}

	public void TestSendVatDeferment_ShouldCheckCredit()
	{
		var declaration = PrepareDeclaration(JobMessageTypeList.Codes.Import);
		var menu = new EDIMenu
		{
			Declaration = declaration
		};
		menu.OnPopup(EventArgs.Empty);
		var vatDefermentMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems
			.FindByText(EDIMenu.Constants.ServiceMessagesCaption).MenuItems
			.FindByText("VAT (Article 23) deferment check");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		const string notSentMessage = "Unable to submit message due to Denied Party Screening cancellation.";
		const string sentMessage = "Message sent successfully";
		var testCases = new[]
		{
			(jobDeclaration: declaration, isCheckPass: false, expectedMessageText: notSentMessage),
			(jobDeclaration: declaration, isCheckPass: true, expectedMessageText: sentMessage)
		};
		foreach (var (jobDeclaration, isCheckPass, expectedMessageText) in testCases)
		{
			jobDeclaration.DPSFreightMovementRestricted = !isCheckPass;
			if (!isCheckPass)
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
			}

			vatDefermentMenuItem.PerformClick();
			using (CheckCreditTestHelper.WithCreditCheck(Factory, jobDeclaration, isCheckPass))
			{
				Assert("Last message after sending", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedMessageText));
			}
		}
	}

	public void TestSendGuaranteeAmount_ShouldCheckCredit()
	{
		var declaration = PrepareDeclaration(JobMessageTypeList.Codes.Import);
		var menu = new EDIMenu
		{
			Declaration = declaration
		};
		menu.OnPopup(EventArgs.Empty);
		var guaranteeAmountMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendToCustomsCaption).MenuItems
			.FindByText(EDIMenu.Constants.ServiceMessagesCaption).MenuItems
			.FindByText(EDIMenu.Constants.GuaranteeAmountCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		const string notSentMessage = "Unable to submit message due to Denied Party Screening cancellation.";
		const string sentMessage = "Message sent successfully";
		var testCases = new[]
		{
			(jobDeclaration: declaration, isCheckPass: false, expectedMessageText: notSentMessage),
			(jobDeclaration: declaration, isCheckPass: true, expectedMessageText: sentMessage)
		};
		foreach (var (jobDeclaration, isCheckPass, expectedMessageText) in testCases)
		{
			jobDeclaration.DPSFreightMovementRestricted = !isCheckPass;
			if (!isCheckPass)
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
			}

			guaranteeAmountMenuItem.PerformClick();
			using (CheckCreditTestHelper.WithCreditCheck(Factory, jobDeclaration, isCheckPass))
			{
				Assert("Last message after sending", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedMessageText));
			}
		}
	}

	public void TestPreSaveAndMergeBeforeSend()
	{
		var dec = Factory.New<JobDeclaration>();

		using (var form = new JobDeclarationFormTestClass(dec))
		{
			var menu = form.EDIMenu;
			menu.Declaration = dec;
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			menu.RefreshMenu();
			menu.MenuItems.FindByText("Send to Customs").MenuItems.FindByText("Declarations").PerformClick();
			AssertEquals(true, ((EdiMenuForTest)menu).PerformMergeCalled);
		}
	}

	public void TestSADHDataEntryFormMenuItem_Removed()
	{
		using (var menu = new EDIMenu())
		{
			var sadhMenuItem = menu.MenuItems.FindByText(EU.GUI.Testing.EDIMenuTest.SADHMenuItemCaption);
			AssertNull("Menu Item should not have been found for: " + EU.GUI.Testing.EDIMenuTest.SADHMenuItemCaption, sadhMenuItem);
		}
	}

	public void TestSingleLineEntryMenuItem_Removed()
	{
		using (var menu = new EDIMenu())
		{
			var sleMenuItem = menu.MenuItems.FindByText(EU.GUI.Testing.EDIMenuTest.SingleLineEntryMenuItemCaption);
			AssertNull("Menu Item should not have been found for: " + EU.GUI.Testing.EDIMenuTest.SingleLineEntryMenuItemCaption, sleMenuItem);
		}
	}
	public void TestPortMessagingMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);

		var portMessagingMenu = menu.MenuItems.FindByText(EDIMenu.Constants.PortMessagingCaption);
		AssertEquals("Menu 'Port Messaging' should be visible", true, portMessagingMenu.Visible);
		AssertEquals("Menu 'Port Messaging' should have 2 child items for Export", 2, portMessagingMenu.MenuItems.Count);
	}

	public void TestPortMessagingMenuVisibleForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var portMessagingMenu = menu.MenuItems.FindByText(EDIMenu.Constants.PortMessagingCaption);
		AssertEquals("Menu 'Port Messaging' should be visible for Import", true, portMessagingMenu.Visible);
		AssertEquals("Menu 'Port Messaging' should have 1 child item for Import", 1, portMessagingMenu.MenuItems.Count);
	}

	public void TestCargonautMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var portMessagingMenu = menu.MenuItems.FindByText(EDIMenu.Constants.PortMessagingCaption);
		var cargonautMenu = portMessagingMenu.MenuItems.FindByText(EDIMenu.Constants.CargonautExportCaption);
		AssertEquals("Menu 'Cargonaut - 750/755' should be visible for Export", true, cargonautMenu.Visible);
	}

	public void TestPortbaseExportDocumentationMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var portMessagingMenu = menu.MenuItems.FindByText(EDIMenu.Constants.PortMessagingCaption);
		var portbaseExportDocMenu = portMessagingMenu.MenuItems.FindByText(EDIMenu.Constants.PortbaseExportCaption);
		AssertEquals("Menu 'Portbase - Export documentation (MED)' should be visible for Export", true, portbaseExportDocMenu.Visible);
	}

	public void TestPortbaseImportDocumentationMenuVisibleForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var portMessagingMenu = menu.MenuItems.FindByText(EDIMenu.Constants.PortMessagingCaption);
		var portbaseImportDocMenu = portMessagingMenu.MenuItems.FindByText(EDIMenu.Constants.PortbaseImportCaption);
		AssertEquals("Menu 'Portbase - Import documentation (MID)' should be visible for Import", true, portbaseImportDocMenu.Visible);
	}

	public void TestAmendDeclarationMenuVisibleForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		AssertEquals("Menu 'Port Messaging' should be visible for Import", true, amendDeclarationMenu.Visible);
		AssertEquals("Menu 'Port Messaging' should have 6 child items for Import", 6, amendDeclarationMenu.MenuItems.Count);
	}

	public void TestAmendDeclarationMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		AssertEquals("Menu 'Port Messaging' should be visible for Export", true, amendDeclarationMenu.Visible);
		AssertEquals("Menu 'Port Messaging' should have 6 child items for Export", 6, amendDeclarationMenu.MenuItems.Count);
	}

	public void TestAmendmentRequestMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var amendmentRequestMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.AmendmentRequestCaption);
		AssertEquals("Menu 'Amendment Request' should be visible for Export", true, amendmentRequestMenu.Visible);
	}

	public void TestResponseToRFISentByCustomsMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var responseToRFICustomsMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.ResponseToRfiSentCaption);
		AssertEquals("Menu 'Response to RFI sent by Customs' should be visible for Export", true, responseToRFICustomsMenu.Visible);
	}

	public void TestSupplementaryDeclarationMenuVisibleForExport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var supplementaryDeclarationMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.SupplementaryDeclarationCaption);
		AssertEquals("Menu 'Supplementary Declaration' should be visible for Export", true, supplementaryDeclarationMenu.Visible);
	}

	public void TestAmendmentRequestMenuVisibleForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var amendmentRequestMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.AmendmentRequestCaption);
		AssertEquals("Menu 'Amendment Request' should be visible for Import", true, amendmentRequestMenu.Visible);
	}

	public void TestResponseToRFISentByCustomsMenuVisibleForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var responseToRFICustomsMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.ResponseToRfiSentCaption);
		AssertEquals("Menu 'Response to RFI sent by Customs' should be visible for Import", true, responseToRFICustomsMenu.Visible);
	}

	public void TestSupplementaryDeclarationMenuVisibleForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var supplementaryDeclarationMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.SupplementaryDeclarationCaption);
		AssertEquals("Menu 'Supplementary Declaration' should be visible for Import", true, supplementaryDeclarationMenu.Visible);
	}

	public void TestSupplementaryDeclarationMenuEnabled()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var supplementaryDeclarationMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.SupplementaryDeclarationCaption);
		AssertEquals("Menu 'Supplementary Declaration' should be disabled when there are no entry headers in the correct status", false, supplementaryDeclarationMenu.Enabled);

		var entryHeader1 = dec.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		menu.RefreshMenu();

		AssertEquals("Menu 'Supplementary Declaration' should be enabled when there are entry headers in the correct status", true, supplementaryDeclarationMenu.Enabled);
	}

	public void TestCancelSupplementaryDeclarationMenuVisible()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var cancelSupplementaryDeclarationMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.CancelSupplementaryDeclarationCaption);
		AssertEquals("Menu 'Cancel Supplementary Declaration' should be invisible when there are no entry headers in the correct status", false, cancelSupplementaryDeclarationMenu.Visible);

		var entryHeader1 = dec.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		menu.RefreshMenu();

		AssertEquals("Menu 'Cancel Supplementary Declaration' should be enabled when there are entry headers in the correct status", true, cancelSupplementaryDeclarationMenu.Visible);
	}

	public void TestEntrySelection_SupplementaryDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader2.CH_CEI_Instruction = entryInstruction.PK;

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var supplementaryDeclarationMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.SupplementaryDeclarationCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

		supplementaryDeclarationMenu.PerformClick();
		var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest;

		CombineAssertions(() =>
		{
			AssertEquals("Type", typeof(EntrySelectionForm), form.GetType());
			AssertEquals("Caption", "Select Entry For Supplementary Mode", form.CaptionResourceString.Caption);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertNoExceptionThrown(() => supplementaryDeclarationMenu.PerformClick());

			var entrySelection1 = new EntrySelection(entryHeader1);
			var entrySelection2 = new EntrySelection(entryHeader2);
			declaration.EntrySelections.Add(entrySelection1);
			declaration.EntrySelections.Add(entrySelection2);

			entrySelection1.Selected = true;
			AssertNoExceptionThrown(() => supplementaryDeclarationMenu.PerformClick());

			entrySelection1.Selected = true;
			entrySelection2.Selected = true;
			AssertNoExceptionThrown(() => supplementaryDeclarationMenu.PerformClick());
		});
	}

	public void TestEntrySelection_CancelSupplementaryDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var supplementaryDeclarationMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.CancelSupplementaryDeclarationCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		supplementaryDeclarationMenu.PerformClick();

		CombineAssertions(() =>
		{
			var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest;
			AssertEquals("Type", typeof(EntrySelectionForm), form.GetType());
			AssertEquals("Caption", "Cancel Supplementary", form.CaptionResourceString.Caption);
		});
	}

	public void TestAmendRequestMenuEnabled()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var amendRequestMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.AmendmentRequestCaption);
		AssertEquals("Menu 'Amend Request' should be disabled when there are no entry headers in the correct status", false, amendRequestMenu.Enabled);

		var entryHeader1 = dec.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.PreLodged;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.REG;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		menu.RefreshMenu();

		AssertEquals("Menu 'Amend Request' should be enabled when there are entry headers in the correct status", true, amendRequestMenu.Enabled);
	}

	public void TestCancelAmendRequestMenuVisible()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var cancelAmendRequestMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.CancelAmendRequestCaption);
		AssertEquals("Menu 'Cancel Amend Declaration' should be invisible when there are no entry headers in the correct status", false, cancelAmendRequestMenu.Visible);

		var entryHeader1 = dec.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		menu.RefreshMenu();

		AssertEquals("Menu 'Cancel Amend Declaration' should be enabled when there are entry headers in the correct status", true, cancelAmendRequestMenu.Visible);
	}

	public void TestEntrySelection_AmendRequest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.PreLodged;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.REG;

		var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.PreLodged;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.REG;

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var amendRequestMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.AmendmentRequestCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

		amendRequestMenu.PerformClick();
		var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest;

		CombineAssertions(() =>
		{
			AssertEquals("Type", typeof(EntrySelectionForm), form.GetType());
			AssertEquals("Caption", "Select Entry For Amendment Mode", form.CaptionResourceString.Caption);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertNoExceptionThrown(() => amendRequestMenu.PerformClick());

			var entrySelection1 = new EntrySelection(entryHeader1);
			var entrySelection2 = new EntrySelection(entryHeader2);
			declaration.EntrySelections.Add(entrySelection1);
			declaration.EntrySelections.Add(entrySelection2);

			entrySelection1.Selected = true;
			AssertNoExceptionThrown(() => amendRequestMenu.PerformClick());

			entrySelection1.Selected = true;
			entrySelection2.Selected = true;
			AssertNoExceptionThrown(() => amendRequestMenu.PerformClick());
		});
	}

	public void TestEntrySelection_CancelAmendRequest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var amendRequestMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.CancelAmendRequestCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		amendRequestMenu.PerformClick();

		CombineAssertions(() =>
		{
			var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest;
			AssertEquals("Type", typeof(EntrySelectionForm), form.GetType());
			AssertEquals("Caption", "Cancel Amendment", form.CaptionResourceString.Caption);
		});
	}

	public void TestResponseToRFISentMenuEnabled()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		dec.CustomsEntryInstructions.AddNew();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var responseToRFISentMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.ResponseToRfiSentCaption);
		AssertEquals("Menu 'Response to RFI sent' should be disabled when there are no entry headers in the correct status", false, responseToRFISentMenu.Enabled);

		var entryHeader1 = dec.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		menu.RefreshMenu();

		AssertEquals("Menu 'Response to RFI sent' should be enabled when there are entry headers in the correct status", true, responseToRFISentMenu.Enabled);
	}

	public void TestCancelResponseToRFISentMenuVisible()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();

		var menu = new EDIMenu
		{
			Declaration = dec
		};
		menu.OnPopup(EventArgs.Empty);
		menu.RefreshMenu();

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var cancelResponseToRFISentMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.CancelResponseToRFISentCaption);
		AssertEquals("Menu 'Cancel Response to RFI sent' should be invisible when there are no entry headers in the correct status", false, cancelResponseToRFISentMenu.Visible);

		var entryHeader1 = dec.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;
		entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

		menu.RefreshMenu();

		AssertEquals("Menu 'Cancel Response to RFI sent' should be enabled when there are entry headers in the correct status", true, cancelResponseToRFISentMenu.Visible);
	}

	public void TestEntrySelection_ResponseToRFISent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var responseToRFISentMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.ResponseToRfiSentCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

		responseToRFISentMenu.PerformClick();
		var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest;

		CombineAssertions(() =>
		{
			AssertEquals("Type", typeof(EntrySelectionForm), form.GetType());
			AssertEquals("Caption", "Select Entry For CRE Mode", form.CaptionResourceString.Caption);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertNoExceptionThrown(() => responseToRFISentMenu.PerformClick());

			var entrySelection1 = new EntrySelection(entryHeader1);
			var entrySelection2 = new EntrySelection(entryHeader2);
			declaration.EntrySelections.Add(entrySelection1);
			declaration.EntrySelections.Add(entrySelection2);

			entrySelection1.Selected = true;
			AssertNoExceptionThrown(() => responseToRFISentMenu.PerformClick());

			entrySelection1.Selected = true;
			entrySelection2.Selected = true;
			AssertNoExceptionThrown(() => responseToRFISentMenu.PerformClick());
		});
	}

	public void TestEntrySelection_CancelResponseToRFISent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var menu = new EDIMenu
		{
			Declaration = declaration
		};

		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;

		var amendDeclarationMenu = menu.MenuItems.FindByText(EDIMenu.Constants.AmendDeclarationCaption);
		var cancelResponseToRFISentMenu = amendDeclarationMenu.MenuItems.FindByText(EDIMenu.Constants.CancelResponseToRFISentCaption);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		cancelResponseToRFISentMenu.PerformClick();

		CombineAssertions(() =>
		{
			var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest;
			AssertEquals("Type", typeof(EntrySelectionForm), form.GetType());
			AssertEquals("Caption", "Cancel CRE Mode", form.CaptionResourceString.Caption);
		});
	}

	JobDeclarationForTest PrepareDeclaration(string messageType)
	{
		var declaration = Factory.New<JobDeclarationForTest>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = messageType;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		return declaration;
	}

	sealed class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool DPSFreightMovementRestricted { get; set; }

		protected override bool IsDPSFreightMovementRestrictedCore() => DPSFreightMovementRestricted;
		protected override ScreeningParty[] GetScreeningPartiesCore() => Array.Empty<ScreeningParty>();
	}

	class JobDeclarationFormTestClass : BaseJobDeclarationFormTestClass
	{
		public JobDeclarationFormTestClass(JobDeclaration dec) : base(dec) { }

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EdiMenuForTest();
	}

	class EdiMenuForTest : EDIMenu
	{
		public bool PerformMergeCalled;

		protected override bool PerformMerge()
		{
			PerformMergeCalled = true;
			return base.PerformMerge();
		}
	}
}
