using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingContainerEventTransformerTest : TestCaseWithFactory
	{
		public void TestTransform_PickupToGateOut_Reference()
		{
			var eventValue = new EventValue(Events.PickedUp, reference: "|FAC=CY|TYP=EMT");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("GOU event", "GOU", destEventValue.Code);
			AssertEquals("GOU description", "Gate Out", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CY")));
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "EMT")));
		}

		public void TestTransform_PickupToGateOut_Parameters()
		{
			var eventValue = new EventValue(Events.PickedUp,
				parameters: new Dictionary<string, string>()
				{
					{ "FAC", "CY" },
					{ "TYP", "EMT" },
				});

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("GOU event", "GOU", destEventValue.Code);
			AssertEquals("GOU description", "Gate Out", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CY")));
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "EMT")));
		}

		public void TestTransform_Consignor_Pickup()
		{
			var eventValue = new EventValue(Events.PickedUp, eventTime: new ZDateTimeOffset(2015, 12, 3, 0, 0, 0), isEstimate: true, reference: "|FAC=CNR");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("Pickup event", "PUP", destEventValue.Code);
			AssertEquals("Pickup description", "Picked Up", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CNR")));
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "FUL")));
			AssertEquals("Pickup event time", new ZDateTime(2015, 12, 3, 0, 0, 0), destEventValue.EventTime.ToZDateTime());
			AssertEquals("Pickup estimated", true, destEventValue.IsEstimate);
		}

		public void TestTransform_CFS_Pickup()
		{
			var eventValue = new EventValue(Events.PickedUp, reference: "|FAC=CFS");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("Pickup event", "PUP", destEventValue.Code);
			AssertEquals("Pickup description", "Picked Up", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CFS")));
			AssertEquals("No TYP parameter", 1, destEventValue.Parameters.Count);
		}

		public void TestTransform_CTO_Pickup()
		{
			var eventValue = new EventValue(Events.PickedUp, reference: "|FAC=CTO");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("GateOut event", "GOU", destEventValue.Code);
			AssertEquals("GateOut description", "Gate Out", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CTO")));
			AssertEquals("No TYP parameter", 1, destEventValue.Parameters.Count);
		}

		public void TestTransform_Consignee_Delivery()
		{
			var eventValue = new EventValue(Events.Delivered, reference: "|FAC=CNE");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("Delivery event", "DLV", destEventValue.Code);
			AssertEquals("Delivery description", "Delivered", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CNE")));
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "FUL")));
		}

		public void TestTransform_DeliveryToGateIn()
		{
			var eventValue = new EventValue(Events.Delivered, reference: "|FAC=CY");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("GateIn event", "GIN", destEventValue.Code);
			AssertEquals("GateIn description", "Gate In", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CY")));
			AssertEquals("No TYP parameter", 1, destEventValue.Parameters.Count);
		}

		public void TestTransform_CFS_Delivery()
		{
			var eventValue = new EventValue(Events.Delivered, reference: "|FAC=CFS");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("Delivery event", "DLV", destEventValue.Code);
			AssertEquals("Delivery description", "Delivered", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CFS")));
			AssertEquals("No TYP parameter", 1, destEventValue.Parameters.Count);
		}

		public void TestTransform_CTO_Delivery()
		{
			var eventValue = new EventValue(Events.Delivered, reference: "|FAC=CTO");

			var destEventValue = ForwardingContainerEventTransformer.Transform(eventValue);
			AssertEquals("GateIn event", "GIN", destEventValue.Code);
			AssertEquals("GateIn description", "Gate In", destEventValue.Description);
			Assert("FAC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("FAC", "CTO")));
			AssertEquals("No TYP parameter", 1, destEventValue.Parameters.Count);
		}

		public void TestTransform_OnlyWhenUniversalEventComesFromTransportBooking_UpdateEventLocation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONA";

			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataSource(DataContextType.ForwardingConsol, null);

			var eventValue = new EventValue(Events.PickedUp, reference: "|FAC=CTO");

			var transformedEventValue = ForwardingContainerEventTransformer.Transform(eventValue, universalEvent, container);
			AssertEquals("Universal event not from TransportBooking: Location was not updated", "|FAC=CTO", transformedEventValue.Reference);

			universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataSource(DataContextType.TransportBooking, null);

			eventValue = new EventValue(Events.PickedUp, reference: "|FAC=CTO");

			transformedEventValue = ForwardingContainerEventTransformer.Transform(eventValue, universalEvent, container);
			AssertEquals("Universal event from TransportBooking: Location was updated", "|FAC=CTO|LOC=NZAKL", transformedEventValue.Reference);
		}

		public void TestTransform_UniversalEventComesFromTransportBooking_UpdateEventLocation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONA";

			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataSource(DataContextType.TransportBooking, null);

			Action<Event, string, string, string> assertTransformResult = (originalEventType, originalReference, resultingEventCode, resultingReference) =>
			{
				var eventValue = new EventValue(originalEventType, reference: originalReference);

				var transformedEventValue = ForwardingContainerEventTransformer.Transform(eventValue, universalEvent, container);

				AssertEquals("Resulting event code", resultingEventCode, transformedEventValue.Code);
				AssertEquals("Resulting event reference", resultingReference, transformedEventValue.Reference);
			};

			assertTransformResult(Events.PickedUp, "", Events.PickedUpCode, "");
			assertTransformResult(Events.PickedUp, "|FAC=XXX", Events.PickedUpCode, "|FAC=XXX");

			assertTransformResult(Events.PickedUp, "|FAC=CTO", Events.GateOutCode, "|FAC=CTO|LOC=NZAKL");
			assertTransformResult(Events.PickedUp, "|FAC=CTO|LOC=NZAKL", Events.GateOutCode, "|FAC=CTO|LOC=NZAKL");
			assertTransformResult(Events.PickedUp, "|FAC=CTO|LOC=MASCOT", Events.GateOutCode, "MASCOT|FAC=CTO|LOC=NZAKL");
			assertTransformResult(Events.PickedUp, "Hello there!|FAC=CTO", Events.GateOutCode, "Hello there!|FAC=CTO|LOC=NZAKL");
			assertTransformResult(Events.PickedUp, "Hello there!|FAC=CTO|LOC=MASCOT", Events.GateOutCode, "MASCOT,Hello there!|FAC=CTO|LOC=NZAKL");

			assertTransformResult(Events.PickedUp, "|FAC=CY", Events.GateOutCode, "|FAC=CY|LOC=AUSYD");
			assertTransformResult(Events.PickedUp, "Hello there!|FAC=CY|LOC=MASCOT", Events.GateOutCode, "MASCOT,Hello there!|FAC=CY|LOC=AUSYD");

			assertTransformResult(Events.Delivered, "", Events.DeliveredCode, "");
			assertTransformResult(Events.Delivered, "|FAC=XXX", Events.DeliveredCode, "|FAC=XXX");

			assertTransformResult(Events.Delivered, "|FAC=CTO", Events.GateInCode, "|FAC=CTO|LOC=AUSYD");
			assertTransformResult(Events.Delivered, "|FAC=CTO|LOC=AUSYD", Events.GateInCode, "|FAC=CTO|LOC=AUSYD");
			assertTransformResult(Events.Delivered, "|FAC=CTO|LOC=MASCOT", Events.GateInCode, "MASCOT|FAC=CTO|LOC=AUSYD");
			assertTransformResult(Events.Delivered, "Hello there!|FAC=CTO", Events.GateInCode, "Hello there!|FAC=CTO|LOC=AUSYD");
			assertTransformResult(Events.Delivered, "Hello there!|FAC=CTO|LOC=MASCOT", Events.GateInCode, "MASCOT,Hello there!|FAC=CTO|LOC=AUSYD");

			assertTransformResult(Events.Delivered, "|FAC=CY", Events.GateInCode, "|FAC=CY|LOC=NZAKL");
			assertTransformResult(Events.Delivered, "Hello there!|FAC=CY|LOC=MASCOT", Events.GateInCode, "MASCOT,Hello there!|FAC=CY|LOC=NZAKL");
		}
	}
}
