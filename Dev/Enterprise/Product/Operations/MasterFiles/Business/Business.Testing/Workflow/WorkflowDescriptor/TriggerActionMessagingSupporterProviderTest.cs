using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class TriggerActionMessagingSupporterProviderTest<T> : TestCaseWithFactory
			where T : BusinessObject, ITriggerActionMessagingSupporterProvider, IWorkflowProvider
	{
		public virtual void TestTriggerActionMessaging()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var supporter = GetNewSupporter();

				var trigger = supporter.WorkflowItems.AddNew();
				trigger.P9_Description = "Message Sending Trigger";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var triggerTypes = trigger.WorkflowDescriptor.GetScheduleDeferredMessageSendTriggerActions(trigger, supporter).ToList();
				AssertEquals(ExpectedTriggerTypes.Count(), triggerTypes.Count);

				foreach (var code in ExpectedTriggerTypes)
				{
					Assert(triggerTypes.Contains(code));
					Env.OutgoingMailManager.EmailsCreated.Clear();
					var action = trigger.ProcessTaskNotifications.AddNew();
					action.PQ_TriggerType = code;
					supporter.GetLogs().AddNew(Events.Authorised);
					var processor = trigger.WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
					var notifications = new NotificationBuffer();
					processor.Process(notifications);
					AssertContains(ExpectedStringInNotifications, notifications.AsString);
				}
			}
		}

		public void TestWorkflowGetScheduleDeferredMessageSendTriggerActions_ProcessTaskIsLineTrigger_ReturnTypesFromLine()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);

			var parent = Factory.New<DummyWithWorkflow>();
			var lineTrigger = GetNewSupporter();

			var trigger = parent.WorkflowItems.AddNew();
			trigger.P9_LineTriggerType = lineTrigger.WorkflowType;

			var actualTriggerTypes = trigger.WorkflowDescriptor.GetScheduleDeferredMessageSendTriggerActions(trigger, lineTrigger);
			AssertContainsExactElementsInAnyOrder("Trigger types", ExpectedTriggerTypes, actualTriggerTypes);
		}

		protected virtual T GetNewSupporter()
		{
			return Factory.New<T>();
		}

		protected abstract IEnumerable<string> ExpectedTriggerTypes { get; }
		protected abstract string ExpectedStringInNotifications { get; }

		protected virtual ZString CountryCode { get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; } }
	}
}
