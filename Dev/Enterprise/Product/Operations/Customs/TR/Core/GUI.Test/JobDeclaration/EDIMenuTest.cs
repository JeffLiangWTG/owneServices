using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.TR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class EDIMenuTest : TestCaseWithFactory
	{
		public void TestCustomsSendMessageMenuVisibility()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();

				var sendToControlMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send Control Message");
				Assert("Menu is visible", sendToControlMenu.Visible);

				var sendToRegistrationMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send Registration Message");
				Assert("Menu is visible", sendToRegistrationMenu.Visible);
			}
		}

		public void TestSendControlMessage()
		{
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 34.33m, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(0, entryHeader.Messages.Count);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send Control Message");
				sendToCustomsMenu.PerformClick();
				AssertContains(SuccessfullyQueuedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendRegistrationMessage()
		{
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 34.33m, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(0, entryHeader.Messages.Count);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send Registration Message");
				sendToCustomsMenu.PerformClick();
				AssertEquals("Dialog Name", "ESignatureForm", ZFormModaliser.LastFormShownDialogForTest.Name);
			}
		}

		public void TestCheckExchangeRateBeforeSendControlMessage()
		{
			declaration.JE_ValuationDate = ZDate.Today;
			CurrencyTestHelper.RemoveExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, declaration.JE_ValuationDate, Factory, ExchangeRateType.Customs);
			CurrencyTestHelper.RemoveExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, declaration.JE_ValuationDate, Factory, ExchangeRateType.CustomsSecondary);
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send Control Message");

				CombineAssertions(() =>
				{
					sendToCustomsMenu.PerformClick();
					AssertContains("Export | Exchange Error", NoExchangeRateMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 34.33m, declaration.JE_ValuationDate, Factory, ExchangeRateType.CustomsSecondary);
					sendToCustomsMenu.PerformClick();
					AssertContains("Export | Send Message", SuccessfullyQueuedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendToCustomsMenu.PerformClick();
					AssertContains("Import | Exchange Error", NoExchangeRateMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 35.45m, declaration.JE_ValuationDate, Factory, ExchangeRateType.Customs);
					sendToCustomsMenu.PerformClick();
					AssertContains("Import | Send Message", SuccessfullyQueuedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestCheckExchangeRateBeforeSendRegistrationMessage()
		{
			declaration.JE_ValuationDate = ZDate.Today;
			CurrencyTestHelper.RemoveExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, declaration.JE_ValuationDate, Factory, ExchangeRateType.Customs);
			CurrencyTestHelper.RemoveExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, declaration.JE_ValuationDate, Factory, ExchangeRateType.CustomsSecondary);
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send Registration Message");

				CombineAssertions(() =>
				{
					sendToCustomsMenu.PerformClick();
					AssertContains("Export | Exchange Error", NoExchangeRateMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 34.33m, declaration.JE_ValuationDate, Factory, ExchangeRateType.CustomsSecondary);
					sendToCustomsMenu.PerformClick();
					AssertEquals("Dialog Name", "ESignatureForm", ZFormModaliser.LastFormShownDialogForTest.Name);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendToCustomsMenu.PerformClick();
					AssertContains("Import | Exchange Error", NoExchangeRateMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 35.45m, declaration.JE_ValuationDate, Factory, ExchangeRateType.Customs);
					sendToCustomsMenu.PerformClick();
					AssertEquals("Dialog Name", "ESignatureForm", ZFormModaliser.LastFormShownDialogForTest.Name);
				});
			}
		}

		public void TestExportUnionDeclarationMenuVisibility()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();

				var exportUnionDeclarationMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Export Union Declaration");
				Assert("Menu is visible", exportUnionDeclarationMenu.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				exportUnionDeclarationMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Export Union Declaration");
				Assert("Menu is not visible", !exportUnionDeclarationMenu.Visible);
			}
		}

		public void TestExportUnionDeclaration()
		{
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(0, entryHeader.Messages.Count);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var exportUnionDeclarationMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Export Union Declaration");

				exportUnionDeclarationMenu.PerformClick();

				AssertType<ESignatureForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestCheckValuationDateBeforeSendMessage()
		{
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			declaration.JE_ValuationDate = ZDate.Today.AddDays(-1);
			Factory.Save();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(0, entryHeader.Messages.Count);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				var exportUnionDeclarationMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Export Union Declaration");

				AssertEquals(ZDate.Today.AddDays(-1), declaration.JE_ValuationDate);

				exportUnionDeclarationMenu.PerformClick();

				ZFormModaliser.ShowDialogsInTest = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertType<ESignatureForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(ZDate.Today, declaration.JE_ValuationDate);
			}
		}

		public void TestGenerateEntriesMergeShouldCreateCharges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("89", 624.10m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Stamp Duty");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoices = declaration.Invoices.AddNew();
			invoices.JZ_InvoiceDate = DateTime.Now;
			invoices.JZ_RX_NKInvoice_Currency = "EUR";
			invoices.JZ_InvoiceAmount = 1000m;

			var invoiceLine = invoices.InvoiceLines.AddNew();

			using (var menu = new EDIMenu())
			{
				ZFormModaliser.ShowDialogsInTest = false;
				menu.Declaration = declaration;
				var generateEntriesMenuItem = menu.MenuItems.FindByText("Generate Entries (Merge)");
				menu.RefreshMenu();
				generateEntriesMenuItem.PerformClick();

				CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
				var charges = entryHeader.Charges.GetChargeWithThisCode("89");

				CombineAssertions(() =>
				{
					AssertEquals("C1_ChargeType", DeclarationHelper.StampDutyConstants.ChargeType, charges.C1_ChargeType);
					AssertEquals("DescriptionOfChargeType", "Stamp Duty", charges.DescriptionOfChargeType);
					AssertEquals("C1_ChargeAmount", DeclarationHelper.StampDutyConstants.ChargeAmount, charges.C1_ChargeAmount);
					AssertEquals("C1_MethodOfPayment", DeclarationHelper.StampDutyConstants.MethodOfPayment, charges.C1_MethodOfPayment);
					AssertEquals("C1_RateOverrideReasonCode", DeclarationHelper.StampDutyConstants.RateOverrideReasonCode, charges.C1_RateOverrideReasonCode);
					AssertEquals("C1_Source", DeclarationHelper.StampDutyConstants.Source, charges.C1_Source);
				});

				menu.RefreshMenu();
				generateEntriesMenuItem.PerformClick();

				AssertEquals("Expected only 1 charge in entryHeader", 1 , entryHeader.Charges.Count);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			MasterFiles.Business.GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
			var currentUser = TRGlbStaffWrapper.Get(MasterFiles.Business.GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "A";
			cusEntryInstruction.CEI_Description = "Description 1";
			cusEntryInstruction.CEI_DisplaySequence = 2;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			cusEntryHeader.EntryNumber = "entry1";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			var invoices = declaration.Invoices.AddNew();
			invoices.JZ_InvoiceDate = DateTime.Now;
			invoices.JZ_RX_NKInvoice_Currency = "EUR";
			invoices.JZ_InvoiceAmount = 1000m;

			var invoiceLine = invoices.InvoiceLines.AddNew();

			var cusContainer = invoices.JobDeclaration.CusContainers.AddNew();
			cusContainer.CO_RN_NKOwnerCountry = "";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
		}
		JobDeclaration declaration;

		internal const string SuccessfullyQueuedMessage = "1 Message(s) queued for sending";
		internal const string NoExchangeRateMessage = "There is no exchange rate for today; the duty calculations can not be done correctly, and message sending is aborted.Please enter exchange rates for today.";
	}
}
