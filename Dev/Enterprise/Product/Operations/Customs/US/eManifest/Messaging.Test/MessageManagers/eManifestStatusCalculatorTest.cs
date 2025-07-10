using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using EDIMessage = Enterprise.Customs.US.eManifest.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	[TestedType(typeof(eManifestStatusCalculator))]
	sealed class eManifestStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestIsLodged()
		{
			Assert("IsLodged", calculator.IsLodged(TripEntryStatusList.Codes.AcceptedComplete));
			Assert("IsLodged", !calculator.IsLodged(EntryStatusList.Codes.Error));
			Assert("IsLodged", !calculator.IsLodged(MessageTypes.Codes.SyntaxError));
			Assert("IsLodged", !calculator.IsLodged(EntryStatusList.Codes.Cancelled));
			Assert("IsLodged", !calculator.IsLodged(""));
		}

		public void TestIsCrewInfoLodged()
		{
			var trip = Factory.New<Trip>();
			var linkedObject = new eManifestMessageWrapper(trip, string.Empty);
			var calculator1 = (eManifestStatusCalculator)calculator;
			Assert("IsCrewInfoLodged", !calculator1.IsCrewInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, TripEntryStatusList.Codes.Error, "1"));
			Assert("IsCrewInfoLodged", !calculator1.IsCrewInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, TripEntryStatusList.Codes.AcceptedPreliminary, "2"));
			Assert("IsCrewInfoLodged", calculator1.IsCrewInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.Cancelled, "3"));
			Assert("IsCrewInfoLodged", !calculator1.IsCrewInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, TripEntryStatusList.Codes.AcceptedPreliminary, "4"));
			Assert("IsCrewInfoLodged", calculator1.IsCrewInfoLodged(linkedObject));
		}

		public void TestIsEquipmentInfoLodged()
		{
			var trip = Factory.New<Trip>();
			var linkedObject = new eManifestMessageWrapper(trip, string.Empty);
			var calculator1 = (eManifestStatusCalculator)calculator;
			Assert("IsEquipmentInfoLodged", !calculator1.IsEquipmentInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "1", eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.eManifest, TripEntryStatusList.Codes.Error, "1"));
			Assert("IsEquipmentInfoLodged", !calculator1.IsEquipmentInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "2", eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.eManifest, TripEntryStatusList.Codes.AcceptedComplete, "2"));
			Assert("IsEquipmentInfoLodged", calculator1.IsEquipmentInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.eManifest, TripEntryStatusList.Codes.Cancelled, "3"));
			Assert("IsEquipmentInfoLodged", !calculator1.IsEquipmentInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "4", eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.eManifest, TripEntryStatusList.Codes.AcceptedComplete, "4"));
			Assert("IsEquipmentInfoLodged", calculator1.IsEquipmentInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.eManifest, TripEntryStatusList.Codes.Cancelled, "5"));
			Assert("IsEquipmentInfoLodged", !calculator1.IsEquipmentInfoLodged(linkedObject));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "6", eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.AcceptedPreliminary, "6"));
			Assert("IsEquipmentInfoLodged", calculator1.IsEquipmentInfoLodged(linkedObject));
		}

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", "Complete e-Manifest w/ACE ID", calculator.MessageTypeDescription);
			calculator = new eManifestStatusCalculator(MessageTypes.Codes.CrewAndPassenger);
			AssertEquals("MessageTypeDescription", "Crew/Passengers Details", calculator.MessageTypeDescription);
		}

		public override void TestCalculatedJobStatus()
		{
			var linkedObject = new eManifestMessageWrapper(Factory.New<Trip>(), string.Empty);
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(linkedObject));
			const string invalidMessage = @"UNH+1956+CUSRES:D:03B:UN'BGM+132:::ST+LOCKMAN0000001'DTM+132:200412301200:203'UNT+38+1956'";
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "10", "XXX", ZDateTime.Today, invalidMessage);
			AssertEquals("Should ignore invalid messages", ZString.Empty, calculator.CalculatedJobStatus(linkedObject));
			const string syntaxErrorMessage = @"UNH+17+CONTRL:D:03B:UN'UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCM+17+CUSCAR:D:03B:UN+4'UCS+5'UCD+13+2:1'UCD+12+2:1'UCS+6'UCD+12+2:1'UCS+8+15'UNT+11+17'UNZ+1+17'";
			AddEDIMessage(linkedObject, MessageTypes.Codes.SyntaxError, "20", MessageTypes.Codes.CrewAndPassenger, ZDateTime.Today.AddDays(1), syntaxErrorMessage);
			AssertEquals("Crew syntax error response doesn't affect job status", ZString.Empty, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.SyntaxError, "25", MessageTypes.Codes.UnassociatedShipments, ZDateTime.Today.AddDays(2), syntaxErrorMessage);
			AssertEquals("Unassociated shipments syntax error response doesn't affect job status", ZString.Empty, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.SyntaxError, "30", MessageTypes.Codes.eManifest, ZDateTime.Today.AddDays(3), syntaxErrorMessage);
			AssertEquals("Syntax error message received", MessageTypes.Codes.SyntaxError, calculator.CalculatedJobStatus(linkedObject));
			const string errorMessage = @"UNH+1956+CUSRES:D:03B:UN'BGM+132:::ST+LOCKMAN0000001'ERP+1'ERC+511'FTX+AAO+++Man Returned to Preliminary'UNT+38+1956'";
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "40", EntryStatusList.Codes.Error, ZDateTime.Today.AddDays(4), errorMessage);
			AssertEquals("Error message received", EntryStatusList.Codes.Error, calculator.CalculatedJobStatus(linkedObject));
			const string acceptedMessage = @"UNH+1778+CUSRES:D:03B:UN'BGM+132:::ST+LOCKMAN0000001'DTM+163:200611302300:203'ERP+1'ERC+000'FTX+AAO+++Manifest Accepted'UNT+10+1778'";
			message = AddEDIMessage(linkedObject, MessageTypes.Codes.CrewAndPassenger, "40", TripEntryStatusList.Codes.AcceptedComplete, ZDateTime.Today.AddDays(5), acceptedMessage);
			AssertEquals("Crew response doesn't affect job status", EntryStatusList.Codes.Error, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "45", TripEntryStatusList.Codes.AcceptedPreliminary, ZDateTime.Today.AddDays(6), acceptedMessage);
			AssertEquals("Clear message received", TripEntryStatusList.Codes.AcceptedPreliminary, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "50", TripEntryStatusList.Codes.AcceptedComplete, ZDateTime.Today.AddDays(7), acceptedMessage);
			AssertEquals("Clear message received", TripEntryStatusList.Codes.AcceptedComplete, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "60", EntryStatusList.Codes.Error, ZDateTime.Today.AddDays(8), errorMessage);
			AssertEquals("Clear status not changed if error received", TripEntryStatusList.Codes.AcceptedComplete, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.SyntaxError, "70", MessageTypes.Codes.eManifest, ZDateTime.Today.AddDays(9), syntaxErrorMessage);
			AssertEquals("Clear status not changed if syntex error received", TripEntryStatusList.Codes.AcceptedComplete, calculator.CalculatedJobStatus(linkedObject));
			const string statusMessage1 = @"UNH+1778+CUSRES:D:03B:UN'BGM+34:::ST+LOCKMAN0000001'DTM+163:200611302300:203'ERP+1'ERC+SN030'FTX+AAO+++Trip arrived'UNT+10+1778'";
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "75", TripEntryStatusList.Codes.ShipmentsStatusUpdate, ZDateTime.Today.AddDays(10), statusMessage1);
			AssertEquals("Shipments status update doesn't affect job status", TripEntryStatusList.Codes.AcceptedComplete, calculator.CalculatedJobStatus(linkedObject));
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "80", TripEntryStatusList.Codes.TripArrived, ZDateTime.Today.AddDays(11), statusMessage1);
			AssertEquals("Status updated", TripEntryStatusList.Codes.TripArrived, calculator.CalculatedJobStatus(linkedObject));
			const string statusMessage2 = @"UNH+1778+CUSRES:D:03B:UN'BGM+34:::ST+LOCKMAN0000001'DTM+163:200611302300:203'ERP+1'ERC+SN037'FTX+AAO+++Hold Trip'UNT+10+1778'";
			AddEDIMessage(linkedObject, MessageTypes.Codes.eManifest, "90", TripEntryStatusList.Codes.HoldTrip, ZDateTime.Today.AddDays(12), statusMessage2);
			AssertEquals("Status updated", TripEntryStatusList.Codes.HoldTrip, calculator.CalculatedJobStatus(linkedObject));
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator() => new eManifestStatusCalculator(MessageTypes.Codes.eManifest);

		static EDIMessage AddEDIMessage(IEDIMessageCollectionProvider linkedObject, string messageType, string messageNum, string messageSubType, ZDateTime createTime, ZString messageText)
		{
			var result = linkedObject.Messages.AddNew(typeof(EDIMessage));
			result.EM_MessageType = messageType;
			result.EM_MessageSubType = messageSubType;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_MessageNum = messageNum;
			result.EM_SystemCreateTimeUtc = createTime;
			result.EM_MessageText = messageText;
			return (EDIMessage)result;
		}
	}
}
