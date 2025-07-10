using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.InBond.Business.OperationalActions.Testing
{
	sealed class SendDepartureAddOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestJobEligibleForSending_DocumentOnly()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			Factory.Save();
			var log = new DummyOperationalActionSectionLog();
			var runner = new SendDepartureAddOperationalActionRunner(log);
			var inBondJobPKs = runner.PerformFunctionOperationalAction(true, new[] { inBondHeader });
			AssertEquals(0, inBondJobPKs.Count());
			AssertContains("Departure Add message can not be sent in Document Only mode.", log.MessagesString());
		}

		public void TestJobEligibleForSending_PostDepartureMessageOnly()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_PostDepartureOnly = true;
			Factory.Save();
			var log = new DummyOperationalActionSectionLog();
			var runner = new SendDepartureAddOperationalActionRunner(log);
			var inBondJobPKs = runner.PerformFunctionOperationalAction(true, new[] { inBondHeader });
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
			var log = new DummyOperationalActionSectionLog();
			var runner = new SendDepartureAddOperationalActionRunner(log);
			inBondHeader.LockSendCustomsMessageMutex();
			var inBondJobPKs = runner.PerformFunctionOperationalAction(true, new[] { inBondHeader });
			AssertEquals(0, inBondJobPKs.Count());
			AssertContains("is in the process of sending messages for INB00001, please try again later.", log.MessagesString());
			inBondHeader.UnlockSendCustomsMessageMutex();
			Assert("Mutex is unlocked", !inBondHeader.IsSendCustomsMessageMutexLocked);
		}
	}
}
