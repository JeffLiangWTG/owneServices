using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class EDIMessageContentFilterLineLookupsTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Filter = Factory.New<EDIMessageContentFilter>();
		}

		EDIMessageContentFilter Filter { get; set; }
		public void TestAllSchemaElements()
		{
			AssertEquals(true, Filter.UniversalEvent.Lines.AddNew().Lookups.AllSchemaElements.ContainsCode("ContextCollection"));
			AssertEquals(false, Filter.UniversalEvent.Lines.AddNew().Lookups.AllSchemaElements.ContainsCode("RelatedShipmentCollection"));
			AssertEquals(true, Filter.UniversalShipment.Lines.AddNew().Lookups.AllSchemaElements.ContainsCode("RelatedShipmentCollection"));
			AssertEquals(true, Filter.UniversalTransaction.Lines.AddNew().Lookups.AllSchemaElements.ContainsCode("TransactionCollection"));
		}

		public void TestSchemaElement_MaxLength()
		{
			var max = Filter.UniversalEvent.Lines.AddNew().Lookups.AllSchemaElements.GetAllCodes().Select(str => str.Length).Max();
			var max2 = Filter.UniversalShipment.Lines.AddNew().Lookups.AllSchemaElements.GetAllCodes().Select(str => str.Length).Max();
			var max3 = Filter.UniversalTransaction.Lines.AddNew().Lookups.AllSchemaElements.GetAllCodes().Select(str => str.Length).Max();
			AssertLessThan("If this fails we need to update max length of ElementName in Filter : IDataObject", (new[] { max, max2, max3 }).Max(), 50);
		}

		public void TestAllSchemaElements_ShouldNotHaveDuplicates()
		{
			var eventElements = Filter.UniversalEvent.Lines.AddNew().Lookups.AllSchemaElements;
			AssertEquals("Event schema Elements should not have duplicates", eventElements.ToArray().Distinct().Count(), eventElements.ToArray().Length);

			var shipmentElements = Filter.UniversalShipment.Lines.AddNew().Lookups.AllSchemaElements;
			AssertEquals("Shipment schema Elements should not have duplicates", shipmentElements.ToArray().Distinct().Count(), shipmentElements.ToArray().Length);

			var transactionElements = Filter.UniversalTransaction.Lines.AddNew().Lookups.AllSchemaElements;
			AssertEquals("Transaction schema Elements should not have duplicates", transactionElements.ToArray().Distinct().Count(), transactionElements.ToArray().Length);
		}

		public void TestSchemaElements()
		{
			AssertEquals(true, Filter.UniversalEvent.Lines.AddNew().Lookups.SchemaElements.ContainsCode("ContextCollection"));
			AssertEquals(false, Filter.UniversalEvent.Lines.AddNew().Lookups.SchemaElements.ContainsCode("RelatedShipmentCollection"));
			AssertEquals(true, Filter.UniversalShipment.Lines.AddNew().Lookups.SchemaElements.ContainsCode("RelatedShipmentCollection"));
			AssertEquals(true, Filter.UniversalTransaction.Lines.AddNew().Lookups.AllSchemaElements.ContainsCode("TransactionCollection"));
		}

		public void TestSchemaElements_RemoveAll()
		{
			foreach (var element in Filter.UniversalEvent.Lines.AddNew().Lookups.AllSchemaElements.GetAllCodes())
			{
				var line = Filter.UniversalEvent.Lines.AddNew();
				line.SchemaElement = element;
			}

			AssertNotEquals(1, Filter.UniversalEvent.Lines.Count);
			AssertEquals(0, Filter.UniversalEvent.Lines.AddNew().Lookups.SchemaElements.Count);
		}

		public void TestDataContexts()
		{
			var line = Filter.UniversalShipment.Lines.AddNew();
			line.SchemaElement = "SubShipmentCollection";
			AssertEquals("Shipment", line.ElementType);
			AssertEquals(true, line.Lookups.DataContexts.ContainsCode("HVLVConsignment"));
		}
	}
}
