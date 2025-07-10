using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	[TestedType(typeof(OneOffQuoteDataContextManager))]
	public class OneOffQuoteDataContextManagerTest : ShipmentDataContextManagerTestCase<OneOffQuoteDataContextManager, QuotedBooking>
	{
		#region Tests Not Applicable due to different Data Context

		//Note - tests under this region are not valid as :-
		//In case of QuotedBooking, for Quoted Booking - we have three types of objects
		//1. Spot Quote (one off quote)
		//2. Quick Booking
		//3. Booking with Quote
		//For writing all above objects, we are using QuotedBookingDataContextManager,
		//and for reading 2 & 3 types above, we are also using QuotedBookingDataContextManager.
		//but for reading type 1 (Spot Quote), we will be using OneOffQuoteDataContextManager
		//as we can't identify by using same data context which is ForwardingBooking that
		//whether it's OOQ or BWQ or Quick Booking, the only way is to check by using whether booking property exists or not,
		//which is not a very good clean way of deciding,
		//so we decided that we will use a separate Data Cotext Manager instead to keep code clean.

		protected override void TestAttributeIsOnBusinessObjectCore()
		{
			//This test is not valid as this test was meant for checking
			//whether attribute defined on class QuotedBooking matches attribute defined in datacontext manager or not.
			Assert("Invalid test for OneOffQuoteDataContextManager as we have different DataContext Type than defined on QuotedBooking", true);
		}

		protected override void TestManagerIsLoadableViaSpringCore()
		{
			//this test is not valid as this test was meant for loading Data Context Manager by checking attribute defined on class QuotedBooking.
			Assert("Invalid test for OneOffQuoteDataContextManager as we have different DataContext Type than defined on QuotedBooking", true);
		}

		#endregion

		public void TestDataContextType()
		{
			AssertEquals(DataContextType.OneOffQuote, new OneOffQuoteDataContextManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			AssertEquals(ZString.Empty, new OneOffQuoteDataContextManager().DataContextKey);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertEquals(null, new OneOffQuoteDataContextManager().DefaultOutputDirectory);
		}

		public void TestManagesEventsAndShipments()
		{
			var contextManager = (IDataContextManager)new OneOffQuoteDataContextManager();
			AssertEquals(true, contextManager.ManagesEvents());
			AssertEquals(true, contextManager.ManagesShipments());
		}

		public void TestImportOneOffQuote()
		{
			AssertImportOneOffQuote("Should import a new quote.", string.Empty, "00001000");
			AssertImportOneOffQuote("Should not update.", "00001000", "00001001", shouldProcess: false);
			AssertImportOneOffQuote("Should import another new quote.", string.Empty, "00001001");
		}

		void AssertImportOneOffQuote(string assertionMessage, string key, string expectedQuoteNumber, bool shouldProcess = true)
		{
			AssertNull("Pre-condition:Quote should not exists.", GetQuote(expectedQuoteNumber));

			var message = GetQueuedUniversalShipmentMessage(string.Format(UniversalShipmentMessageOneOffQuote, key));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(assertionMessage, () =>
			{
				if (shouldProcess)
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", $@"
Added One Off Quote from UniversalShipment.
Successfully saved One Off Quote - Quote ({expectedQuoteNumber}).
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", $@"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Added One Off Quote from UniversalShipment.
Successfully saved One Off Quote - Quote ({expectedQuoteNumber}).
".Trim(), logNoteText);

					AssertNotNull("Loaded Quote.", GetQuote(expectedQuoteNumber));
				}
				else
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", $@"
ERROR - Match couldn't be found for OneOffQuote with Key {key}
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", $@"
Error - Match couldn't be found for OneOffQuote with Key {key}
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

					AssertNull("Loaded Quote.", GetQuote(expectedQuoteNumber));
				}
			});
		}

		public void TestImportXMLFileHasNoContainerModeAndInvalidTransportMode_TransportModeShouldBeSeenAsMode()
		{
			var expectedQuoteNumber = "00001000";
			AssertNull("Pre-condition:Quote should not exists.", GetQuote(expectedQuoteNumber));
			var message = GetQueuedUniversalShipmentMessage(string.Format(UniversalShipmentMessageOneOffQuoteWithInvalidTransportMode, string.Empty));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions("Should import a new quot ewhich Transport Mode is SEA, and Container Mode is LCL.", () =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", $@"
Added One Off Quote from UniversalShipment.
Successfully saved One Off Quote - Quote ({expectedQuoteNumber}).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", $@"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Added One Off Quote from UniversalShipment.
Successfully saved One Off Quote - Quote ({expectedQuoteNumber}).
".Trim(), logNoteText);

				AssertNotNull("Loaded Quote.", GetQuote(expectedQuoteNumber));
				var quote = GetQuote(expectedQuoteNumber) as Quote;
				AssertEquals("LCL", quote.CurrentOneOffQuote.TT_ContainerMode);
				AssertEquals("SEA", quote.CurrentOneOffQuote.TT_TransportMode);
			});
		}

		public void TestImportXMLFileHasEmptyContainerModeAndValidTransportMode_ContainerModeShouldBeEmpty()
		{
			var expectedQuoteNumber = "00001000";
			AssertNull("Pre-condition:Quote should not exists.", GetQuote(expectedQuoteNumber));
			var message = GetQueuedUniversalShipmentMessage(string.Format(UniversalShipmentMessageOneOffQuoteWithValidTransportModeAndEmptyContainerMode, string.Empty));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions("Should import a new quote which Transport Mode is AIR, and Container Mode is empty.", () =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", $@"
Added One Off Quote from UniversalShipment.
Successfully saved One Off Quote - Quote ({expectedQuoteNumber}).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", $@"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Added One Off Quote from UniversalShipment.
Successfully saved One Off Quote - Quote ({expectedQuoteNumber}).
".Trim(), logNoteText);

				AssertNotNull("Loaded Quote.", GetQuote(expectedQuoteNumber));
				var quote = GetQuote(expectedQuoteNumber) as Quote;
				AssertEquals("", quote.CurrentOneOffQuote.TT_ContainerMode);
				AssertEquals("AIR", quote.CurrentOneOffQuote.TT_TransportMode);
			});
		}

		public void TestOneOffQuoteIsNotMatched_ResposneIsNotCO2()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(UniversalShipmentMessageOneOffQuote, quotedBooking.Quote.TH_QuoteNumber));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var factory = new BusinessObjectFactory();
			var viewBooking = factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking.PK));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", $@"
ERROR - Match couldn't be found for OneOffQuote with Key {quotedBooking.Quote.TH_QuoteNumber}
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestOneOffQuoteIsNotMatched_WhenOOQIsCancelledAndResponseIsCO2()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
			quotedBooking.Quote.TH_IsCancelled = true;
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(UniversalShipmentCO2MessageOneOffQuote, quotedBooking.Quote.TH_QuoteNumber));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var factory = new BusinessObjectFactory();
			var viewBooking = factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking.PK));

			CombineAssertions(delegate
			{
				Assert("OOQ is cancelled", viewBooking.VB_IsCanceled);
				AssertMultilineASCIIEquals("Service Task Log", $@"
ERROR - Match couldn't be found for OneOffQuote with Key {quotedBooking.Quote.TH_QuoteNumber}
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestOneOffQuoteIsMatched_WhenOOQIsNotCancelledAndResponseIsCo2()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
			quotedBooking.Quote.TH_IsCancelled = false;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUCBR";
			quotedBooking.TransportMode = "ROA";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(UniversalShipmentCO2MessageOneOffQuote, quotedBooking.Quote.TH_QuoteNumber));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var factory = new BusinessObjectFactory();
			var viewBooking = factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking.PK));

			CombineAssertions(delegate
			{
				var quoteNumber = quotedBooking.Quote.TH_QuoteNumber;
				Assert("OOQ is not cancelled", !viewBooking.VB_IsCanceled);
				AssertMultilineASCIIEquals("Service Task Log", $@"
Updated One Off Quote - Quote ({quoteNumber}) from UniversalShipment.
Successfully saved One Off Quote - Quote ({quoteNumber}).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", $@"
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Importing greenhouse gas emissions calculation result.
CO2e is calculated for QuotedBooking: One Off Quote - Quote ({quoteNumber})
Updated One Off Quote - Quote ({quoteNumber}) from UniversalShipment.
Successfully saved One Off Quote - Quote ({quoteNumber}).
".Trim(), logNoteText);

				AssertNotNull("Loaded Quote.", GetQuote(quoteNumber));
			});
		}

		public void TestImportEventWithEventTypeIRJAndMessageTypeCO2e()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(string.Format(UniversalEventWithEventTypeIRJAndMessageTypeCO2e, quotedBooking.Quote.TH_QuoteNumber));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(CO2eStatusList.Codes.Rejected, quotedBooking.GetCO2eStatus());

			AssertEquals(1, quotedBooking.Logs.Find(log =>
			log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
			log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Rejected) &&
			log.Parameters.TryGetValue(Params.Reason, out var reason) && reason == "ERROR! Something went wrong.").Count());
		}

		protected override QuotedBooking GetNewBusinessObjectForTesting()
		{
			return QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
		}

		RatingHeader GetQuote(string quoteNumber)
		{
			return new BusinessObjectFactory().LoadTop1<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_QuoteNumber, quoteNumber));
		}

		const string UniversalShipmentMessageOneOffQuote = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OneOffQuote</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
  </Shipment>
</UniversalShipment>
";

		const string UniversalShipmentMessageOneOffQuoteWithInvalidTransportMode = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OneOffQuote</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <TransportMode>
      <Code>LCL</Code>
      <Description>LCL</Description>
    </TransportMode>
  </Shipment>
</UniversalShipment>
";

		const string UniversalShipmentMessageOneOffQuoteWithValidTransportModeAndEmptyContainerMode = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OneOffQuote</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <ContainerMode>
      <Code></Code>
      <Description></Description>
    </ContainerMode>
    <TransportMode>
      <Code>AIR</Code>
      <Description>AIR</Description>
    </TransportMode>
  </Shipment>
</UniversalShipment>
";

		const string UniversalShipmentCO2MessageOneOffQuote = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>OneOffQuote</Type>
        </DataTarget>
      </DataTargetCollection>
      <DataSource>
        <DataProvider>WTG Greenhouse Gas Emission</DataProvider>
      </DataSource>
    </DataContext>
    <PortOfLoading>AUSYD</PortOfLoading>
    <PortOfDischarge>AUCBR</PortOfDischarge>
    <TotalWeight>12051.000</TotalWeight>
    <TotalWeightUnit>KG</TotalWeightUnit>
    <TransportMode>ROA</TransportMode>
    <TransportLegCollection>
      <TransportLeg>
        <LegOrder>0</LegOrder>
        <PortOfLoading>AUSYD</PortOfLoading>
        <PortOfDischarge>AUCBR</PortOfDischarge>
        <EstimatedDeparture>2023-06-08T09:19:00</EstimatedDeparture>
        <EstimatedArrival>2023-06-08T14:19:00</EstimatedArrival>
        <VoyageFlightNo>RUN#4562</VoyageFlightNo>
        <TransportMode>Road</TransportMode>
        <LegType>Main</LegType>
        <AircraftType />
        <GreenhouseGasEmission>
          <CO2ePerTonne>22.191060658866484109202555804</CO2ePerTonne>
          <CO2ePerTonneUnit>KG</CO2ePerTonneUnit>
        </GreenhouseGasEmission>
      </TransportLeg>
    </TransportLegCollection>
    <GreenhouseGasEmission>
      <CO2e>22.191060658866484109202555804</CO2e>
      <CO2eUnit>KG</CO2eUnit>
      <CO2ePerTonne>22.191060658866484109202555804</CO2ePerTonne>
      <CO2ePerTonneUnit>KG</CO2ePerTonneUnit>
    </GreenhouseGasEmission>
  </Shipment>
</UniversalShipment>
";

		const string UniversalEventWithEventTypeIRJAndMessageTypeCO2e = @"
<UniversalEvent Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OneOffQuote</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2020-02-02T20:20:22</EventTime>
    <EventType>IRJ</EventType>
    <EventParameters>
      <Department>WiseTech Global</Department>
      <MessageType>WTG Greenhouse Gas Emission</MessageType>
      <ReferenceNumber>C2300188272</ReferenceNumber>
    </EventParameters>
    <ContextCollection>
      <Context>
        <Type>FailureReason</Type>
        <Value>ERROR! Something went wrong.</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => string.Format(UniversalShipmentMessageOneOffQuote, string.Empty);
	}
}
