using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsOrderAndReceiveEventParentFinderTest<T> : WhsEventParentFinderTest<WhsOrderAndReceiveEventParentFinder<T>, T>
		where T : WhsDocket
	{
		#region Event processing

		public void TestCOEEventProcessed()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = AutoEvents.ConfirmationOfExitCode;

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			subscriber.GetLogParentsForEvent(universalEvent);

			Assert("Event should be processed.", logger.Logs.Contains("The COE event misses Event Time information."));
		}

		public void TestCENEventProcessed()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = AutoEvents.CustomsNumberEnteredCode;

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			subscriber.GetLogParentsForEvent(universalEvent);

			Assert("Event should be processed.", logger.Logs.Contains("No order line matching the event was found."));
		}

		#endregion

		#region TestUniversalEventDoesNotHaveContextCollection

		public void TestUniversalEventDoesNotHaveContextCollection()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
  </Event>
</UniversalEvent>";

			var logger = new DummyLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			AssertNoExceptionThrown(() => subscriber.GetLogParentsForEvent(xmlEvent));
		}

		#endregion

		#region TestCustomsDataSourceEvents_AreMatchedByJobNumberAndDataProvider

		public void TestCustomsDataSourceEvents_AreMatchedByJobNumberAndDataProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var matchingDocket = GetNewDocket(data, "B123");
			matchingDocket.WD_CustomsParentReference = "B123-EDIDATEDI";
			GetNewDocket(data, "W000001");

			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.WarehouseJobCanNowBeFinalisedCode, matchingDocket, "Should have matched the docket with the Customs Job Number.");
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.CancelTheWarehouseJobCode, matchingDocket, "Should have matched the docket with the Customs Job Number.");
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.HoldTheWarehouseOrderCode, matchingDocket, "Should have matched the docket with the Customs Job Number.");
		}

		#endregion

		#region TestCustomsDataSourceEvents_RejectEventWhenMatchingCancelledDockets

		public void TestCustomsDataSourceEvents_RejectEventWhenMatchingCancelledDockets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var cancelledDocket = GetNewDocket(data, "B123");
			cancelledDocket.WD_CustomsParentReference = "B123-EDIDATEDI";
			cancelledDocket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.Save();
			AssertEquals("Precondition - Docket should be cancelled.", true, cancelledDocket.IsCancelled);

			if (RejectHoldEventIfDocketIsCancelled)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					string.Format("Rejected Event 'WBH'. Matching {0} is already Canceled.", cancelledDocket.HumanReadableName),
					() => ProcessEventXML(GetCustomsEventXml(Events.HoldTheWarehouseOrderCode)));

				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					string.Format("Rejected Event 'WBF'. Matching {0} is already Canceled.", cancelledDocket.HumanReadableName),
					() => ProcessEventXML(GetCustomsEventXml(Events.WarehouseJobCanNowBeFinalisedCode)));
			}
			else
			{
				AssertCustomsSourceEventIsAcceptedOrIgnored(Events.HoldTheWarehouseOrderCode, cancelledDocket, "HeldByCustoms Event should be accepted/ignored for Change of Ownership Dockets.");
				AssertCustomsSourceEventIsAcceptedOrIgnored(Events.WarehouseJobCanNowBeFinalisedCode, cancelledDocket, "CustomsFinaliseJob Event should be accepted/ignored for Change of Ownership Dockets.");
			}

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format("Rejected Event 'WBC'. Matching {0} is already Canceled.", cancelledDocket.HumanReadableName),
				() => ProcessEventXML(GetCustomsEventXml(Events.CancelTheWarehouseJobCode)));

			data.Whs1.WW_IsVirtualWarehouse = true;
			var cancelledByCustomsDocket = GetNewDocket(data, "B123");
			cancelledByCustomsDocket.WD_CustomsParentReference = "B123-EDIDATEDI";
			cancelledByCustomsDocket.WD_DocketSubType = ReceiveType.Codes.Customs;
			cancelledByCustomsDocket.WD_ExternalReferenceSplit = new ZByte(1);
			cancelledByCustomsDocket.Logs.AddNew(Events.CancelTheWarehouseJob);
			cancelledByCustomsDocket.Logs.AddNew(Events.Cancelled);
			Factory.Save();
			// even though Virtual Jobs don't do anything with the Hold or Accept Event, we need to accept 
			// the Event onto the Job, otherwise Universal will complain that no Module took the Event.
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.HoldTheWarehouseOrderCode, cancelledByCustomsDocket, "HeldByCustoms Event should be accepted for Dockets in virtual Warehouse.");
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.WarehouseJobCanNowBeFinalisedCode, cancelledByCustomsDocket, "CustomsFinaliseJob Event should be accepted for Dockets in virtual Warehouse.");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format("Rejected Event 'WBC'. Matching {0} is already Canceled.", cancelledByCustomsDocket.HumanReadableName),
				() => ProcessEventXML(GetCustomsEventXml(Events.CancelTheWarehouseJobCode)));
		}

		protected virtual bool RejectHoldEventIfDocketIsCancelled => true;

		#endregion

		#region TestCustomsDataSourceEvents_OnlyMatchByJobNumberAndDataProvider

		public void TestCustomsDataSourceEvents_OnlyMatchByJobNumberAndDataProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			GetNewDocket(data, "W000001");

			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.CancelTheWarehouseJobCode, null, "Should not match on other references if Event is from Customs Data Source.");
		}

		#endregion

		#region TestCustomsDataSourceEvents_AllWarehouseJobEventsAreAcceptedForVirtualWarehouses

		public void TestCustomsDataSourceEvents_AllWarehouseJobEventsAreAcceptedForVirtualWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var docketInVirtualWarehouse = GetNewDocket(data, "B123");
			docketInVirtualWarehouse.WD_CustomsParentReference = "B123-EDIDATEDI";

			// even though Virtual Jobs don't do anything with the Hold or Accept Event, we need to accept 
			// the Event onto the Job, otherwise Universal will complain that no Module took the Event.
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.HoldTheWarehouseOrderCode, docketInVirtualWarehouse, "HeldByCustoms Event should be accepted for Dockets in virtual Warehouse.");
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.WarehouseJobCanNowBeFinalisedCode, docketInVirtualWarehouse, "CustomsFinaliseJob Event should be accepted for Dockets in virtual Warehouse.");
			AssertCustomsSourceEventIsAcceptedOrIgnored(Events.CancelTheWarehouseJobCode, docketInVirtualWarehouse, "Cancel Event should be accepted for Dockets in virtual Warehouse.");
		}

		#endregion

		#region TestCustomsDataSourceEvents_HoldEventIsRejectedIfDocketIsFinalised

		public void TestCustomsDataSourceEvents_HoldEventIsRejectedIfDocketIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var finalisedDocket = GetNewFinalisedDocket(data, "B123", "B123-EDIDATEDI");
			Factory.Save();

			if (RejectHoldEventIfDocketIsFinalised)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), string.Format("Rejected Event 'WBH'. Cannot Hold or Cancel {0} as it is already Finalized.", finalisedDocket.HumanReadableName), () => ProcessEventXML(GetCustomsEventXml(Events.HoldTheWarehouseOrderCode)));
				AssertNoExceptionThrown(() => ProcessEventXML(GetCustomsEventXml(Events.WarehouseJobCanNowBeFinalisedCode)));
				AssertExceptionThrown(typeof(DataObjectReadFailureException), string.Format("Rejected Event 'WBC'. Cannot Hold or Cancel {0} as it is already Finalized.", finalisedDocket.HumanReadableName), () => ProcessEventXML(GetCustomsEventXml(Events.CancelTheWarehouseJobCode)));
			}
			else
			{
				AssertCustomsSourceEventIsAcceptedOrIgnored(Events.HoldTheWarehouseOrderCode, finalisedDocket, "HeldByCustoms Event should be accepted for Change Of Ownership.");
				AssertCustomsSourceEventIsAcceptedOrIgnored(Events.WarehouseJobCanNowBeFinalisedCode, finalisedDocket, "CustomsFinaliseJob Event should be accepted for Change Of Ownership.");
				AssertCustomsSourceEventIsAcceptedOrIgnored(Events.CancelTheWarehouseJobCode, finalisedDocket, "Cancel Event should be accepted for Change Of Ownership.");
			}
		}

		protected virtual bool RejectHoldEventIfDocketIsFinalised => true;

		#endregion

		#region Implementation

		void AssertCustomsSourceEventIsAcceptedOrIgnored(string eventCode, T matchingDocket, string assertionMessage)
		{
			var eventXml = GetCustomsEventXml(eventCode);

			if (matchingDocket != null)
			{
				var logParent = ProcessEventXML(eventXml);
				AssertLogParentIsCorrect(matchingDocket, logParent, assertionMessage);
			}
			else
			{
				AssertLogParentIsNull(eventXml, assertionMessage);
			}
		}

		string GetCustomsEventXml(string eventCode)
		{
			return string.Format(@"
     <UniversalEvent>
       <Event>
         <DataContext>
           <DataSourceCollection>
             <DataSource>
               <Type>CustomsDeclaration</Type>
               <Key>B123</Key>
             </DataSource>
           </DataSourceCollection>
           <DataTargetCollection>
             <DataTarget>
               <Type>{0}</Type>
             </DataTarget>
           </DataTargetCollection>

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
			{2}
         </DataContext>

			   <EventType>{1}</EventType>
			   <EventTime>10-JUL-2010 18:00</EventTime>
			   <EventReference>Customs</EventReference>
			   <ContextCollection>
           <Context>
			       <Type>OrderNumber</Type>
			       <Value>W000001</Value>
           </Context>
			   </ContextCollection>
       </Event>
     </UniversalEvent>", DataContextType, eventCode, GetRecipientRoleCollectionString());
		}

		protected abstract DataContextType DataContextType { get; }

		protected abstract T GetNewDocket(TestDataSimpleEnvironment data, ZString externalReference);
		protected abstract T GetNewFinalisedDocket(TestDataSimpleEnvironment data, ZString externalReference, ZString customsParentReference);

		protected abstract string GetRecipientRoleCollectionString();

		#endregion
	}
}
