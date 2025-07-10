using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPutawayJob))]
	class WhsPutawayJobTest : WhsBusinessObjectTestCase
	{
		public void TestLines_AddNew()
		{
			var job = Factory.New<WhsPutawayJob>();
			var line = job.Lines.AddNew();
			var lineInFactory =
				Factory.LoadTop1<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, job.PK));
			AssertEquals(line.PK, lineInFactory.PK);
		}

		public void TestLines_Sort()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation,
				"PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation,
				"PLT_2", 20m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation,
				"PLT_3", 30m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_3", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			AssertArrayEqualsByElements(new[] { "PLT_1", "PLT_2", "PLT_3" },
				putawayJob.Lines.Select(l => l.WPL_PalletID.ToString()).ToArray());

			Helper.Factory.Save();
			AssertArrayEqualsByElements(new[] { "PLT_1", "PLT_2", "PLT_3" },
				putawayJob.Lines.Select(l => l.WPL_PalletID.ToString()).ToArray());

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var putawayJobInFactory2 = factory2.Load<WhsPutawayJob>(putawayJob.PK);
			AssertArrayEqualsByElements(new[] { "PLT_1", "PLT_2", "PLT_3" },
				putawayJobInFactory2.Lines.Select(l => l.WPL_PalletID.ToString()).ToArray());
		}

		#region IsFinalised

		public void TestIsFinalised()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			helper.Factory.Save();

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff);
			AssertEquals("Not finalised", false, putawayJob.IsFinalised);

			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("IsFinalised == true", true, putawayJob.IsFinalised);

			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.Empty;
			AssertEquals("IsFinalised == false", false, putawayJob.IsFinalised);
		}

		#endregion

		#region IWhsLogEventParent

		[TestDate(2022, 8, 24, 13, 56, 7)]
		public void TestEventFreeTextReference()
		{
			var putawayJob = (WhsPutawayJob)GetNewBusinessObject();
			putawayJob.WPJ_SystemCreateTimeUtc = DateTime.UtcNow;

			var logParent = (IWhsLogEventParent)putawayJob;
			var expectedReference =
				$"{putawayJob.Warehouse.WW_WarehouseCode} {putawayJob.User.GS_Code} {putawayJob.WPJ_SystemCreateTimeUtc.ToSmallDateTime()}";
			AssertEquals((ZString)expectedReference, logParent.EventFreeTextReference);
		}

		public void TestEventReferenceParameterType()
		{
			var logParent = (IWhsLogEventParent)GetNewBusinessObject();
			AssertEquals(Constants.EventReferenceParameterTypes.PutawayJob, logParent.EventReferenceParameterType);
		}

		public void TestIWhsLogEventParentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var putawayjob = (WhsPutawayJob)GetNewBusinessObject();
			putawayjob.WPJ_WW_Warehouse = data.Whs1.PK;
			IWhsLogEventParent logParent = putawayjob;
			AssertEquals(data.Whs1, logParent.Warehouse);
		}

		#endregion

		#region Test Triggers

		public void TestTriggerPreventsFinalizingPutawayJobWithUnfinalizedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = false;
			Factory.Save();

			putawayJob.WPJ_FinalizedTimeUtc = DateTime.Now;
			var exception = AssertExceptionThrown<ZConcurrencyCheckFailureException>("Trigger should prevent save.", () => Factory.Save());
			Assert(exception.Message.Contains("Attempt to save a Finalized Putaway Job with an unfinalized Putaway Line."));
		}

		public void TestTriggerFinalizingPutawayJobWithFinalizedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.Empty;
			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = true;
			Helper.Factory.Save();

			putawayJob.WPJ_FinalizedTimeUtc = DateTime.Now;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestTriggerUnfinalizingPutawayJobWithFinalizedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			putawayJob.WPJ_FinalizedTimeUtc = DateTime.Now;
			var line = Helper.CreateWhsPutawayLine(putawayJob, "PLT_1", true);
			line.WPL_IsFinalized = true;
			Helper.Factory.Save();

			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.Empty;
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() =>
			Helper.CreateWhsPutawayJob(Helper.CreateWarehouse("TST"), Helper.CreateGlbStaff("ST1", "Staff1"));

		#endregion
	}
}
