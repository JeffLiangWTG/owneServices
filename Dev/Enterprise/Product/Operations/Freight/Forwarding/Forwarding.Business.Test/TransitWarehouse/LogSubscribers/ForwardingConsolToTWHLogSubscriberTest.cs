using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;
using EventRefParams = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolToTWHLogSubscriber))]
	sealed class ForwardingConsolToTWHLogSubscriberTest : LogSubscriberTest<ForwardingConsolToTWHLogSubscriber>
	{
		public void TestSendReceiptInstructionInPickup() => TestTransitWarehouseInstructionCore(Direction.Pickup, ServiceRequest.Receipt);

		public void TestSendDispatchInstructionInPickup() => TestTransitWarehouseInstructionCore(Direction.Pickup, ServiceRequest.Dispatch);

		public void TestSendReceiveAndDispatchInstructionInPickup() => TestTransitWarehouseInstructionCore(Direction.Pickup, ServiceRequest.ReceiveAndDispatch);

		public void TestSendReceiptInstructionInDelivery() => TestTransitWarehouseInstructionCore(Direction.Delivery, ServiceRequest.Receipt);

		public void TestSendDispatchInstructionInDelivery() => TestTransitWarehouseInstructionCore(Direction.Delivery, ServiceRequest.Dispatch);

		public void TestSendReceiveAndDispatchInstructionInDelivery() => TestTransitWarehouseInstructionCore(Direction.Delivery, ServiceRequest.ReceiveAndDispatch);

		void TestTransitWarehouseInstructionCore(Direction direction, ServiceRequest serviceRequest)
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventRefParams.Direction] = direction.ToString();
			parameters[EventRefParams.Service] = serviceRequest.ToString();
			Consol.Logs.AddNew(AutoEvents.MessageSendingRequest, ZDateTimeOffset.Now, parameters.ToArray());
			Factory.Save();

			CreateEventsAndRunLogWalker(AutoEvents.MessageSendingRequest);
			var supporter = Consol as ITransitWarehouseInstructionSupporter;
			if (direction == Direction.Pickup)
			{
				if (serviceRequest == ServiceRequest.Receipt)
				{
					AssertNotNull(supporter.PickupReceiptRequestedDate);
				}
				else if (serviceRequest == ServiceRequest.Dispatch)
				{
					AssertNotNull(supporter.PickupDispatchRequestedDate);
				}
				else
				{
					AssertNotNull(supporter.PickupReceiptRequestedDate);
					AssertNotNull(supporter.PickupDispatchRequestedDate);
				}
			}
			else
			{
				if (serviceRequest == ServiceRequest.Receipt)
				{
					AssertNotNull(supporter.DeliveryReceiptRequestedDate);
				}
				else if (serviceRequest == ServiceRequest.Dispatch)
				{
					AssertNotNull(supporter.DeliveryDispatchRequestedDate);
				}
				else
				{
					AssertNotNull(supporter.DeliveryReceiptRequestedDate);
					AssertNotNull(supporter.DeliveryDispatchRequestedDate);
				}
			}
		}

		public void TestSendPrepareDispatchInstructionInPickup() => TestSendPrepareDispatchInstructionCore(Direction.Pickup);

		public void TestSendPrepareDispatchInstructionInDelivery() => TestSendPrepareDispatchInstructionCore(Direction.Delivery);

		void TestSendPrepareDispatchInstructionCore(Direction direction)
		{
			var shipment1 = Consol.Shipments.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var shipment3 = Consol.Shipments.AddNew();
			Factory.Save();

			var consolForPrepareDipatch = new ConsolPrepareForDispatchInstruction(Consol, direction);
			var shipmentsForSelection = consolForPrepareDipatch.ShipmentsForSelection;
			shipmentsForSelection[0].SelectedForDelivery = true;
			shipmentsForSelection[1].SelectedForDelivery = true;

			var shipmentSelected1 = shipmentsForSelection[0].Shipment;
			var shipmentSelected2 = shipmentsForSelection[1].Shipment;

			var eventTime = ZDateTimeOffset.Now;
			var parameters = new Dictionary<string, string>();
			parameters[EventRefParams.Direction] = direction.ToString();
			parameters[EventRefParams.Service] = nameof(ServiceRequest.PrepareDispatch);
			Consol.Logs.AddNew(AutoEvents.MessageSendingRequest, eventTime, parameters.ToArray());

			var shipmentParams = new Dictionary<string, string>();
			shipmentParams[EventRefParams.DeclarationID] = Consol.JK_UniqueConsignRef;
			shipmentSelected1.Logs.AddNew(AutoEvents.JobShipmentSelected, eventTime, shipmentParams.ToArray());
			shipmentSelected2.Logs.AddNew(AutoEvents.JobShipmentSelected, eventTime, shipmentParams.ToArray());
			Factory.Save();

			AssertEquals(consolForPrepareDipatch.GetAllSelectedShipments().Count(), 2);

			CreateEventsAndRunLogWalker(AutoEvents.MessageSendingRequest);
			var supporter = Consol as ITransitWarehouseInstructionSupporter;
			AssertEquals(supporter.PickupReceiptRequestedDate, ZDateTime.Empty);
			AssertEquals(supporter.PickupDispatchRequestedDate, ZDateTime.Empty);
		}

		#region Implementation

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent => true;

		ProcessTask CreateEventsAndRunLogWalker(Event ev)
		{
			Factory.Save();

			ProcessTask task = Consol.WorkflowItems.Milestones[ev];
			if (task == null)
			{
				task = ((IWorkflowProvider)Consol).WorkflowItems.Triggers.AddNew();
				task.TriggerConditions.TriggerEventCode = ev.Code;
			}

			Factory.Save();
			RunLogWalkerCycleForTest();
			task.Reload();
			return task;
		}

		ForwardingConsol Consol => consol ??= Factory.NewWithValidTestData<ForwardingConsol>();
		ForwardingConsol consol;

		#endregion
	}
}
