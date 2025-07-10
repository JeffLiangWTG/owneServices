using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgDebtorGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOJ_Code()
		{
			DebtorGroup.OJ_Code = ZString.Empty;
			Assert("Expecting OJ_Code to be empty and have errors.", DebtorGroup.OJ_CodeInfo.HasErrors());
			DebtorGroup.OJ_Code = new ZString("ETO");
			Assert("OJ_Code should be correct, not expecting errors.", !DebtorGroup.OJ_CodeInfo.HasNotifications());
		}

		public void TestCheckOJ_Code_DuplicationInDB()
		{
			DebtorGroup.OJ_Code = "TST";
			TestFactory.Save();

			var targetDebtorGroup = TestFactory.New<OrgDebtorGroup>();
			targetDebtorGroup.OJ_Code = "TS2";

			AssertNoErrors("Pre-condition", DebtorGroup.OJ_CodeInfo);
			AssertNoErrors(targetDebtorGroup.OJ_CodeInfo);
			AssertEquals(true, DebtorGroup.IsInDatabase);

			targetDebtorGroup.OJ_Code = "TST";
			AssertHasError(targetDebtorGroup.OJ_CodeInfo, "Debtor Group Code must be unique.");

			DebtorGroup.Validation.ValidateAll();
			AssertHasError(DebtorGroup.OJ_CodeInfo, "Debtor Group Code must be unique.");
		}

		public void TestCheckOJ_Code_DuplicationInFactory()
		{
			DebtorGroup.OJ_Code = "TST";

			var targetDebtorGroup = TestFactory.New<OrgDebtorGroup>();
			targetDebtorGroup.OJ_Code = "TS2";

			AssertNoErrors("Pre-condition", DebtorGroup.OJ_CodeInfo);
			AssertNoErrors(targetDebtorGroup.OJ_CodeInfo);
			AssertEquals(false, DebtorGroup.IsInDatabase);

			targetDebtorGroup.OJ_Code = "TST";
			AssertHasError(targetDebtorGroup.OJ_CodeInfo, "Debtor Group Code must be unique.");

			DebtorGroup.Validation.ValidateAll();
			AssertHasError(DebtorGroup.OJ_CodeInfo, "Debtor Group Code must be unique.");
		}

		public void TestBankAccountValidation()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();
			group.OJ_Code = "Asd";
			group.OJ_Desc = "Asd description";
			group.DefaultBankAccountPK = bankAccount.PK;

			AssertNoErrors("Should be no errors", group.DefaultBankAccountPKInfo);

			group.DefaultBankAccountPK = ZGuid.NewZGuid();
			group.Validation.ValidateDefaultBankAccountPK();

			AssertHasErrors("Should be error from list validation", group.DefaultBankAccountPKInfo);

			group.DefaultBankAccountPK = bankAccount.PK;
			group.Validation.ValidateDefaultBankAccountPK();

			AssertNoErrors("Should be no errors", group.DefaultBankAccountPKInfo);

			group.OverrideRegistryCurrencyToBankSetting = true;
			group.DefaultBankAccountPK = ZGuid.Empty;
			group.Validation.ValidateDefaultBankAccountPK();
			AssertHasErrors("Should be error from Check entered", group.DefaultBankAccountPKInfo);

			group.OverrideRegistryCurrencyToBankSetting = false;
			group.DefaultBankAccountPK = ZGuid.Empty;
			group.Validation.ValidateDefaultBankAccountPK();
			AssertNoErrors("Should be no errors", group.DefaultBankAccountPKInfo);
		}

		public void TestCheckOJ_Desc()
		{
			DebtorGroup.OJ_Desc = ZString.Empty;
			Assert("Expecting OJ_Desc to be empty and have errors.", DebtorGroup.OJ_DescInfo.HasErrors());

			DebtorGroup.OJ_Desc = new ZString("asd");
			Assert("Expecting OJ_Desc to have too few characters and have errors.", DebtorGroup.OJ_DescInfo.HasErrors());

			DebtorGroup.OJ_Desc = new ZString("Australia, Dollars");
			Assert("OJ_Desc should be correct, not expecting errors.", !DebtorGroup.OJ_DescInfo.HasNotifications());
		}

		#region Implementation

		BusinessObjectFactory TestFactory;
		OrgDebtorGroup DebtorGroup;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			DebtorGroup = TestFactory.New(typeof(OrgDebtorGroup)) as OrgDebtorGroup;
		}

		#endregion

	}
}
