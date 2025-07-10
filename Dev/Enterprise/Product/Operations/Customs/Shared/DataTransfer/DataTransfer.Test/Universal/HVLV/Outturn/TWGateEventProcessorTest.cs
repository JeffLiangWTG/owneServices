using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class TWGateEventProcessorTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void TestConstructor_LoggerNotNull()
		{
			var universalEvent = new Event();
			AssertExceptionThrown<ArgumentNullException>(() => new TWGateEventProcessor(universalEvent, null));
		}

		public void TestConstructor_EventObjectNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TWGateEventProcessor(null, new TestErrorLogger()));
		}

		public void Test_PopulateCargoReceiptDateOnOutturns()
		{
			var matchingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			matchingDepotAddress.OA_Code = "Matching Depot Code";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_VoyageNum = "Voyage1";
			outturnHeader.C6_OA_OutturningPremise = matchingDepotAddress.PK;
			Factory.SaveForTesting();

			var outturn = CreateOuttrun("FCL", "CTN1", "MB12345", "HB123", outturnHeader);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, outturn.C5_CargoReceiptDate);

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(ginEventWithGateInTime);
			var processor = new TWGateEventProcessor(xmlEvent, logger);
			processor.Process(outturn);

			AssertNotEquals("Cargo Receipt Date has been populated", ZDateTime.Empty, outturn.C5_CargoReceiptDate);
			ZDateTime.TryParseExact(GateInTimeForTesting, out var expectedGateInTime, "yyyy-MM-dd hh:mm:ss");
			AssertEquals("Cargo Receipt Date has been populated", expectedGateInTime, outturn.C5_CargoReceiptDate);
		}

		public void Test_ShouldNotPopulateCargoReceiptDateOnOutturns_NoGateInTime()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_VoyageNum = "Voyage1";
			Factory.SaveForTesting();

			var outturn = CreateOuttrun("FCL", "CTN1", "MB12345", "HB123", outturnHeader);
			AssertEquals("Cargo Receipt Date hasn't been populated yet", ZDateTime.Empty, outturn.C5_CargoReceiptDate);

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(GinEventWithoutGateInTime);
			var processor = new TWGateEventProcessor(xmlEvent, logger);
			processor.Process(outturn);

			AssertEquals("Cargo Receipt Date hasn't been populated", ZDateTime.Empty, outturn.C5_CargoReceiptDate);
		}

		const string GateInTimeForTesting = "2020-09-10 10:10:10";

		readonly string ginEventWithGateInTime = $@"<UniversalEvent>
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
				<Type>DepotCode</Type>
				<Value>Matching Depot Code</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CTN1</Value>
			</Context>
			<Context>
				<Type>TimeOfArrival</Type>
			<Value>{GateInTimeForTesting}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string GinEventWithoutGateInTime = @"<UniversalEvent>
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
				<Type>DepotCode</Type>
				<Value>Matching Depot Code</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>CTN1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
	}
}
