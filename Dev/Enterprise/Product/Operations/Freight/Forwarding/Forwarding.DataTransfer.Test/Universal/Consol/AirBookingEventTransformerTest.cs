using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class AirBookingEventTransformerTest : TestCaseWithFactory
	{
		#region TestMapEventTimeFromUTCToLocal

		public void TestMapEventTimeFromUTCToLocal()
		{
			var eventDateTime = new ZDateTime(2020, 1, 1, 10, 15, 0, 0, System.DateTimeKind.Unspecified);

			var parameters = new Dictionary<string, string>
			{
				["DEP"] = "WiseTech Global",
				["MST"] = "Air Booking",
				["RFN"] = "618-73808291"
			};

			var eventValue = new EventValue(Events.InterchangeSent, true, eventTime: eventDateTime.ToOffset(), reference: "|DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291", parameters: parameters);

			var uxmlEvent = new UniversalDataBuss.DataObjects.Universal.Event
			{
				DataContext = new DataContext
				{
					DocumentaryOverride = new UniversalDataBuss.DataObjects.Universal.DocumentaryOverride
					{
						DocumentName = "AirBooking",
						SubmissionVersion = 1
					}
				}
			};

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var transformed = new AirBookingEventTransformer().Transform(eventValue, uxmlEvent, consol);

			var utcOffset = TimeZoneInfo.Local.GetUtcOffset(eventDateTime.ToDateTime());
			var expectedDateTime = eventDateTime.Add(utcOffset);

			AssertEquals("EventTime was converted", expectedDateTime, transformed.EventTime.ToZDateTime());
		}

		#endregion

		#region TestRejectDuplicate

		public void TestRejectDuplicate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			var interchange = Factory.New<IXmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_From = "eBookingAPI";
			interchange.EI_To = "zzz";
			interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;

			var message = interchange.AddNeweHubMessage();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			message.EM_Status = EDIInterchangeStatusList.Codes.Received;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageText = universalEvent_ISN;

			Factory.Save();

			var eventDateTime = new ZDateTime(2020, 1, 1, 10, 15, 0, 0, System.DateTimeKind.Unspecified);

			var parameters = new Dictionary<string, string>
			{
				["DEP"] = "WiseTech Global",
				["MST"] = "Air Booking",
				["RFN"] = "618-73808291"
			};

			var eventValue = new EventValue(Events.InterchangeSent, true, eventTime: eventDateTime.ToOffset(), reference: "|DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291", parameters: parameters);

			var uxmlEvent = new UniversalDataBuss.DataObjects.Universal.Event
			{
				DataContext = new DataContext
				{
					DocumentaryOverride = new UniversalDataBuss.DataObjects.Universal.DocumentaryOverride
					{
						DocumentName = "AirBooking",
						SubmissionVersion = 1
					}
				}
			};

			new AirBookingEventTransformer().Transform(eventValue, uxmlEvent, consol);

			var isn = consol.Logs.AddNew(Events.InterchangeSent, reference: "|DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291");

			var messageLogPivot = Factory.New<IGenPivot>();
			messageLogPivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			messageLogPivot.XX_Relation1ID = isn.PK;
			messageLogPivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			messageLogPivot.XX_Relation2ID = message.PK;
			messageLogPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;

			Factory.Save();

			ReleaseFactory();
			consol = Factory.Load<ForwardingConsol>(consol.PK);

			AssertExceptionThrown<MessageProcessingBusinessFailureException>("Duplicate Event was rejected",
				"This Universal Event submission version is: 1. A Universal Event with the same or newer submission versions (1) has already been imported.",
				() => new AirBookingEventTransformer().Transform(eventValue, uxmlEvent, consol));

			uxmlEvent = new UniversalDataBuss.DataObjects.Universal.Event
			{
				DataContext = new DataContext
				{
					DocumentaryOverride = new UniversalDataBuss.DataObjects.Universal.DocumentaryOverride
					{
						DocumentName = "AirBooking",
						SubmissionVersion = 2
					}
				}
			};

			new AirBookingEventTransformer().Transform(eventValue, uxmlEvent, consol);
		}

		const string universalEvent_ISN = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>ISN</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>";

		#endregion
	}
}
