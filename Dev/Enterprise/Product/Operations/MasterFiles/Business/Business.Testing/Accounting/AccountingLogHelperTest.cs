using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccountingLogHelperTest : TestCaseWithFactory
	{
		public void TestAddLogOnEdited()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals(0, logs.Count);

			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.Z0_Description = "Test Add";
			AssertEquals("Percondition", false, dummy.IsInDatabase);
			AssertEquals("Percondition", false, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == "Test Add"));
			AccountingLogHelper.AddLog(chargeCode, dummy);
			logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("A log added", 1, logs.Count);
			AssertEquals(true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == "Test Add"));

			Factory.Save();
			AssertEquals("Percondition", true, dummy.IsInDatabase);
			AssertEquals("Percondition", false, dummy.HasChanges);
			AccountingLogHelper.AddLog(chargeCode, dummy);
			logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("No new log added", 1, logs.Count);

			dummy.Z0_Description = "Test Edit";
			AssertEquals("Percondition", true, dummy.IsInDatabase);
			AssertEquals("Percondition", true, dummy.HasChanges);
			AssertEquals("Percondition", false, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == "Test Edit"));
			AccountingLogHelper.AddLog(chargeCode, dummy);
			logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("A log added", 2, logs.Count);
			AssertEquals(true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == "Test Edit"));

			AssertNoExceptionThrown(() => { AccountingLogHelper.AddLog(null, dummy); });
		}

		public void TestAddLogOnRemoved()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals(0, logs.Count);

			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.Z0_Description = "Test Delete";
			dummy.Delete();
			AssertEquals("Percondition", false, dummy.IsInDatabase);
			AssertEquals("Percondition", true, dummy.IsDeleted);
			AccountingLogHelper.AddLog(chargeCode, dummy, true);
			logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("No new log added", 0, logs.Count);

			var dummy2 = Factory.New<DummyBusinessObjectLogging>();
			dummy2.Z0_Description = "Test Delete";
			Factory.Save();
			AssertEquals("Percondition", true, dummy2.IsInDatabase);
			AssertEquals("Percondition", false, dummy2.IsDeleted);
			AssertEquals("Percondition", false, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == "Test Delete"));
			AccountingLogHelper.AddLog(chargeCode, dummy2, true);

			AssertEquals("A log added", 1, logs.Count);
			AssertEquals(true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == "Test Delete"));

			dummy2.Delete();
			AssertEquals("Percondition", true, dummy2.IsInDatabase);
			AssertEquals("Percondition", true, dummy2.IsDeleted);
			AccountingLogHelper.AddLog(chargeCode, dummy2, true);
			AssertEquals("No new log added", 1, logs.Count);

			AssertNoExceptionThrown(() => { AccountingLogHelper.AddLog(null, dummy2); });
		}

		public void TestAddLogOnCreated()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var logs = chargeCode.Logs.GetAllLogs();

			var dummy = Factory.New<DummyBusinessObjectLogging>();
			var logTextMessage = "Object not in database";
			dummy.Z0_Description = logTextMessage;

			AssertEquals("Percondition IsInDatabase", false, dummy.IsInDatabase);
			AssertEquals("Percondition HasChanges", true, dummy.HasChanges);
			AssertEquals("Percondition IsDeleted", false, dummy.IsDeleted);
			AssertEquals("Percondition", false, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == logTextMessage));

			AccountingLogHelper.AddLog(chargeCode, dummy);
			logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("Expecting a new Log entry", 1, logs.Count);
			AssertEquals(true, logs.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference == logTextMessage));
		}

		public void TestAddLogWithNoChanges()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.Z0_Description = "Test Delete";
			Factory.Save();

			AssertEquals("Percondition IsInDatabase", true, dummy.IsInDatabase);
			AssertEquals("Percondition HasChanges", false, dummy.HasChanges);
			AssertEquals("Percondition IsDeleted", false, dummy.IsDeleted);

			AccountingLogHelper.AddLog(chargeCode, dummy);
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("Expecting no new Log", 0, logs.Count);
		}

		public void TestAddLogWithEmptyText()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.Z0_Description = " ";

			AssertEquals("Percondition IsInDatabase", false, dummy.IsInDatabase);
			AssertEquals("Percondition HasChanges", true, dummy.HasChanges);
			AssertEquals("Percondition IsDeleted", false, dummy.IsDeleted);

			AccountingLogHelper.AddLog(chargeCode, dummy);
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("Expecting no new Log for Empty Reference String", 0, logs.Count);
		}

		public void TestAddLogWithValidText()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.Z0_Description = "Valid Log Text";

			AssertEquals("Percondition IsInDatabase", false, dummy.IsInDatabase);
			AssertEquals("Percondition HasChanges", true, dummy.HasChanges);
			AssertEquals("Percondition IsDeleted", false, dummy.IsDeleted);

			AccountingLogHelper.AddLog(chargeCode, dummy);
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("Expecting a new Log", 1, logs.Count);
		}

		public void TestAddLogWithNullReference()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.Z0_Description = "BusinessObject as Null";

			AssertEquals("Percondition IsInDatabase", false, dummy.IsInDatabase);
			AssertEquals("Percondition HasChanges", true, dummy.HasChanges);
			AssertEquals("Percondition IsDeleted", false, dummy.IsDeleted);

			AccountingLogHelper.AddLog(chargeCode, (DummyBusinessObjectLogging)null);
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("Expecting no new Log for Null Object", 0, logs.Count);
		}

		public void TestAddLogAddMultipleLogsForMultilineReference()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var dummy = Factory.New<DummyBusinessObjectLogging>();
			dummy.LogReference = "Test string1.\r\nTest string2.\r\nTest string3.";

			AssertNotNull("Percondition LogReference", dummy.LogReference);

			AccountingLogHelper.AddLog(chargeCode, dummy);
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals("Expecting 3 Logs", 3, logs.Count);
			AssertEquals("Test string1.", logs[0].SL_Reference);
			AssertEquals("Test string2.", logs[1].SL_Reference);
			AssertEquals("Test string3.", logs[2].SL_Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingLogHelper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccountingLogHelper();
		}
		IAccountingLogHelper AccountingLogHelper;

		sealed class DummyBusinessObjectLogging : DummyBusinessObject, IBusinessObjectLogging
		{
			public DummyBusinessObjectLogging(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string LogReference { get; set; }

			public ZString GetLogReference() => LogReference ?? Z0_Description;
		}
	}
}
