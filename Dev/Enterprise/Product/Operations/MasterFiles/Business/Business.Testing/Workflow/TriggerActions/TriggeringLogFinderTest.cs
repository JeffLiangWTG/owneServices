using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TriggeringLogFinderTest : TestCaseWithFactory
	{
		public void TestTryFindLog_FindEstimates()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.ShouldTriggerOnEstimateEvents = true;
			dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow, isEstimate: true);
			Factory.Save();

			StmALog result;
			AssertEquals(true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, dummy, out result));
		}

		public void TestTryFindLog_DoNotFindCancelled()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow, isEstimate: false);
			Factory.Save();
			log.Cancel();
			Factory.Save();

			StmALog result;
			AssertEquals(false, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, dummy, out result));
		}

		public void TestTryFindLog_FindCancelled()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow, isEstimate: false);
			Factory.Save();
			log.Cancel();
			Factory.Save();

			StmALog result;
			AssertEquals(true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, dummy, out result, findCancelled: true));
		}

		public void TestTryFindLog_WteFallbackLogIsSet()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "TES";
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow, isEstimate: false);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var wteLogs = Factory.Load<StmALog>(query);
			AssertEquals("Trigger hasn't been fired", 1, wteLogs.Length);

			var wteDataWithEmptyTriggeringLog = new WorkflowTriggerEventData(wteLogs[0]);
			wteDataWithEmptyTriggeringLog.TriggeringLogPK = Guid.Empty;
			var queuedLog = new Mock<IQueuedLog>();
			queuedLog.Setup(m => m.SJ_EventTime).Returns(ZDateTime.Now);
			queuedLog.Setup(m => m.SJ_GS_NKUser).Returns(testUser.GS_Code);
			queuedLog.Setup(m => m.Factory).Returns(Factory);

			StmALog result;
			Assert(TriggeringLogFinder.TryFindLog(wteDataWithEmptyTriggeringLog, trigger, dummy, out result, wteFallbackLog: queuedLog.Object));
			AssertEquals("Found log was recreated from setup queuedlog data", testUser.GS_Code, result.SL_GS_NKUser);
		}

		public void TestTryFindLog()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow, isEstimate: false);
			Factory.Save();

			StmALog result;
			AssertEquals(true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, dummy, out result));
		}

		public void TestTryFindLog_RelatedBusinessObjectFromAnotherCompany()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.StatusUpdatedCode;
			Factory.Save();

			jobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			var log = jobHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First();
			AssertNotNull("Precondition: STU event created", log);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			branch.GB_GC = company.PK;
			Factory.Save();

			StmALog result;
			AssertEquals("This user is from the same company as the STU event so we should find it", true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertEquals("findLogsNotVisibleToCurrentUser=true so we should find the log", true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));
			}
		}

		public void TestTryFindLog_BusinessObjectsWithRelatedEvents_IsNull()
		{
			var dummy = Factory.New<DummyWithWorkflowForTest>();
			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			dummy.SetBusinessObjectsWithRelatedEvents(null);
			Factory.Save();

			StmALog result;
			AssertEquals("When BusinessObjectsWithRelatedEvents is null expecting false", false, TriggeringLogFinder.TryFindLog(null, trigger, dummy, out result));
			AssertEquals(null, result);
		}

		public void TestTryFindLog_BusinessObjectsWithRelatedEvents_ContainsNull()
		{
			var dummy = Factory.New<DummyWithWorkflowForTest>();
			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var child = Factory.New<DummyWithWorkflow>();
			var childLog = child.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow, isEstimate: false);

			dummy.SetBusinessObjectsWithRelatedEvents(new BusinessObject[] { null, child });
			Factory.Save();

			var wte = new WorkflowTriggerEventData(childLog, child.PK, "ABC", "ABC", "ABC", "ABC", "ABC");
			StmALog result;
			AssertEquals("BusinessObjectsWithRelatedEvents should not contain null but when it does we should not throw here", true, TriggeringLogFinder.TryFindLog(wte, trigger, dummy, out result));
			AssertEquals(childLog, result);
		}

		public void TestIsLogVisibleToCurrentUser_IsSafe()
		{
			var dummy = Factory.New<DummyWithWorkflowForTest>();
			dummy.SetBusinessObjectsWithRelatedEvents(null);
			AssertNoExceptionThrown(() => TriggeringLogFinder.IsLogVisibleToCurrentUser(dummy, ZGuid.NewZGuid()));

			dummy.SetBusinessObjectsWithRelatedEvents(new BusinessObject[] { null, null });
			AssertNoExceptionThrown(() => TriggeringLogFinder.IsLogVisibleToCurrentUser(dummy, ZGuid.NewZGuid()));
		}

		public void TestTryFindLog_RelatedBusinessObjectFromAnotherCompany_IsCancelled()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.StatusUpdatedCode;
			Factory.Save();

			jobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			var log = jobHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First();
			AssertNotNull("Precondition: STU event created", log);

			log.Cancel();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			branch.GB_GC = company.PK;
			Factory.Save();

			StmALog result;
			AssertEquals("Don't find cancelled log", false, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertEquals("Don't find cancelled log", false, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));
				AssertEquals("Don't find cancelled log", false, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));
			}
		}

		public void TestTryFindLog_RelatedBusinessObjectFromAnotherCompany_IsEstimate()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.StatusUpdatedCode;
			Factory.Save();

			jobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			var log = jobHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First();
			AssertNotNull("Precondition: STU event created", log);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			branch.GB_GC = company.PK;
			Factory.Save();

			StmALog result;
			AssertEquals("This user is from the same company as the STU event so we should find it", true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertEquals("findLogsNotVisibleToCurrentUser=true so we should find the log", true, TriggeringLogFinder.TryFindLog(GetWteData(trigger.PK.ToGuid()), trigger, (BusinessObject)shipment, out result));
			}
		}

		public void TestTryFindLog_RelatedBusinessObjectFromAnotherCompany_CanToggle()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.StatusUpdatedCode;
			Factory.Save();

			jobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			branch.GB_GC = company.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var wte = GetWteData(trigger.PK.ToGuid());
				AssertEquals(true, TriggeringLogFinder.TryFindLog(wte, trigger, (BusinessObject)shipment, out _));

				using (WorkflowDataRegistry.Instance.ShouldFindLogsOnRelatedJobsDuringLWKProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(false, TriggeringLogFinder.TryFindLog(wte, trigger, (BusinessObject)shipment, out _));
				}
			}
		}

		public WorkflowTriggerEventData GetWteData(Guid triggerPK)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, triggerPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var log = Factory.Load<StmALog>(query);

			AssertEquals("Trigger hasn't been fired", 1, log.Length);

			return new WorkflowTriggerEventData(log[0]);
		}

		class DummyWithWorkflowForTest : DummyWithWorkflow
		{
			public DummyWithWorkflowForTest(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public void SetBusinessObjectsWithRelatedEvents(BusinessObject[] objects)
			{
				BusinessObjectsWithRelatedEventsOverride.Value = objects;
				alwaysUseOverriddenValue = true;
			}

			protected override BusinessObject[] BusinessObjectsWithRelatedEvents
			{
				get
				{
					if (BusinessObjectsWithRelatedEventsOverride.IsOverriden || alwaysUseOverriddenValue)
					{
						return BusinessObjectsWithRelatedEventsOverride.Value;
					}
					else
					{
						var childrens = Factory.Load<DummyWithWorkflow>(new ZQuery(DummyBizoSchema.Z0_Guid, PK));
						return childrens.OrderBy(c => c.Z0_Number).ToArray();
					}
				}
			}

			readonly Overridable<BusinessObject[]> BusinessObjectsWithRelatedEventsOverride = new Overridable<BusinessObject[]>();
			bool alwaysUseOverriddenValue;
		}
	}
}
