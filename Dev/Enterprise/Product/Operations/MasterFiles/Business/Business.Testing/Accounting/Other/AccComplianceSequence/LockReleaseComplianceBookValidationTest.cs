using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	class LockReleaseComplianceBookValidationTest : TestCaseWithFactory
	{
		protected BusinessObjectFactory TestFactory;
		protected AccComplianceSequence counterSequence, branchSequence, currenUserLockedSequence, notLockedCounterSequence;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			branchSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			branchSequence.XD_SequenceClass = "TXI";
			branchSequence.XD_Code = "LM5";
			branchSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			branchSequence.XD_StartNumber = 200;
			branchSequence.XD_EndNumber = 300;
			branchSequence.XD_MaximumNumberDigits = 6;
			branchSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			branchSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			branchSequence.XD_IsActive = true;

			counterSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			counterSequence.XD_SequenceClass = "TXI";
			counterSequence.XD_Code = "AAA";
			counterSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			counterSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			counterSequence.XD_Prefix = "ABC";
			counterSequence.XD_MaximumNumberDigits = 5;
			counterSequence.XD_StartNumber = 10;
			counterSequence.XD_EndNumber = 30;
			counterSequence.XD_NextNumber = 31;
			counterSequence.XD_IsActive = true;

			currenUserLockedSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			currenUserLockedSequence.XD_SequenceClass = "TXI";
			currenUserLockedSequence.XD_Code = "BBB";
			currenUserLockedSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			currenUserLockedSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			currenUserLockedSequence.XD_Prefix = "CBA";
			currenUserLockedSequence.XD_MaximumNumberDigits = 5;
			currenUserLockedSequence.XD_StartNumber = 10;
			currenUserLockedSequence.XD_EndNumber = 30;
			currenUserLockedSequence.XD_NextNumber = 31;
			currenUserLockedSequence.XD_IsActive = true;
			currenUserLockedSequence.XD_LockBy = GlbStaff.CurrentUser.PK;

			notLockedCounterSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			notLockedCounterSequence.XD_SequenceClass = "TXI";
			notLockedCounterSequence.XD_Code = "CCC";
			notLockedCounterSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			notLockedCounterSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			notLockedCounterSequence.XD_Prefix = "DDD";
			notLockedCounterSequence.XD_MaximumNumberDigits = 5;
			notLockedCounterSequence.XD_StartNumber = 10;
			notLockedCounterSequence.XD_EndNumber = 30;
			notLockedCounterSequence.XD_NextNumber = 31;
			notLockedCounterSequence.XD_IsActive = true;
			TestFactory.Save();
		}

		public void BaseValidateAll(LockReleaseComplianceBook book)
		{
			book.Validation.ValidateAll();
			AssertEquals(true, book.XD_Calc_PKInfo.HasError("Please enter a Compliance Invoice Book."));

			book.XD_Calc_PK = ZGuid.NewZGuid();
			book.Validation.ValidateAll();
			AssertEquals(true, book.XD_Calc_PKInfo.HasError("This Compliance Invoice Book does not exist in Compliance Sequences."));

			book.XD_Calc_PK = branchSequence.PK;
			book.Validation.ValidateAll();
			AssertEquals(true, book.XD_Calc_PKInfo.HasError("The selected book cannot be selected. Only ‘CTR’ Allocation Level Compliance Invoice Book can be selected."));
		}

		public void TestLockComplianceBookValidateAll()
		{
			LockComplianceBook lockComplianceBook = new LockComplianceBook(Factory);

			BaseValidateAll(lockComplianceBook);

			lockComplianceBook.XD_Calc_PK = currenUserLockedSequence.PK;
			lockComplianceBook.Validation.ValidateAll();
			AssertEquals(true, lockComplianceBook.XD_Calc_PKInfo.HasError(@"The selected book 'BBB' has been locked by '" + GlbStaff.CurrentUser.GS_Code + "'. Please select another book."));

			lockComplianceBook.XD_Calc_PK = counterSequence.PK;
			lockComplianceBook.Validation.ValidateAll();
			AssertEquals(true, lockComplianceBook.XD_Calc_PKInfo.HasError(@"'CTR' Allocation Level Compliance Invoice Book for each lock book must be for a different Compliance Sub Type.
Compliance Invoice Book which was locked : 
Sub Type : TXI 
Code : BBB"));

			lockComplianceBook.ComplianceSequenceBo.XD_SequenceClass = "TTT";
			lockComplianceBook.Validation.ValidateAll();
			Assert(!lockComplianceBook.XD_Calc_PKInfo.HasErrors());
		}

		public void TestReleaseComplianceBookValidateAll()
		{
			counterSequence.XD_LockBy = GlbStaff.New(Factory).PK;
			TestFactory.Save();

			ReleaseComplianceBook releaseComplianceBook = new ReleaseComplianceBook(Factory);

			BaseValidateAll(releaseComplianceBook);

			Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

			releaseComplianceBook.XD_Calc_PK = notLockedCounterSequence.PK;
			releaseComplianceBook.Validation.ValidateAll();
			AssertEquals(true, releaseComplianceBook.XD_Calc_PKInfo.HasError(@"The selected book 'CCC' is not locked and does not need to be Released. Please select another book."));

			releaseComplianceBook.XD_Calc_PK = counterSequence.PK;
			releaseComplianceBook.Validation.ValidateAll();
			AssertEquals(true, releaseComplianceBook.XD_Calc_PKInfo.HasError(@"The selected book 'AAA' has been locked by ''. 

If you require access to release ‘CTR’ Allocation Level Compliance Invoice Book locked by other staff, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyReleaseOtherStaff.DisplayTextPathToSecurityRight));

			releaseComplianceBook.XD_Calc_PK = currenUserLockedSequence.PK;
			releaseComplianceBook.Validation.ValidateAll();
			Assert(!releaseComplianceBook.XD_Calc_PKInfo.HasErrors());

			Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;
			releaseComplianceBook.XD_Calc_PK = counterSequence.PK;
			releaseComplianceBook.Validation.ValidateAll();
			Assert(!releaseComplianceBook.XD_Calc_PKInfo.HasErrors());
		}
	}
}
