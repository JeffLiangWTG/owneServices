using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusPollingTransactionMessageSenderTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => new CusPollingTransactionMessageSender(factory: null));
		AssertNoExceptionThrown("All ok", () => new CusPollingTransactionMessageSender(factory: Factory));
	});

	public void TestConstructorNoPolishTimeZone()
	{
		var polandUNLOCO = new RefUNLOCO.Loader(Factory).Load(Constants.PolishTimeZoneUnloco);
		polandUNLOCO.TimeZoneSet.Delete();
		AssertExceptionThrown<InvalidOperationException>("No poland time zone", "Error getting the Polish time zone!", () => new CusPollingTransactionMessageSender(factory: Factory));
	}

	public void TestConstructorNoPolandUNLOCO()
	{
		var polandUNLOCO = new RefUNLOCO.Loader(Factory).Load(Constants.PolishTimeZoneUnloco);
		polandUNLOCO.Delete();
		AssertExceptionThrown<InvalidOperationException>("No poland UNLOCO", "Error getting the Polish time zone!", () => new CusPollingTransactionMessageSender(factory: Factory));
	}

	[TestDate(year: 2024, month: 2, day: 12, hour: 8, minute: 30, second: 0)]
	public void TestSendInContext_PLC()
	{
		var utcNow = ZDateTime.UtcNow;
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var transaction = Factory.CreatePollingTransactionForStaff(staff);
		var statusTimeUtc = utcNow.AddMinutes(-10);
		transaction.CPT_StatusTimeUtc = statusTimeUtc;
		transaction.CPT_EarliestTimeOfNextAttemptUtc = statusTimeUtc.AddMinutes(20);
		Factory.Save();

		EDIMessage sentMessage = null;
		using (DisposableEnvironment.ForBranch(staff.GS_GB_HomeBranch.ToGuid()))
		{
			AssertNoExceptionThrown("SendInContext call", () =>
				sentMessage = cusPollingTransactionMessageSender.SendInContext(transaction));
		}

		AssertNotNull("SendInContext result", sentMessage);

		CombineAssertions(()
			=> CusPollingTransactionTestHelper.AssertMessageCreatedForCusPollingTransaction("SendInContext",
				transaction,
				sentMessage,
				staff.GS_GB_HomeBranch,
				expectedDateOdUTC: new DateTime(2024, 02, 12, 08, 20, 00),
				expectedDateDoUTC: new DateTime(2024, 02, 12, 08, 45, 00)));
	}

	protected override void SetUp()
	{
		cusPollingTransactionMessageSender = new CusPollingTransactionMessageSender(Factory);
	}

	CusPollingTransactionMessageSender cusPollingTransactionMessageSender;
}
