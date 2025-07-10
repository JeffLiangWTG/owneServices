using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ActionWrapperTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		[TestDate(2015, 10, 16)]
		public void TestConstructor()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var lineTriggerParent = Factory.New<DummyWithWorkflow>();

			var trigger = (DummyProcessTask)parent.WorkflowItems.Triggers.AddNew();
			var milestone = (DummyProcessTask)parent.WorkflowItems.Milestones.AddNew();

			SetupProcessTaskProperties(trigger);
			SetupProcessTaskProperties(milestone);

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "TTP";
			notification.PQ_MessagePurpose = "MPR";
			notification.PQ_TriggerParty = nameof(RecipientRoleType.ORP);

			AssertActionWrapperProperties(notification, lineTriggerParent);

			notification.PQ_P9 = milestone.PK;
			AssertActionWrapperProperties(notification, lineTriggerParent);

			var triggeringEvent = Factory.New<StmALog>();
			AssertActionWrapperProperties(notification, lineTriggerParent);
		}

		public void TestHandleTriggerPartyLowerCase()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var lineTrigger = Factory.New<DummyWithWorkflow>();

			var trigger = (DummyProcessTask)parent.WorkflowItems.Triggers.AddNew();
			var milestone = (DummyProcessTask)parent.WorkflowItems.Milestones.AddNew();

			SetupProcessTaskProperties(trigger);
			SetupProcessTaskProperties(milestone);

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "TTP";
			notification.PQ_MessagePurpose = "MPR";
			notification.PQ_TriggerParty = nameof(RecipientRoleType.ORP);

			Factory.Save();

			string sQL = $"update dbo.ProcessTaskNotification set PQ_TriggerParty = 'oRp' where PQ_PK = '{notification.PK}'";
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
			Factory.ReloadAll<ProcessTaskNotification>();

			var wrapper = new ActionWrapper(notification, lineTrigger, null);
			AssertEquals("RecipientRoleCodes count", 1, wrapper.RecipientRoleDetails.Length);
			AssertEquals("Recepient Role Code", RecipientRoleType.ORP, wrapper.RecipientRoleDetails[0].Type);
		}

		static void SetupProcessTaskProperties(DummyProcessTask processTask)
		{
			processTask.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			processTask.P9_Description = "McLaren";
			processTask.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(1);
			processTask.TriggerConditions.TriggerConditionValue = "Alonso";
			processTask.Parent.Logs.AddNew(Events.Departure, ZDateTimeOffset.Now.AddDays(-1));
		}

		static void AssertActionWrapperProperties(ProcessTaskNotification notification, IBusiness lineTriggerParent)
		{
			var wrapper = new ActionWrapper(notification, (BusinessObject)lineTriggerParent, null);

			CombineAssertions(() =>
			{
				AssertEquals("ActionType", "TTP", wrapper.ActionType);
				AssertEquals("PQ_MessagePurpose", "MPR", wrapper.PurposeCode);
				AssertEquals("RecipientRoleCodes count", 1, wrapper.RecipientRoleDetails.Length);
				AssertEquals("Recepient Role Code", RecipientRoleType.ORP, wrapper.RecipientRoleDetails[0].Type);
				AssertEquals("TriggerEventCode", "DEP", wrapper.TriggerEventCode);
				AssertEquals("TriggerDescription", "McLaren", wrapper.TriggerDescription);
				AssertEquals("TriggerType", notification.Parent.WorkflowItemType == "TRG" ? TriggerType.Trigger : TriggerType.Milestone, wrapper.TriggerType);

				if (notification.Parent.IsMilestone())
				{
					AssertEquals("TriggerScheduledDate", ZDateTimeOffset.Now.AddDays(1), wrapper.TriggerScheduledDate);
				}

				AssertEquals("TriggerActualDate", ZDateTimeOffset.Now.AddDays(-1), wrapper.TriggerActualDate);
				AssertEquals("TriggerReference", "Alonso", wrapper.TriggerReference);
				AssertEquals("ParentBO", lineTriggerParent.Identifier, wrapper.ParentBO.PK);
			});
		}

		public void TestConstructor_ProcessTaskActualDateIsEmpty_SetTriggerActualDateToLogEventTime()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var processTask = parent.WorkflowItems.Triggers.AddNew();
			processTask.SetMilestoneActualDateForTest(ZDateTime.Empty);

			var eventTime = new ZDateTimeOffset(1991, 8, 24);
			var log = new Mock<IQueuedLog>();
			log.Setup(m => m.EventTimeOffset).Returns(eventTime);

			var notification = processTask.ProcessTaskNotifications.AddNew();
			var wrapper = new ActionWrapper(notification, parent, null, log.Object);

			AssertEquals("TriggerActualDate", eventTime, wrapper.TriggerActualDate);
		}

		public void TestRecipientService()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { nameof(ServiceCodeType.BRQ) };

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			var actionWrapper_NoRecipient = new ActionWrapper(dataExport, dummyBO);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<RecipientRoleDetail>(), actionWrapper_NoRecipient.RecipientRoleDetails);

			dataExport.Calc_RecipientType = nameof(RecipientRoleType.ORP);
			var actionWrapper_NoRecipientService = new ActionWrapper(dataExport, dummyBO);
			AssertNull(actionWrapper_NoRecipientService.RecipientRoleDetails.Single().ServiceCode);

			dataExport.RecipientService = nameof(ServiceCodeType.BRQ);
			var actionWrapper_BRQ = new ActionWrapper(dataExport, dummyBO);
			AssertEquals(ServiceCodeType.BRQ, actionWrapper_BRQ.RecipientRoleDetails.Single().ServiceCode);
		}
	}
}
