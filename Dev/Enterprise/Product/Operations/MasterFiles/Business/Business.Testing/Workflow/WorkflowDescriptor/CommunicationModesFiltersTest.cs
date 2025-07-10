using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommunicationModesFiltersTest : TemplateApplicationTestCase
	{
		#region Setup

		sealed class Data : IDisposable
		{
			public Data(IDisposable disposable)
			{
				this.disposable = disposable;
			}
			readonly IDisposable disposable;
			public OrgHeader Org { get; set; }
			public EDICommunicationsMode Mode { get; set; }
			public DummyWithWorkflow Dummy { get; set; }
			public ProcessTaskNotification Notification { get; set; }
			public ProcessTask Trigger { get; set; }

			public void Dispose() => disposable.Dispose();
		}

		Data SetupCommunicationsModeTest()
		{
			var org = MakeOrg();
			var mode = MakeCommunicationsMode(org);
			var (company, branch) = MakeCompanyWithBranch();
			company.GC_OH_OrgProxy = org.PK;
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = MakeTrigger(dummy);
			var notification = MakeNotification(trigger);
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			Factory.Save();

			return new Data(Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Org = org,
				Mode = mode,
				Dummy = dummy,
				Trigger = trigger,
				Notification = notification
			};
		}

		#endregion

		#region Testing individual filters

		public void TestEK_TransportMode()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);

				data.Dummy.Z0_Code = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
				Factory.Save();
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestEK_RecipientRole()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_RecipientRole = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);

				data.Mode.EK_RecipientRole = MessageRecipientPartyTypeList.Codes.OrgProxy;
				Factory.Save();
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestEK_EventCode()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_EventCode = Events.CustomisableEvent55Code;
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);

				data.Trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent55Code;
				Factory.Save();
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestEK_EventReferenceCondition_REF()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReference;
				data.Mode.EK_EventReferenceConditionValue = "CROWDEDSNAKE";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);
				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "CROWDEDSNAKE");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestEK_EventReferenceCondition_RFW()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
				data.Mode.EK_EventReferenceConditionValue = "SHA*";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);
				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "SHAME");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestEK_EventReferenceCondition_RFR()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;
				data.Mode.EK_EventReferenceConditionValue = "PUR+";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);

				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "PURRRRRRRRRR");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestEK_EventReferenceCondition_RFP()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReferenceParameters;
				data.Mode.EK_EventReferenceConditionValue = "CMP=BOG";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);
				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "Samuels|CMP=BOG");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		#endregion

		#region Simultaneous filters

		public void TestAllFilterTypesAtTheSameTime()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Mode.EK_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReference;
				data.Mode.EK_RecipientRole = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;
				data.Mode.EK_EventReferenceConditionValue = "CROWDEDSNAKE";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);

				data.Dummy.Z0_Code = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
				data.Mode.EK_RecipientRole = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "CROWDEDSNAKE");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		#endregion

		#region Different action types

		public void TestFiltersApplyToXUS()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReference;
				data.Mode.EK_EventReferenceConditionValue = "CROWDEDSNAKE";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);
				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "CROWDEDSNAKE");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		public void TestFiltersApplyToXUE()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				data.Mode.EK_FileFormat = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReference;
				data.Mode.EK_EventReferenceConditionValue = "CROWDEDSNAKE";
				var notification = data.Notification;
				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "CROWDEDSNAKE");
				var wteLog = data.Trigger.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Single();

				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(wteLog, data.Trigger));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				queuedLog.SJ_EventTime = DateTime.Now;
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}
		 
		public void TestFiltersApplyToXUT()
		{
			using (var data = SetupCommunicationsModeTest())
			{
				data.Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
				data.Mode.EK_FileFormat = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
				data.Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReference;
				data.Mode.EK_EventReferenceConditionValue = "CROWDEDSNAKE";
				var notification = data.Notification;
				Factory.Save();
				var processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, processor.GetDestinations().Destinations.Count);
				var log = data.Dummy.Logs.AddNew(Events.CustomisableEvent00, "CROWDEDSNAKE");
				Factory.Save();
				var queuedLog = new QueuedLogForTesting(Factory);
				var source = new WorkflowTriggerActionSource(data.Dummy, data.Trigger, notification, queuedLog, Lazy.Create<IStmALog>(() => log));
				processor = (IMessageProcessor)DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(source, queuedLog);
				AssertEquals(1, processor.GetDestinations().Destinations.Count);
			}
		}

		#endregion
	}
}
