using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondHeaderEntryNumberSupporterTest : TestCaseWithFactory
	{
		public void TestGetAllocateEntryNumberSupporter()
		{
			var header = Factory.New<CusInBondHeader>();
			IAllocateNumberSupporter supporter = new CusInBondHeaderEntryNumberSupporter(header);
			AssertEquals("Entry Number", supporter.NumberType);

			supporter.DoAllocate("12345678");
			CombineAssertions(() =>
			{
				AssertEquals("Allocated", "12345678", header.EntryNumber);
				AssertEquals("Existing Entry Number", "12345678", supporter.GetExistingNumber());
			});
		}

		public void TestGetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			var header = Factory.New<CusInBondHeader>();
			IAllocateNumberSupporter supporter = new CusInBondHeaderEntryNumberSupporter(header);
			AssertEquals("The job is still waiting for a response. Allocating a new entry number means that this transhipment job will be treated as a new entry in the customs’ system. Do you want to proceed?", supporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms());
		}

		public void TestAllocationUseMutex()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var headerInDiffFactory = newFactory.Load<CusInBondHeader>(header.PK);
			IAllocateNumberSupporter supporterInDiffFactory = new CusInBondHeaderEntryNumberSupporter(headerInDiffFactory);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.DoAllocate("AB  1112300006");
			AssertEquals("Allocated", "AB  1112300006", headerInDiffFactory.EntryNumber);
			IAllocateNumberSupporter supporter = new CusInBondHeaderEntryNumberSupporter(header);
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			CombineAssertions("Unlock number allocation mutex.", () =>
			{
				supporter.UnlockNumberAllocationMutex();
				AssertEquals(false, supporter.LockNumberAllocationMutex);
				AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
				supporterInDiffFactory.UnlockNumberAllocationMutex();
			});
		}

		public void TestGetNewAllocateNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			IAllocateNumberSupporter supporter = new CusInBondHeaderEntryNumberSupporter(header);
			AssertType<TranshipmentAllocateNumber>(supporter.GetNewAllocateNumber());
		}
	}
}
