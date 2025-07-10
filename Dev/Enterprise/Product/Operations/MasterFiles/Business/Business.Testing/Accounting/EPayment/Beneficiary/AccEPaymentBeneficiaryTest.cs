using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEPaymentBeneficiary))]
	sealed class AccEPaymentBeneficiaryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLinkedAccountDetails()
		{
			var beneficiary = GetNewBusinessObject() as AccEPaymentBeneficiary;
			Assert(!beneficiary.IsMatchedWithOrg);
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Assert(beneficiary.IsMatchedWithOrg);
			Factory.Save();
			AssertEquals(accountDetails.PK, beneficiary.LinkedAccountDetails.PK);
		}

		public void TestLinkedOrgHeader()
		{
			var beneficiary = GetNewBusinessObject() as AccEPaymentBeneficiary;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_AB_APDefaultBankAccount = Factory.NewWithValidTestData<AccBankAccount>().PK;
			org.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			var accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();
			AssertEquals(org.PK, beneficiary.LinkedOrgHeader.PK);
			AssertEquals(org.MiscServ.OM_AB_APDefaultBankAccount, beneficiary.LinkedOrgBankAccountPK);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, beneficiary.LinkedOrgAgreedPaymentMethod);
		}

		public void TestMatchOrgAddNewAccountDetailsIfNotExisted()
		{
			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();

			var beneficiary = GetNewBusinessObject() as AccEPaymentBeneficiary;
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_AB_APDefaultBankAccount = bankAccount1.PK;
			org.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			var accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_RX_NKAccountCurrency = "AUD";
			accountDetails.A1_IsDefaultAccount = false;
			Factory.Save();
			AssertEquals(1, org.CompanyData.AccountDetailsCollection.Count);
			beneficiary.MatchOrg(org.PK, EPaymentReasonCodes.OFXReasonCodes.AccountingServices, ZGuid.Empty, ZString.Empty, false, beneficiary, EPaymentReferenceTypes.FreeText, "12345678888");
			AssertEquals(2, org.CompanyData.AccountDetailsCollection.Count);
			AssertEquals(org.PK, beneficiary.LinkedOrgHeader.PK);
			AssertEquals(bankAccount1.PK, org.MiscServ.OM_AB_APDefaultBankAccount);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, org.CompanyData.OB_APCreditAgreedPaymentMethod);

			var newAccountDetails = Factory.LoadTop1<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, beneficiary.PK));
			AssertNotEquals(accountDetails.PK, newAccountDetails.PK);
			AssertEquals(beneficiary.PK, newAccountDetails.A1_EPaymentBeneficiaryId);
			AssertEquals(EPaymentMethods.EPaymentViaOFX, newAccountDetails.A1_PaymentMethod);
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, newAccountDetails.A1_EPaymentReasonCode);
			Assert(newAccountDetails.A1_IsDefaultAccount);
			AssertEquals(beneficiary.ABF_RX_NKAccountCurrency, newAccountDetails.A1_RX_NKAccountCurrency);
			AssertEquals(beneficiary.ABF_BeneficiaryFullName, newAccountDetails.A1_AccountName);
			AssertEquals(beneficiary.ABF_RN_NKCountryCode, newAccountDetails.A1_RN_NKCountryCode);
			AssertEquals(beneficiary.ABF_BankName, newAccountDetails.A1_BankName);
			AssertEquals(beneficiary.ABF_BankBranchName, newAccountDetails.A1_BankBranchName);
			AssertEquals(beneficiary.ABF_BankBsb, newAccountDetails.A1_BankBsb);
			AssertEquals(beneficiary.ABF_BankAccount, newAccountDetails.A1_BankAccount);
			AssertEquals(beneficiary.ABF_BankSwift, newAccountDetails.A1_BankSwift);
			AssertEquals(beneficiary.ABF_BankAddress1, newAccountDetails.A1_BankAddress1);
			AssertEquals(beneficiary.ABF_BankAddress2, newAccountDetails.A1_BankAddress2);
			AssertEquals(beneficiary.ABF_BankAddress3, newAccountDetails.A1_BankAddress3);
			AssertEquals(EPaymentReferenceTypes.FreeText, newAccountDetails.A1_EPaymentReferenceType);
			AssertEquals("12345678888", newAccountDetails.A1_EPaymentReference);
		}

		public void TestMatchOrgUpdateExistingAccountDetails()
		{
			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();

			var beneficiary = GetNewBusinessObject() as AccEPaymentBeneficiary;
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_AB_APDefaultBankAccount = bankAccount1.PK;
			org.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			var accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_RX_NKAccountCurrency = "AUD";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			Factory.Save();
			AssertEquals(1, org.CompanyData.AccountDetailsCollection.Count);
			beneficiary.MatchOrg(org.PK, EPaymentReasonCodes.OFXReasonCodes.AccountingServices, bankAccount2.PK, OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, true, beneficiary, EPaymentReferenceTypes.FreeText, "12345678888");
			AssertEquals(1, org.CompanyData.AccountDetailsCollection.Count);
			AssertEquals(org.PK, beneficiary.LinkedOrgHeader.PK);
			AssertEquals(bankAccount2.PK, org.MiscServ.OM_AB_APDefaultBankAccount);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, org.CompanyData.OB_APCreditAgreedPaymentMethod);

			var existingAccountDetails = Factory.LoadTop1<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, beneficiary.PK));
			AssertEquals(beneficiary.PK, existingAccountDetails.A1_EPaymentBeneficiaryId);
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, existingAccountDetails.A1_EPaymentReasonCode);
			AssertEquals(beneficiary.ABF_BeneficiaryFullName, existingAccountDetails.A1_AccountName);
			AssertEquals(beneficiary.ABF_RX_NKAccountCurrency, existingAccountDetails.A1_RX_NKAccountCurrency);
			AssertEquals(beneficiary.ABF_RN_NKCountryCode, existingAccountDetails.A1_RN_NKCountryCode);
			AssertEquals(beneficiary.ABF_BankName, existingAccountDetails.A1_BankName);
			AssertEquals(beneficiary.ABF_BankBranchName, existingAccountDetails.A1_BankBranchName);
			AssertEquals(beneficiary.ABF_BankBsb, existingAccountDetails.A1_BankBsb);
			AssertEquals(beneficiary.ABF_BankAccount, existingAccountDetails.A1_BankAccount);
			AssertEquals(beneficiary.ABF_BankSwift, existingAccountDetails.A1_BankSwift);
			AssertEquals(beneficiary.ABF_BankAddress1, existingAccountDetails.A1_BankAddress1);
			AssertEquals(beneficiary.ABF_BankAddress2, existingAccountDetails.A1_BankAddress2);
			AssertEquals(beneficiary.ABF_BankAddress3, existingAccountDetails.A1_BankAddress3);
			AssertEquals(EPaymentReferenceTypes.FreeText, existingAccountDetails.A1_EPaymentReferenceType);
			AssertEquals("12345678888", existingAccountDetails.A1_EPaymentReference);
		}

		public void TestUnMatchOrg()
		{
			var beneficiary = GetNewBusinessObject() as AccEPaymentBeneficiary;
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();
			Assert(beneficiary.IsMatchedWithOrg);
			beneficiary.UnMatchOrg();
			Assert(!beneficiary.IsMatchedWithOrg);
			Assert(accountDetails.A1_EPaymentBeneficiaryId.IsEmpty);
		}

		public void TestCreatingUser()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "TST";
			Factory.Save();
			Assert("testUser is saved.", testUser.IsInDatabase);

			var bo = GetNewBusinessObject() as AccEPaymentBeneficiary;
			bo.ABF_SystemCreateUser = testUser.GS_Code;
			Factory.Save();
			Assert("deal is saved.", bo.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var boInNewFactory = newFactory.Load<AccEPaymentBeneficiary>(bo.PK);
			AssertNotNull("Creating user should exist.", boInNewFactory.CreatingUser);
			AssertEquals("Creating user should be testUser.", testUser.PK, boInNewFactory.CreatingUser.PK);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<AccEPaymentBeneficiary>();

		#endregion
	}
}
