using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(DeliveryLabelLineCollection))]
	class DeliveryLabelLineCollectionTest : WhsDocketLabelLineCollectionTest<DeliveryLabelLineCollection>
	{
		public override void TestCollection()
		{
			var docket = Factory.New<WhsOrder>();
			docket.WD_ExternalReference = "ExtRef";
			var collection = new WhsOrderCollection(Factory, new AdhocCollectionRelationship(typeof(WhsOrder)));

			var labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("No lines", 0, labelLines.Count);

			collection.Add(docket);
			docket.WD_UnitsSent = 10;
			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("total labels", 10, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 10, labelLines[0].NumberOfLabelsToPrint);

			docket.WD_PackagesSent = 13;
			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("total labels", 13, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 13, labelLines[0].NumberOfLabelsToPrint);

			docket.WD_PalletsSent = 9;
			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("total labels", 9, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 9, labelLines[0].NumberOfLabelsToPrint);

			var docket2 = Factory.New<WhsOrder>();
			docket2.WD_ExternalReference = "ExtRef2";
			collection.Add(docket2);

			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("2 lines", 2, labelLines.Count);

			docket2.WD_UnitsSent = 11;
			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("total labels", 11, labelLines[1].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 11, labelLines[1].NumberOfLabelsToPrint);

			docket2.WD_PackagesSent = 14;
			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("total labels", 14, labelLines[1].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 14, labelLines[1].NumberOfLabelsToPrint);

			docket2.WD_PalletsSent = 15;
			labelLines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("total labels", 9, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 9, labelLines[0].NumberOfLabelsToPrint);
			AssertEquals("total labels", 15, labelLines[1].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 15, labelLines[1].NumberOfLabelsToPrint);
		}

		public void TestOrders()
		{
			WhsOrder order = null;
			var labelLines = new DeliveryLabelLineCollection(order, Factory);
			AssertEquals("No lines", 0, labelLines.Count);

			order = Factory.New<WhsOrder>();
			order.WD_ExternalReference = "ExtRef";
			order.WD_UnitsSent = 10;
			labelLines = new DeliveryLabelLineCollection(order, Factory);
			AssertEquals("total labels", 10, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 10, labelLines[0].NumberOfLabelsToPrint);

			order.WD_PackagesSent = 13;
			labelLines = new DeliveryLabelLineCollection(order, Factory);
			AssertEquals("total labels", 13, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 13, labelLines[0].NumberOfLabelsToPrint);

			order.WD_PalletsSent = 9;
			labelLines = new DeliveryLabelLineCollection(order, Factory);
			AssertEquals("total labels", 9, labelLines[0].TotalNumberOfLabels);
			AssertEquals("Default printable labels", 9, labelLines[0].NumberOfLabelsToPrint);
		}

		public void TestCollectionWithWorkOrders()
		{
			var docket = Factory.New<WhsWorkOrder>();
			docket.WD_ExternalReference = "ExtRef";
			var collection = new WhsWorkOrderCollection(Factory);

			var lines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("No lines", 0, lines.Count);

			docket.WD_PackagesSent = 10;
			lines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("Still no lines", 0, lines.Count);

			var line = docket.Lines.AddNew();
			lines = new DeliveryLabelLineCollection(collection, Factory);
			AssertEquals("No lines as Work Orders are not supported", 0, lines.Count);
		}

		protected override DeliveryLabelLineCollection GetCollectionToTest()
		{
			var collection = new WhsOrderCollection(Factory);
			return new DeliveryLabelLineCollection(collection, Factory);
		}
	}
}
