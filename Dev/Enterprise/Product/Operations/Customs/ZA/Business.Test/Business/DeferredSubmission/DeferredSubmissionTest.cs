using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DeferredSubmission))]
	sealed class DeferredSubmissionNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var helper = new DeferredSubmissionHelper(declaration);
			var submission = new DeferredSubmission(helper);
			return submission;
		}
	}

	sealed class DeferredSubmissionTest : TestCaseWithFactory
	{
		public void TestPaymentMethod()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(4);
				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, submission.PaymentMethod);

				var line2 = declaration.InvoiceLines.AddNew();
				var header2 = declaration.ActiveEntryHeaders.AddNew();
				var entryLine2 = header2.MergedLines.AddNew();
				entryLine2.InvoiceLines.Add(line2);
				header2.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;

				helper.RefreshData();
				submission = new DeferredSubmission(helper);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, submission.PaymentMethod);
			}
		}

		public void TestPaymentMethodReadOnly()
		{
			var orgheader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader3.CompanyData.OB_IsCreditor = true;
			orgheader3.OH_FullName = "ORG3";
			orgheader3.OH_Code = "AGT000000003";
			orgheader3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);

			var orgheader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader4.CompanyData.OB_IsCreditor = true;
			orgheader4.OH_FullName = "ORG4";
			orgheader4.OH_Code = "AGT000000004";
			orgheader4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(4);
				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, submission.PaymentMethod);
				AssertEquals(true, submission.PaymentMethodReadOnly);

				submission.IsOverwritten = true;
				AssertEquals("Payment Method is adjustable when declaration is deferrable'", false, submission.PaymentMethodReadOnly);
			}
		}

		public void TestSubmissionDateOverride()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(4);
				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				AssertEquals(ZDate.Today.AddDays(3), submission.SubmissionDate);

				submission.IsOverwritten = true;
				submission.SubmissionDate = ZDate.Today.AddDays(2);
				AssertEquals(ZDate.Today.AddDays(2), submission.SubmissionDate);

				submission.PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				AssertEquals(ZDate.Today, submission.SubmissionDate);

				submission.IsOverwritten = false;
				AssertEquals(ZDate.Today.AddDays(3), submission.SubmissionDate);
			}
		}

		public void TestSubmissionDateReadOnly()
		{
			var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
			declaration.JE_DateOfArrival = ZDate.Today.AddDays(4);

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				declaration.DoMerge();
				Factory.Save();
				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				AssertEquals(ZDate.Today.AddDays(3), submission.SubmissionDate);
				AssertEquals(true, submission.SubmissionDateReadOnly);

				submission.IsOverwritten = true;
				AssertEquals(false, submission.SubmissionDateReadOnly);

				submission.PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				AssertEquals(true, submission.SubmissionDateReadOnly);

				submission.PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
				AssertEquals(false, submission.SubmissionDateReadOnly);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(true, submission.SubmissionDateReadOnly);
			}

			messageDeferralSettings.AllowAutomaticDeferredSelection = true;
			messageDeferralSettings.DaysBeforeETA = 0;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				declaration.DoMerge();
				Factory.Save();
				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				AssertEquals(ZDate.Today, submission.SubmissionDate);
				AssertEquals(true, submission.SubmissionDateReadOnly);

				submission.IsOverwritten = true;
				AssertEquals(true, submission.SubmissionDateReadOnly);

				submission.PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				AssertEquals(true, submission.SubmissionDateReadOnly);

				submission.PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
				AssertEquals(true, submission.SubmissionDateReadOnly);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(true, submission.SubmissionDateReadOnly);
			}
		}

		public void TestDeferredAccountOverride()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 0 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1, agent2);
				var helper = new DeferredSubmissionHelper(declaration);
				helper.CalculateDeferralAccountAndPaymentMethod();
				var submission = new DeferredSubmission(helper);

				AssertEquals(agent1.PK, submission.AgentPK);
				AssertEquals("1111111111", submission.DeferredAccount);

				submission.IsOverwritten = true;
				submission.DeferredAccount = "2222222222";
				AssertEquals(agent2.PK, submission.AgentPK);
				AssertEquals("2222222222", submission.DeferredAccount);

				submission.IsOverwritten = false;
				AssertEquals(agent1.PK, submission.AgentPK);
				AssertEquals("1111111111", submission.DeferredAccount);

				submission.IsOverwritten = true;
				submission.DeferredAccount = "2222222222";
				AssertEquals(agent2.PK, submission.AgentPK);
				AssertEquals("2222222222", submission.DeferredAccount);

				AssertEquals(agent1.PK, declaration.JE_OH_AgentOverride);
				submission.UpdateDeclaration();
				AssertEquals(agent2.PK, declaration.JE_OH_AgentOverride);
			}
		}

		public void TestDeferredAccountsList()
		{
			var orgheader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader3.CompanyData.OB_IsCreditor = true;
			orgheader3.OH_FullName = "ORG3";
			orgheader3.OH_Code = "AGT000000003";
			orgheader3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);

			var orgheader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader4.CompanyData.OB_IsCreditor = true;
			orgheader4.OH_FullName = "ORG4";
			orgheader4.OH_Code = "AGT000000004";
			orgheader4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 0 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
				var maps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				var mapping2 = maps.Cast<FinancialAccountNumberPortMap>().FirstOrDefault(m => m.FinancialAccountNumber == "2222222222");
				mapping2.ImporterPays = true;

				var mapping3 = maps.AddNew();
				mapping3.AccountStartDay = 23;
				mapping3.OrganizationPK = orgheader3.PK;
				mapping3.CreditorPK = orgheader3.PK;
				mapping3.FinancialAccountNumber = "3333333333";
				mapping3.Cash = true;
				mapping3.ImporterPays = false;
				var mapping4 = maps.AddNew();
				mapping4.AccountStartDay = 13;
				mapping4.OrganizationPK = declaration.JE_OH_Importer;
				mapping4.CreditorPK = declaration.JE_OH_Importer;
				mapping4.CustomsOfficeCode = "DFM";
				mapping4.FinancialAccountNumber = "4444444444";
				mapping4.Cash = false;
				mapping4.ImporterPays = true;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				AssertEquals(4, ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).Count);

				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				var deferredAccounts = submission.DeferredAccountsList;
				AssertEquals("precondition", PaymentMethodCodeList.Codes.Defer, submission.PaymentMethod);
				Assert("List includes non-cash, non-Imp Pays", deferredAccounts.ContainsCode("1111111111"));
				Assert("List includes Imp Pays when importer has matching FAN", deferredAccounts.ContainsCode("4444444444"));
				AssertEquals("AG1           Start Day: 5  ", deferredAccounts.GetDescriptionFromCode("1111111111"));
				AssertEquals("IMP000000001  Start Day:13  Imp. Pays", deferredAccounts.GetDescriptionFromCode("4444444444"));
				AssertEquals("List has only non-cash mappings", 2, deferredAccounts.Count);

				submission.IsOverwritten = true;
				submission.PaymentMethod = "C";

				var cashAccounts = submission.DeferredAccountsList;
				Assert("List includes Cash", cashAccounts.ContainsCode("3333333333"));
				AssertEquals("AGT000000003  Cash", cashAccounts.GetDescriptionFromCode("3333333333"));
				AssertEquals("List has only mappings for cash", 1, cashAccounts.Count);

				mapping2.CustomsOfficeCode = "";
				mapping2.Cash = true;
				mapping2.ImporterPays = true;
				mapping4.CustomsOfficeCode = "";
				mapping4.Cash = true;
				mapping4.ImporterPays = true;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				helper.RefreshData();

				cashAccounts = submission.DeferredAccountsList;
				AssertEquals("List contents are unchanged due to caching", 1, cashAccounts.Count);

				submission.PaymentMethod = "D";
				submission.PaymentMethod = "C";
				cashAccounts = submission.DeferredAccountsList;
				Assert("List includes Cash", cashAccounts.ContainsCode("3333333333"));
				Assert("List includes Cash + Imp pays when importer has matching FAN", cashAccounts.ContainsCode("4444444444"));
				AssertEquals("IMP000000001  Cash + Imp. Pays", cashAccounts.GetDescriptionFromCode("4444444444"));
				AssertEquals("List excludes Imp. Pays when importer FAN does not match", 2, cashAccounts.Count);
			}
		}

		public void TestUpdateDeclaration()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 3 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var agent1 = Factory.NewWithValidTestData<OrgHeader>();
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1);
				declaration.JE_OH_AgentOverride = agent1.PK;
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(5);

				var line2 = declaration.InvoiceLines.AddNew();
				var header2 = declaration.ActiveEntryHeaders.AddNew();
				var entryLine2 = header2.MergedLines.AddNew();
				entryLine2.InvoiceLines.Add(line2);
				header2.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;

				var freeLine = declaration.InvoiceLines.AddNew();
				var freeHeader = declaration.ActiveEntryHeaders.AddNew();
				var freeEntryLine = freeHeader.MergedLines.AddNew();
				freeEntryLine.InvoiceLines.Add(freeLine);
				freeHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;

				var maps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				var mapping2 = maps.Cast<FinancialAccountNumberPortMap>().FirstOrDefault(m => m.FinancialAccountNumber == "2222222222");
				mapping2.Cash = true;

				Factory.Save();

				var helper = new DeferredSubmissionHelper(declaration);
				var submission = new DeferredSubmission(helper);
				AssertEquals("precondition", ZDate.Today.AddDays(2), submission.SubmissionDate);
				AssertEquals("precondition", PaymentMethodCodeList.Codes.Defer, submission.PaymentMethod);
				AssertEquals("precondition", "1111111111", submission.DeferredAccount);
				AssertEquals("precondition", "11223344DFM,11223344DFM,11223344DFM", ZString.Join(",", declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Select(h => h.CH_BGMReference.Left(11)).ToArray()));

				submission.UpdateDeclaration();
				var logEntry = declaration.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_Reference.StartsWith("Default Deferring Options were overwritten."));
				AssertNull("Not expecting a log entry when using defaults", logEntry);

				submission.IsOverwritten = true;
				submission.PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				submission.DeferredAccount = "2222222222";

				submission.UpdateDeclaration();
				Factory.Save();
				AssertEquals("C", declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(mapping2.OrganizationPK, declaration.JE_OH_AgentOverride);

				logEntry = declaration.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_Reference.StartsWith("Default Deferring Options were overwritten."));
				AssertNotNull("Expected a log entry when defaults are overridden", logEntry);
				AssertEquals("Logged as Edit", AutoEvents.EditedARecord.Code, logEntry.Event.SE_Code);

				var expectedLog = ZString.Format("Default Deferring Options were overwritten. Submission Date changed from {0} to {1}, Agent changed from 11223344 to 22334455, Payment changed from D to C", ZDate.Today.AddDays(2), ZDateTime.Today);
				AssertEquals("Logged changes", expectedLog, logEntry.SL_Reference);

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();

				AssertEquals("All Deferrable or Cash Payment methods are updated", 2, entryHeaders.Count(h => h.CH_PaymentMethod == PaymentMethodCodeList.Codes.Cash));
				AssertEquals("Non-deferrable Payment Methods are ignored", 1, entryHeaders.Count(h => h.CH_PaymentMethod == PaymentMethodCodeList.Codes.Free));
				AssertEquals("BGM is updated", "22334455DFM,22334455DFM,22334455DFM", ZString.Join(",", entryHeaders.Select(h => h.CH_BGMReference.Left(11)).ToArray()));
			}
		}
	}
}
