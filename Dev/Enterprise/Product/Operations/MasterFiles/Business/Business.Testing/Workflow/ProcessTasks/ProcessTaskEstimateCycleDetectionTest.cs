using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessTaskEstimateCycleDetectionTest : TestCaseWithFactory
	{
		[TestDate(2018, 7, 3)]
		public void TestDontCheckForCyclesWhenTheEstimateDoesntChange()
		{
			Globals.IsUserInteractive = false;
			WorkflowDataRegistry.Instance.EstimateCalculationCycleLimitBeforeDisable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent05Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();

			var time = ZDateTime.Now;
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(1);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();

			var initialHitCount = Factory.GetTableHitCount(StmALogSchema.Constants.TableName);
			milestone.DefaultEstimateIfRequired();
			AssertTableHitCount(initialHitCount, StmALogSchema.Constants.TableName);
		}

		[TestDate(2018, 7, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestDontCheckForCyclesWhenSuppressCreatingWorkflowFromTemplate()
		{
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.New<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;

			dummy.Z0_Date = ZDateTime.Now;
			var initialHitCount = Factory.GetTableHitCount(StmALogSchema.Constants.TableName);

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				milestone.DefaultEstimateIfRequired();
				Factory.Save();
			}

			AssertTableHitCount(initialHitCount, StmALogSchema.Constants.TableName);
			AssertEquals(ZDateTimeOffset.Empty, milestone.P9_ScheduledDateForBinding);
			AssertEquals(0, milestone.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.CalculatedEstimateCode));
		}

		[TestDate(2018, 7, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestEstimationCreatesEvents()
		{
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.New<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;

			dummy.Z0_Date = ZDateTime.Now;
			milestone.DefaultEstimateIfRequired();

			Factory.Save();
			AssertEquals(ZDateTimeOffset.Now.AddDays(1), milestone.P9_ScheduledDateForBinding);
			AssertEquals(1, milestone.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.CalculatedEstimateCode));
		}

		[TestDate(2018, 7, 3)]
		public void TestCeeEventOnlyCreatedWhenP9_ScheduledDateIsChanged()
		{
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.New<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;

			dummy.Z0_Date = ZDateTime.Now;
			using (milestone.SuspendedScheduledDateChange())
			{
				milestone.DefaultEstimateIfRequired();
				Factory.Save();
			}

			AssertEquals(0, milestone.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.CalculatedEstimateCode));
		}

		[TestDate(2018, 7, 3)]
		public void TestCeeEvent_TracksTheLWKServiceTask()
		{
			var hours = 24;
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone1 = dummy.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Milestone1";
			milestone1.P9_Sequence = 1;
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;

			var milestone2 = dummy.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "Milestone2";
			milestone2.P9_Sequence = 2;
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;
			milestone2.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(hours);
			milestone2.P9_RecalculateScheduledDate = true;
			milestone2.P9_EstimatedDefaultFromPredecessor = 1;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Scoopy";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent04Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Sequence>\"==\"1\").P9_ScheduledDate>";
			action.PQ_FieldValue = "<Now>";

			dummy.Logs.AddNew(Events.CustomisableEvent04);

			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			dummy.Z0_Date = ZDateTime.Now;
			var query = new ZQuery(StmALogSchema.SL_Parent, milestone2.PK)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CalculatedEstimateCode);
			var evt = Factory.Load<StmALog>(query).Single();

			AssertContains("|SRV=LWK", evt.SL_Reference);
			AssertContains("|SUB=WorkflowEventTrigger", evt.SL_Reference);
		}

		public void TestChangingTimezonesIsNotALoop()
		{
			var bneBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			var sinBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SIN"));
			var sinUtcOffset = new RefUNLOCO.Loader(Factory).Load("SGSIN").StandardZoneUTCOffset;
			var bneUtcOffset = new RefUNLOCO.Loader(Factory).Load("AUBNE").StandardZoneUTCOffset;
			AssertNotEquals("Precondition, SIN and BNE should have a different UTC offset", sinUtcOffset, bneUtcOffset);

			ZGuid taskPK = ZGuid.Empty;
			ZGuid shipmentPK = ZGuid.Empty;
			var utcDate = ZDateTime.UtcNow.AddDays(1).ToSmallDateTimeFloor();
			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, bneBranch.PK.ToGuid(), Env.CurrentDepartment.PK)))
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				shipment.JS_E_ARV = Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime());
				shipmentPK = shipment.PK;

				var task = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
				task.P9_Type = "UDF";
				task.P9_Description = "TASK";
				task.P9_EstimatedDefaultedFrom = "ARR";
				task.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(0);
				task.P9_RecalculateScheduledDate = true;
				taskPK = task.PK;

				Factory.Save();
				AssertEquals("Use current company to calculate date when arrival destination is null", utcDate, task.P9_ScheduledDateUtc);
				AssertEquals("Use current company to calculate date when arrival destination is null", Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime()), task.P9_ScheduledDate);
				AssertEquals("Use current company to calculate date when arrival destination is null", Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime()), task.P9_ScheduledDateLocalForBinding);

				shipment.JS_RL_NKDestination = sinBranch.HomePort.Code;
				utcDate = Env.Time.GetUtcFromUnlocoTime(sinBranch.HomePort.Code, shipment.JS_E_ARV.ToDateTime());
				Factory.Save();
				AssertEquals("Schedule date should be set using sinBranch timezone", utcDate, task.P9_ScheduledDateUtc);
				AssertEquals("Schedule date should be set using sinBranch timezone", Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime()), task.P9_ScheduledDateLocalForBinding);
			}

			var query = new ZQuery(StmALogSchema.SL_Parent, taskPK)
	.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CalculatedEstimateCode);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("Estimate calculated once when arrival destination is null and once when it is set", 2, logs.Length);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, sinBranch.PK.ToGuid(), Env.CurrentDepartment.PK)))
			{
				var newFactory = new BusinessObjectFactory();
				var shipment = newFactory.Load<Forwarding.IForwardingShipment>(shipmentPK);
				var task = newFactory.Load<ProcessTask>(taskPK);
				task.P9_Description = "NEW DESCRIPTION";
				newFactory.Save();

				AssertEquals("In a different timezone, UTC should be a same", utcDate, task.P9_ScheduledDateUtc);
				AssertEquals(Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime()), task.P9_ScheduledDate);
			}
		}

		[TestDate(2018, 7, 3)]
		public void TestSmallDateTimeBiggedness()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.MaxSmallDateTimeValue.AddHours(1);
			milestone.DefaultEstimateIfRequired();

			AssertEquals("If the source date isn't a valid small date time, then do not do anything.", ZDateTimeOffset.Empty, milestone.P9_ScheduledDateForBinding);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoop_ExternalSource()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.Now;
			var now = ZDateTimeOffset.Now;

			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.AddSeconds(1);
				milestone.P9_ScheduledDateForBinding = now.AddDays(2);
				Factory.Save();
				TestDateAttribute.AddSeconds(1);
				milestone.DefaultEstimateIfRequired();
				Factory.Save();
			}

			AssertEquals("Eventually give up on recalculating based on the estimate.", now.AddDays(2), milestone.P9_ScheduledDateForBinding);
			Assert("disable recalculate due to loop", !milestone.P9_RecalculateScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoop_ExternalSource_RegistryUpdated()
		{
			Globals.IsUserInteractive = false;
			WorkflowDataRegistry.Instance.EstimateCalculationCycleLimitBeforeDisable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.Now;
			var now = ZDateTimeOffset.Now;

			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.AddSeconds(1);
				milestone.P9_ScheduledDateForBinding = now.AddDays(2);
				Factory.Save();
				TestDateAttribute.AddSeconds(1);
				milestone.DefaultEstimateIfRequired();
				Factory.Save();
			}

			Assert("Not disabled because the loop wasn't long enough", milestone.P9_RecalculateScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestLoop()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code; // Note this is the same event code as the one that Dummy generates
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.Now;

			var now = ZDateTimeOffset.Now;
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.AddSeconds(1);
				Factory.Save();
				milestone.DefaultEstimateIfRequired();
			}

			CombineAssertions(() =>
			{
				AssertEquals("Should go into the future, but should not loop a second time", now.ToZDateTime().AddDays(1), dummy.Z0_Date);
				AssertEquals("Should go into the future, but should not loop a second time", now.AddDays(1), milestone.P9_ScheduledDateForBinding);
				Assert("disable recalculate due to loop", !milestone.P9_RecalculateScheduledDate);
			});
		}

		[TestDate(2020, 1, 1)]
		public void TestSmallDateValidation()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();

			//Invalid Smalldate, Doesn't modify P9_ScheduledDate (min)
			dummy.Z0_Date = ZDateTime.MinSmallDateTimeValue.AddYears(-1);
			Factory.Save();
			milestone.P9_ScheduledDate = ZDateTime.Empty;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			AssertEquals(ZDateTime.Empty, milestone.P9_ScheduledDate);

			//Invalid Smalldate, Doesn't modify P9_ScheduledDate (max)
			dummy.Z0_Date = ZDateTime.MaxSmallDateTimeValue.AddYears(1);
			Factory.Save();
			milestone.P9_ScheduledDate = ZDateTime.Empty;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			AssertEquals(ZDateTime.Empty, milestone.P9_ScheduledDate);

			//Valid Smalldate, Modifies P9_ScheduledDate
			dummy.Z0_Date = new ZDateTime(2020, 01, 01);
			Factory.Save();
			milestone.P9_ScheduledDate = ZDateTime.Empty;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			AssertEquals(dummy.Z0_Date, milestone.P9_ScheduledDate);

			//Valid Smalldate, Doesn't change (older than 10 years)
			dummy.Z0_Date = new ZDateTime(1900, 01, 01);
			Factory.Save();
			milestone.P9_ScheduledDate = ZDateTime.Empty;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			AssertEquals(ZDateTime.Empty, milestone.P9_ScheduledDate);

			//Valid Smalldate, Doesn't change (newer than 5 years)
			dummy.Z0_Date = new ZDateTime(2030, 01, 01);
			Factory.Save();
			milestone.P9_ScheduledDate = ZDateTime.Empty;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			AssertEquals(ZDateTime.Empty, milestone.P9_ScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoop_ABABA()
		{
			Globals.IsUserInteractive = false;
			WorkflowDataRegistry.Instance.EstimateCalculationCycleLimitBeforeDisable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent05Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();

			var time = ZDateTime.Now;
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(1);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(1);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(1);
			Assert(milestone.P9_RecalculateScheduledDate);
			milestone.DefaultEstimateIfRequired();

			Assert("disable recalculate due to loop", !milestone.P9_RecalculateScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoop_ABA()
		{
			Globals.IsUserInteractive = false;
			WorkflowDataRegistry.Instance.EstimateCalculationCycleLimitBeforeDisable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent05Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();

			var time = ZDateTime.Now;
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(1);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(1);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();

			Assert("Since there were 3 changes of exactly 1 hour it disables itself.", !milestone.P9_RecalculateScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoop_SmallLoopCheck()
		{
			Globals.IsUserInteractive = false;
			WorkflowDataRegistry.Instance.EstimateCalculationCycleLimitBeforeDisable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent05Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();

			var time = ZDateTime.Now;
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(4);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time;
			milestone.DefaultEstimateIfRequired();
			Factory.Save();
			TestDateAttribute.AddSeconds(1);
			dummy.Z0_Date = time.AddHours(12);
			milestone.DefaultEstimateIfRequired();
			Factory.Save();

			Assert("No pattern, so no cancellation", milestone.P9_RecalculateScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoop_Task()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var task = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			task.P9_ParentID = dummy.PK;
			task.P9_ParentTableCode = "Z0";
			task.P9_Type = "UDF";
			task.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code; // Note this is the same event code as the one that Dummy generates
			task.P9_EstimatedDefaultedFrom = "DAT";
			task.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			task.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.Now;

			var now = ZDateTimeOffset.Now;
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.AddSeconds(1);
				Factory.Save();
				task.DefaultEstimateIfRequired();

				if (dummy.Z0_Date == now.ToZDateTime())
				{
					dummy.Z0_Date = now.ToZDateTime().AddHours(24);
				}
				else
				{
					dummy.Z0_Date = now.ToZDateTime();
				}
			}
			Assert("disable recalculate due to loop", !task.P9_RecalculateScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestLoopTimeLimit()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var task = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			task.P9_ParentID = dummy.PK;
			task.P9_ParentTableCode = "Z0";
			task.P9_Type = "UDF";
			task.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code; // Note this is the same event code as the one that Dummy generates
			task.P9_EstimatedDefaultedFrom = "DAT";
			task.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			task.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.Now;

			var now = ZDateTimeOffset.Now;
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.AddMinutes(20);
				Factory.Save();
				task.DefaultEstimateIfRequired();

				if (dummy.Z0_Date == now.ToZDateTime())
				{
					dummy.Z0_Date = now.ToZDateTime().AddHours(24);
				}
				else
				{
					dummy.Z0_Date = now.ToZDateTime();
				}
			}
			Assert("Only considered as a loop if there are > 5 changes in a 60 minutes period", task.P9_RecalculateScheduledDate);
		}

		[TestDate(2010, 1, 1)]
		public void TestDateTooBig()
		{
			Globals.IsUserInteractive = false;
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code; // Some random event code. We aren't creating a loop here.
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(48);
			milestone.P9_RecalculateScheduledDate = false;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.MaxSmallDateTimeValue;

			Factory.Save();
			milestone.DefaultEstimateIfRequired();
			AssertEquals(ZDateTime.Empty, milestone.P9_ScheduledDate);
		}

		[TestDate(2018, 7, 3)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCancelEvent()
		{
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("DAT", "Date thingy"));
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowThatDefaultsZ0_DateFromZ00>();
			var milestone = Factory.New<SpoofDummyTaskForLoopoMcTesto>();
			milestone.P9_ParentID = dummy.PK;
			milestone.P9_ParentTableCode = "Z0";
			milestone.P9_Type = "MIL";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent09Code;
			milestone.P9_EstimatedDefaultedFrom = "DAT";
			milestone.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone.P9_RecalculateScheduledDate = true;
			Factory.Save();
			dummy.Z0_Date = ZDateTime.Now;

			Factory.Save();
			AssertEquals("Be in the future", ZDateTimeOffset.Now.AddDays(1), milestone.P9_ScheduledDateForBinding);

			var log = dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent09Code).Single();
			log.Cancel();
			AssertEquals("Be empty", ZDateTimeOffset.Empty, milestone.P9_ScheduledDateForBinding);
			Factory.Save();

			AssertEquals("Be in the future", ZDateTimeOffset.Now.AddDays(1), milestone.P9_ScheduledDateForBinding);
			dummy.Logs.GetAllLogs().Reload(true);
			AssertNotNull(dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent09Code && !l.IsCancelled).SingleOrDefault());
		}

		[TestDate(2018, 11, 27)]
		public void TestLoopUsingMultipleMilestones_UserInteractive_1Hour()
		{
			LoopUsingMultipleMilestones(true, 1);
		}

		[TestDate(2018, 11, 27)]
		public void TestLoopUsingMultipleMilestones_UserInteractive_10Hours()
		{
			LoopUsingMultipleMilestones(true, 10);
		}

		[TestDate(2018, 11, 27)]
		public void TestLoopUsingMultipleMilestones_UserInteractive_30Hours()
		{
			LoopUsingMultipleMilestones(true, 30);
		}

		[TestDate(2018, 11, 27)]
		public void TestLoopUsingMultipleMilestones_NonUserInteractive_1Hour()
		{
			LoopUsingMultipleMilestones(false, 1);
		}

		[TestDate(2018, 11, 27)]
		public void TestLoopUsingMultipleMilestones_NonUserInteractive_10Hours()
		{
			LoopUsingMultipleMilestones(false, 10);
		}

		[TestDate(2018, 11, 27)]
		public void TestLoopUsingMultipleMilestones_NonUserInteractive_30Hours()
		{
			LoopUsingMultipleMilestones(false, 30);
		}

		[TestUtcOffset(10, 0, 0)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public void LoopUsingMultipleMilestones(bool userInteractive, int hours)
		{
			Globals.IsUserInteractive = userInteractive;
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone1 = Factory.New<DummyProcessTask>();
			milestone1.P9_Description = "Milestone1";
			milestone1.P9_ParentID = dummy.PK;
			milestone1.P9_Sequence = 1;
			milestone1.P9_ParentTableCode = "Z0";
			milestone1.P9_Type = "MIL";
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			milestone1.P9_EstimatedDefaultFromPredecessor = 2;
			milestone1.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(hours);
			milestone1.P9_RecalculateScheduledDate = true;
			milestone1.P9_ScheduledDate = ZDateTime.Now.AddHours(hours);

			var milestone2 = Factory.New<DummyProcessTask>();
			milestone2.P9_Description = "Milestone2";
			milestone2.P9_ParentID = dummy.PK;
			milestone2.P9_ParentTableCode = "Z0";
			milestone2.P9_Sequence = 2;
			milestone2.P9_Type = "MIL";
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;
			milestone2.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(hours);
			milestone2.P9_RecalculateScheduledDate = true;
			milestone2.P9_EstimatedDefaultFromPredecessor = 1;
			milestone2.P9_ScheduledDate = ZDateTime.Now.AddHours(hours);

			Factory.Save();
			Factory.Save();
			Factory.Save();

			var lastDateScheduledMilestone1 = milestone1.P9_ScheduledDateForBinding;
			var lastDateScheduledMilestone2 = milestone2.P9_ScheduledDateForBinding;

			Factory.Save();
			AssertEquals($"Loop not detected = {TimeSpan.FromTicks(lastDateScheduledMilestone1.Ticks - milestone1.P9_ScheduledDateForBinding.Ticks)}", lastDateScheduledMilestone1, milestone1.P9_ScheduledDateForBinding);
			AssertEquals($"Loop not detected = {TimeSpan.FromTicks(lastDateScheduledMilestone2.Ticks - milestone2.P9_ScheduledDateForBinding.Ticks)}", lastDateScheduledMilestone2, milestone2.P9_ScheduledDateForBinding);

			Assert("Recaclulate should be disabled for one of the milestones", !milestone1.P9_RecalculateScheduledDate || !milestone2.P9_RecalculateScheduledDate);
		}

		class DummyWithWorkflowThatDefaultsZ0_DateFromZ00 : DummyWithWorkflow
		{
			public DummyWithWorkflowThatDefaultsZ0_DateFromZ00(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[EventDateProperty(Events.CustomisableEvent00Code, EstimateActual.Estimate)]
			public override ZDateTime Z0_Date
			{
				get => base.Z0_Date;
				set
				{
					if (Z0_Date != value)
					{
						base.Z0_Date = value;
						Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Estimate, value.ToOffset());
					}
				}
			}
		}

		class SpoofDummyTaskForLoopoMcTesto : DummyProcessTask
		{
			public SpoofDummyTaskForLoopoMcTesto(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				var dummyWorkflowDescriptor = (DummyWorkflowDescriptor)WorkflowDescriptor;
				dummyWorkflowDescriptor.SetCustomDefaultsFromDate((defaultedFromDateProvider, defaultsFrom) =>
				{
					if (defaultsFrom == "DAT")
					{
						return (((DummyWithWorkflow)defaultedFromDateProvider.Parent).Z0_Date, null);
					}
					else
					{
						return dummyWorkflowDescriptor.GetDefaultsFromDateBase(defaultedFromDateProvider, defaultsFrom);
					}
				});
			}

			protected internal override Type ParentType => typeof(DummyWithWorkflowThatDefaultsZ0_DateFromZ00);

			protected override string WorkflowTypeCore => "DUM";
		}
	}
}
