using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingMessageExtensionsTest : TestCaseWithFactory
	{
		#region HasTermsAndConditions

		public void TestHasTermsAndConditions()
		{
			AssertEquals("null AirBookingRequest", false, ((AirBookingRequest)null).HasTermsAndConditions());

			var airBookingRequest = new AirBookingRequest("test", "123");
			AssertEquals("AirBookingRequest with no T&Cs", false, airBookingRequest.HasTermsAndConditions());

			airBookingRequest.TermsAndConditions = Array.Empty<ZString>();
			AssertEquals("AirBookingRequest with empty T&Cs", false, airBookingRequest.HasTermsAndConditions());

			airBookingRequest.TermsAndConditions = new ZString[]
			{
				"terms",
				"and",
				"conditions"
			};

			AssertEquals("AirBookingRequest with T&Cs", true, airBookingRequest.HasTermsAndConditions());
		}

		#endregion

		#region CalculateLogs

		public void TestCalculateLogs()
		{
			AssertEquals("null VisualizerDocumentData", 0, ((IVisualizerDocumentData)null).CalculateLogs(Events.MessageSentCode));

			var documentData = Factory.New<VisualizerDocumentData>();
			AssertEquals("VisualizerDocumentData with no logs", 0, documentData.CalculateLogs(Events.MessageSentCode));

			documentData.Logs.AddNew(Events.MessageSent);
			AssertEquals("VisualizerDocumentData with log", 1, documentData.CalculateLogs(Events.MessageSentCode));

			documentData.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals("VisualizerDocumentData with 2 different logs", 1, documentData.CalculateLogs(Events.MessageSentCode));

			documentData.Logs.AddNew(Events.MessageSent);
			AssertEquals("VisualizerDocumentData with logs", 2, documentData.CalculateLogs(Events.MessageSentCode));
		}

		#endregion

		#region TestAddCancellationNote

		public void TestAddCancellationNote()
		{
			const string uxml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
    <Shipment>
        <DataContext>
            <DataSource>
                <Key>CCN1406309</Key>
                <Type>ForwardingConsol</Type>
            </DataSource>
        </DataContext>
        <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
        <PortOfOrigin>HAM</PortOfOrigin>
        <PortOfDestination>HKG</PortOfDestination>
        <WayBillNumber>000-12345678</WayBillNumber>
    </Shipment>
</UniversalShipment>";

			var uxmlWithNote = uxml.AddCancellationNote("reason");

			const string expectedUXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>CCN1406309</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>
    <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
    <PortOfOrigin>HAM</PortOfOrigin>
    <PortOfDestination>HKG</PortOfDestination>
    <WayBillNumber>000-12345678</WayBillNumber>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageCancellation</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>reason</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			AssertMultilineASCIIEquals("LastSentMessage", expectedUXml, uxmlWithNote);
		}

		#endregion
	}
}
