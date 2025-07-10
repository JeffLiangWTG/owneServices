using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitCusDecProcessorTest : TestCaseWithFactory
	{
		public void TestAddPermitRecordsAndLockMutexIfNeededONlyAddedManagedPermits()
		{
			var entry = Factory.New<CusEntryHeader>();
			var permitProcessor = new CusPermitCusDecProcessorForTesting(entry);
			permitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
			var permitRecords = permitProcessor.PermitRecords;

			AssertEquals(1, permitRecords.Count);
			AssertEquals("123", permitRecords[0].PermitHeader.CPH_Number);

			permitProcessor.UnlockPermitMutexes();  //Clean up
		}

		public void TestICusPermitCusDecProcessor()
		{
			var entry = Factory.New<CusEntryHeader>();
			var permitProcessor = new CusPermitCusDecProcessorForTesting(entry);

			var intFace = permitProcessor as ICusPermitCusDecProcessor<Messaging.Business.EDIMessage>;

			AssertNotNull("Should be able to cast as ICusPermitCusDecProcessor<EDIMessage>", intFace);
		}
	}
}
