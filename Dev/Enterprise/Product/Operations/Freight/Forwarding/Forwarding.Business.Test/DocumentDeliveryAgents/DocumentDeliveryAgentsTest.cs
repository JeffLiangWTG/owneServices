using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DocumentDeliveryAgents))]
	sealed class DocumentDeliveryAgentsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DeliveryAgentToSelectFromForPrintingCollection collection = new DeliveryAgentToSelectFromForPrintingCollection(Factory);
			return new DocumentDeliveryAgents(collection);
		}

		public void TestDeliveryAgentsToSelectFrom()
		{
			DeliveryAgentToSelectFromForPrinting deliveryAgent1 = Factory.New<DeliveryAgentToSelectFromForPrinting>();
			DeliveryAgentToSelectFromForPrinting deliveryAgent2 = Factory.New<DeliveryAgentToSelectFromForPrinting>();
			DeliveryAgentToSelectFromForPrintingCollection deliveryAgentCollection = new DeliveryAgentToSelectFromForPrintingCollection(Factory);

			deliveryAgentCollection.Add(deliveryAgent1);
			deliveryAgentCollection.Add(deliveryAgent2);

			DocumentDeliveryAgents docDeliveryAgents = new DocumentDeliveryAgents(deliveryAgentCollection);
			AssertNotNull("Delivery Agents To Select From is not null", docDeliveryAgents.DeliveryAgentsToSelectFrom);
			AssertEquals("Delivery Agents To Select From Count", 2, docDeliveryAgents.DeliveryAgentsToSelectFrom.Count);
		}

		public void TestDeliveryAgentsToPrint()
		{
			DeliveryAgentToSelectFromForPrinting deliveryAgent1 = Factory.New<DeliveryAgentToSelectFromForPrinting>();
			DeliveryAgentToSelectFromForPrinting deliveryAgent2 = Factory.New<DeliveryAgentToSelectFromForPrinting>();
			deliveryAgent2.OH_Calc_PrintDocumentForDeliveryAgent = ZBool.False;
			DeliveryAgentToSelectFromForPrintingCollection deliveryAgentCollection = new DeliveryAgentToSelectFromForPrintingCollection(Factory);

			deliveryAgentCollection.Add(deliveryAgent1);
			deliveryAgentCollection.Add(deliveryAgent2);

			DocumentDeliveryAgents docDeliveryAgents = new DocumentDeliveryAgents(deliveryAgentCollection);
			AssertNotNull("Delivery Agents To Print is not null", docDeliveryAgents.DeliveryAgentsToPrint);
			AssertEquals("Delivery Agents To Print Count", 1, docDeliveryAgents.DeliveryAgentsToPrint.Count);
		}
	}
}
