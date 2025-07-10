using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class CESEventProcessorTest : TransitUniversalTestCase
	{
		#region TestConstructor_FactoryNotNull

		public void TestConstructor_FactoryNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CESEventProcessor(new UniversalEvent(), Logger, null));
		}

		#endregion

		#region TestConstructor_LoggerNotNull

		public void TestConstructor_LoggerNotNull()
		{
			var universalEvent = new UniversalEvent();
			AssertExceptionThrown<ArgumentNullException>(() => new CESEventProcessor(universalEvent, null, factory));
		}

		#endregion

		#region TestConstructor_EventObjectNotNull

		public void TestConstructor_EventObjectNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CESEventProcessor(null, Logger, factory));
		}

		#endregion

		#region TestProcessPopulatesCustomsDetails

		public void TestProcessPopulatesCustomsDetails()
		{
			var xmlEvent = (UniversalEvent)EventDeserializer.Parse(TransitReceiveCESEventXMLBuilder("CLR", "CCL"));

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK, "RC00000001");
			receiveConsignment.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			AssertNull("Customs Release Number hasn't been populated yet", receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber));

			var processor = new CESEventProcessor(xmlEvent, Logger, factory);
			processor.Process(receiveConsignment);

			var customsReleaseNumberReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertNotNull("Customs Release Number should have been populated.", customsReleaseNumberReference);
				AssertEquals("Customs Release Number should be Customs Cleared.", "Customs Cleared", customsReleaseNumberReference.CE_EntryNum);
			});
		}

		public void TestProcessPopulatesCustomsDetails_UpdatesCustomsReleaseNumber()
		{
			var xmlEvent = (UniversalEvent)EventDeserializer.Parse(TransitReceiveCESEventXMLBuilder("CLR", "CCL"));

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK, "RC00000001");
			receiveConsignment.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var customsEntryNumber = (ICusEntryNumber)receiveConsignment.CustomsReferenceNumbers.AddNew();
			customsEntryNumber.CE_EntryType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber;
			customsEntryNumber.CE_EntryNum = "OldNumber";

			AssertEquals("Customs Release Number is OldNumber", "OldNumber", receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber).CE_EntryNum);

			var processor = new CESEventProcessor(xmlEvent, Logger, factory);
			processor.Process(receiveConsignment);

			var customsReleaseNumberReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertContains("Information - Customs Release Number 'Customs Cleared' has been added to Receive Consignment 'RCN1'. It is now Cleared for Release.", Logger.Logs);
				AssertEquals("Customs Release Number has been updated to Customs Cleared.", "Customs Cleared", customsReleaseNumberReference.CE_EntryNum);
			});
		}

		public void TestProcessPopulatesCustomsDetails_NoCustomsStatus()
		{
			var xmlEvent = (UniversalEvent)EventDeserializer.Parse(TransitReceiveCESEventWithoutCustomsStatusXML);

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK, "RC00000001");

			AssertNull("Customs Release Number hasn't been populated yet", receiveConsignment.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber));

			var processor = new CESEventProcessor(xmlEvent, Logger, factory);
			processor.Process(receiveConsignment);

			var customsReleaseNumberReference = receiveConsignment.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertNull("Customs Release Number was not populated.", customsReleaseNumberReference);
			});
		}

		#endregion

		#region Implementation

		XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		BusinessObjectFactory factory => Factory.BOFactory;

		string TransitReceiveCESEventXMLBuilder(string customsStatusCode, string customsReasonCode) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>CES</EventType>
		<EventReference>|SER=PCS|TYP={customsStatusCode}{(!string.IsNullOrEmpty(customsReasonCode) ? $"|RES={customsReasonCode}" : "")}</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>3438</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>RCN1</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>ANLU7766112</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		string TransitReceiveCESEventWithoutCustomsStatusXML => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>CES</EventType>
		<EventReference>|SER=PCS</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>3438</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>RCN1</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>ANLU7766112</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
	}

	#endregion
}
