using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EventsWithSourceTypeTest : TestCaseWithFactory
	{
		public void TestEmptyInstance()
		{
			EventsWithSourceType item = EventsWithSourceType.Empty;
			AssertEquals("Events returned", 0, item.GetEvents(EventsWithSourceType.SourceType.Consol).Length);
		}

		public void TestGetEvents()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			EventsWithSourceType item = new EventsWithSourceType(EventsWithSourceType.SourceType.Shipment, action, dummy);

			AssertEquals("Events returned", 0, item.GetEvents(EventsWithSourceType.SourceType.Consol).Length);
			AssertEquals("Events returned", 1, item.GetEvents(EventsWithSourceType.SourceType.Shipment).Length);
		}

		public void TestGetTriggerringLogs()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			ZGuid pk2 = ZGuid.NewZGuid();
			ZGuid pk5 = ZGuid.NewZGuid();

			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			trigger.Logs.AddNew(AutoEvents.WorkflowTriggerEvent, pk1.ToString());
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			trigger.Logs.AddNew(AutoEvents.EditedARecord, pk2.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			trigger.Logs.AddNew(AutoEvents.WorkflowTriggerEvent, "abcde");
			trigger.Logs.AddNew(AutoEvents.WorkflowTriggerEvent);
			trigger.Logs.AddNew(AutoEvents.WorkflowTriggerEvent, pk5.ToString());

			var item = new EventsWithSourceType(EventsWithSourceType.SourceType.Shipment, triggerAction, dummy);

			AssertEquals(2, item.GetTriggerringLogPKs().Count);
			AssertCollectionContains(pk1, item.GetTriggerringLogPKs());
			AssertCollectionContains(pk5, item.GetTriggerringLogPKs());
			AssertCollectionNotContains(pk2, item.GetTriggerringLogPKs());
		}
	}
}
