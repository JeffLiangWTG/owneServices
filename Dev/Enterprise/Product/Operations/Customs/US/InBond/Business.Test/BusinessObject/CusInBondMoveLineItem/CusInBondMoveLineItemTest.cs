using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveLineItem))]
	sealed class CusInBondMoveLineItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestWeight()
		{
			moveLine.BI_Weight = 10.1234m;
			moveLine.BI_WeightUnit = WeightUnitList.Codes.Kilograms;
			AssertEquals(new ZWeight(10.1234m, Core.Constants.Weight.Kilograms), moveLine.Weight);
		}

		public void TestReAssigningSequenceNumbers()
		{
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			var line2 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			var line3 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 3", (short)3, line3.BI_PrintingSequenceNo);
			line3.BI_PrintingSequenceNo = 2;
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)3, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)2, line3.BI_PrintingSequenceNo);
			moveLine.BI_PrintingSequenceNo = 3;
			AssertEquals("Printing Sequence Number for Line 1", (short)3, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)1, line3.BI_PrintingSequenceNo);
			line2.BI_PrintingSequenceNo = 8;
			AssertEquals("Printing Sequence Number for Line 1", (short)2, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)3, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)1, line3.BI_PrintingSequenceNo);
			var line4 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 4", (short)4, line4.BI_PrintingSequenceNo);
			line4.BI_PrintingSequenceNo = 0;
			AssertEquals("Printing Sequence Number for Line 1", (short)2, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)3, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)1, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)4, line4.BI_PrintingSequenceNo);
			line4.BI_PrintingSequenceNo = 1;
			AssertEquals("Printing Sequence Number for Line 1", (short)3, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)4, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)2, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)1, line4.BI_PrintingSequenceNo);
			line3.BI_PrintingSequenceNo = 8;
			AssertEquals("Printing Sequence Number for Line 1", (short)2, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)3, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)4, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)1, line4.BI_PrintingSequenceNo);
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			line4.BI_B9 = moveDetail2.PK;
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)3, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)1, line4.BI_PrintingSequenceNo);
			line4.BI_B9 = moveDetail.PK;
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)3, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)4, line4.BI_PrintingSequenceNo);
			line4.BI_PrintingSequenceNo = 3;
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)4, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)3, line4.BI_PrintingSequenceNo);
		}

		public void TestReAssigningSequenceNumbersWithMoreLines()
		{
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			var line2 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			var line3 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 3", (short)3, line3.BI_PrintingSequenceNo);
			var line4 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 4", (short)4, line4.BI_PrintingSequenceNo);
			var line5 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 5", (short)5, line5.BI_PrintingSequenceNo);
			line3.BI_PrintingSequenceNo = 2;
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)3, line2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)2, line3.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 4", (short)4, line4.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 5", (short)5, line5.BI_PrintingSequenceNo);
		}

		public void TestReAssigningSequenceNumbersWithMultiMoveDetails()
		{
			moveDetail.B9_InBoundQty = 10;
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			var line2 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 2", (short)2, line2.BI_PrintingSequenceNo);
			var invoiceLine3 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 3", (short)3, invoiceLine3.BI_PrintingSequenceNo);
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_InBoundQty = 20;
			var moveDetail2InvLine1 = moveDetail2.CBP7512Lines.AddNew();
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 1", (short)1, moveDetail2InvLine1.BI_PrintingSequenceNo);
			var moveDetail2InvLine2 = moveDetail2.CBP7512Lines.AddNew();
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 2", (short)2, moveDetail2InvLine2.BI_PrintingSequenceNo);
			var moveDetail2InvLine3 = moveDetail2.CBP7512Lines.AddNew();
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 3", (short)3, moveDetail2InvLine3.BI_PrintingSequenceNo);
			line2.BI_PrintingSequenceNo = 1;
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 1", (short)2, moveLine.BI_PrintingSequenceNo);
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 2", (short)1, line2.BI_PrintingSequenceNo);
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 3", (short)3, invoiceLine3.BI_PrintingSequenceNo);
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 1", (short)1, moveDetail2InvLine1.BI_PrintingSequenceNo);
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 2", (short)2, moveDetail2InvLine2.BI_PrintingSequenceNo);
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 3", (short)3, moveDetail2InvLine3.BI_PrintingSequenceNo);
			moveDetail2InvLine1.BI_PrintingSequenceNo = 3;
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 1", (short)2, moveLine.BI_PrintingSequenceNo);
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 2", (short)1, line2.BI_PrintingSequenceNo);
			AssertEquals("MoveDetail1 - Printing Sequence Number for Line 3", (short)3, invoiceLine3.BI_PrintingSequenceNo);
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 1", (short)3, moveDetail2InvLine1.BI_PrintingSequenceNo);
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 2", (short)1, moveDetail2InvLine2.BI_PrintingSequenceNo);
			AssertEquals("moveDetail2 - Printing Sequence Number for Line 3", (short)2, moveDetail2InvLine3.BI_PrintingSequenceNo);
		}

		public void TestResAssigningSequenceNumbersWithWeirdNumbers()
		{
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			var invoiceLine2 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 2", (short)2, invoiceLine2.BI_PrintingSequenceNo);
			var invoiceLine3 = moveDetail.CBP7512Lines.AddNew();
			AssertEquals("Printing Sequence Number for Line 3", (short)3, invoiceLine3.BI_PrintingSequenceNo);
			invoiceLine3.BI_PrintingSequenceNo = 55;
			AssertEquals("Printing Sequence Number for Line 1", (short)1, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)2, invoiceLine2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)3, invoiceLine3.BI_PrintingSequenceNo);
			moveLine.BI_PrintingSequenceNo = 65;
			AssertEquals("Printing Sequence Number for Line 1", (short)3, moveLine.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 2", (short)1, invoiceLine2.BI_PrintingSequenceNo);
			AssertEquals("Printing Sequence Number for Line 3", (short)2, invoiceLine3.BI_PrintingSequenceNo);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			moveHeader = header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			moveLine = moveDetail.CBP7512Lines.AddNew();
			return moveLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveLine = moveDetail.CBP7512Lines.AddNew();
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		CusInBondMoveDetail moveDetail;
		CusInBondMoveLineItem moveLine;
	}
}
