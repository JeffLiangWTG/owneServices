using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeaderCompanyFilter))]
	sealed class AccGLHeaderCompanyFilterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CCC";
			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			company4.GC_Code = "DDD";
			var branch = company4.Branches.AddNew();
			Factory.Save();

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var companyFilter1 = glHeader.CompanyFilters.AddNew();
			companyFilter1.ACF_GC_Company = company1.PK;
			var companyFilter2 = glHeader.CompanyFilters.AddNew();
			companyFilter2.ACF_GC_Company = company2.PK;
			var companyFilter3 = glHeader.CompanyFilters.AddNew();
			companyFilter3.ACF_GC_Company = company3.PK;
			var companyFilter4 = glHeader.CompanyFilters.AddNew();
			companyFilter4.ACF_GC_Company = company4.PK;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_GC = company1.PK;
			transaction.AH_AG = glHeader.PK;
			Assert(companyFilter1.CanDelete); // Not in DB

			Factory.Save();
			Assert(companyFilter1.CanDelete);
			AssertEquals("This GL Account has been used (Transaction Headers) in Company AAA.", companyFilter1.GetWarningBeforeBeingDeleted());
			Assert(companyFilter2.CanDelete);
			Assert(companyFilter3.CanDelete);
			Assert(companyFilter4.CanDelete);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company2.PK;
			chargeCode.AC_AG_AccrualAccount = glHeader.PK;
			Assert(companyFilter1.CanDelete);
			AssertEquals("This GL Account has been used (Transaction Headers) in Company AAA.", companyFilter1.GetWarningBeforeBeingDeleted());
			Assert(!companyFilter2.CanDelete);
			AssertEquals("Company BBB cannot be removed as this GL Account has been used (Charge Codes).", companyFilter2.ReasonForNotAbleToDelete);
			Assert(companyFilter3.CanDelete);
			Assert(companyFilter4.CanDelete);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_GC = company3.PK;
			bankAccount.AB_AG = glHeader.PK;
			Assert(companyFilter1.CanDelete);
			AssertEquals("This GL Account has been used (Transaction Headers) in Company AAA.", companyFilter1.GetWarningBeforeBeingDeleted());
			Assert(!companyFilter2.CanDelete);
			AssertEquals("Company BBB cannot be removed as this GL Account has been used (Charge Codes).", companyFilter2.ReasonForNotAbleToDelete);
			Assert(!companyFilter3.CanDelete);
			AssertEquals("Company CCC cannot be removed as this GL Account has been used (Bank Accounts).", companyFilter3.ReasonForNotAbleToDelete);
			Assert(companyFilter4.CanDelete);

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Accrual;
			line.AL_GB = branch.PK;
			line.AL_AG = glHeader.PK;
			Factory.Save();
			Assert(companyFilter1.CanDelete);
			AssertEquals("This GL Account has been used (Transaction Headers) in Company AAA.", companyFilter1.GetWarningBeforeBeingDeleted());
			Assert(!companyFilter2.CanDelete);
			AssertEquals("Company BBB cannot be removed as this GL Account has been used (Charge Codes).", companyFilter2.ReasonForNotAbleToDelete);
			Assert(!companyFilter3.CanDelete);
			AssertEquals("Company CCC cannot be removed as this GL Account has been used (Bank Accounts).", companyFilter3.ReasonForNotAbleToDelete);
			Assert(companyFilter4.CanDelete);
			AssertEquals("This GL Account has been used (Transaction Lines) in Company DDD.", companyFilter4.GetWarningBeforeBeingDeleted());

			transaction.AH_GC = chargeCode.AC_GC = bankAccount.AB_GC = company4.PK;
			Assert(!companyFilter4.CanDelete);
			AssertEquals("Company DDD cannot be removed as this GL Account has been used (Charge Codes, Bank Accounts).", companyFilter4.ReasonForNotAbleToDelete);
			AssertEquals("This GL Account has been used (Transaction Headers, Transaction Lines) in Company DDD.", companyFilter4.GetWarningBeforeBeingDeleted());
		}

		public void TestChargeCodeUseGLAccountWithCompanyNotSet()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_IsGlobal = true;
			var chargeCodeCompany = Factory.NewWithValidTestData<GlbCompany>();
			chargeCodeCompany.GC_Code = "CHC";
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AG_AccrualAccount = header.PK;
			chargeCode.AC_GC = chargeCodeCompany.PK;
			var chargeCodeCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			chargeCodeCompany2.GC_Code = "CH2";
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_AG_AccrualAccount = header.PK;
			chargeCode2.AC_GC = chargeCodeCompany2.PK;
			var chargeCodeCompany3 = Factory.NewWithValidTestData<GlbCompany>();
			chargeCodeCompany3.GC_Code = "CH3";
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_AG_AccrualAccount = header.PK;
			chargeCode3.AC_GC = chargeCodeCompany3.PK;
			Factory.Save();

			var filter = header.CompanyFilters.AddNew();
			var isCompanyFilterUsedInChargeCode = filter.GetIsCompanyFilterUsedInChargeCode([chargeCodeCompany.PK, chargeCodeCompany2.PK, chargeCodeCompany3.PK]);
			var chargeCodeUseGLAccountWithCompanyNotSet = filter.GetChargeCodeUseGLAccountWithCompanyNotSet([chargeCodeCompany.PK, chargeCodeCompany2.PK, chargeCodeCompany3.PK]);
			Assert(isCompanyFilterUsedInChargeCode);
			AssertEquals(3, chargeCodeUseGLAccountWithCompanyNotSet.Count);
			AssertContainsExactElementsInAnyOrder([chargeCode, chargeCode2, chargeCode3], chargeCodeUseGLAccountWithCompanyNotSet);

			isCompanyFilterUsedInChargeCode = filter.GetIsCompanyFilterUsedInChargeCode(System.Array.Empty<ZGuid>());
			chargeCodeUseGLAccountWithCompanyNotSet = filter.GetChargeCodeUseGLAccountWithCompanyNotSet(System.Array.Empty<ZGuid>());

			Assert(!isCompanyFilterUsedInChargeCode);
			AssertEquals(0, chargeCodeUseGLAccountWithCompanyNotSet.Count);
		}

		public void TestBankAccountUseGLAccountWithCompanyNotSet()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_IsGlobal = true;
			var bankAccountCompany = Factory.NewWithValidTestData<GlbCompany>();
			bankAccountCompany.GC_Code = "BAC";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AG = header.PK;
			bankAccount.AB_GC = bankAccountCompany.PK;
			var bankAccountCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			bankAccountCompany2.GC_Code = "BA2";
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount2.AB_AG = header.PK;
			bankAccount2.AB_GC = bankAccountCompany2.PK;
			var bankAccountCompany3 = Factory.NewWithValidTestData<GlbCompany>();
			bankAccountCompany3.GC_Code = "BA3";
			var bankAccount3 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount3.AB_AG = header.PK;
			bankAccount3.AB_GC = bankAccountCompany3.PK;
			Factory.Save();

			var filter = header.CompanyFilters.AddNew();
			var isCompanyFilterUsedInBankAccount = filter.GetIsCompanyFilterUsedInBankAccount([bankAccountCompany.PK, bankAccountCompany2.PK, bankAccountCompany3.PK]);
			var bankAccountUseGLAccountWithCompanyNotSet = filter.GetBankAccountUseGLAccountWithCompanyNotSet([bankAccountCompany.PK, bankAccountCompany2.PK, bankAccountCompany3.PK]);
			Assert(isCompanyFilterUsedInBankAccount);
			AssertEquals(3, bankAccountUseGLAccountWithCompanyNotSet.Count);
			AssertContainsExactElementsInAnyOrder([bankAccount, bankAccount2, bankAccount3], bankAccountUseGLAccountWithCompanyNotSet);

			isCompanyFilterUsedInBankAccount = filter.GetIsCompanyFilterUsedInBankAccount(System.Array.Empty<ZGuid>());
			bankAccountUseGLAccountWithCompanyNotSet = filter.GetBankAccountUseGLAccountWithCompanyNotSet(System.Array.Empty<ZGuid>());
			Assert(!isCompanyFilterUsedInBankAccount);
			AssertEquals(0, bankAccountUseGLAccountWithCompanyNotSet.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccGLHeaderCompanyFilter>();
		}
	}
}
