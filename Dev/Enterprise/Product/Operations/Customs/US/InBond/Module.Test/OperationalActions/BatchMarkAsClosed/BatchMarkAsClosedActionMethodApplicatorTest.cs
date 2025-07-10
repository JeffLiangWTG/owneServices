using System;
using Enterprise.Customs.US.InBond.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions.Testing
{
	[TestedType(typeof(BatchMarkAsClosedActionMethodApplicator))]
	sealed class BatchMarkAsClosedActionMethodApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestBatchMarkedAsClosed()
		{
			var inBond1 = Factory.New<CusInBondHeader>();
			var movement1 = inBond1.MovementHeaders.AddNew();
			var movement2 = inBond1.MovementHeaders.AddNew();
			movement1.InBondNumber = "12345678";
			movement2.InBondNumber = "22345678";
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			AssertEquals(true, moveHeader1.MoveHeader.BM_InBondClosedDate.IsEmpty);
			AssertEquals(true, moveHeader2.MoveHeader.BM_InBondClosedDate.IsEmpty);

			var log1 = SimulateRun(new USInBondMoveHeader[] { moveHeader1, moveHeader2 }, false);

			AssertEquals(false, moveHeader1.MoveHeader.BM_InBondClosedDate.IsEmpty);
			AssertEquals(false, moveHeader2.MoveHeader.BM_InBondClosedDate.IsEmpty);
			AssertContains("SUCCESS: In-Bond Movement [HL INB0000001 (12345678)]: has been closed manually.", log1.MessagesString());
			AssertContains("SUCCESS: In-Bond Movement [HL INB0000001 (22345678)]: has been closed manually.", log1.MessagesString());

			var movement3 = inBond1.MovementHeaders.AddNew();
			movement3.InBondNumber = "32345678";
			var closedDate = new DateTime(2024, 11, 29);
			movement3.BM_InBondClosedDate = closedDate;
			Factory.Save();
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var log2 = SimulateRun(new USInBondMoveHeader[] { moveHeader3 }, false);
			AssertEquals(closedDate, moveHeader3.MoveHeader.BM_InBondClosedDate);
			AssertContains("INFO: In-Bond Movement [HL INB0000001 (32345678)]: has already been closed.", log2.MessagesString());

			var inBond2 = Factory.New<CusInBondHeader>();
			var movement21 = inBond2.MovementHeaders.AddNew();
			var movement22 = inBond2.MovementHeaders.AddNew();
			movement21.InBondNumber = "12345679";
			movement22.InBondNumber = "22345679";
			Factory.Save();

			AssertEquals(true, movement21.BM_InBondClosedDate.IsEmpty);
			AssertEquals(true, movement22.BM_InBondClosedDate.IsEmpty);
			var log3 = SimulateRun(new CusInBondHeader[] { inBond2 }, false);
			AssertEquals(false, movement21.BM_InBondClosedDate.IsEmpty);
			AssertEquals(false, movement22.BM_InBondClosedDate.IsEmpty);
			AssertContains("SUCCESS: In-Bond Movement 12345679 in Job [HL INB0000002] has been closed manually.", log3.MessagesString());
			AssertContains("SUCCESS: In-Bond Movement 22345679 in Job [HL INB0000002] has been closed manually.", log3.MessagesString());
		}
	}
}
