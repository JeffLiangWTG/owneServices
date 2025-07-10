using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(GlbExternalPasswordLogSubscriber))]
sealed class GlbExternalPasswordLogSubscriberTest : LogSubscriberTest<GlbExternalPasswordLogSubscriber>
{
	protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent => true;

	public void TestTransactionIsCreated_OnNewPassword()
	{
		var password = CreatePassword(mailBox: "Test@gmail.com", password: "Password");

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertEquals("Password branch is equal to staff branch", password.Staff.GS_GB_HomeBranch, password.GP_GB);
		AssertNotNull("Transaction is created", GetPasswordTransaction(password));
	}

	public void TestTransactionIsNotCreated_ForNotPlbPassword()
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: true, mailBox: "Test@gmail.com", password: "Password");
		var password = GlbStaffWrapper.Get(glbStaff).GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>(PasswordTypesList.Codes.CCT, GlbCompany.CurrentCompany.PK);
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNull("Transaction is not created", GetPasswordTransaction(password));
	}

	public void TestTransactionIsCreated_OnlyOnCorrectProperties()
	{
		var password = CreatePassword(mailBox: "", password: "Password");

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
			AssertNull("No email: Transaction is not created", GetPasswordTransaction(password));

			password.GP_MailBoxID = "Test@gmail.com";
			password.GP_CurrentPassword = "";
			Factory.Save();

			AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
			AssertNull("No password: Transaction is not created", GetPasswordTransaction(password));

			password.GP_CurrentPassword = "Password";
			Factory.Save();

			AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
			AssertNotNull("All ok: Transaction is created", GetPasswordTransaction(password));
		});
	}

	public void TestTransactionDeleted_OnEmptyPassword()
	{
		var password = CreatePassword(mailBox: "Test@gmail.com", password: "Password");

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNotNull("Transaction is created", GetPasswordTransaction(password));

		password.GP_CurrentPassword = "";
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNull("No password: Transaction is deleted", GetPasswordTransaction(password));
	}

	public void TestTransactionDeleted_OnEmptyEMail()
	{
		var password = CreatePassword(mailBox: "Test@gmail.com", password: "Passw");

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNotNull("Transaction is created", GetPasswordTransaction(password));

		password.GP_MailBoxID = "";
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNull("No password: Transaction is deleted", GetPasswordTransaction(password));
	}

	public void TestTransactionDeleted_OnPasswordDeleted()
	{
		var password = CreatePassword(mailBox: "Test@gmail.com", password: "Password");

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		var transaction = GetPasswordTransaction(password);
		AssertNotNull("Transaction is created", transaction);
		var transactionPK = password.PK;

		password.Delete();
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNull("No password: Transaction is deleted", GetPasswordTransaction(transactionPK));
	}

	public void TestTransactionDeleted_OnStaffDeleted()
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: true, mailBox: "Test@gmail.com", password: "Password");
		var password = GlbStaffWrapper.Get(glbStaff).GlbExternalPassword;
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		var transaction = GetPasswordTransaction(password);
		AssertNotNull("Transaction is created", transaction);
		var transactionPK = password.PK;

		glbStaff.Delete();
		Factory.Save();

		AssertNoExceptionThrown(nameof(RunLogWalkerCycleForTest), RunLogWalkerCycleForTest);
		AssertNull("No password: Transaction is deleted", GetPasswordTransaction(transactionPK));
	}

	GlbExternalPassword CreatePassword(string mailBox, string password)
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: true, mailBox, password);
		var result = GlbStaffWrapper.Get(glbStaff).GlbExternalPassword;
		Factory.Save();
		return result;
	}

	CusPollingTransaction GetPasswordTransaction(GlbExternalPassword password)
		=> Factory.LoadTop1<CusPollingTransaction>(
			new ZDBOnlyQuery(typeof(CusPollingTransaction)) { OrderBy = CusPollingTransactionSchema.CPT_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCodes.PLCustoms)
			.AddToFilter(CusPollingTransactionSchema.CPT_ParentID, password.PK));

	CusPollingTransaction GetPasswordTransaction(ZGuid transactionPK)
		=> Factory.LoadTop1<CusPollingTransaction>(new ZDBOnlyQuery(typeof(CusPollingTransaction)).AddToFilter(CusPollingTransactionSchema.PK, transactionPK));
}
