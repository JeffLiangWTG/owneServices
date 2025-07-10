using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SingleActionPerTransactionExtentionsTest : TestCaseWithFactory
	{
		public void TestRunSingleActionPerTransaction()
		{
			var actionID = "do this once";
			var bizo1 = Factory.NewWithValidTestData<DummyLogged>();
			var bizo2 = Factory.NewWithValidTestData<DummyLogged>();
			int actionRunCounter = 0;
			Action action = () => actionRunCounter++;

			bizo1.RunSingleActionPerTransaction(actionID, action);
			bizo1.RunSingleActionPerTransaction(actionID, action);
			bizo1.RunSingleActionPerTransaction(actionID, action);
			AssertEquals("Action should be run only once.", 1, actionRunCounter);
			bizo2.RunSingleActionPerTransaction(actionID, action);
			bizo2.RunSingleActionPerTransaction(actionID, action);
			bizo2.RunSingleActionPerTransaction(actionID, action);
			AssertEquals("Action should be run only once.", 2, actionRunCounter);

			Factory.Save();

			bizo1.RunSingleActionPerTransaction(actionID, action);
			bizo1.RunSingleActionPerTransaction(actionID, action);
			bizo1.RunSingleActionPerTransaction(actionID, action);
			AssertEquals("Action should be run only once.", 3, actionRunCounter);
			bizo2.RunSingleActionPerTransaction(actionID, action);
			bizo2.RunSingleActionPerTransaction(actionID, action);
			bizo2.RunSingleActionPerTransaction(actionID, action);
			AssertEquals("Action should be run only once.", 4, actionRunCounter);
		}

		public void TestRunSingleActionPerTransactionWithUnsuccessfulSave()
		{
			var actionID = "do this once";
			var bizo1 = Factory.NewWithValidTestData<DummyLogged>();
			int actionRunCounter = 0;
			Action action = () => actionRunCounter++;
			bizo1.RunSingleActionPerTransaction(actionID, action);

			Factory.Saving += Factory_Saving;
			try
			{
				Factory.Save();
				Fail("Exception must be thrown.");
			}
			catch (NotImplementedException)
			{
			}

			AssertEquals("Action should be run only once.", 1, actionRunCounter);

			bizo1.RunSingleActionPerTransaction(actionID, action);
			AssertEquals("Action should be run only once.", 1, actionRunCounter);
		}

		public void TestAddNewSinglePerTransaction()
		{
			var expectedEventType = Events.BillingJobEdit;
			var bizo1 = Factory.NewWithValidTestData<DummyLogged>();
			var bizo2 = Factory.NewWithValidTestData<DummyLogged>();
			bizo1.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo1.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo1.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo2.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo2.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo2.Logs.AddNewSinglePerTransaction(expectedEventType);
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, bizo1.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, expectedEventType.Code);
			var logs1 = Factory.Load<StmALog>(filter);
			AssertEquals("Event should be created only once.", 1, logs1.Length);

			var filter2 = new ZQuery(StmALogSchema.SL_Parent, bizo2.PK);
			filter2.AddToFilter(StmALogSchema.SL_SE_NKEvent, expectedEventType.Code);
			var logs2 = Factory.Load<StmALog>(filter);
			AssertEquals("Event should be created only once.", 1, logs2.Length);

			bizo1.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo1.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo1.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo2.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo2.Logs.AddNewSinglePerTransaction(expectedEventType);
			bizo2.Logs.AddNewSinglePerTransaction(expectedEventType);
			Factory.Save();
			logs1 = Factory.Load<StmALog>(filter);
			AssertEquals("In next transaction event should be created again and only once.", 2, logs1.Length);
			logs2 = Factory.Load<StmALog>(filter);
			AssertEquals("In next transaction event should be created again and only once.", 2, logs2.Length);
		}

		public void TestAddNewSinglePerTransactionWithUnsuccessfulSave()
		{
			var expectedEventType = Events.BillingJobEdit;
			var bizo = Factory.NewWithValidTestData<DummyLogged>();
			bizo.Logs.AddNewSinglePerTransaction(expectedEventType);
			Factory.Saving += Factory_Saving;
			try
			{
				Factory.Save();
				Fail("Exception must be thrown.");
			}
			catch (NotImplementedException)
			{
			}

			var filter = new ZQuery(StmALogSchema.SL_Parent, bizo.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, expectedEventType.Code);
			var logs = Factory.Load<StmALog>(filter);
			AssertEquals("Event should be created.", 1, logs.Length);

			bizo.Logs.AddNewSinglePerTransaction(expectedEventType);
			logs = Factory.Load<StmALog>(filter);
			AssertEquals("Event should be created only once after failed saving.", 1, logs.Length);
		}

		public void TestAddNewSinglePerTransactionAcceptsReference()
		{
			var expectedEventType = Events.BillingJobEdit;
			var bizo = Factory.NewWithValidTestData<DummyLogged>();
			bizo.Logs.AddNewSinglePerTransaction(expectedEventType, "Reference 1");
			bizo.Logs.AddNewSinglePerTransaction(expectedEventType, "Reference 2");
			bizo.Logs.AddNewSinglePerTransaction(expectedEventType);
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, bizo.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, expectedEventType.Code);
			var logs = Factory.Load<StmALog>(filter);
			AssertEquals("Event should be created only once.", 1, logs.Length);
			AssertEquals("", "Reference 1", logs[0].SL_Reference);

			var longReferenceBuilder = new StringBuilder(StmALogSchema.SL_Reference.MaxLength + 1);
			for (int i = 0; i <= StmALogSchema.SL_Reference.MaxLength; i++)
			{
				longReferenceBuilder.Append("A");
			}
			bizo.Logs.AddNewSinglePerTransaction(expectedEventType, longReferenceBuilder.ToString());
			try
			{
				Factory.Save();
			}
			catch (Exception ex)
			{
				Fail(string.Format("Expect no exception on saving as we truncate Reference to its MaxLength but was {0} with message: {1}", ex.GetType().FullName, ex.Message));
			}

			logs = Factory.Load<StmALog>(filter);
			AssertEquals("In next transaction event should be created.", 2, logs.Length);
			Assert("Long reference should be truncated to maximum length of SL_Reference", logs.Any(x => x.SL_Reference.Length == StmALogSchema.SL_Reference.MaxLength));
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}
	}
}
