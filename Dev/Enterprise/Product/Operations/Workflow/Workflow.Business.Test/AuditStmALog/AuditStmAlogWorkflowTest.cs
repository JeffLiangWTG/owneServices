using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Workflow.StmALog;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	sealed class AuditStmALogWorkflowTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			registrySetup = TurnOffAutoLogsForDummyInRegistry();
		}

		protected override void TearDown()
		{
			base.TearDown();
			registrySetup?.Dispose();
		}

		public static IDisposable TurnOffAutoLogsForDummyInRegistry()
		{
			var mockAuditLogDecider = new Mock<IAuditStmALogDecider>();
			mockAuditLogDecider.Setup(x =>
					x.AuditLogConfigNonPersistedForEventCode(It.IsAny<IBusiness>(), It.IsAny<string>()))
				.Returns(true);
			mockAuditLogDecider.Setup(x => x.IsAutoAdminBusinessObjectLoggerEnabled(It.IsAny<DummyBusinessObject>(),
					It.IsAny<EnterpriseBusinessObject.AutologState>()))
				.Returns(true);

			return ObjectFactory.Substitute(mockAuditLogDecider.Object);
		}

		public void TestCreateDeletedLog()
		{
			var dummy = Factory.New<IDummyWithWorkflow>() as IAutoAdminLogTarget;
			Factory.Save();
			((BusinessObject)dummy).Delete();
			var log = dummy.Logs.GetAllLogs().First(x => ((StmALog)x).SL_SE_NKEvent == AutoEvents.DeletedARecordInTheSystemCode);
			Assert(!log.IsInDatabase);
		}

		IDisposable registrySetup;

		public void TestDontTryRecreateNonPersistentAddedLog()
		{
			var dummy = Factory.New<IDummyWithWorkflow>();
			var trigger = dummy.AddNewTrigger();
			dummy.TurnOnAutoLogging();
			((IBaseTrigger)trigger).TriggerEventCode = AutoEvents.AddedARecordToTheSystemCode;
			((ITriggerConditions)trigger).TriggerFiredCountdown = 10;
			var dummyAutoLog = dummy as IAutoAdminLogTarget;
			var addedLog = dummyAutoLog.Logs.AddedLog;
			AssertNotNull(addedLog);
			Assert(!addedLog.IsInDatabase);
			Assert(!addedLog.IsSavedByFactory);

			Factory.Save();

			AssertNull(dummyAutoLog.Logs.AddedLog);
			AssertEquals((short)9, ((ITriggerConditions)trigger).TriggerFiredCountdown);
		}

		public void TestNonPersistentEventsArentSavedButFireWorkflow()
		{
			var dummy = Factory.New<IDummyWithWorkflow>();
			var trigger = dummy.AddNewTrigger();
			((IBaseTrigger)trigger).TriggerEventCode = AutoEvents.AddedARecordToTheSystemCode;
			var trigger2 = dummy.AddNewTrigger();
			((IBaseTrigger)trigger2).TriggerEventCode = AutoEvents.EditedARecordCode;
			Factory.Save();

			dummy.Z0_Description = "new";
			Factory.Save();

			var dummyAutoLog = dummy as IAutoAdminLogTarget;
			AssertEquals("No audit logs saved", 0, dummyAutoLog.Logs.GetAllLogs().Find(x => (x.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode || x.SL_SE_NKEvent == AutoEvents.EditedARecordCode) && x.IsInDatabase).Count());
			AssertWTECreated(trigger as IAutoAdminLogTarget);
			AssertWTECreated(trigger2 as IAutoAdminLogTarget);
		}

		public void TestNonPersistentEventsIfSaveFails()
		{
			var dummy = Factory.New<IDummyWithWorkflow>();
			dummy.TurnOnAutoLogging();
			var trigger = dummy.AddNewTrigger();
			((IBaseTrigger)trigger).TriggerEventCode = AutoEvents.AddedARecordToTheSystemCode;

			var saveAttempt = 1;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(onSavingFactory =>
			{
				if (saveAttempt == 1)
				{
					saveAttempt++;
					throw new ZSaveException(new ZDataException(new TransactionException("Transactions can never be trusted!!!!", OdysseyDataErrorType.TransactionRolledBack), null, Db.Connection), Factory);
				}
			});
			try
			{
				Factory.Save();
				Fail("Factory save should have thrown exception");
			}
			catch (Exception) { }
			Factory.Save();

			Assert("ADD log is in memory but not saved to the db", !((IAutoAdminLogTarget)dummy).Logs.GetAllLogs().First(x => ((StmALog)x).SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode).IsInDatabase);
			AssertWTECreated(trigger as IAutoAdminLogTarget);
		}

		void AssertWTECreated(IAutoAdminLogTarget trigger)
		{
			var wteList = trigger.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode);
			AssertEquals("should be an expected number of wte logs", 1, wteList.Count());
			var wte = wteList.FirstOrDefault();
			AssertNotNull("workflow should have fired", wte);
			var wteData = new WorkflowTriggerEventData(wte);
			AssertContains("wte log reference should have an empty ZGuid", ZGuid.Empty.ToString(), wteData.TriggeringLogPK.ToString());
		}
	}

	[UseSnapshotProtection]
	sealed class StmALogAddedToQueueOnlyNonTransactionedWorkflowTest : TestCase
	{
		public void TestNonPersistentADDEventAddedToQueue()
		{
			using (AuditStmALogWorkflowTest.TurnOffAutoLogsForDummyInRegistry())
			{
				var factory = new BusinessObjectFactory();
				var dummy = factory.New<IDummyWithWorkflow>();
				var trigger = dummy.AddNewTrigger();
				((IBaseTrigger)trigger).TriggerEventCode = AutoEvents.AddedARecordToTheSystemCode;
				var dummyAutoLog = dummy as IAutoAdminLogTarget;
				var addedLog = dummyAutoLog.Logs.AddedLog;
				var props = new StmALogQueueProperties
				{
					TableName = addedLog.SL_Table,
					ParentId = addedLog.SL_Parent.ToGuid(),
					UserCode = addedLog.SL_GS_NKUser,
					BranchCode = addedLog.SL_GB_NKBranch,
					DepartmentCode = addedLog.SL_GE_NKDepartment,
					FireWorkflow = addedLog.SL_FireWorkflow,
					EventCode = addedLog.SL_SE_NKEvent,
					Reference = addedLog.SL_Reference,
					EventTime = addedLog.EventTimeOffset.ToDateTimeOffset(),
					IsCancelled = addedLog.SL_IsCancelled,
					IsEstimate = addedLog.SL_IsEstimate,
					ALogReference = Guid.Empty
				};

				factory.Save();

				Assert("Added Log not saved", !dummyAutoLog.Logs.GetAllLogs().First(x => ((StmALog)x).SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode).IsInDatabase);
				StmALogWriterTest.CheckForStmALogQueueRecord(props);
			}
		}
	}
}
