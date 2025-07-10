using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.EPayment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(StaffTokenManagementForm))]
	sealed class StaffTokenManagementFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestDisclaimerMessage()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
			Factory.Save();

			using (var form = new StaffTokenManagementForm(bankAccount))
			{
				form.Show();

				var disclaimerMessage = (ZLabel)form.Controls.Find("DisclaimerMessage", true)[0];
				AssertNotNull(disclaimerMessage);
				AssertEquals(@"Please add the details of any person who you want to be able request FX quotes and book FX transactions with OFX from the Payments Processing Module.
Please note each user must first be added as an authorized user on your organization’s OFX account.
When you add a user to your E-Payment Account below, they will be asked to authorize the connection of your E-payment Account and OFX account using their OFX user name and password.
Once a user’s status below is listed as ""Authorized"" they will be able to request FX quotes and book FX transactions with OFX from the Payment Processing module.
Click ""Learn More"" for more information about OFX and CargoWise Global Integrated Payments.", disclaimerMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestLearnMoreButtonOpensUpProductMarketingPage()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
			Factory.Save();

			using (var form = new StaffTokenManagementForm(bankAccount))
			{
				form.Show();

				var learnMoreButton = (ZButton)form.Controls.Find("LearnMoreButton", true)[0];
				AssertNotNull(learnMoreButton);
				var productMarketingWebURL = AccountingMasterFilesRegistry.Instance.EPaymentProductMarketingWebURL.Value;
				WebUrlLauncher.ClearLastUrlLaunched();
				learnMoreButton.PerformClick();
				AssertEquals(productMarketingWebURL, WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestFormProperties()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";
			var staffTokens = new AccEPaymentStaffTokenDependentCollection(bankAccount);
			var staffToken = staffTokens.AddNew();
			staffToken.TK_GS_NKStaffCode = "A";
			Factory.Save();

			var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
			using (var form = new StaffTokenManagementForm(bankAccountReloaded))
			{
				form.Show();

				var minSize = form.MinimumSize;
				AssertEquals(988, minSize.Width);
				AssertEquals(599, minSize.Height);

				var staffTokenGrid = (ZGrid)form.Controls.Find("staffTokenGrid", true)[0];
				var columns = staffTokenGrid.Columns;
				Assert("Staff Token Grid should contain User Full Name", columns.Contains("StaffCode+GS_FullName"));
				Assert("Staff Token Grid should not contain Account name", !columns.Contains("TK_AccountName"));
			}
		}

		public void TestRefresh()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";
			var staffTokens = new AccEPaymentStaffTokenDependentCollection(bankAccount);
			var staffToken = staffTokens.AddNew();
			staffToken.TK_GS_NKStaffCode = "A";
			Factory.Save();

			var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
			using (var form = new StaffTokenManagementForm(bankAccountReloaded))
			{
				form.Show();
				AssertEquals("NAT", bankAccount.EPaymentStaffTokenCollection[0].TK_Status);

				var sql = $@"
				UPDATE dbo.AccEPaymentStaffToken
				SET TK_Status = 'PND',
					TK_SystemLastEditTimeUtc = GETUTCDATE(),
					TK_SystemLastEditUser = 'TST'
				WHERE TK_PK = '{bankAccount.EPaymentStaffTokenCollection[0].PK}'";
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}

				var refreshButton = (ZButton)form.Controls.Find("refreshButton", true)[0];
				refreshButton.PerformClick();
				AssertEquals("PND", bankAccountReloaded.EPaymentStaffTokenCollection[0].TK_Status);
			}
		}

		public void TestCloseForm()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var staffTokens = new AccEPaymentStaffTokenDependentCollection(bankAccount);
			staffTokens.AddNew();
			Factory.Save();
			var tokenInDB = Factory.Load<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, bankAccount.PK));
			AssertEquals("Preconditon", 1, tokenInDB.Length);
			var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
			using (var form = new StaffTokenManagementForm(bankAccountReloaded))
			{
				form.Show();
				var staffTokenGrid = (ZGrid)form.Controls.Find("staffTokenGrid", true)[0];

				var newStaffToken = bankAccountReloaded.EPaymentStaffTokenCollection.AddNew();
				newStaffToken.TK_GS_NKStaffCode = "ZZZ";
				Assert(newStaffToken.HasErrors);

				var closeButton = (ZButton)form.Controls.Find("CloseButton", true)[0];
				closeButton.PerformClick();
				AssertEquals("Some records have errors, please fix the errors before close the form", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				newStaffToken.TK_GS_NKStaffCode = Factory.LoadTop1<GlbStaff>(new ZQuery()).GS_Code;
				Assert(!newStaffToken.HasErrors);
				closeButton.PerformClick();
				AssertEquals("There are unsaved changes, do you want to save them first before close the form?", UnitTestUserNotification.Instance.LastMessage.Text);

				tokenInDB = Factory.Load<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, bankAccount.PK));
				AssertEquals("Post condition", 2, tokenInDB.Length);
			}
		}

		public void TestAuthorizeURL()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "XYZ";
			var staffTokens = new AccEPaymentStaffTokenDependentCollection(bankAccount);
			var staffToken = staffTokens.AddNew();
			staffToken.TK_GS_NKStaffCode = "A";
			Factory.Save();

			var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
			using (var form = new StaffTokenManagementForm(bankAccountReloaded))
			{
				form.Show();
				var authorizeButton = (ZButton)form.Controls.Find("AuthorizeButton", true)[0];
				authorizeButton.PerformClick();
				AssertEquals("Users can only authorize their own accounts", UnitTestUserNotification.Instance.LastMessage.Text);

				bankAccountReloaded.EPaymentStaffTokenCollection[0].TK_GS_NKStaffCode = "ZZZ";
				authorizeButton.PerformClick();
				AssertEquals("Please save the form before start the authorization", UnitTestUserNotification.Instance.LastMessage.Text);

				bankAccountReloaded.EPaymentStaffTokenCollection[0].TK_GS_NKStaffCode = "E";
				var saveButton = (ZButton)form.Controls.Find("saveButton", true)[0];
				saveButton.PerformClick();

				var oAuthURL = AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.Value;
				var callbackURL = AccountingMasterFilesRegistry.Instance.WTCCallbackSiteWebURL.Value;

				authorizeButton.PerformClick();
				var encryptedStateBeginAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&state=") + 7;
				var encryptedStateEndAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&scope=");
				var encryptedState = WebUrlLauncher.LastUrlLaunched.Substring(encryptedStateBeginAt, encryptedStateEndAt - encryptedStateBeginAt);
				var ofxSecret = (new AESCrypto()).DecryptStringAES(AccountingMasterFilesRegistry.Instance.OFXEncryptionKey.Value, AESCrypto.RANDOM_SHAREDSECRET);
				AssertEquals("EDIDAT.EDI.E.XYZ.payments.Insert.False.False", (new AESCrypto()).DecryptStringAES(encryptedState, ofxSecret));
				var expectedURL = string.Format("{0}?response_type=code&client_id=xwFiSN389IiPKBdqXROEyUFpcG3w6lM6&state={2}&scope=payments&redirect_uri={1}", oAuthURL, callbackURL, encryptedState);
				AssertEquals(expectedURL, WebUrlLauncher.LastUrlLaunched);

				bankAccountReloaded.EPaymentStaffTokenCollection[0].TK_AccountName = "72A17513-9437-42D4-AE65-6301BE5B94EA";
				saveButton.PerformClick();
				authorizeButton.PerformClick();
				encryptedStateBeginAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&state=") + 7;
				encryptedStateEndAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&scope=");
				encryptedState = WebUrlLauncher.LastUrlLaunched.Substring(encryptedStateBeginAt, encryptedStateEndAt - encryptedStateBeginAt);
				AssertEquals("EDIDAT.EDI.E.XYZ.payments.Update.False.False", (new AESCrypto()).DecryptStringAES(encryptedState, ofxSecret));
				expectedURL = string.Format("{0}?response_type=code&client_id=xwFiSN389IiPKBdqXROEyUFpcG3w6lM6&state={2}&scope=payments&redirect_uri={1}", oAuthURL, callbackURL, encryptedState);
				AssertEquals(expectedURL, WebUrlLauncher.LastUrlLaunched);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			var collection = new AccEPaymentStaffTokenDependentCollection(bankAccount);
			collection.AddNew();
			return new StaffTokenManagementForm(bankAccount);
		}
	}
}
