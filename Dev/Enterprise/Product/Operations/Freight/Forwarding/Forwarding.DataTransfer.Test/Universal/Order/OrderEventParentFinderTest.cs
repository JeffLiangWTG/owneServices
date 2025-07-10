using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class OrderEventParentFinderTest : TestCaseWithFactory
	{
		public void TestMatchGetsBestMatch()
		{
			const string orderLevelEventXmlText = @"
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
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_Waybill = "ONTHEHOUSE";
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_InvoiceNumber = "INVOICE2";

			Factory.Save();

			Thread.Sleep(200);

			var dummyOrder = Factory.NewWithValidTestData<Order>();
			dummyOrder.JD_Waybill = "ONTHEHOUSE";
			dummyOrder.JD_BookingConfRef = "REFERME123";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestMatchGetsLatestWhenEqualMatches()
		{
			const string orderLevelEventXmlText = @"
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
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var dummyOrder = Factory.NewWithValidTestData<Order>();
			dummyOrder.JD_BookingConfRef = "REFERME123";

			Factory.Save();

			Thread.Sleep(200);

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_BookingConfRef = "REFERME123";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestMatchFallsBackToPortOfDestinationWhenEqualMatches()
		{
			const string orderLevelEventXmlText = @"
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
				  <Type>HBOLDestinationUNLOCO</Type>
				  <Value>NZCHC</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_BookingConfRef = "REFERME123";
			matchingOrder.JD_RL_NKGoodsDeliveredTo = "NZCHC";

			Factory.Save();

			Thread.Sleep(200);

			var dummyOrder = Factory.NewWithValidTestData<Order>();
			dummyOrder.JD_BookingConfRef = "REFERME123";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestMatchFallsBackToPortOfOriginWhenEqualMatches()
		{
			const string orderLevelEventXmlText = @"
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
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUMEL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_BookingConfRef = "REFERME123";
			matchingOrder.JD_RL_NKGoodsAvailableAt = "AUMEL";

			Factory.Save();

			Thread.Sleep(200);

			var dummyOrder = Factory.NewWithValidTestData<Order>();
			dummyOrder.JD_BookingConfRef = "REFERME123";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestDoesNotMatchOnHouseBillLevelAirOrder()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>ONTHEHOUSE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Air;
			order.JD_Waybill = "ONTHEHOUSE";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		public void TestMatchOnAirHouseBill()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HAWBNumber</Type>
				  <Value>ONTHEHOUSE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_TransportMode = Constants.TransportModes.Air;
			matchingOrder.JD_Waybill = "ONTHEHOUSE";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestDoesNotMatchOnHouseBillLevelSeaOrder()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HAWBNumber</Type>
				  <Value>ONTHEHOUSE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_Waybill = "ONTHEHOUSE";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		public void TestMatchOnSeaHouseBill()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>ONTHEHOUSE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_TransportMode = Constants.TransportModes.Sea;
			matchingOrder.JD_Waybill = "ONTHEHOUSE";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestDoesNotMatchOnMasterLevelAirOrder()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>IAMTHEMASTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Air;
			order.JD_MasterWaybill = "IAMTHEMASTER";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		public void TestMatchOnAirMasterBill()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>IAMTHEMASTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_TransportMode = Constants.TransportModes.Air;
			matchingOrder.JD_MasterWaybill = "IAMTHEMASTER";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestDoesNotMatchOnMasterLevelSeaOrder()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MAWBNumber</Type>
				  <Value>IAMTHEMASTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_MasterWaybill = "IAMTHEMASTER";

			AssertLogParentIsNull(orderLevelEventXmlText);
		}

		public void TestMatchOnSeaMasterBill()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>IAMTHEMASTER</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_TransportMode = Constants.TransportModes.Sea;
			matchingOrder.JD_MasterWaybill = "IAMTHEMASTER";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestMatchOnInvoiceNumber()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CommercialInvoiceNumber</Type>
				  <Value>INVOICE2</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_InvoiceNumber = "INVOICE2";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestMatchOnBookingConfirmationReference()
		{
			const string orderLevelEventXmlText = @"
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
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";
			matchingOrder.JD_BookingConfRef = "REFERME123";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		public void TestMatchOnOrderNumber()
		{
			const string orderLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>OrderNumber</Type>
				  <Value>P000001</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = "P000001";

			var logParent = ProcessEventXML(orderLevelEventXmlText);

			AssertLogParentIsCorrect(matchingOrder, logParent);
		}

		#region Implementation

		static void AssertLogParentIsCorrect(Order matchingOrder, BusinessObject logParent)
		{
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(Order), logParent.GetType());

			var logParentOrder = logParent as Order;

			AssertNotNull("logParentOrder", logParentOrder);
			AssertEquals("Should get best matching Order.", GetHumanReadableID(matchingOrder), GetHumanReadableID(logParentOrder));
		}

		void AssertLogParentIsNull(string orderLevelEventXmlText)
		{
			Factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(orderLevelEventXmlText);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertNull("logParents", logParents);
		}

		OrderEventParentFinder GetNewEventParentFinder()
		{
			return new OrderEventParentFinder(Factory, new OrderManagerOrderDataContextManager(), new DummyLogger());
		}

		BusinessObject ProcessEventXML(string orderLevelEventXmlText)
		{
			Factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(orderLevelEventXmlText);
			return subscriber.GetLogParentsForEvent(xmlEvent)[0];
		}

		static string GetHumanReadableID(BusinessObject businessObject)
		{
			return businessObject.HumanReadableName + " - PK: " + businessObject.PK;
		}

		#endregion
	}
}
