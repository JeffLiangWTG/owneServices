using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	class CFSShipmentEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParentForEventUsingContext_WithConsolIncluded()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CFSShipment</Type>
				</DataTarget>
				<DataTarget>
					<Type>CFSLoadListConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2020-06-02T09:50:33.27</EventTime>
		<EventType>ARV</EventType>
		<EventParameters>
			<Department>Customs</Department>
			<Facility>CTO</Facility>
			<Location>MEL</Location>
			<VoyageFlightNumber>QF16</VoyageFlightNumber>
			<FlightDate>2020-06-02</FlightDate>
			<MessageType>AWB Automation</MessageType>
			<ReferenceNumber>554640661S</ReferenceNumber>
			<Total>1</Total>
		</EventParameters>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>S54640661S</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "FCL";
			consol.JK_MasterBillNum = "012345678905";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "S54640661S";
			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new CFSShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var parents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNull("no parent should have been found because this case will be handled by ConsolEventParentFinder as there is a CFSLoadListConsol DataTarget", parents);
		}

		public void TestGetLogParentForEventUsingContext_WithoutConsolIncluded()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CFSShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2020-06-02T09:50:33.27</EventTime>
		<EventType>ARV</EventType>
		<EventParameters>
			<Department>Customs</Department>
			<Facility>CTO</Facility>
			<Location>MEL</Location>
			<VoyageFlightNumber>QF16</VoyageFlightNumber>
			<FlightDate>2020-06-02</FlightDate>
			<MessageType>AWB Automation</MessageType>
			<ReferenceNumber>554640661S</ReferenceNumber>
			<Total>1</Total>
		</EventParameters>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>S54640661S</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "FCL";
			consol.JK_MasterBillNum = "012345678905";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "S54640661S";
			Factory.Save();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var parentFinder = new CFSShipmentEventParentFinder(Factory, manager, new DummyLogger());

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);

			var parents = parentFinder.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("Should have found parent", parents);
			AssertEquals("GetLogParentsForEvent.Length", 1, parents.Length);
			AssertEquals("", shipment.JS_UniqueConsignRef, ((CFSShipment)parents[0]).JS_UniqueConsignRef);
		}
	}
}
