using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingContainerEventHelperTest : TestCaseWithFactory
	{
		public void TestIsExportPickupOrDeliveryEvent()
		{
			var exportEventInfos = new List<Tuple<string, string>>()
			{
				Tuple.Create(Events.PickedUpCode, "|FAC=CY|TYP=EMT"),
				Tuple.Create(Events.PickedUpCode, "|FAC=CY"),

				Tuple.Create(Events.DeliveredCode, "|FAC=CNR|TYP=EMT"),
				Tuple.Create(Events.DeliveredCode, "|FAC=CNR"),
				Tuple.Create(Events.DeliveredCode, "|FAC=CFS|TYP=EMT"),
				Tuple.Create(Events.DeliveredCode, "|TYP=EMT"),

				Tuple.Create(Events.PickedUpCode, "|FAC=CNR|TYP=FUL"),
				Tuple.Create(Events.PickedUpCode, "|FAC=CNR"),
				Tuple.Create(Events.PickedUpCode, "|FAC=CFS|TYP=FUL"),
				Tuple.Create(Events.PickedUpCode, "|TYP=FUL"),

				Tuple.Create(Events.DeliveredCode, "|FAC=CTO|TYP=FUL"),
				Tuple.Create(Events.DeliveredCode, "|FAC=CTO"),
			};

			CombineAssertions(() =>
			{
				foreach (var exportEventInfo in exportEventInfos)
				{
					string eventCode = exportEventInfo.Item1;
					string eventReference = exportEventInfo.Item2;

					string eventKey = $"{eventCode} event with reference {eventReference}";

					AssertEquals($"{eventKey} should be matched as export", true, ForwardingContainerEventHelper.IsExportPickupOrDeliveryEvent(eventCode, eventReference));
					AssertEquals($"{eventKey} should not be matched as import", false, ForwardingContainerEventHelper.IsImportPickupOrDeliveryEvent(eventCode, eventReference));
				}
			});
		}

		public void TestIsImportPickupOrDeliveryEvent()
		{
			var importEventInfos = new List<Tuple<string, string>>()
			{
				Tuple.Create(Events.PickedUpCode, "|FAC=CTO|TYP=FUL"),
				Tuple.Create(Events.PickedUpCode, "|FAC=CTO"),

				Tuple.Create(Events.DeliveredCode, "|FAC=CNE|TYP=FUL"),
				Tuple.Create(Events.DeliveredCode, "|FAC=CNE"),
				Tuple.Create(Events.DeliveredCode, "|FAC=CFS|TYP=FUL"),
				Tuple.Create(Events.DeliveredCode, "|TYP=FUL"),

				Tuple.Create(Events.PickedUpCode, "|FAC=CNE|TYP=EMT"),
				Tuple.Create(Events.PickedUpCode, "|FAC=CNE"),
				Tuple.Create(Events.PickedUpCode, "|FAC=CFS|TYP=EMT"),
				Tuple.Create(Events.PickedUpCode, "|TYP=EMT"),

				Tuple.Create(Events.DeliveredCode, "|FAC=CY|TYP=EMT"),
				Tuple.Create(Events.DeliveredCode, "|FAC=CY"),
			};

			CombineAssertions(() =>
			{
				foreach (var importEventInfo in importEventInfos)
				{
					string eventCode = importEventInfo.Item1;
					string eventReference = importEventInfo.Item2;

					string eventKey = $"{eventCode} event with reference {eventReference}";

					AssertEquals($"{eventKey} should be matched as import", true, ForwardingContainerEventHelper.IsImportPickupOrDeliveryEvent(eventCode, eventReference));
					AssertEquals($"{eventKey} should not be matched as export", false, ForwardingContainerEventHelper.IsExportPickupOrDeliveryEvent(eventCode, eventReference));
				}
			});
		}

		public void TestUnmatchedEvents_NotExport_And_NotImport_PickupOrDeliveryEvent()
		{
			var unmatchedEventInfos = new List<Tuple<string, string>>()
			{
				Tuple.Create(Events.PickedUpCode, "|FAC=FOO|TYP=EMT"),
				Tuple.Create(Events.PickedUpCode, "|FAC=FOO"),
				Tuple.Create(Events.PickedUpCode, ""),

				Tuple.Create(Events.DeliveredCode, "|FAC=FOO|TYP=EMT"),
				Tuple.Create(Events.DeliveredCode, "|FAC=FOO"),
				Tuple.Create(Events.DeliveredCode, ""),

				Tuple.Create(Events.GateInCode, "|FAC=CTO"),
				Tuple.Create(Events.GateOutCode, "|FAC=CTO"),

				Tuple.Create(Events.FreightLoadedCode, "|FAC=CNR"),
				Tuple.Create(Events.FreightUnloadedCode, "|FAC=CNE"),
			};

			CombineAssertions(() =>
			{
				foreach (var unmatchedEventInfo in unmatchedEventInfos)
				{
					string eventCode = unmatchedEventInfo.Item1;
					string eventReference = unmatchedEventInfo.Item2;

					string eventKey = $"{eventCode} event with reference {eventReference}";

					AssertEquals($"{eventKey} should not be matched as export", false, ForwardingContainerEventHelper.IsExportPickupOrDeliveryEvent(eventCode, eventReference));
					AssertEquals($"{eventKey} should not be matched as import", false, ForwardingContainerEventHelper.IsImportPickupOrDeliveryEvent(eventCode, eventReference));
				}
			});
		}
	}
}
