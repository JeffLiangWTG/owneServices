using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class OrderLineEventParentFinderTest : TestCaseWithFactory
	{
		public void TestMatchGetsBestMatch()
		{
			#region Event XML

			const string orderLineEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>ShippersReference</Type>
				<Value>REFERME123</Value>
			</Context>
			<Context>
				<Type>CommercialInvoiceNumber</Type>
				<Value>INVOICE2</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>ONTHEHOUSE</Value>
			</Context>
			<Context>
				<Type>OrderLineNumber</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>OrderLineSubLineNumber</Type>
				<Value>2</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			#endregion

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_Waybill = "ONTHEHOUSE";
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_InvoiceNumber = "INVOICE2";
			var orderLine = matchingOrder.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			var dummyOrder = Factory.NewWithValidTestData<Order>();
			dummyOrder.JD_Waybill = "ONTHEHOUSE";
			dummyOrder.JD_BookingConfRef = "REFERME123";
			var orderLine2 = dummyOrder.OrderLines.AddNew();
			orderLine2.JO_LineNo = 1;
			orderLine2.JO_SubLineNo = 2;

			Factory.Save();

			var logParent = ProcessEventXML(orderLineEventXmlText).FirstOrDefault() as OrderLine;
			AssertNotNull(logParent);
			AssertEquals(orderLine.PK, logParent.PK);
		}

		public void TestNoOrderLineMatched()
		{
			#region Event XML

			const string orderLineEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>ShippersReference</Type>
				<Value>REFERME123</Value>
			</Context>
			<Context>
				<Type>CommercialInvoiceNumber</Type>
				<Value>INVOICE2</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>ONTHEHOUSE</Value>
			</Context>
			<Context>
				<Type>OrderLineNumber</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>OrderLineSubLineNumber</Type>
				<Value>3</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			#endregion

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_Waybill = "ONTHEHOUSE";
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_InvoiceNumber = "INVOICE2";
			var orderLine = matchingOrder.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			var dummyOrder = Factory.NewWithValidTestData<Order>();
			dummyOrder.JD_Waybill = "ONTHEHOUSE";
			dummyOrder.JD_BookingConfRef = "REFERME123";
			var orderLine2 = dummyOrder.OrderLines.AddNew();
			orderLine2.JO_LineNo = 1;
			orderLine2.JO_SubLineNo = 2;

			Factory.Save();

			var logParents = ProcessEventXML(orderLineEventXmlText);
			AssertNull(logParents);
		}

		#region Implementation

		BusinessObject[] ProcessEventXML(string orderLevelEventXmlText)
		{
			var subscriber = new OrderLineEventParentFinder(Factory, new OrderLineDataContextManager(), new DummyLogger());
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(orderLevelEventXmlText);
			return subscriber.GetLogParentsForEvent(xmlEvent);
		}

		#endregion
	}
}
