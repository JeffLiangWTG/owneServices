using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	class OutturnDataEventParentFinderTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void Test_CargoReceiptDateIsPopulatedOnMatchingOutturns_FoundMatchingOutturns()
		{
			var matchingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			matchingDepotAddress.OA_Code = "Matching Depot Code";

			var nonMatchingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			nonMatchingDepotAddress.OA_Code = "Non-Matching Depot Code";

			var matchingCTNOutturn = CreateCusOutturnForTesting("Header1", matchingDepotAddress.PK, "MatchingCTN", "MatchingMB");
			var matchingCTNDifferentMasterBill = CreateCusOutturnForTesting("Header2", matchingDepotAddress.PK, "MatchingCTN", "NonMatchingMB");
			var unMatchingContainerSameMasterBill = CreateCusOutturnForTesting("Header3", matchingDepotAddress.PK, "NonMatchingCTN", "MatchingMB");
			var emptyContainer = CreateCusOutturnForTesting("Header4", matchingDepotAddress.PK, string.Empty, "MatchingMB");
			var matchingCTNNoMasterBill = CreateCusOutturnForTesting("Header5", matchingDepotAddress.PK, "MatchingCTN", string.Empty);
			var unMatchingCTNDifferntMasterBill = CreateCusOutturnForTesting("Header6", nonMatchingDepotAddress.PK, "NonMatchingCTN", "NonMatchingMB");
			var noMasterBillNoCTN = CreateCusOutturnForTesting("Header7", ZGuid.Empty, string.Empty, string.Empty);

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(TransitWarehouseGINEventXMLBuilder("MatchingCTN", matchingDepotAddress.OA_Code, "MatchingMB"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, matchingCTNOutturn.C5_CargoReceiptDate);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, matchingCTNDifferentMasterBill.C5_CargoReceiptDate);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, unMatchingContainerSameMasterBill.C5_CargoReceiptDate);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, emptyContainer.C5_CargoReceiptDate);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, matchingCTNNoMasterBill.C5_CargoReceiptDate);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, unMatchingCTNDifferntMasterBill.C5_CargoReceiptDate);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, noMasterBillNoCTN.C5_CargoReceiptDate);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null.", null, results);
			AssertEquals("Should have only found 3 business objects.", 3, results.Length);

			var matchedOutturns = (CusOutturn[])results;
			CombineAssertions(() =>
			{
				AssertType<CusOutturn[]>("Type should be Outturn", results);
				AssertContains(@"Information - Found Outturns with Premise - 'Matching Depot Code', Container Number - 'MatchingCTN'. Cargo Receipt Date has been updated to '10-Sep-2020 10:27:09'.", logger.Logs);
				AssertContainsExactElementsInAnyOrder(new[] { matchingCTNOutturn.PK, matchingCTNDifferentMasterBill.PK, matchingCTNNoMasterBill.PK }, matchedOutturns.Select(o => o.PK));
				AssertNotEquals("Cargo Receipt Date has been populated", ZDateTime.Empty, matchedOutturns[0].C5_CargoReceiptDate);
				AssertNotEquals("Cargo Receipt Date has been populated", ZDateTime.Empty, matchedOutturns[1].C5_CargoReceiptDate);
				AssertNotEquals("Cargo Receipt Date has been populated", ZDateTime.Empty, matchedOutturns[2].C5_CargoReceiptDate);
			});
		}

		public void Test_CargoReceiptDateIsNotPopulated_NoMatchingOutturns()
		{
			var matchingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			matchingDepotAddress.OA_Code = "Matching Depot Address";

			var nonMatchingOutturn = CreateCusOutturnForTesting("Header", matchingDepotAddress.PK, "NonMatchingCTN", "MB123");

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(TransitWarehouseGINEventXMLBuilder("MatchingCTN", matchingDepotAddress.OA_Code, "MB123"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
			AssertContains(@"Information - Gate In event received but did not find any matching Outturn with Premise - 'Matching Depot Address', Container Number - 'MatchingCTN' to update.", logger.Logs);
		}

		[ExpectNoExceptions]
		public void Test_CargoReceiptDateIsNotPopulated_NoContext()
		{
			var matchingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			matchingDepotAddress.OA_Code = "Matching Depot Address";

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse($@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
	</Event>
</UniversalEvent>");
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
		}

		public void Test_CargoReceiptDateIsNotPopulated_EmptyDepotCode()
		{
			string missingDepotCodeXML = $@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB12345</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>Container</Value>
			</Context>
			<Context>
				<Type>TimeOfArrival</Type>
				<Value>2020-09-10T10:27:09.643</Value>
			</Context>
			<Context>
				<Type>DepotCode</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(missingDepotCodeXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
			AssertContains(@"Information - Missing mandatory context types from Gate In event: Depot Code", logger.Logs);
		}

		public void Test_CargoReceiptDateIsNotPopulated_MissingGateInTime()
		{
			string missingDepotCodeXML = $@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB12345</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>Container</Value>
			</Context>
			<Context>
				<Type>DepotCode</Type>
				<Value>DepotCode</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(missingDepotCodeXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
			AssertContains(@"Information - Missing mandatory context types from Gate In event: Gate In Time", logger.Logs);
		}

		public void Test_CargoReceiptDateIsNotPopulated_MissingContainerNumber()
		{
			string missingDepotCodeXML = $@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB12345</Value>
			</Context>
			<Context>
				<Type>TimeOfArrival</Type>
				<Value>2020-09-10T10:27:09.643</Value>
			</Context>
			<Context>
				<Type>DepotCode</Type>
			<Value>DepotCode</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(missingDepotCodeXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
			AssertContains(@"Information - Missing mandatory context types from Gate In event: Container Code", logger.Logs);
		}

		public void Test_CargoReceiptDateIsNotPopulated_MissingMasterBill()
		{
			string missingDepotCodeXML = $@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
		<ContextCollection>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>Container</Value>
			</Context>
			<Context>
				<Type>TimeOfArrival</Type>
				<Value>2020-09-10T10:27:09.643</Value>
			</Context>
			<Context>
				<Type>DepotCode</Type>
			<Value>DepotCode</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(missingDepotCodeXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
			AssertContains(@"Information - Gate In event received but did not find any matching Outturn with Premise - 'DepotCode', Container Number - 'Container' to update.", logger.Logs);
		}

		public void Test_CargoReceiptDateIsNotPopulated_MissingMultipleRequiredContextTypes()
		{
			string missingDepotCodeXML = $@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
		<ContextCollection>
			<Context>
				<Type>DepotCode</Type>
				<Value>DepotCode</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(missingDepotCodeXML);
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Output should be empty", 0, results.Length);
			AssertContains(@"Information - Missing mandatory context types from Gate In event: Container Code, Gate In Time", logger.Logs);
		}

		EventParentFinder GetFinder(IXmlImportLogger logger = null)
		{
			return new OutturnDataEventParentFinder(Factory.BOFactory, new OutturnDataContextManager(), logger ?? new TestErrorLogger());
		}

		CusOutturn CreateCusOutturnForTesting(string voyageNumber, ZGuid outturningPremise, string containerNumber, string masterBill, string houseBill = "HB123", string cargoType = "FCL")
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_VoyageNum = voyageNumber;
			outturnHeader.C6_OA_OutturningPremise = outturningPremise;
			Factory.SaveForTesting();

			var outturn = CreateOuttrun(cargoType, containerNumber, masterBill, houseBill, outturnHeader);

			return outturn;
		}

		string TransitWarehouseGINEventXMLBuilder(string containerNumber, string depotCode, string masterBill) => $@"<UniversalEvent>
	<Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>TransitReceiveHeader</Type>
			  <Key>RTU1</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>GIN</EventType>
		<EventReference>|FAC=CFS|LOC=DSADSA|TYP=ContainerID|REF=SHIPMENT 2</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>{masterBill}</Value>
			</Context>
			<Context>
				<Type>DepotCode</Type>
				<Value>{depotCode}</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>{containerNumber}</Value>
			</Context>
			<Context>
				<Type>TimeOfArrival</Type>
			<Value>2020-09-10T10:27:09.643</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
	}
}
