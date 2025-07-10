using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	internal abstract class NZEDIMenuAbstractTest : TestCaseWithFactory
	{
		public void TestTitleInCreditOverTheLimitMessageShouldRespondToTheContextOfTheMessagesItself()
		{
			SetupValidPINIfRequired();
			SetMostRecentExchangeRateEndDateToCurrent();
			declaration.Importer.MiscServ.OM_ARCreditLimit = -1m;
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessages();
			using (AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				menu.Declaration = declaration;
				menu.SubmitJobMenuItem_Run();
				var expectedMessage = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(m => m.Caption == "Submit Message - Credit Restriction");
				AssertNotNull(expectedMessage);
				AssertMultilineASCIIEquals("expectedMessage.Text", @"Delivery of this message is restricted because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this message?
", expectedMessage.Text);
			}
		}

		public void TestTitleInCreditOnHoldMessageShouldRespondToTheContextOfTheMessagesItself()
		{
			SetupValidPINIfRequired();
			SetMostRecentExchangeRateEndDateToCurrent();
			declaration.Importer.OH_IsDebtor = declaration.Importer.OH_IsDebtor.IsValid;
			var debtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery(OrgDebtorGroupSchema.OJ_Code, "INT"));
			declaration.Importer.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			declaration.Importer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			menu.Declaration = declaration;
			UnitTestUserNotification.Instance.ClearMessages();
			menu.SubmitJobMenuItem_Run();

			var expectedMessage = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(m => m.Caption == "Submit Message - Credit Restriction");
			AssertNotNull(expectedMessage);
			AssertMultilineASCIIEquals("Message.Text", @"Delivery of this message is restricted because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this message?
", expectedMessage.Text);
		}

		public void TestSubmitToCustomsDeniedPartyScreeningMessage()
		{
			SetupValidPINIfRequired();
			Factory.Save();
			var complianceRiskStatusSupporterMock = new Mock<IComplianceRiskStatusSupporter>();
			complianceRiskStatusSupporterMock.Setup(m => m.IsDPSFreightMovementRestricted(It.IsAny<ZString>(), It.IsAny<IComplianceJobDirectionProvider>(), It.IsAny<BusinessObject>())).Returns(() => true);
			using (ObjectFactory.Substitute(nameof(IComplianceRiskStatusSupporter), x => complianceRiskStatusSupporterMock.Object))
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SubmitJobMenuItem_Run();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Warning - Denied Party Screening", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertContains("LastMessage.Text", "Warning – Submitting a Customs Declaration to Customs for an Organization which may be on the Denied Party Screening list could incur many severe penalties.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("LastConfirmationStringShown", "I UNDERSTAND THE CONSEQUENCES OF OVERRIDING THE DENIED PARTY MOVEMENT RESTRICTION", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				});
			}
		}

		public void TestResetToOriginalGetsStoppedIfSecurityIsInsuficient()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(declarant.PK.ToGuid()).Encrypt("TEST1");
			declarant.GS_EmailAddress = "test.user@company.org";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Factory.Save();
			Env.Security.CustomsResetToOriginal.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.Declaration = declaration;
			menu.ResetToOriginalMenuItem_Run();
			AssertEquals("User should have been notified that they dont have a security right", Env.Security.CustomsResetToOriginal.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendingProhibitedIfJobHasChanges()
		{
			declaration.DeclarationNumber = "";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menu.Declaration = declaration;
			menu.SubmitJobMenuItem_Run();
			AssertEquals("User should have been notified that the declaration needs to be saved before sending", "You must save the current Declaration details before generating a message.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation
		protected TestNZEDIMenu menu;
		protected JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			menu = new TestNZEDIMenu();
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestHelper.SetupMessagingEnvironment();
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;
			return settings;
		}

		protected override void TearDown()
		{
			menu.Dispose();
			base.TearDown();
		}

		internal class TestNZEDIMenu : NZEDIMenu
		{
			public void SubmitJobMenuItem_Run()
			{
				SubmitJobMenuItem_Click(null, new EventArgs());
			}

			public void CancelJobMenuItem_Run()
			{
				CancelJobMenuItem_Click(null, new EventArgs());
			}

			public void ResetToOriginalMenuItem_Run()
			{
				ResetToOriginalMenuItem_Click(null, new EventArgs());
			}

			public void QueueJobMenuItem_Run()
			{
				QueueJobMenuItem_Click(null, new EventArgs());
			}
		}

		protected NZEDIMenu GetNZEDIMenu()
		{
			return new NZEDIMenu();
		}

		protected virtual void SetupValidPINIfRequired()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
		}

		void SetMostRecentExchangeRateEndDateToCurrent()
		{
			TestCaseHelper.ClearTable(NZCCustomsExchangeRate.Schema.TableName);
			CreateExchangeRateRow("USD", new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day), 1.12m);
		}

		void CreateExchangeRateRow(string currencyCode, ZDateTime endDate, decimal rate)
		{
			NZCCustomsExchangeRate exchangeRate = Factory.New<NZCCustomsExchangeRate>();
			exchangeRate.U7_CurrencyCode = currencyCode;
			exchangeRate.U7_DateActiveFrom = endDate.AddDays(-14);
			exchangeRate.U7_DateActiveTo = endDate;
			exchangeRate.U7_Rate = rate;
		}
		#endregion
	}
}
