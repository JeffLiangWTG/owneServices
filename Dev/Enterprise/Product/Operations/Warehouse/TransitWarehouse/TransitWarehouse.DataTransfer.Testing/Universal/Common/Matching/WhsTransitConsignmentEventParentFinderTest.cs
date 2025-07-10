using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public abstract class WhsTransitConsignmentEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestSCMEvent

		public void TestSCMEvent_MissingDataForMatchingFromEventParameters()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", "", "", "", "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.
Expected department to be 'Customs'.", logger.Logs);
			});
		}

		public void TestSCMEvent_MissingDataForMatchingFromEventReference()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.
Expected department to be 'Customs'.", logger.Logs);
			});
		}

		#endregion

		#region TestSHLEvent

		public void TestSHLEvent_MissingDataForMatchingFromEventParameters()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", "", "", "", "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.
Expected department to be 'Customs' or Facility to be 'CFS'.", logger.Logs);
			});
		}

		public void TestSHLEvent_MissingDataForMatchingFromEventParameters_HasValidFacilityButMissingReasonAndMessageType()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", "", Facilities.Code.Depot, "", "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@$"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.
Expected message type to be {MessageTypeForSHLEvent}.", logger.Logs);
			});
		}

		public void TestSHLEvent_MissingDataForMatchingFromEventReference()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.
Expected department to be 'Customs' or Facility to be 'CFS'.", logger.Logs);
			});
		}

		public void TestSHLEvent_MissingDataForMatchingFromEventReference_HasValidFacilityButMissingReasonAndMessageType()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.TransportConsignmentRunSheetInstruction), "", "|FAC=CFS"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals(0, results.Length);
				AssertContains(@$"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.
Expected DataSource to be 'ForwardingShipment'.
Location is not found.
Data source key is not found.
Customs reference number is not found.
Expected message type to be {MessageTypeForSHLEvent}.", logger.Logs);
			});
		}

		protected abstract string MessageTypeForSHLEvent { get; }

		#endregion

		#region TestCRESANote

		protected void AddCRESANoteToConsignment(WhsItemReceiveConsignment rcn)
		{
			var note = rcn.FindOrCreateCRESAStmNote();
			note.ST_NoteText = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
		}

		protected void AddCRESANoteToConsignment(WhsItemDispatchConsignment dcn)
		{
			var note = dcn.FindOrCreateCRESAStmNote();
			note.ST_NoteText = @"User: CargoWise One Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Organization Provider ID:
Buyer Provider ID    Supplier Provider ID    Sending Party Provider ID    Forwarder Provider ID    Agent Provider ID
-                    -                       -                            -                        -
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
RC1                  RC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			helper = null;
			data = null;
			eventDeserializer = null;
		}

		protected XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		protected TestDataForUniversal Data => data ?? (data = new TestDataForUniversal(Factory, new TestErrorLogger()));
		TestDataForUniversal data;

		protected TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory.BOFactory);

		protected WhsLocation CreateLocation(string code, WhsWarehouse warehouse = null)
		{
			return Helper.CreateRowAndGenerateLocations(warehouse ?? Data.Warehouse, code, 1, 1).Locations[0];
		}

		protected abstract EventParentFinder GetFinder(IXmlImportLogger logger = null);

		#endregion

		#region Assertion

		protected void AssertAdditionalReferece(ICusEntryNumber reference, string entryCategory, string entryType, string entryNum, string entryStatus = "", string entryLineReference = "")
		{
			AssertEquals(entryCategory, reference.CE_Category);
			AssertEquals(entryType, reference.CE_EntryType);
			AssertEquals(entryNum, reference.CE_EntryNum);
			AssertEquals(entryStatus, reference.CE_EntryStatus);
			AssertEquals(entryLineReference, reference.CE_EntryLineReference);
		}

		#endregion
	}
}
