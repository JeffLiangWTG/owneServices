using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderEntryNumberSupporter))]
	sealed class AsycudaManifestHeaderEntryNumberSupporterTest : TestCaseWithFactory
	{
		public void TestGetAllocateEntryNumberSupporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			IAllocateNumberSupporter supporter = new AsycudaManifestHeaderEntryNumberSupporter(header);
			AssertEquals("Entry Number", supporter.NumberType);
			supporter.DoAllocate("12345678");
			CombineAssertions(() =>
			{
				AssertEquals("Allocated", "12345678", header.DeclarationNumber);
				AssertEquals("Existing Entry Number", "12345678", supporter.GetExistingNumber());
			});
		}

		public void TestGetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			IAllocateNumberSupporter supporter = new AsycudaManifestHeaderEntryNumberSupporter(header);
			AssertEquals("The job is still waiting for a response. Allocating a new entry number means that this transhipment job will be treated as a new entry in the customs’ system. Do you want to proceed?", supporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms());
		}

		public void TestAllocationUseMutex()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var headerInDiffFactory = newFactory.Load<AsycudaManifestHeader>(header.PK);
			IAllocateNumberSupporter supporterInDiffFactory = new AsycudaManifestHeaderEntryNumberSupporter(headerInDiffFactory);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.DoAllocate("AB  1112300006");
			AssertEquals("Allocated", "AB  1112300006", headerInDiffFactory.DeclarationNumber);
			IAllocateNumberSupporter supporter = new AsycudaManifestHeaderEntryNumberSupporter(header);
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
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			IAllocateNumberSupporter supporter = new AsycudaManifestHeaderEntryNumberSupporter(header);
			AssertType<AsycudaManifestHeaderAllocateNumber>(supporter.GetNewAllocateNumber());
		}
	}
}

