using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class CsvOrderDataImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbGroup group = newFactory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_EmailAddress = "zac@cargowise.com";
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			newFactory.Save();

			int orderCount = Factory.GetDatabaseCount(typeof(Order));
			AssertEquals("Precondition", 0, orderCount);

			NotificationBuffer buffer = new NotificationBuffer();
			CsvOrderDataImporter importer = new CsvOrderDataImporter();

			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testDocumentPath = resourceRetriever.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.Order.csv");
				importer.ImportData(testDocumentPath, buffer, SourceInfo.EmptySourceInfo);
			}

			Order[] loadedOrders = Factory.Load<Order>(new ZQuery());

			AssertEquals("2 new order should have been loaded", 2, loadedOrders.Length);
			AssertEquals("Order number should be", "123456", loadedOrders[0].JD_OrderNumber);
			AssertEquals("Order has only one order line", 2, loadedOrders[0].OrderLines.Count);
			AssertEquals("Order number should be", "654321", loadedOrders[1].JD_OrderNumber);
			AssertEquals("Order has only one order line", 1, loadedOrders[1].OrderLines.Count);

			AssertEquals("should add both orders to report", 2, importer.importedOrdersForTest.Count);
			AssertEquals("order1", loadedOrders[0].JD_OrderNumberAndSplit, importer.importedOrdersForTest[0].JD_OrderNumberAndSplit.Value);
			AssertEquals("order2", loadedOrders[1].JD_OrderNumberAndSplit, importer.importedOrdersForTest[1].JD_OrderNumberAndSplit.Value);
			AssertEquals("Should call ImportedOrderChangesDocumentManager and a report", 1, Factory.Load<DocumentEngine.Scheduler.Business.StmPrintJob>(new ZQuery()).Length);
		}

		public void TestImport_InvalidData()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbGroup group = newFactory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_EmailAddress = "zac@cargowise.com";
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			newFactory.Save();

			int orderCount = Factory.GetDatabaseCount(typeof(Order));
			AssertEquals("Precondition", 0, orderCount);

			NotificationCollection notifications = new NotificationCollection();
			CsvOrderDataImporter importer = new CsvOrderDataImporter();

			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testDocumentPath = resourceRetriever.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.InvalidOrder.csv");
				importer.ImportData(testDocumentPath, notifications, SourceInfo.EmptySourceInfo);
			}

			Order[] loadedOrders = Factory.Load<Order>(new ZQuery());

			AssertEquals("Two valid orders loaded", 2, loadedOrders.Length);
			AssertEquals("Order number should be", "123456", loadedOrders[0].JD_OrderNumber);
			AssertEquals("Order number should be", "654321", loadedOrders[1].JD_OrderNumber);

			AssertEquals("Two orders on report", 2, importer.importedOrdersForTest.Count);
			AssertEquals("Should call ImportedOrderChangesDocumentManager and a report", 1, Factory.Load<DocumentEngine.Scheduler.Business.StmPrintJob>(new ZQuery()).Length);
			Assert("Error notification for order 45678 should be present", notifications.Any(x => x.Message == "Error: Order 45678 could not be imported due to invalid data."));
		}
	}
}
