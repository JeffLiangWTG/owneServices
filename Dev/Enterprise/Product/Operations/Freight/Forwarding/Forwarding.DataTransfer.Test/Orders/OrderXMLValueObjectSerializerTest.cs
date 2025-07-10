using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderXMLValueObjectSerializerTest : TestCaseWithFactory
	{
		public void TestDoesntUpdateAttachedOrderButAddsToReport()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			Stream fileStream = GetType().Assembly.GetManifestResourceStream(TestResourcePath);

			try
			{
				//Set group and edi comms so report delivery creates a report
				GlbGroup group = Factory.New<GlbGroup>();
				var staff = group.Staff.AddNew();
				staff.GS_Code = "ZAC";
				staff.GS_FullName = "Test";
				NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

				EDICommunicationsMode mode = Factory.New<EDICommunicationsMode>();
				mode.EK_Module = WorkflowDescriptors.OrderWorkflowDescriptorCode;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.OrderImportReport;
				mode.EK_Destination = "unit.test@cw1.com";

				Order order11 = Factory.New<Order>();
				order11.JD_OrderNumber = "11111";

				OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABNAMR"));

				header.EDICommunicationsModes.Add(mode);

				AssertNotNull("Couldn't find the OrgHeader with OH_Code [ABNAMR]", header);
				order11.BuyerPK = header.PK;
				order11.OrderLines.AddNew().JO_InnerPacks = 5;
				order11.JD_OrderDate = ZDateTime.Now.AddYears(-10);

				Order order33 = Factory.New<Order>();
				order33.JD_OrderNumber = "3";
				order33.JD_OrderDate = ZDateTime.Now.AddYears(-78);

				//Set edi comms so report delivery creates a report
				mode = Factory.New<EDICommunicationsMode>();
				mode.EK_Module = WorkflowDescriptors.OrderWorkflowDescriptorCode;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.OrderImportReport;
				mode.EK_Destination = "unit.test@cw1.com";

				header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ADDABL"));
				header.EDICommunicationsModes.Add(mode);

				AssertNotNull("Couldn't find the OrgHeader with OH_Code [ADDABL]", header);
				order33.BuyerPK = header.PK;
				Factory.Save();

				var buffer = new NotificationBuffer();
				OrderXMLValueObjectSerializer xm = new OrderXMLValueObjectSerializer();
				OrderCollection collection = new OrderCollection(Factory);
				xm.ImportXmlData(fileStream, new OrderValueObjectDataAdapter(), collection, null, buffer);
				fileStream.Seek(0, SeekOrigin.Begin);

				AssertEquals("should add both orders to report", 2, xm.importedOrdersForTest.Count);
				AssertEquals("should add order33 to report", order33.JD_OrderNumberAndSplit, xm.importedOrdersForTest[0].JD_OrderNumberAndSplit.Value);
				AssertEquals("should add order11 to report", order11.JD_OrderNumberAndSplit, xm.importedOrdersForTest[1].JD_OrderNumberAndSplit.Value);

				AssertEquals("Should've updated order11", new ZDateTime("2008-07-07T15:05:40.337+10:00"), order11.JD_OrderDate);
				AssertEquals("Should've added order lines for order11", 3, order11.OrderLines.Count);
				AssertEquals("Should've updated order11 orderlines", (ZDecimal)0, order11.OrderLines[0].JO_InnerPacks);
				AssertEquals("Should've updated order11 orderlines", (ZDecimal)0, order11.OrderLines[1].JO_InnerPacks);
				AssertEquals("Should've updated order11 orderlines", (ZDecimal)0, order11.OrderLines[2].JO_InnerPacks);
				AssertEquals("Should've updated order33", new ZDateTime("2008-06-24T13:26:00+10:00"), order33.JD_OrderDate);
				AssertEquals("Should've added order lines for order33", 1, order33.OrderLines.Count);

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				order11.JD_JS = shipment.PK;
				ZDateTime newOrderDate = ZDateTime.Now.AddYears(-10);
				order11.JD_OrderDate = newOrderDate;
				order11.OrderLines[0].Delete();
				order11.OrderLines[0].JO_InnerPacks = 11;
				order11.OrderLines[1].JO_InnerPacks = 22;

				order33.JD_OrderDate = ZDateTime.Now.AddYears(-6);
				order33.OrderLines.DeleteAll();

				Factory.Save();

				xm.ImportXmlData(fileStream, new OrderValueObjectDataAdapter(), collection, null, buffer);

				AssertEquals("should add both orders to report", 2, xm.importedOrdersForTest.Count);
				AssertEquals("should add order33 to report", order33.JD_OrderNumberAndSplit, xm.importedOrdersForTest[0].JD_OrderNumberAndSplit.Value);
				AssertEquals("should add order11 to report", order11.JD_OrderNumberAndSplit, xm.importedOrdersForTest[1].JD_OrderNumberAndSplit.Value);

				AssertEquals("Should not update order it has a shipment", newOrderDate, order11.JD_OrderDate);
				AssertEquals("Should not update order11 order lines it has a shipment", 2, order11.OrderLines.Count);
				AssertEquals("Should not update order11 order lines it has a shipment", (ZDecimal)11, order11.OrderLines[0].JO_InnerPacks);
				AssertEquals("Should not update order11 order lines it has a shipment", (ZDecimal)22, order11.OrderLines[1].JO_InnerPacks);

				AssertEquals("Should update order33", new ZDateTime("2008-06-24T13:26:00+10:00"), order33.JD_OrderDate);
				AssertEquals("Should've added order lines for order33", 1, order33.OrderLines.Count);
				AssertEquals("Should call ImportedOrderChangesDocumentManager twice to create two reports", 2, Factory.Load<StmPrintJob>(new ZQuery()).Length);
				AssertContains("Should update order11 once", "Order 11111 updated", buffer.AsString);
				AssertContains("Should not update order11 2nd time", "Warning: Order 11111 is linked to a shipment and cannot be updated.", buffer.AsString);
			}
			finally
			{
				fileStream.Close();
				fileStream.Dispose();
			}
		}

		public void TestHandleArgumentExceptionInImport()
		{
			using (MemoryStream memStream = new MemoryStream())
			using (Stream fileStream = GetType().Assembly.GetManifestResourceStream(TestResourcePath))
			{
				StreamWriter writer = new StreamWriter(memStream);
				StreamReader reader = new StreamReader(memStream);

				fileStream.CopyTo(memStream);

				try
				{
					var buffer = new NotificationBuffer();
					var xm = new OrderXMLValueObjectSerializer();
					var collection = new OrderCollection(Factory);
					var dataAdapter = new Mock<OrderValueObjectDataAdapter>(MockBehavior.Strict);

					dataAdapter.Setup(o => o.RootElementName).Returns("Order");

					dataAdapter.Setup(o => o.RootCollectionElementName)
						.Throws(new ArgumentException("[TEST] An incorrect character was found"));

					memStream.Seek(500, SeekOrigin.Begin);
					writer.Write((char)0);
					writer.Flush();
					memStream.Seek(500, SeekOrigin.Begin);

					Exception e = null;
					try
					{
						xm.ImportXmlData(memStream, dataAdapter.Object, collection, null, buffer);
					}
					catch (Exception ex)
					{
						e = ex;
					}

					AssertNotNull("Exception should have been caught", e);
					AssertEquals("Argument exception should have been caught", typeof(ArgumentException), e.GetType());
					Assert("Argument exception should have been caught and rethrown with more information about the error", e.Message.StartsWith("Unexpected error occurred importing XML"));
				}
				finally
				{
					fileStream.Close();
					fileStream.Dispose();
				}
			}
		}

		const string TestResourcePath = "Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.TwoOrders.xml";
	}
}
