using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.InBond.Business.OperationalActions.Testing
{
	public class SendDepartureAddPerMovementOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestJobEligibleForSending_DocumentOnly()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			var moveHeader = inBondHeader.MovementHeader;
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var log = new DummyOperationalActionSectionLog();
			var runner = new SendDepartureAddPerMovementOperationalActionRunner(log);
			var inBondJobPKs = runner.PerformFunctionOperationalAction(true, new[] { movement });
			AssertEquals(0, inBondJobPKs.Count());
			AssertContains("Departure Add message can not be sent in Document Only mode.", log.MessagesString());
		}

		public void TestJobEligibleForSending_PostDepartureMessageOnly()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_PostDepartureOnly = true;
			var moveHeader = inBondHeader.MovementHeader;
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var log = new DummyOperationalActionSectionLog();
			var runner = new SendDepartureAddPerMovementOperationalActionRunner(log);
			var inBondJobPKs = runner.PerformFunctionOperationalAction(true, new[] { movement });
			AssertEquals(0, inBondJobPKs.Count());
			AssertContains("Departure Add message can not be sent when 'Post Departure Messages Only' is ticked.", log.MessagesString());
		}

		public void TestNoDataSendWhenSendCustomsMessageMutexIsLocked()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			inBondHeader.BH_ImportTransportMode = US.Messaging.Business.TransportModeCodes.Codes.VesselNonContainer;
			var bill = inBondHeader.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB16082101";
			bill.B0_HouseBillNumber = "HB16082101";
			var moveHeader = inBondHeader.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = US.Messaging.Business.InbondCommonTypeList.Codes._1ImmediateTransport;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var log = new DummyOperationalActionSectionLog();
			var runner = new SendDepartureAddPerMovementOperationalActionRunner(log);
			inBondHeader.LockSendCustomsMessageMutex();
			var inBondJobPKs = runner.PerformFunctionOperationalAction(true, new[] { movement });
			AssertEquals(0, inBondJobPKs.Count());
			AssertContains("is in the process of sending messages for INB00001, please try again later.", log.MessagesString());

			inBondHeader.UnlockSendCustomsMessageMutex();
			Assert("Mutex is unlocked", !inBondHeader.IsSendCustomsMessageMutexLocked);
		}
	}
}
