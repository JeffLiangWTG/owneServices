using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(GlbStaffLogSubscriber))]
sealed class GlbStaffLogSubscriberTest : LogSubscriberTest<GlbStaffLogSubscriber>
{
	public void TestUpdateGlbStaffHomeBranch()
	{
		var testStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var testPassword = GlbStaffWrapper.Get(testStaff).GlbExternalPassword;
		Factory.Save();
		testStaff.Reload();
		testPassword.Reload();
		AssertNotEquals("Pre-check: Not empty GS_HOME_BRANCH", ZGuid.Empty, testStaff.GS_GB_HomeBranch);
		AssertEquals("Pre-check: GP_GB equal to GS_GB_HomeBranch", testStaff.GS_GB_HomeBranch, testPassword.GP_GB);

		var plBranch2 = Factory.NewCompanyAndBranchWith(companyCode: "PL2", branchCode: "PL2", countryCode: Core.Constants.CountryCodes.Poland);
		testStaff.GS_GB_HomeBranch = plBranch2.PK;
		Factory.Save();
		testStaff.Reload();
		testPassword.Reload();
		AssertEquals("Pre-check: GS_HOME_BRANCH equal to new branch PK", plBranch2.PK, testStaff.GS_GB_HomeBranch);
		AssertNotEquals("Pre-check: GP_GB not equal to GS_GB_HomeBranch", testStaff.GS_GB_HomeBranch, testPassword.GP_GB);

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
			testStaff.Reload();
			testPassword.Reload();

			AssertNotEquals("Not empty GS_HOME_BRANCH", ZGuid.Empty, testStaff.GS_GB_HomeBranch);
			AssertEquals("GP_GB equal to GS_GB_HomeBranch", testStaff.GS_GB_HomeBranch, testPassword.GP_GB);
		});
	}

	public void TestTransactionDeleted_OnStaffDeactivate()
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: true, mailBox: "Test@gmail.com", password: "Password");
		var glbPassword = GlbStaffWrapper.Get(glbStaff).GlbExternalPassword;
		Factory.TryCreateNewCusPollingTransaction(glbPassword.PK);
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		var transaction = GetPasswordTransaction(glbPassword);
		AssertNotNull("Transaction is created", transaction);

		glbStaff.GS_IsActive = false;
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		transaction = GetPasswordTransaction(glbPassword);
		AssertNull("Staff is inactive: Transaction is deleted", transaction);
	}

	public void TestTransactionCreated_OnStaffActivate_WithActivatedEvent()
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: true, mailBox: "Test@gmail.com", password: "Password");
		var glbPassword = GlbStaffWrapper.Get(glbStaff).GlbExternalPassword;
		Factory.Save();

		glbStaff.GS_IsActive = false;
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		var transaction = GetPasswordTransaction(glbPassword);
		AssertNull("Transaction does not exist", transaction);

		glbStaff.GS_IsActive = true;
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		transaction = GetPasswordTransaction(glbPassword);
		AssertNotNull("Staff is active: Transaction is created", transaction);
	}

	public void TestTransactionCreated_OnStaffActivate_WithEditedEvent()
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: false, mailBox: "Test@gmail.com", password: "Password");
		var glbPassword = GlbStaffWrapper.Get(glbStaff).GlbExternalPassword;
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		var transaction = GetPasswordTransaction(glbPassword);
		AssertNull("Transaction does not exist", transaction);

		glbStaff.GS_IsActive = true;
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		transaction = GetPasswordTransaction(glbPassword);
		AssertNotNull("Staff is active: Transaction is created", transaction);
	}

	CusPollingTransaction GetPasswordTransaction(GlbExternalPassword password)
		=> Factory.LoadTop1<CusPollingTransaction>(
			new ZDBOnlyQuery(typeof(CusPollingTransaction)) { OrderBy = CusPollingTransactionSchema.CPT_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCodes.PLCustoms)
			.AddToFilter(CusPollingTransactionSchema.CPT_ParentID, password.PK));

	CusPollingTransaction GetPasswordTransaction(ZGuid transactionPK)
		=> Factory.LoadTop1<CusPollingTransaction>(new ZDBOnlyQuery(typeof(CusPollingTransaction)).AddToFilter(CusPollingTransactionSchema.PK, transactionPK));

	protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent => true;
}
