using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.GateManagement.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementBookingEventParentFinder))]
	public class GteGateMovementBookingEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGivenIncomingDataTarget_IsNotGteGateMovementBooking_ThenReturnNull()
		{
			CreateGateMovementBookingForTest();

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>TransitReceive</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value>100000</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var eventParentFinder = new GteGateMovementBookingEventParentFinder(Factory, new GteGateMovementBookingDataContextManager(), new DummyLogger());
			AssertNull(eventParentFinder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGivenIncomingDataTarget_IsGateMovementBooking_ThenReturnMatch()
		{
			var gateMovementBooking = CreateGateMovementBookingForTest();

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>GateMovementBooking</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value>100000</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var eventParentFinder = new GteGateMovementBookingEventParentFinder(Factory, new GteGateMovementBookingDataContextManager(), new DummyLogger());
			var parents = eventParentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNotNull(parents);
			AssertEquals("parents.Count", 1, parents.Length);
			AssertEquals("parent[0]", gateMovementBooking, parents.FirstOrDefault());
		}

		public void TestGivenIncomingDataTarget_IsBlank_ThenReturnMatch()
		{
			var gateMovementBooking = CreateGateMovementBookingForTest();

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection />
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value>100000</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var eventParentFinder = new GteGateMovementBookingEventParentFinder(Factory, new GteGateMovementBookingDataContextManager(), new DummyLogger());
			var parents = eventParentFinder.GetLogParentsForEvent(xmlEvent);

			AssertNotNull(parents);
			AssertEquals("parents.Count", 1, parents.Length);
			AssertEquals("parent[0]", gateMovementBooking, parents.FirstOrDefault());
		}

		public void TestGivenIncomingShippersReference_HasNoMatch_ThenReturnNull()
		{
			CreateGateMovementBookingForTest();

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>GateMovementBooking</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value>INVALIDREFERENCENUMBER</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var eventParentFinder = new GteGateMovementBookingEventParentFinder(Factory, new GteGateMovementBookingDataContextManager(), new DummyLogger());
			AssertNull(eventParentFinder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGivenIncomingShippersReference_HasEmptyReference_ThenReturnNull()
		{
			CreateGateMovementBookingForTest(sourceReferenceNumber: string.Empty);

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>GateMovementBooking</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value></Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var eventParentFinder = new GteGateMovementBookingEventParentFinder(Factory, new GteGateMovementBookingDataContextManager(), new DummyLogger());
			AssertNull(eventParentFinder.GetLogParentsForEvent(xmlEvent));
		}

		GteGateMovementBooking CreateGateMovementBookingForTest(string sourceReferenceNumber = "100000")
		{
			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_SourceReferenceNumber = sourceReferenceNumber;
			return gateMovementBooking;
		}
	}
}
