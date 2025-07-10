using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class StartedReceivingProcessorTest : ReceiveActionProcessorTest
	{
		protected override void TestProcessCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			Factory.Save();

			AssertEquals("Precondition", false, receive.StartedReceiving);

			var generator = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			generator.Process(notifications);

			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(1, receive.AsnLines.Count);

			var asnLine = receive.AsnLines[0];
			CombineAssertions(() =>
			{
				AssertEquals(1, asnLine.WN_LineNo);
				AssertEquals(0, asnLine.WN_SubLineNo);
				AssertEquals(data.Part1.PK, asnLine.WN_OP);
				AssertEquals("UNT", asnLine.WN_QuantityUQ);
				AssertEquals(10m, asnLine.WN_Quantity);
			});
		}

		public void TestProcess_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "XXX";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			receiveLine.WE_PartAttrib1 = "P1";
			receiveLine.WE_PartAttrib2 = "P2";
			receiveLine.WE_PartAttrib3 = "P3";
			receiveLine.WE_SerialNumber = "S123";
			receiveLine.WE_PackingDate = ZDate.Today;
			receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(1);
			receiveLine.WE_PalletID = "P123";
			Factory.Save();

			AssertEquals("Precondition", false, receive.StartedReceiving);

			var generator = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			generator.Process(notifications);

			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(1, receive.AsnLines.Count);

			var asnLine = receive.AsnLines[0];
			CombineAssertions(() =>
			{
				AssertEquals(data.Part1.PK, asnLine.WN_OP);
				AssertEquals("P1", asnLine.WN_PartAttrib1);
				AssertEquals("P2", asnLine.WN_PartAttrib2);
				AssertEquals("P3", asnLine.WN_PartAttrib3);
				AssertEquals("S123", asnLine.WN_SerialNumber);
				AssertEquals(ZDate.Today, asnLine.WN_PackingDate);
				AssertEquals(ZDate.Today.AddDays(1), asnLine.WN_ExpiryDate);
				AssertEquals("P123", asnLine.WN_PalletId);
				AssertEquals("XXX", asnLine.WN_QuantityUQ);
				AssertEquals(1, asnLine.WN_LineNo);
				AssertEquals(0, asnLine.WN_SubLineNo);
				AssertEquals(10m, asnLine.WN_Quantity);
			});
		}

		public void TestProcess_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var product1 = Helper.CreateProduct("Product1", data.Org1);
			var product2 = Helper.CreateProduct("Product2", data.Org1);
			var product3 = Helper.CreateProduct("Product3", data.Org1);

			Helper.CreateWhsReceiveLine(receive, product1, 10);
			Helper.CreateWhsReceiveLine(receive, product2, 20);
			Helper.CreateWhsReceiveLine(receive, product3, 30);
			Factory.Save();

			AssertEquals("Precondition", false, receive.StartedReceiving);

			var generator = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			generator.Process(notifications);

			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(3, receive.AsnLines.Count);

			var asnLine1 = receive.AsnLines.Cast<WhsAsnLine>().Single(l => l.WN_OP == product1.PK);
			CombineAssertions(() =>
			{
				AssertEquals(1, asnLine1.WN_LineNo);
				AssertEquals(0, asnLine1.WN_SubLineNo);
				AssertEquals(product1.PK, asnLine1.WN_OP);
				AssertEquals("UNT", asnLine1.WN_QuantityUQ);
				AssertEquals(10m, asnLine1.WN_Quantity);
			});

			var asnLine2 = receive.AsnLines.Cast<WhsAsnLine>().Single(l => l.WN_OP == product2.PK);
			CombineAssertions(() =>
			{
				AssertEquals(2, asnLine2.WN_LineNo);
				AssertEquals(0, asnLine2.WN_SubLineNo);
				AssertEquals(product2.PK, asnLine2.WN_OP);
				AssertEquals("UNT", asnLine2.WN_QuantityUQ);
				AssertEquals(20m, asnLine2.WN_Quantity);
			});

			var asnLine3 = receive.AsnLines.Cast<WhsAsnLine>().Single(l => l.WN_OP == product3.PK);
			CombineAssertions(() =>
			{
				AssertEquals(3, asnLine3.WN_LineNo);
				AssertEquals(0, asnLine3.WN_SubLineNo);
				AssertEquals(product3.PK, asnLine3.WN_OP);
				AssertEquals("UNT", asnLine3.WN_QuantityUQ);
				AssertEquals(30m, asnLine3.WN_Quantity);
			});
		}

		public void TestProcess_ArrivalDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Factory.Save();

			AssertEquals("Precondition", true, receive.WD_ArrivalDate.IsEmpty);

			var generator = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			generator.Process(notifications);

			AssertEquals(false, receive.WD_ArrivalDate.IsEmpty);
		}

		public void TestDBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 30);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 40);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 20);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 30);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 40);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 50);
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				var generator = GetProcessor(receiveInNewFactory);
				var notifications = new NotificationBuffer();
				generator.Process(notifications);
			}
		}

		protected override ReceiveActionProcessor GetProcessor(WhsReceive receive)
		{
			return new StartedReceivingProcessor(receive);
		}

		protected override ZString ExpectedActionName => "started receiving";
	}
}
