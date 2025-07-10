using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class SendAirBookingCommandTest : TestCaseWithFactory
	{
		#region TestSend_OK

		const string airBookingDataStoreName = "AirBooking";

		public void TestSend_OK_CarrierCheckIsOff()
		{
			using (AirBookingTestHelper.TempDisableCarrierConfiguration())
			{
				AssertSend_OK(false);
			}
		}

		public void TestSend_OK_CarrierCheckIsOn()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				AssertSend_OK(false);
			}
		}

		public void TestSend_OK_AirlineHasTermsAndConditons()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			if (!AirBookingTestHelper.SetAirlineTermsAndConditions(Factory, "618", "terms & conditions from airline\r\nhttp://some-link.com"))
			{
				Fail("Prereq: failed to set up T&Cs for test");
			}

			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				AssertSend_OK(true);
			}
		}

		bool hardRefreshCalled;

		void AssertSend_OK(bool saveCopyToEDocs)
		{
			const string requestUrl = "http://test.com/";

			var consol = GetAirConsolWithTransports();

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_ISN);
				var mockContext = new SendAirBookingCommandMockContext(consol, handler);
				var bookingRequest = mockContext.BookingRequest;

				AssertBookingRequestStatusesSetupCorrectly(bookingRequest);

				var broker = mockContext.DocumentInfo.Object.Services.Resolve<IEventBroker>();

				var disposable = broker.GetEvent<DocumentHardRefreshEvent>().Subscribe(_ =>
				{
					hardRefreshCalled = true;
				});

				hardRefreshCalled = false;

				var res = mockContext.Command.Invoke();

				Assert("Booking has been sent", res);
				Assert("Booking Send command is disabled", !mockContext.Command.IsEnabled);
				Assert("Hard refresh should be called", hardRefreshCalled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);

				var consolLogs = GetLogs(consol);

				AssertContainsExactElementsInAnyOrder("consol data logs",
					new[]
					{
						"ADD",
						"PAA",
						"WBA Master Bill Number \"61873808291\" Was Entered",
						"MSN Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
					},
					consolLogs);

				var documentDataLogs = GetLogs(mockContext.DocumentData);

				AssertContainsExactElementsInAnyOrder("document data logs",
					new[]
					{
						"MSN |DEP=Carrier|MST=Air Booking",
						"DEX",
						"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
					},
					documentDataLogs);

				AssertContainsExactElementsInAnyOrder("flight statues were updated",
					new[]
					{
						Core.Constants.TransportStatus.Requested,
						Core.Constants.TransportStatus.Requested
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));
			}

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Requested, consol.Transports[0].JW_Status);

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Requested, consol.Transports[1].JW_Status);

			var printJobsQuery = new ZQuery();
			printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobConsolSchema.Constants.TableName);
			printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, consol.PK);

			var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);

			if (saveCopyToEDocs)
			{
				AssertEquals("Document was added to eDocs", 1, printJobs.Length);
				AssertEquals("SP_EmailSubjectLine will become eDoc description",
					"Eagle Datamation International - BN - AUBNE - Air Booking (1)", printJobs[0].SP_EmailSubjectLine);
			}
			else
			{
				AssertEquals("Document was not added to eDocs", 0, printJobs.Length);
			}
		}

		string[] GetLogs(IStmALogParent logParent)
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent != Events.WorkflowTemplateAppliedCode)
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();
		}

		const string expectedRequestContent = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
            <Key>CCN1406309</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
          </DocumentaryOverride>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
              <Name>Eagle Datamation International</Name>
            </Company>
            <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
            <EventDepartment Name=""Department"">BRN</EventDepartment>
            <EventUser Name=""CargoWise Support"">E</EventUser>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CCN1406309</BookingConfirmationReference>
        <GoodsDescription></GoodsDescription>
        <PortOfDestination Name=""Singapore"">SIN</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <RequiredTemperatureMaximum>25</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>15</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalVolume>0</TotalVolume>
        <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
        <TotalWeight>0</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>618-73808291</WayBillNumber>
        <WayBillType Description=""Master Waybill"">MWB</WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Airline</AddressType>
            <CompanyName></CompanyName>
            <OrganizationCode></OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>Agent</AddressType>
            <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""IATA CASS Number"">CAS</Type>
                <Value></Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <TransportLegCollection>
          <TransportLeg>
            <PortOfDischarge Name=""Brisbane"">BNE</PortOfDischarge>
            <PortOfLoading Name=""Sydney"">SYD</PortOfLoading>
            <LegOrder>1</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight1</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
          <TransportLeg>
            <PortOfDischarge Name=""Singapore"">SIN</PortOfDischarge>
            <PortOfLoading Name=""Brisbane"">BNE</PortOfLoading>
            <LegOrder>2</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight2</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		const string eBookingAPIResponse_ISN = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestSend_401

		public void TestSend_401()
		{
			const string requestUrl = "http://test.com/";

			var consol = GetAirConsolWithTransports();

			using (AirBookingTestHelper.TempSetSupportedCarriers())
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var handler = TestHandler.Create(HttpStatusCode.Unauthorized);
				var mockContext = new SendAirBookingCommandMockContext(consol, handler);
				var bookingRequest = mockContext.BookingRequest;

				var res = mockContext.Command.Invoke();

				Assert("Booking has not been sent", !res);
				Assert("Booking Send command is enabled", mockContext.Command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291", handler.Url);

				var consolLogs = GetLogs(consol);

				AssertContainsExactElementsInAnyOrder("consol data logs",
					new[]
					{
						"ADD",
						"PAA",
						"WBA Master Bill Number \"61873808291\" Was Entered"
					},
					consolLogs);

				var documentDataLogs = GetLogs(mockContext.DocumentData);

				AssertContainsExactElementsInAnyOrder("document data logs",
					Array.Empty<string>(),
					documentDataLogs);

				AssertContainsExactElementsInAnyOrder("flight statues were not updated",
					new[]
					{
						Core.Constants.TransportStatus.Planned,
						Core.Constants.TransportStatus.Planned
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));
			}

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Planned, consol.Transports[0].JW_Status);

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Planned, consol.Transports[1].JW_Status);
		}

		#endregion

		#region TestSend_SelectRates

		public void TestSend_SelectRates()
		{
			const string requestUrl = "http://test.com/";

			IBookingRate GetCheapestRate(IReadOnlyCollection<IBookingRate> rates) => rates.OrderBy(rate => rate.Amount).FirstOrDefault();

			var selector = new Mock<IBookingRateSelector>();
			selector.Setup(s => s.SelectRate(It.IsAny<IReadOnlyCollection<IBookingRate>>())).Returns<IReadOnlyCollection<IBookingRate>>(GetCheapestRate);

			using (AirBookingTestHelper.TempSetSupportedCarriers())
			using (ObjectFactory.Substitute(selector.Object))
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var consol = GetAirConsolWithTransports();
				var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_Rates);
				var mockContext = new SendAirBookingCommandMockContext(consol, handler);
				var bookingRequest = mockContext.BookingRequest;

				AssertBookingRequestStatusesSetupCorrectly(bookingRequest);

				var res = mockContext.Command.Invoke();

				AssertEquals("prerequisite - message was sent twice (once without rates and another time with selected rate)",
					2, handler.Methods.Count);

				AssertEquals("First ABE Call: Method", HttpMethod.Post, handler.Methods.First());
				AssertEquals("First ABE Call: RequestUri", "http://test.com/booking/618-73808291", handler.Urls.First());
				AssertMultilineASCIIEquals("First ABE Call: Content", expectedRequestWithoutSelectedRateContent, handler.Contents.First());

				AssertNoExceptionThrown("selector has been shown for airline prefix 618 with 2 rates",
					() => selector.Verify(s => s.SelectRate(It.Is<IReadOnlyCollection<IBookingRate>>(actual =>
						actual.Count == 2
							&& actual.All(ar => ar.CarrierPrefix == "618")
					)), Times.Once));

				AssertEquals("Second ABE Call: Method", HttpMethod.Post, handler.Methods.Last());
				AssertEquals("Second ABE Call: RequestUri", "http://test.com/booking/618-73808291", handler.Urls.Last());
				AssertMultilineASCIIEquals("Second ABE Call: Content with selected rate", expectedRequestWithSelectedRateContent, handler.Contents.Last());
			}
		}

		public void TestSend_NoExceptionWhenSelectRatesReturnsNull()
		{
			const string requestUrl = "http://test.com/";

			var selector = new Mock<IBookingRateSelector>();
			selector.Setup(s => s.SelectRate(It.IsAny<IReadOnlyCollection<IBookingRate>>())).Returns((IReadOnlyCollection<IBookingRate> rates) => null);

			using (ObjectFactory.Substitute(selector.Object))
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var consol = GetAirConsolWithTransports();
				var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_Rates);
				var mockContext = new SendAirBookingCommandMockContext(consol, handler);

				AssertBookingRequestStatusesSetupCorrectly(mockContext.BookingRequest);

				AssertNoExceptionThrown("Sending the air booking commmand should not throw any exceptions when no rates are selected", () => mockContext.Command.Invoke());
			}
		}

		const string eBookingAPIResponse_Rates = @"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>WTLDAUXX1</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
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
        <PortOfOrigin>CDG</PortOfOrigin>
        <PortOfDestination>JFK</PortOfDestination>
        <SubShipmentCollection>
          <SubShipment>
            <TransportLegCollection>
              <TransportLeg>
                <LegOrder>1</LegOrder>
                <PortOfLoading>CDG</PortOfLoading>
                <PortOfDischarge>JFK</PortOfDischarge>
                <EstimatedDeparture>2020-12-01T10:20:00</EstimatedDeparture>
                <EstimatedArrival>2020-12-01T13:01:00</EstimatedArrival>
                <VoyageFlightNo>DL263</VoyageFlightNo>
                <BookingStatus>PLN</BookingStatus>
                <LegType>Other</LegType>
                <AircraftType Description=""Airbus A330-300 Passenger"">333</AircraftType>
              </TransportLeg>
            </TransportLegCollection>
            <ConsolCosts>
              <ConsolCostLineCollection>
                <ConsolCostLine>
                  <ChargeCode>
                    <Code>STANDARD</Code>
                    <Description>BOOKABLE</Description>
                  </ChargeCode>
                  <SupplierReference>2847abac-e09e-44b8-a43f-74b51973b008</SupplierReference>
                  <CostOSAmount>600.00</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                  <RatingBehaviour>SPT</RatingBehaviour>
                </ConsolCostLine>
              </ConsolCostLineCollection>
            </ConsolCosts>
            <AddInfoCollection>
              <AddInfo>
                <Key>RateDescription</Key>
                <Value>Online Confirmation - Booking will be confirmed online</Value>
              </AddInfo>
              <AddInfo>
                <Key>Customer</Key>
                <Value>132812</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shipment</Key>
                <Value>31635553-eb00-4896-b11b-416e6158f442</Value>
              </AddInfo>
            </AddInfoCollection>
          </SubShipment>
          <SubShipment>
            <TransportLegCollection>
              <TransportLeg>
                <LegOrder>1</LegOrder>
                <PortOfLoading>CDG</PortOfLoading>
                <PortOfDischarge>JFK</PortOfDischarge>
                <EstimatedDeparture>2020-12-02T10:20:00</EstimatedDeparture>
                <EstimatedArrival>2020-12-02T13:01:00</EstimatedArrival>
                <VoyageFlightNo>DL263</VoyageFlightNo>
                <BookingStatus>PLN</BookingStatus>
                <LegType>Other</LegType>
                <AircraftType Description=""Airbus A330-300 Passenger"">333</AircraftType>
              </TransportLeg>
            </TransportLegCollection>
            <ConsolCosts>
              <ConsolCostLineCollection>
                <ConsolCostLine>
                  <ChargeCode>
                    <Code>STANDARD</Code>
                    <Description>BOOKABLE</Description>
                  </ChargeCode>
                  <SupplierReference>3d828051-43ff-48f4-abe1-32714fff4b50</SupplierReference>
                  <CostOSAmount>400.00</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                </ConsolCostLine>
              </ConsolCostLineCollection>
            </ConsolCosts>
            <AddInfoCollection>
              <AddInfo>
                <Key>RateDescription</Key>
                <Value>Online Confirmation - Booking will be confirmed online</Value>
              </AddInfo>
              <AddInfo>
                <Key>Customer</Key>
                <Value>132812</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shipment</Key>
                <Value>31635553-eb00-4896-b11b-416e6158f442</Value>
              </AddInfo>
            </AddInfoCollection>
          </SubShipment>
        </SubShipmentCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		const string expectedRequestWithoutSelectedRateContent = @"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
            <Key>CCN1406309</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
          </DocumentaryOverride>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
              <Name>Eagle Datamation International</Name>
            </Company>
            <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
            <EventDepartment Name=""Department"">BRN</EventDepartment>
            <EventUser Name=""CargoWise Support"">E</EventUser>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CCN1406309</BookingConfirmationReference>
        <GoodsDescription></GoodsDescription>
        <PortOfDestination Name=""Singapore"">SIN</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <RequiredTemperatureMaximum>25</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>15</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalVolume>0</TotalVolume>
        <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
        <TotalWeight>0</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>618-73808291</WayBillNumber>
        <WayBillType Description=""Master Waybill"">MWB</WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Airline</AddressType>
            <CompanyName></CompanyName>
            <OrganizationCode></OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>Agent</AddressType>
            <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""IATA CASS Number"">CAS</Type>
                <Value></Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <TransportLegCollection>
          <TransportLeg>
            <PortOfDischarge Name=""Brisbane"">BNE</PortOfDischarge>
            <PortOfLoading Name=""Sydney"">SYD</PortOfLoading>
            <LegOrder>1</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight1</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
          <TransportLeg>
            <PortOfDischarge Name=""Singapore"">SIN</PortOfDischarge>
            <PortOfLoading Name=""Brisbane"">BNE</PortOfLoading>
            <LegOrder>2</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight2</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		const string expectedRequestWithSelectedRateContent = @"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
            <Key>CCN1406309</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
          </DocumentaryOverride>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
              <Name>Eagle Datamation International</Name>
            </Company>
            <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
            <EventDepartment Name=""Department"">BRN</EventDepartment>
            <EventUser Name=""CargoWise Support"">E</EventUser>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CCN1406309</BookingConfirmationReference>
        <GoodsDescription></GoodsDescription>
        <PortOfDestination Name=""Singapore"">SIN</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <RequiredTemperatureMaximum>25</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>15</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalVolume>0</TotalVolume>
        <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
        <TotalWeight>0</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>618-73808291</WayBillNumber>
        <WayBillType Description=""Master Waybill"">MWB</WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Airline</AddressType>
            <CompanyName></CompanyName>
            <OrganizationCode></OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>Agent</AddressType>
            <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""IATA CASS Number"">CAS</Type>
                <Value></Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <SubShipmentCollection>
          <SubShipment>
            <ConsolCosts>
              <ConsolCostLineCollection>
                <ConsolCostLine>
                  <ChargeCode>
                    <Code>STANDARD</Code>
                    <Description>BOOKABLE</Description>
                  </ChargeCode>
                  <CostOSAmount>400.00</CostOSAmount>
                  <CostOSCurrency>EUR</CostOSCurrency>
                  <SupplierReference>3d828051-43ff-48f4-abe1-32714fff4b50</SupplierReference>
                </ConsolCostLine>
              </ConsolCostLineCollection>
            </ConsolCosts>
            <AddInfoCollection>
              <AddInfo>
                <Key>RateDescription</Key>
                <Value>Online Confirmation - Booking will be confirmed online</Value>
              </AddInfo>
              <AddInfo>
                <Key>Customer</Key>
                <Value>132812</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shipment</Key>
                <Value>31635553-eb00-4896-b11b-416e6158f442</Value>
              </AddInfo>
            </AddInfoCollection>
            <TransportLegCollection>
              <TransportLeg>
                <PortOfDischarge>JFK</PortOfDischarge>
                <PortOfLoading>CDG</PortOfLoading>
                <LegOrder>1</LegOrder>
                <AircraftType Description=""Airbus A330-300 Passenger"">333</AircraftType>
                <BookingStatus>PLN</BookingStatus>
                <EstimatedArrival>2020-12-02T13:01:00</EstimatedArrival>
                <EstimatedDeparture>2020-12-02T10:20:00</EstimatedDeparture>
                <LegType>Other</LegType>
                <VoyageFlightNo>DL263</VoyageFlightNo>
              </TransportLeg>
            </TransportLegCollection>
          </SubShipment>
        </SubShipmentCollection>
        <TransportLegCollection>
          <TransportLeg>
            <PortOfDischarge Name=""Brisbane"">BNE</PortOfDischarge>
            <PortOfLoading Name=""Sydney"">SYD</PortOfLoading>
            <LegOrder>1</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight1</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
          <TransportLeg>
            <PortOfDischarge Name=""Singapore"">SIN</PortOfDischarge>
            <PortOfLoading Name=""Brisbane"">BNE</PortOfLoading>
            <LegOrder>2</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight2</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		#endregion

		#region TestCheckHasNoChanges_SavingErrorMessage

		public void TestCheckHasNoChanges_SavingErrorMessage()
		{
			var consol = GetAirConsolWithTransports();
			var handler = TestHandler.Create(HttpStatusCode.OK);

			var userMessages = new List<string>();
			void UserMessageShown(string message) => userMessages.Add(message);

			var mockContext = new SendAirBookingCommandMockContext(consol, handler, onUserMessageShown: UserMessageShown);
			mockContext.DynamicData.SetupGet(d => d.HasChanges).Returns(true);
			var res = mockContext.Command.Invoke();
			AssertEquals("A message should have popped up", "Please save changes before sending message.", string.Join("\r\n", userMessages));
		}

		#endregion

		#region TestReplyWithConsolCosting

		public void TestReplyWithConsolCosting_Success()
		{
			const string requestUrl = "http://test.com/";

			var consol = GetAirConsolWithTransports();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "USORD";

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			Factory.Save();

			using (AirBookingTestHelper.TempSetSupportedCarriers())
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var response = GetAPIResponse_BKC_WithConsolCosting(Core.Constants.CurrencyCodes.EuropeanUnion);

				var userMessages = new List<string>();
				void UserMessageShown(string message) => userMessages.Add(message);

				var handler = TestHandler.Create(HttpStatusCode.OK, response);
				var mockContext = new SendAirBookingCommandMockContext(consol, handler, onUserMessageShown: UserMessageShown);
				var bookingRequest = mockContext.BookingRequest;

				AssertBookingRequestStatusesSetupCorrectly(bookingRequest);

				var res = mockContext.Command.Invoke();

				Assert("Booking has been sent", res);
				Assert("Booking Send command is disabled", !mockContext.Command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);

				AssertEquals("A message should have popped up", @"Airline Booking Response:
CDG-ORD AF136/27Feb - Confirmed

The Consol will be updated automatically if any additional response(s) is received from the Airline at a later time. In the meantime, please check the Events Log for more information.", string.Join("\r\n", userMessages));

				costs.Reload(true);
				AssertEquals("created cost from ABE response have been created", 1, costs.Count);

				AssertEquals("Consol Cost charge code", "FRT", costs[0].ChargeCode.AC_Code);
				AssertEquals("Consol Cost charge code", "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55", costs[0].E6_CostReference);
				AssertEquals("Consol Cost charge code", 1251.9m, costs[0].E6_LocalCostAmount);
				AssertEquals("Consol Cost charge code", "EUR", costs[0].E6_RX_NKCurrency);
				AssertEquals("Consol Cost charge code", "CHG", costs[0].E6_ApportionmentMethod);
			}
		}

		public void TestReplyWithConsolCosting_HandleCriticalValidation()
		{
			const string requestUrl = "http://test.com/";

			var consol = GetAirConsolWithTransports();

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			using (AirBookingTestHelper.TempSetSupportedCarriers())
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var response = GetAPIResponse_BKC_WithConsolCosting("XXX");

				var userMessages = new List<string>();
				void UserMessageShown(string message) => userMessages.Add(message);

				var handler = TestHandler.Create(HttpStatusCode.OK, response);
				var mockContext = new SendAirBookingCommandMockContext(consol, handler, onUserMessageShown: UserMessageShown);
				var bookingRequest = mockContext.BookingRequest;

				AssertBookingRequestStatusesSetupCorrectly(bookingRequest);

				var res = mockContext.Command.Invoke();

				Assert("Booking has been sent", res);
				Assert("Booking Send command is disabled", !mockContext.Command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);

				var userMessage = string.Join("\r\n", userMessages);

				AssertContains("A message should have popped up", @"Airline Booking Response:
CDG-ORD AF136/27Feb - Confirmed

The Consol will be updated automatically if any additional response(s) is received from the Airline at a later time. In the meantime, please check the Events Log for more information.", userMessage);
				AssertContains("A message should contain critical validation message", "Error Message: Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.", userMessage);

				costs.Reload(true);
				AssertEquals("no consol costs have been created", 0, costs.Count);
			}
		}

		string GetAPIResponse_BKC_WithConsolCosting(string currencyCode) => $@"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>AIR_BOOKING_ENGINE</SenderID>
    <RecipientID>HYEBNEUAT</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Shipment>
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
          <Workflow>
            <CodesMappedToTarget>true</CodesMappedToTarget>
          </Workflow>
        </DataContext>
        <WayBillNumber>057-78864796</WayBillNumber>
        <PortOfOrigin>CDG</PortOfOrigin>
        <PortOfDestination>ORD</PortOfDestination>
        <TotalNoOfPacks>6</TotalNoOfPacks>
        <TotalWeight>300.0</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <TotalVolume>1.92</TotalVolume>
        <TotalVolumeUnit>M3</TotalVolumeUnit>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <PortOfLoading>CDG</PortOfLoading>
            <PortOfDischarge>ORD</PortOfDischarge>
            <EstimatedDeparture>2021-02-27T13:05:00</EstimatedDeparture>
            <EstimatedArrival>2021-02-27T15:25:00</EstimatedArrival>
            <VoyageFlightNo>AF136</VoyageFlightNo>
            <BookingStatus>CNF</BookingStatus>
            <TransportMode>AIR</TransportMode>
            <LegType>Flight1</LegType>
          </TransportLeg>
        </TransportLegCollection>
        <NoteCollection>
          <Note>
            <Description>Booking Confirmation Notes</Description>
            <NoteText>Cost as provided by carrier is EUR1251.9
Please note the Total Volume requested for this booking (1.924 M3) is different to what is confirmed (1.92 M3).</NoteText>
            <IsCustomDescription>true</IsCustomDescription>
          </Note>
        </NoteCollection>
        <ConsolCosts>
          <ConsolCostLineCollection>
            <ConsolCostLine>
              <ChargeCode>
                <Code>FRT</Code>
                <Description>International Freight</Description>
              </ChargeCode>
              <SupplierReference>93c45e64-0467-4fd1-8ef3-9e2b7b45fb55</SupplierReference>
              <CostOSAmount>1251.9</CostOSAmount>
              <CostOSCurrency>{currencyCode}</CostOSCurrency>
              <ApportionmentMethod>CHG</ApportionmentMethod>
              <ImportMetaData>
                <Instruction>Insert</Instruction>
              </ImportMetaData>
            </ConsolCostLine>
          </ConsolCostLineCollection>
        </ConsolCosts>
      </Shipment>
    </UniversalShipment>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
            <SubmissionVersion>1</SubmissionVersion>
          </DocumentaryOverride>
          <DataTargetCollection>
            <DataTarget>
              <Key>COBA0000690080</Key>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-02-22T21:31:33</EventTime>
        <EventType>BKC</EventType>
        <EventParameters>
          <Department>Carrier</Department>
          <Location>CDG</Location>
          <Type>AWB</Type>
          <VoyageFlightNumber>AF136</VoyageFlightNumber>
          <FlightDate>2021-02-27</FlightDate>
          <MessageType>Air Booking</MessageType>
          <ReferenceNumber>057-78864796</ReferenceNumber>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>057-78864796</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>CDG</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>ORD</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>CDG</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>ORD</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>AF136</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2021-02-27</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>6</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>300.0Kilograms</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

		#endregion

		#region Implementation

		IDisposable airBookingProgressManagerSubstitute;

		sealed class SendAirBookingCommandMockContext
		{
			public Mock<IDocumentInfo> DocumentInfo;
			public IDocument Document;
			public Mock<IDocumentDescriptor> Descriptor;
			public Mock<IPrintInstructions> PrintInstructions;
			public Mock<IEDocsInstructions> EDocsInstructions;
			public Mock<IDynamicData> DynamicData;
			public Mock<IDocumentSecurityService> SecurityService;

			public VisualizerDocumentData DocumentData;
			public AirBookingRequest BookingRequest;
			public SendAirBookingCommand Command;

			public SendAirBookingCommandMockContext(ForwardingConsol consol, TestHandler handler, Action<string> onUserMessageShown = null)
			{
				DocumentData = consol.Factory.New<VisualizerDocumentData>();
				DocumentData.JDD_Name = airBookingDataStoreName;
				DocumentData.JDD_ParentID = consol.PK;
				DocumentData.JDD_ParentTableCode = consol.TablePrefix;

				consol.Factory.Save();

				DocumentInfo = new Mock<IDocumentInfo>();
				Descriptor = new Mock<IDocumentDescriptor>();
				PrintInstructions = new Mock<IPrintInstructions>();
				EDocsInstructions = new Mock<IEDocsInstructions>();
				DynamicData = new Mock<IDynamicData>();
				SecurityService = new Mock<IDocumentSecurityService>();

				Document = new DummyBookingRequestDocument(DynamicData.Object);

				SecurityService.SetupGet(ss => ss.CanSendMessage).Returns(true);

				var services = new ServiceContainer();
				var broker = new EventBroker();
				services.Register<IEventBroker>(broker);
				services.Register<IDocumentSecurityService>(SecurityService.Object);

				var bookingRequestBuilder = new AirBookingRequestBuilder(consol);
				BookingRequest = bookingRequestBuilder.Build();

				DocumentInfo.SetupGet(di => di.Document).Returns(Document);
				DocumentInfo.SetupGet(di => di.Descriptor).Returns(Descriptor.Object);
				DocumentInfo.SetupGet(di => di.DocumentData).Returns(DocumentData);
				DocumentInfo.SetupGet(di => di.Services).Returns(services);

				Descriptor.SetupGet(di => di.Name).Returns("eBooking");
				Descriptor.SetupGet(di => di.DocumentType).Returns("BKC");
				Descriptor.SetupGet(di => di.PrintInstructions).Returns(PrintInstructions.Object);
				Descriptor.SetupGet(di => di.EDocsInstructions).Returns(EDocsInstructions.Object);

				PrintInstructions.SetupGet(pi => pi.Title).Returns("eBooking Request");

				EDocsInstructions.SetupGet(edoc => edoc.SaveCopyToEDocs).Returns(false);
				EDocsInstructions.SetupGet(edoc => edoc.Parent).Returns(consol);

				DynamicData.SetupGet(di => di.Value).Returns(BookingRequest);

				ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler));

				Command = new SendAirBookingCommand();
				Command.NotifyDocumentInfoCreated(DocumentInfo.Object);

				var notificationService = new Mock<IUserNotificationService>();
				notificationService.Setup(n => n.ShowMessage(It.IsAny<string>(), It.IsAny<string>())).Callback((string message, string caption) => onUserMessageShown?.Invoke(message));
				services.Register<IUserNotificationService>(notificationService.Object);
			}
		}

		ForwardingConsol GetAirConsolWithTransports()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.SecurityStatusCode = SecurityJobConsolAWBSpecialHandling.NotSecured;
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = Core.Constants.TransportStatus.Planned;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = Core.Constants.TransportStatus.Planned;
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			Factory.Save();

			return consol;
		}

		void AssertBookingRequestStatusesSetupCorrectly(AirBookingRequest bookingRequest)
		{
			AssertContainsExactElementsInAnyOrder(
					"prerequisite: original flight statues",
					new[]
					{
						Core.Constants.TransportStatus.Planned,
						Core.Constants.TransportStatus.Planned
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code)
				);
		}

		protected override void SetUp()
		{
			base.SetUp();

			airBookingProgressManagerSubstitute = ObjectFactory.Substitute<IAirBookingProgressManager>(new NoShowAirBookingProgressManager());
		}

		protected override void TearDown()
		{
			base.TearDown();
			airBookingProgressManagerSubstitute?.Dispose();
			AirBookingCarrierConfigurationManager.ClearSupportedAirlinesApplicationCache();
		}

		#endregion
	}
}
