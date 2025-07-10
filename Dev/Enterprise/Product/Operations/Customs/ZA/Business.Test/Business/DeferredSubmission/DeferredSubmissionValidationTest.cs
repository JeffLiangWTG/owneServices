using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DeferredSubmissionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDeferredAccount()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DFM");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsCreditor = true;
			org1.OH_FullName = "ORG1";
			org1.OH_Code = "AG1";
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 5;
			mapping1.ImporterPays = false;
			mapping1.OrganizationPK = org1.PK;
			mapping1.CreditorPK = org1.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.Cash = false;
			mapping1.FinancialAccountNumber = "1111111111";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "DFM";
			var helper = new DeferredSubmissionHelper(declaration);
			var deferredSubmission = new DeferredSubmission(helper);
			var messageDeferralSettings = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				deferredSubmission.IsOverwritten = false;
				AssertNotEquals("agent does not have mapping", org1.PK, declaration.JE_OH_AgentOverride);
				deferredSubmission.Validation.ValidateDeferredAccount();
				AssertEquals("Does not report an error", false, deferredSubmission.DeferredAccountInfo.HasNotifications());
				deferredSubmission.IsOverwritten = true;
				deferredSubmission.Validation.ValidateDeferredAccount();
				AssertHasErrorContaining(deferredSubmission.DeferredAccountInfo, "An Account must be selected.");
				deferredSubmission.DeferredAccount = "1111111111";
				deferredSubmission.Validation.ValidateDeferredAccount();
				AssertNoErrorContaining(deferredSubmission.DeferredAccountInfo, "An Account must be selected.");
			}
		}

		public void TestCheckSubmissionDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var helper = new DeferredSubmissionHelper(declaration);
			var deferredSubmission = new DeferredSubmission(helper);
			var messageDeferralSettings = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(30);
				deferredSubmission.IsOverwritten = false;
				CombineAssertions("Calculated Values Not Overridden", () =>
				{
					deferredSubmission.SubmissionDate = ZDate.Today.AddDays(-1);
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be earlier than today.");
					deferredSubmission.SubmissionDate = ZDate.Today.AddDays(22);
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be more than 21 days from today.");
					declaration.JE_DateOfArrival = ZDate.Today.AddDays(14);
					deferredSubmission.Validation.ValidateSubmissionDate();
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be later than Arrival Date.");
				});
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(30);
				deferredSubmission.IsOverwritten = true;
				CombineAssertions("Calculated Values Overridden", () =>
				{
					deferredSubmission.SubmissionDate = ZDate.Today.AddDays(-1);
					AssertHasErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be earlier than today.");
					deferredSubmission.SubmissionDate = ZDate.Today;
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be earlier than today.");
					deferredSubmission.SubmissionDate = ZDate.Today.AddDays(22);
					AssertHasErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be more than 21 days from today.");
					deferredSubmission.SubmissionDate = ZDate.Today.AddDays(21);
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be more than 21 days from today.");
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be later than Arrival Date.");
					declaration.JE_DateOfArrival = ZDate.Today.AddDays(14);
					deferredSubmission.Validation.ValidateSubmissionDate();
					AssertHasErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be later than Arrival Date.");
					declaration.JE_DateOfArrival = ZDate.Today.AddDays(-1);
					deferredSubmission.Validation.ValidateSubmissionDate();
					AssertNoErrorContaining(deferredSubmission.SubmissionDateInfo, "Submission Date cannot be later than Arrival Date.");
				});
			}
		}

		public void TestCheckPaymentMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var helper = new DeferredSubmissionHelper(declaration);
			var deferredSubmission = new DeferredSubmission(helper);
			deferredSubmission.IsOverwritten = false;
			deferredSubmission.Validation.ValidatePaymentMethod();
			AssertEquals("Does not report an error", false, deferredSubmission.PaymentMethodInfo.HasNotifications());
			deferredSubmission.IsOverwritten = true;
			deferredSubmission.Validation.ValidatePaymentMethod();
			AssertHasErrorContaining(deferredSubmission.PaymentMethodInfo, "A Payment Method must be selected.");
			deferredSubmission.PaymentMethod = "D";
			deferredSubmission.Validation.ValidatePaymentMethod();
			AssertNoErrorContaining(deferredSubmission.DeferredAccountInfo, "A Payment Method must be selected.");
		}
	}
}
