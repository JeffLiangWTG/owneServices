using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveProductSummaryCollection))]
	abstract class WhsReceiveProductSummaryCollectionTest : WhsNonPersistentBusinessObjectCollectionTestCase<WhsReceiveProductSummaryCollection>
	{
		#region TestBuildCollection

		public void TestBuildCollection_NullReceive()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Receives should throw expection",
				() => new WhsReceiveProductSummaryCollection(Factory, null));
		}

		public void TestBuildCollection_NoReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has no summaries", 0, collection.Count);
		}

		public void TestBuildCollection_ProductPopulatedCorrectly_OneProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 7m);
			Factory.Save();

			MakeASNsIfRequired(receive);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			var summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Product Description on summary", "P1", summary.ProductDescription);
		}

		public void TestBuildCollection_ProductPopulatedCorrectly_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 7m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 15m);

			MakeASNsIfRequired(receive);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			var summary1 = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Product Description on summary1", "P1", summary1.ProductDescription);
			var summary2 = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("Collection has correct Product Description on summary2", "P2", summary2.ProductDescription);
		}

		public void TestBuildCollection_ProductPopulatedCorrectly_TemporaryProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, ZGuid.BrettsGuid, 7m);
			receiveLine.ProductCode = "TemporaryTest";

			MakeASNsIfRequired(receive);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			var summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == "TemporaryTest");
			AssertEquals("Collection has correct Product Description on summary", "", summary.ProductDescription);
		}

		public void TestBuildCollection_ProductPopulatedCorrectly_MultipleTemporaryProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, ZGuid.Invalid, 7m);
			receiveLine1.ProductCode = "TemporaryTest";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, ZGuid.Invalid, 9m);
			receiveLine2.ProductCode = "MisingInAction";
			receiveLine2.ProductDesc = "Product Description of MisingInAction";

			MakeASNsIfRequired(receive);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has two summaries", 2, collection.Count);

			var summary1 = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == "TemporaryTest");
			AssertEquals("Collection has correct Product Description on summary1", "", summary1.ProductDescription);
			AssertEquals("Collection has correct Expected Quantity on summary1", 7m, summary1.ExpectedQuantity);
			AssertEquals("Collection has correct Received Quantity on summary1", 7m, summary1.ReceivedQuantity);

			var summary2 = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == "MisingInAction");
			AssertEquals("Collection has correct Product Description on summary2", "Product Description of MisingInAction", summary2.ProductDescription);
			AssertEquals("Collection has correct Expected Quantity on summary2", 9m, summary2.ExpectedQuantity);
			AssertEquals("Collection has correct Received Quantity on summary2", 9m, summary2.ReceivedQuantity);
		}

		public void TestBuildCollection_NormalReceiveLines_OneProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			Factory.Save();

			MakeASNsIfRequired(receive);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has one summary", 1, collection.Count);

			var summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct summary - ExpectedQuantity", 5m, summary.ExpectedQuantity);
			AssertEquals("Collection has correct summary - ReceivedQuantity", 5m, summary.ReceivedQuantity);
		}

		public void TestBuildCollection_NormalReceiveLines_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 9m);
			Factory.Save();

			MakeASNsIfRequired(receive);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has two summary", 2, collection.Count);

			var part2Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("Collection has correct Part2 summary - ExpectedQuantity", 9m, part2Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part2 summary - ReceivedQuantity", 9m, part2Summary.ReceivedQuantity);

			var part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 5m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 5m, part1Summary.ReceivedQuantity);
		}

		public void TestRebuildCollection_MultipleLinesOfTheSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			Factory.Save();

			receiveLine1.WE_ClientOrderedUnits = 15m;
			receiveLine1.WE_TransactionQuantity = 3m;
			Factory.Save();

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has one summary", 1, collection.Count);

			var part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 15m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 3m, part1Summary.ReceivedQuantity);

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine2.WE_ClientOrderedUnits = 10m;
			receiveLine2.WE_TransactionQuantity = 18m;

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_ClientOrderedUnits = 110m;
			receiveLine3.WE_TransactionQuantity = 80m;

			MakeASNsIfRequired(receive);

			collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 135m,
				part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 101m,
				part1Summary.ReceivedQuantity);
		}

		#endregion

		#region ICollection Members

		public void TestCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			Factory.Save();

			var summaries = receive.ReceiveProductSummaryCollection;
			AssertEquals("Count should show One Product Summary.", 1, summaries.Count);
			AssertEquals("Count should show One Product Summary.", 1, ((ICollection)summaries).Count);
			AssertEquals("Count should show One Product Summary.", 1,
				((BusinessObjectCollection)summaries).Count);

			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 15m);

			MakeASNsIfRequired(receive);

			AssertEquals("Count should show Two Product Summaries.", 2, summaries.Count);
			AssertEquals("Count should show Two Product Summaries.", 2, ((ICollection)summaries).Count);

			summaries.ClearCollection();

			using (summaries.SuspendRebuild())
			{
				AssertEquals("While Product Summary Collection is invalidated, it should be empty.", 0,
					summaries.Count);
			}

			AssertEquals("Accessing count through interface should rebuild invalidated Product Summaries.", 2,
				((ICollection)summaries).Count);
			summaries.ClearCollection();

			using (summaries.SuspendRebuild())
			{
				AssertEquals("While Product Summaries Collection is invalidated, it should be empty.", 0,
					summaries.Count);
			}

			AssertEquals("Accessing count through class should rebuild invalidated Product Summaries.", 2,
				summaries.Count);
		}

		#endregion

		#region IEnumerable Members

		public void TestIEnumerable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			Factory.Save();

			var summaries = receive.ReceiveProductSummaryCollection;
			AssertEquals("Enumeration should find One Release Line.", 1,
				summaries.Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find One Release Line.", 1,
				((IEnumerable<BusinessObject>)summaries).Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find One Release Line.", 1,
				((BusinessObjectCollection)summaries).Where(r => r != null).ToArray().Length);

			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 15m);

			MakeASNsIfRequired(receive);

			AssertEquals("Enumeration should find Two Release Lines.", 2,
				summaries.Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find Two Release Lines.", 2,
				((IEnumerable<BusinessObject>)summaries).Where(r => r != null).ToArray().Length);

			summaries.ClearCollection();

			using (summaries.SuspendRebuild())
			{
				AssertEquals("While Release Lines Collection is invalidated, it should be empty.", 0,
					summaries.Where(r => r != null).ToArray().Length);
			}

			AssertEquals("Accessing Enumerator through interface should rebuild invalidated Release Lines.", 2,
				((IEnumerable<BusinessObject>)summaries).Where(r => r != null).ToArray().Length);
			summaries.ClearCollection();

			using (summaries.SuspendRebuild())
			{
				AssertEquals("While Release Lines Collection is invalidated, it should be empty.", 0,
					summaries.Where(r => r != null).ToArray().Length);
			}

			AssertEquals("Accessing Enumerator through class should rebuild invalidated Release Lines.", 2,
				summaries.Where(r => r != null).ToArray().Length);
		}

		#endregion

		protected virtual void MakeASNsIfRequired(WhsReceive receive)
		{
		}
	}

	[TestedType(typeof(WhsReceiveProductSummaryCollection))]
	class WhsReceiveWithAsnLinesProductSummaryCollectionTest : WhsReceiveProductSummaryCollectionTest
	{
		#region TestBuildCollection_ExpectedOneProductReceivedAnother

		public void TestBuildCollection_ExpectedOneProductReceivedAnother()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
			Factory.Save();

			receive.PopulateASNLines();
			receiveLine.WE_TransactionQuantity = 0m;
			Factory.Save();

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Precondition: Collection has one summary", 1, collection.Count);
			var part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Precondition: Collection has correct Part1 summary - ExpectedQuantity", 3m,
				part1Summary.ExpectedQuantity);
			AssertEquals("Precondition: Collection has correct Part1 summary - ReceivedQuantity", 0m,
				part1Summary.ReceivedQuantity);

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 3m);
			receiveLine2.WE_ClientOrderedUnits = 0m;

			collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has two summaries", 2, collection.Count);

			part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 3m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 0m, part1Summary.ReceivedQuantity);

			var part2Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("Collection has correct Part2 summary - ExpectedQuantity", 0m, part2Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part2 summary - ReceivedQuantity", 3m, part2Summary.ReceivedQuantity);
		}

		#endregion

		#region TestBuildCollection_ExpectedAttributesDoNotMatchReceived

		public void TestBuildCollection_ExpectedAttributesDoNotMatchReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory,
				"Colour");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory,
				"Batch");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			receiveLine.WE_PartAttrib1 = "Blue";
			receiveLine.WE_PartAttrib2 = "15782";
			receiveLine.WE_PackingDate = ZDate.BrettsBirthday;
			Factory.Save();

			receive.PopulateASNLines();
			receiveLine.WE_TransactionQuantity = 0m;
			Factory.Save();

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Precondition: Collection has one summary", 1, collection.Count);
			var summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Precondition: Collection has correct summary - ExpectedQuantity", 30m,
				summary.ExpectedQuantity);
			AssertEquals("Precondition: Collection has correct summary - ReceivedQuantity", 0m,
				summary.ReceivedQuantity);

			var receivedLineA = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receivedLineA.WE_ClientOrderedUnits = 0m; // to replicate functional readonly.
			receivedLineA.WE_PartAttrib1 = "Green";
			receivedLineA.WE_PartAttrib2 = "74185";
			receivedLineA.WE_PackingDate = ZDate.Today;

			var receivedLineB = Helper.CreateWhsReceiveLine(receive, data.Part1, 6m);
			receivedLineB.WE_ClientOrderedUnits = 0m;
			receivedLineB.WE_PartAttrib1 = "Green";
			receivedLineB.WE_PartAttrib2 = "74185";
			receivedLineB.WE_PackingDate = ZDate.Today;

			var receivedLineC = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receivedLineC.WE_ClientOrderedUnits = 0m;
			receivedLineC.WE_PartAttrib1 = "Green";
			receivedLineC.WE_PartAttrib2 = "74185";
			receivedLineC.WE_PackingDate = ZDate.Today;

			collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has one summary", 1, collection.Count);

			summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct summary - ExpectedQuantity", 30m, summary.ExpectedQuantity);
			AssertEquals("Collection has correct summary - ReceivedQuantity", 21m, summary.ReceivedQuantity);
		}

		#endregion

		#region TestRebuildCollection_ReceiveLinesIsLessThanAsnLines

		public void TestRebuildCollection_ReceiveLinesIsLessThanAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_TransactionQuantity = 3m;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_TransactionQuantity = 2m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.Lines.Delete(receiveLine1);

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has one summary", 1, collection.Count);

			var part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 15m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 2m, part1Summary.ReceivedQuantity);
		}

		#endregion

		#region TestRebuildCollection_NoReceiveLinesButHaveAsnLines

		public void TestRebuildCollection_NoReceiveLinesButHaveAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_TransactionQuantity = 3m;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_TransactionQuantity = 2m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.Lines.DeleteAll();

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Collection has one summary", 1, collection.Count);

			var part1Summary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 15m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 0m, part1Summary.ReceivedQuantity);
		}

		#endregion

		#region Implementation

		protected override WhsReceiveProductSummaryCollection GetCollectionToTest()
		{
			return new WhsReceiveProductSummaryCollection(Factory, Factory.New<WhsReceive>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
		}

		protected override void MakeASNsIfRequired(WhsReceive receive)
		{
			receive.PopulateASNLines();
		}

		#endregion
	}

	[TestedType(typeof(WhsReceiveProductSummaryCollection))]
	class WhsReceiveWithoutAsnLinesProductSummaryCollectionTest : WhsReceiveProductSummaryCollectionTest
	{
		#region TestBuildCollection_ExpectedQuantityOfReceiveLineIsZero

		public void TestBuildCollection_ExpectedQuantityOfReceiveLineIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
			receiveLine.WE_ClientOrderedUnits = 0m;
			receiveLine.WE_TransactionQuantity = 3m;
			Factory.Save();

			var collection = new WhsReceiveProductSummaryCollection(Factory, receive);
			AssertEquals("Precondition: Collection has one summary", 1, collection.Count);
			var productSummary = collection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Precondition: Collection has correct Part1 summary - ExpectedQuantity", 0m,
				productSummary.ExpectedQuantity);
			AssertEquals("Precondition: Collection has correct Part1 summary - ReceivedQuantity", 3m,
				productSummary.ReceivedQuantity);
		}

		#endregion

		#region Implementation

		protected override WhsReceiveProductSummaryCollection GetCollectionToTest()
		{
			return new WhsReceiveProductSummaryCollection(Factory, Factory.New<WhsReceive>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
		}

		#endregion
	}
}
