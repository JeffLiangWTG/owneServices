using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPutawayLine))]
	class WhsPutawayLineTest : WhsBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var putawayJob =
				Helper.CreateWhsPutawayJob(Helper.CreateWarehouse("TST"), Helper.CreateGlbStaff("ST1", "Staff1"));
			var putawayLine = Factory.New<WhsPutawayLine>();
			putawayLine.WPL_WPJ_PutawayJob = putawayJob.PK;
			return putawayLine;
		}

		#region TestWPL_PalletID_MaxLength

		public void TestWPL_PalletID_MaxLength()
		{
			var putawayLine = Factory.New<WhsPutawayLine>();
			AssertNoExceptionThrown(() => putawayLine.WPL_PalletID = "123456789012345678901234567890");
		}

		#endregion

		#region TestFinalizeLine

		public void TestFinalizeLine_IsFinalized_IsNotPuttingAway()
		{
			var putawayLine = Factory.New<WhsPutawayLine>();
			putawayLine.WPL_IsFinalized = true;
			putawayLine.WPL_IsPuttingAway = false;

			putawayLine.FinalizeLine();

			AssertEquals(true, putawayLine.WPL_IsFinalized);
			AssertEquals(false, putawayLine.WPL_IsPuttingAway);
		}

		public void TestFinalizeLine_IsFinalized_IsPuttingAway()
		{
			var putawayLine = Factory.New<WhsPutawayLine>();
			putawayLine.WPL_IsFinalized = true;
			putawayLine.WPL_IsPuttingAway = true;

			putawayLine.FinalizeLine();

			AssertEquals(true, putawayLine.WPL_IsFinalized);
			AssertEquals(false, putawayLine.WPL_IsPuttingAway);
		}

		public void TestFinalizeLine_IsNotFinalized_IsNotPuttingAway()
		{
			var putawayLine = Factory.New<WhsPutawayLine>();
			putawayLine.WPL_IsFinalized = false;
			putawayLine.WPL_IsPuttingAway = false;

			putawayLine.FinalizeLine();

			AssertEquals(true, putawayLine.WPL_IsFinalized);
			AssertEquals(false, putawayLine.WPL_IsPuttingAway);
		}

		public void TestFinalizeLine_IsNotFinalized_IsPuttingAway()
		{
			var putawayLine = Factory.New<WhsPutawayLine>();
			putawayLine.WPL_IsFinalized = false;
			putawayLine.WPL_IsPuttingAway = true;

			putawayLine.FinalizeLine();

			AssertEquals(true, putawayLine.WPL_IsFinalized);
			AssertEquals(false, putawayLine.WPL_IsPuttingAway);
		}

		#endregion

		#region Test Triggers

		public void TestTriggerPreventsSettingUnfinalizedLineOnFinalizedPutawayJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			putawayJob.WPJ_FinalizedTimeUtc = DateTime.Now;
			Helper.Factory.Save();

			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = false;
			var exception = AssertExceptionThrown<ZSaveConcurrencyException>("Trigger should prevent save.", () => Factory.Save());
			Assert(exception.Message.Contains("Attempt to save an Unfinalized Putaway Line on a Finalized Putaway Job"));
		}

		public void TestTriggerSettingUnfinalizedLineOnUnfinalizedPutawayJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.Empty;
			Helper.Factory.Save();

			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = false;

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestTriggerSettingFinalizedLineOnFinalizedJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			putawayJob.WPJ_FinalizedTimeUtc = DateTime.Now;
			Helper.Factory.Save();

			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = true;

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestTriggerSettingFinalizedLineOnUnfinalizedJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.Empty;
			Helper.Factory.Save();

			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = false;

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion
	}
}
