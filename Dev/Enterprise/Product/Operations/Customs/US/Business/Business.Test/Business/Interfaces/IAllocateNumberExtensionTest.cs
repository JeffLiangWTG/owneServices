using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.EntryNumber.Testing
{
	sealed class IAllocateNumberExtensionTest : TestCaseWithFactory
	{
		public void TestGetAllocateEntryNumberSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var supporter = declaration.GetAllocateNumberSupporter();
			AssertEquals("Entry Number", supporter.NumberType);
			supporter.DoAllocate("12345678");
			AssertEquals("Allocated", "12345678", declaration.ImportEntryNumber);
		}

		public void TestAllocationUseMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			Factory.Save();
			var decEntryNumberRefreshCount = 0;
			declaration.DecEntryNumberInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				decEntryNumberRefreshCount++;
			};
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			var supporterInDiffFactory = declarationInDiffFactory.GetAllocateNumberSupporter();
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.DoAllocate("12345678");
			AssertEquals("Allocated", "12345678", declarationInDiffFactory.ImportEntryNumber);
			var entryHeader = declarationInDiffFactory.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			var supporter = declaration.GetAllocateNumberSupporter();
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			supporter.UnlockNumberAllocationMutex();
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			AssertEquals("", supporter.GetReasonToStopProceeding());
			AssertEquals("", declaration.ImportEntryNumber);
			AssertEquals(0, decEntryNumberRefreshCount);
			newFactory.Save();
			AssertNotEquals(JobDeclaration.Constants.DisallowEntryNumberAllocation.EntryNumberAlreadyAllocated("12345678"), supporter.GetReasonToStopProceeding());
			AssertEquals("12345678", declaration.ImportEntryNumber);
			AssertEquals(1, decEntryNumberRefreshCount);
		}

		public void TestDisallowAllocateImportEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = declaration.GetAllocateNumberSupporter();
			AssertNull(supporter);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			supporter = declaration.GetAllocateNumberSupporter();
			AssertEquals("", supporter.GetReasonToStopProceeding());
			entry.EntryNumber = "TEST";
			AssertEquals("HasNotBeenLodged yet. It is OK to allow a new allocation", "", supporter.GetReasonToStopProceeding());
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals(true, entry.IsWaitingForResponse);
			AssertEquals(false, entry.HasBeenLodgedAtCustoms);
			AssertEquals(JobDeclaration.Constants.DisallowEntryNumberAllocation.EntryNumberAlreadyAllocated(entry.EntryNumber), supporter.GetReasonToStopProceeding());
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals(false, entry.IsWaitingForResponse);
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);
			AssertEquals(JobDeclaration.Constants.DisallowEntryNumberAllocation.EntryNumberAlreadyAllocated(entry.EntryNumber), supporter.GetReasonToStopProceeding());
		}
	}
}
