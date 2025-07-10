using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SingleActionPerTransactionOnDifferentLevelsTest : TestCaseWithFactory
	{
		public void TestRunSingleActionPerTransactionOnDifferentLevels()
		{
			var singleAction = new SingleActionPerTransactionOnDifferentLevels() as ISingleActionPerTransactionOnDifferentLevels;
			var actionID = "Test Action ID 1";
			var bizo1 = Factory.NewWithValidTestData<DummyLogged>();
			var bizo2 = Factory.NewWithValidTestData<DummyLogged>();
			int actionRunCounter = 0;
			Action action = () => actionRunCounter++;

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			AssertEquals("Action Run", 2, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			AssertEquals(4, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 2);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 2);
			AssertEquals(4, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 3);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 3);
			AssertEquals(4, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 2);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 2);
			AssertEquals(6, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 2);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 2);
			AssertEquals(8, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 3);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 3);
			AssertEquals(8, actionRunCounter);

			Factory.Save();

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			AssertEquals(10, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 2);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 2);
			AssertEquals(10, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 3);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 3);
			AssertEquals(10, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 2);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 2);
			AssertEquals(12, actionRunCounter);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 3);
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo2, actionID, action, 3);
			AssertEquals(12, actionRunCounter);
		}

		public void TestRunSingleActionPerTransactionWithUnsuccessfulSave()
		{
			var singleAction = new SingleActionPerTransactionOnDifferentLevels() as ISingleActionPerTransactionOnDifferentLevels;
			var actionID = "do this once";
			var bizo1 = Factory.NewWithValidTestData<DummyLogged>();
			int actionRunCounter = 0;
			Action action = () => actionRunCounter++;
			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);

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

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			AssertEquals("Action should be run only once.", 2, actionRunCounter);

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 2);
			AssertEquals("Action should be run only once.", 2, actionRunCounter);

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 3);
			AssertEquals("Action should be run only once.", 2, actionRunCounter);

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			AssertEquals("Action should be run only once.", 3, actionRunCounter);

			singleAction.RunSingleActionPerTransactionOnDifferentLevels(bizo1, actionID, action, 1);
			AssertEquals("Action should be run only once.", 4, actionRunCounter);
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}
	}
}
