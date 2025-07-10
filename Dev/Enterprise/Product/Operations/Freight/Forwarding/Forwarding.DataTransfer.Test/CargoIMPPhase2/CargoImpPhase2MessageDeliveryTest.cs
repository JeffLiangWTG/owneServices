using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class CargoImpPhase2MessageDeliveryTest : TestCaseWithFactory
	{
		public void TestForSeveralCompanies()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "111";
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			company1.GC_OH_OrgProxy = org1.PK;
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "222";
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();

			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			processTask.TriggerConditions.TriggerEventCode = Events.Attached.Code;
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			NotificationBuffer notifications = new NotificationBuffer();

			var log = new QueuedLogForTesting(Factory);
			var processor = new CargoImpPhase2MessageDeliveryForTest(action, log);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(false, notifications.HasErrors);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, processor.ContextCompanyCode);

			processTask.P9_GC = company1.PK;
			processor.Process(notifications);
			Factory.Save();
			AssertEquals(false, notifications.HasErrors);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, processor.ContextCompanyCode);

			processTask.P9_GC = ZGuid.Empty;
			processor.Process(notifications);
			Factory.Save();
			AssertEquals(false, notifications.HasErrors);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, processor.ContextCompanyCode);
		}

		public void TestGetCargoIMPEvent()
		{
			var collection = new CargoIMPPhase2MSUEventsMappingCollection();

			var map1 = collection.AddNew();
			map1.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.RIW;
			map1.EnterpriseEvent = AutoEvents.GateInCode;
			map1.EnterpriseEventReference = "FAC=CFS,LOC=AU";

			var map2 = collection.AddNew();
			map2.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.DEW;
			map2.EnterpriseEvent = AutoEvents.GateInCode;
			map2.EnterpriseEventReference = "FAC=CFS,LOC=US";

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2MSUEventsMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.New<ForwardingShipment>();

			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			processTask.TriggerConditions.TriggerEventCode = AutoEvents.GateInCode;

			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;

			var notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.GateInCode, SJ_Reference = "|LOC=AU|FAC=CFS|TYP=FUL" };
			var processor = new CargoImpPhase2MessageDeliveryForTest(action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("Should be RIW as the SJ_Reference matched the EventReference", CargoIMPPhase2MSUEventCodeList.Codes.RIW, processor.CargoIMPEvent);

			queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.GateInCode, SJ_Reference = "|LOC=US|FAC=CFS" };
			processor = new CargoImpPhase2MessageDeliveryForTest(action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("Should be DEW as the SJ_Reference matched the EventReference", CargoIMPPhase2MSUEventCodeList.Codes.DEW, processor.CargoIMPEvent);

			queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.GateInCode, SJ_Reference = "|LOC=US|FAC=CTO" };
			processor = new CargoImpPhase2MessageDeliveryForTest(action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("Should be empty as FAC is not CFS in the SJ_Reference", string.Empty, processor.CargoIMPEvent);

			queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.GateOutCode, SJ_Reference = string.Empty };
			processor = new CargoImpPhase2MessageDeliveryForTest(action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("Should be empty as the SJ_Reference matched nothing", string.Empty, processor.CargoIMPEvent);
		}

		class CargoImpPhase2MessageDeliveryForTest : CargoImpPhase2MessageDelivery
		{
			public CargoImpPhase2MessageDeliveryForTest(ProcessTaskNotification action, IQueuedLog queuedLog)
				: base(action, queuedLog)
			{
			}

			public ZString ContextCompanyCode { get; private set; }
			public ZString CargoIMPEvent { get; private set; }

			protected override void ProcessCore(ZString messageType, ZString cargoIMPEvent, INotifications notifications)
			{
				ContextCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				CargoIMPEvent = cargoIMPEvent;
			}
		}
	}
}
