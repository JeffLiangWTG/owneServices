using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderMergingCheckerTest : TestCaseWithFactory
	{
		public void TestIsAllowedToMergeOrgsWithUnknownOrgs()
		{
			string mergeFailingReason;
			var mergeResult = OrgHeaderMergingChecker.IsAllowedToMergeOrgs(RetainedOrg.PK, ZGuid.BrettsGuid, out mergeFailingReason);
			AssertEquals(false, mergeResult);
			AssertEquals("Unknown organization(s).", mergeFailingReason);

			mergeResult = OrgHeaderMergingChecker.IsAllowedToMergeOrgs(ZGuid.BrettsGuid, DissolvedOrg.PK, out mergeFailingReason);
			AssertEquals(false, mergeResult);
			AssertEquals("Unknown organization(s).", mergeFailingReason);
		}

		public void TestPTBothOrgsHaveTransactionsAndDifferentIVACodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, "456");
				CreatePostedTransaction(RetainedOrg, "INV01");
				CreatePostedTransaction(DissolvedOrg, "INV02");
				Factory.Save();

				AssertEquals(true, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(true, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(false, "The organizations have different PT IVA registration codes and posted transactions linked to these codes. Please mark the organization to dissolve as inactive.");
			}
		}

		public void TestPTBothOrgsHaveTransactionsAndSameIVACodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, retainedOrgCusCode.OK_CustomsRegNo);
				CreatePostedTransaction(RetainedOrg, "INV01");
				CreatePostedTransaction(DissolvedOrg, "INV02");
				Factory.Save();

				AssertEquals(true, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(true, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(true);
			}
		}

		public void TestPTRetainedOrgDoesNotHaveTransactionButHasIVACodeDifferentFromDissolvedOrgAndDissolvedOrgHasTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, "456");
				CreatePostedTransaction(DissolvedOrg, "INV02");
				Factory.Save();

				AssertEquals(false, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(true, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(false, "The retained organization does not have posted transaction. The dissolved organization has posted transactions. Please, merge into the organization that has transactions.");
			}
		}

		public void TestPTRetainedOrgDoesNotHaveTransactionButHasSameIVACodeAsDissolvedOrgAndDissolvedOrgHasTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, retainedOrgCusCode.OK_CustomsRegNo);
				CreatePostedTransaction(DissolvedOrg, "INV02");
				Factory.Save();

				AssertEquals(false, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(true, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(true);
			}
		}

		public void TestPTRetainedOrgDoesNotHaveTransactionAndHasEmptyIVACodeAndDissolvedOrgHasTransactionsAndIVACode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, EmptyPortugalIVA);
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, "456");
				CreatePostedTransaction(DissolvedOrg, "INV02");
				Factory.Save();

				AssertEquals(false, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(true, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(false, "The retained organization does not have posted transaction. The dissolved organization has posted transactions. Please, merge into the organization that has transactions.");
			}
		}

		public void TestPTRetainedOrgHasTransactionsAndHasIVACodeDifferentFromDissolvedOrgAndDissolvedOrgDoesNotHaveTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, "456");
				CreatePostedTransaction(RetainedOrg, "INV01");
				Factory.Save();

				AssertEquals(true, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(false, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(true);
			}
		}

		public void TestPTBothOrgsHaveDifferentIVACodeButDoesNotHaveTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrgCusCode = AddIVACustomsCode(RetainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(DissolvedOrg, "456");
				Factory.Save();

				AssertEquals(false, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(false, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				AssertIsAllowedToMergeOrgsPTResult(true);
			}
		}

		public void TestPTRetainedOrgIVACodesDoesNotContainAllIVACodesFromDissolvedOrg()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				AddIVACustomsCode(RetainedOrg, "123");
				AddIVACustomsCode(DissolvedOrg, "456");

				CreatePostedTransaction(RetainedOrg, "INV01");
				CreatePostedTransaction(DissolvedOrg, "INV02");

				Factory.Save();

				AssertIsAllowedToMergeOrgsPTResult(false, "The organizations have different PT IVA registration codes and posted transactions linked to these codes. Please mark the organization to dissolve as inactive.");
			}
		}

		public void TestPTRetainedOrgIVACodesDoesContainAllIVACodesFromDissolvedOrg()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				AddIVACustomsCode(RetainedOrg, "456");
				AddIVACustomsCode(DissolvedOrg, "456");

				CreatePostedTransaction(RetainedOrg, "INV01");
				CreatePostedTransaction(DissolvedOrg, "INV02");

				Factory.Save();

				AssertIsAllowedToMergeOrgsPTResult(true);
			}
		}

		public void TestPTBothOrgsWithEmptyIVACodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				Assert("no customs codes in RetainedOrg", !RetainedOrg.CustomsCodes.Cast<OrgCusCode>().Any());
				Assert("no customs codes in DissolvedOrg", !DissolvedOrg.CustomsCodes.Cast<OrgCusCode>().Any());
				AssertIsAllowedToMergeOrgsPTResult(true);

				AddIVACustomsCode(RetainedOrg, string.Empty);
				AddIVACustomsCode(DissolvedOrg, string.Empty);
				Factory.Save();
				Assert("customsCode with empty RegNo are automatically deleted during OrgCusCode Save", !RetainedOrg.CustomsCodes.Cast<OrgCusCode>().Any());
				Assert("customsCode with empty RegNo are automatically deleted during OrgCusCode Save", !DissolvedOrg.CustomsCodes.Cast<OrgCusCode>().Any());
				AssertIsAllowedToMergeOrgsPTResult(true);

				AddIVACustomsCode(RetainedOrg, EmptyPortugalIVA);
				AddIVACustomsCode(DissolvedOrg, EmptyPortugalIVA);
				Factory.Save();
				Assert("RetainedOrg has portugal Empty IVA number", RetainedOrg.CustomsCodes.Cast<OrgCusCode>().Any());
				Assert("DissolvedOrg has portugal Empty IVA number", DissolvedOrg.CustomsCodes.Cast<OrgCusCode>().Any());
				AssertIsAllowedToMergeOrgsPTResult(true);
			}
		}

		public void TestPTDissolvedOrgCheckForDraftCommissionAgreements()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = DissolvedOrg.PK;
			var agreementApprove1 = opportunity.CommissionAgreements.AddNew();
			agreementApprove1.FillWithValidTestData();
			agreementApprove1.ApproveDraft();
			var agreementApprove2 = opportunity.CommissionAgreements.AddNew();
			agreementApprove2.FillWithValidTestData();
			agreementApprove2.ApproveDraft();
			var agreementApprove3 = opportunity.CommissionAgreements.AddNew();
			agreementApprove3.FillWithValidTestData();
			agreementApprove3.ApproveDraft();

			var agreementDraft1 = agreementApprove1.CreateDraft();
			var agreementDraft2 = agreementApprove2.CreateDraft();

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OH = RetainedOrg.PK;
			var agreementApprove4 = opportunity.CommissionAgreements.AddNew();
			agreementApprove4.FillWithValidTestData();
			agreementApprove4.ApproveDraft();
			var agreementDraft4 = agreementApprove4.CreateDraft();

			Factory.Save();

			var expected = $"There are pending commission agreements for the merging organizations, {RetainedOrg.OH_Code} and {DissolvedOrg.OH_Code}. These must be approved or disapproved prior to merge. These are: {agreementDraft1.AgreementId}, {agreementDraft2.AgreementId}, {agreementDraft4.AgreementId}";

			AssertIsAllowedToMergeOrgsPTResult(false, expected);

			agreementDraft1.DisapproveDraft();
			agreementDraft2.ApproveDraft();
			agreementDraft4.ApproveDraft();

			Factory.Save();

			AssertIsAllowedToMergeOrgsPTResult(true, string.Empty);
		}

		public void TestIsAllowedToMergeOrgsWithDuplicateContracts()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_OH = RetainedOrg.PK;
			contract2.RCT_OH = RetainedOrg.PK;
			contract3.RCT_OH = DissolvedOrg.PK;
			contract4.RCT_OH = DissolvedOrg.PK;

			contract1.RCT_ContractType = "PRO";
			contract2.RCT_ContractType = "CLI";
			contract3.RCT_ContractType = "PRO";
			contract4.RCT_ContractType = "CLI";

			contract1.RCT_ContractNumber = "123";
			contract2.RCT_ContractNumber = "456";
			contract3.RCT_ContractNumber = "789";
			contract4.RCT_ContractNumber = "000";

			Factory.Save();

			AssertIsAllowedToMergeOrgsPTResult(true);

			contract3.RCT_ContractNumber = "123";
			Factory.Save();

			AssertIsAllowedToMergeOrgsPTResult(false, "Cannot merge organizations since duplicate Contract ID 123 was found under Carrier Contracts & Allocations of DISSOLVEDORG and RETAINEDORG.");

			contract1.RCT_ContractType = "CLI";
			contract3.RCT_ContractType = "CLI";
			Factory.Save();

			AssertIsAllowedToMergeOrgsPTResult(false, "Cannot merge organizations since duplicate Contract ID 123 was found under Client Contracts & Allocations of DISSOLVEDORG and RETAINEDORG.");
		}

		void AssertIsAllowedToMergeOrgsPTResult(bool expectedResult, string expectedFailingMessage = "")
		{
			string mergeFailingReason;
			var mergeResult = OrgHeaderMergingChecker.IsAllowedToMergeOrgs(RetainedOrg.PK, DissolvedOrg.PK, out mergeFailingReason);
			AssertEquals(expectedResult, mergeResult);
			AssertEquals(expectedFailingMessage, mergeFailingReason);
		}

		OrgCusCode AddIVACustomsCode(OrgHeader header, string customsRegNo)
		{
			var retainedOrgCusCode = header.CustomsCodes.AddNew();
			retainedOrgCusCode.OK_OH = header.PK;
			retainedOrgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			retainedOrgCusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			retainedOrgCusCode.OK_CustomsRegNo = customsRegNo;
			var newAddress = header.Addresses.AddNew();
			newAddress.OA_Address1 = "newAddress";
			retainedOrgCusCode.OK_OA_PremisesAddress = newAddress.PK;

			return retainedOrgCusCode;
		}

		void CreatePostedTransaction(OrgHeader header, string transactionNum)
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = header.PK;
			transactionHeader.AH_GC = Company.PK;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_OH = header.PK;
			transactionLine.AL_GC = Company.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Company = Factory.NewWithValidTestData<GlbCompany>();

			RetainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			RetainedOrg.OH_Code = "RETAINEDORG";
			RetainedOrg.OH_FullName = "RETAINED ORG PTY";
			RetainedOrg.MainAddress.OA_Address1 = "Test Address 1";
			RetainedOrg.MainAddress.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			DissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			DissolvedOrg.OH_Code = "DISSOLVEDORG";
			DissolvedOrg.OH_FullName = "DISSOLVED ORG PTY";
			DissolvedOrg.MainAddress.OA_Address1 = "Test Address 2";

			Factory.Save();
		}

		readonly string EmptyPortugalIVA = AccountingCountrySpecificValidationHelper.EmptyPortugalIVA;
		OrgHeader RetainedOrg;
		OrgHeader DissolvedOrg;
		GlbCompany Company;
	}
}
