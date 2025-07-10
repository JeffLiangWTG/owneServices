using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class EntryHeaderAmendmentAndEntryLineDeletionTest : TestCaseWithFactory
	{
		public virtual void TestMarkPendingEntryLinesToDeletedWhenAnAmendmentIsCleared()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			if (testDec.ShouldKeepDeletedLinesOnAmendment)
			{
				var entryHeader = testDec.CustomsEntryHeaders.AddNew();
				AssertEntryLineDeletion(entryHeader);
			}
			else
			{
				Assert(true);
			}
		}

		protected void AssertEntryLineDeletion(CusEntryHeader entryHeader)
		{
			var entryLine = entryHeader.PendingDeletionEntryLines.AddNew();
			AssertEquals("Customs Posted Status", EntryLineStatusList.Codes.DeletePending, entryLine.CL_CustomsPostedStatus);

			entryHeader.CH_Status = AmendmentPendingCode;
			AssertEquals("Customs Posted Status stays the same", EntryLineStatusList.Codes.DeletePending, entryLine.CL_CustomsPostedStatus);

			entryHeader.CH_Status = AmendmentClearedCode;
			if (entryHeader.Declaration.ShouldKeepDeletedLinesOnAmendmentCleared)
			{
				AssertEquals("Entry Line is NOT deleted", false, entryLine.IsDeleted);
				AssertEquals("Entry Line is finalised now", EntryLineStatusList.Codes.Deleted, entryLine.CL_CustomsPostedStatus);
			}
			else
			{
				AssertEquals("Entry Line is deleted now", true, entryLine.IsDeleted);
			}
		}

		protected abstract ZString AmendmentClearedCode { get; }
		protected abstract ZString AmendmentPendingCode { get; }
	}

	public class EntryHeaderAmendmentAndEntryLineDeletionBaseOnlyTest : EntryHeaderAmendmentAndEntryLineDeletionTest
	{
		public override void TestMarkPendingEntryLinesToDeletedWhenAnAmendmentIsCleared()
		{
			var testDec = Factory.New<JobDeclarationForEntryLineDeletionTest>();
			var entryHeader = Factory.New<EntryHeaderForEntryLineDeletionTest>();
			testDec.CustomsEntryHeaders.Add(entryHeader);
			AssertEntryLineDeletion(entryHeader);

			testDec.ShouldKeepDeletedLinesOnAmendmentClearedCore = true;
			AssertEntryLineDeletion(entryHeader);
		}

		protected override ZString AmendmentClearedCode => MessageStatusList.Codes.ClearChange;
		protected override ZString AmendmentPendingCode => MessageStatusList.Codes.AwaitingChange;

		class JobDeclarationForEntryLineDeletionTest : BaseJobDeclaration
		{
			public JobDeclarationForEntryLineDeletionTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ShouldKeepDeletedLinesOnAmendmentCore => true;
			public override bool ShouldKeepDeletedLinesOnAmendmentCleared => ShouldKeepDeletedLinesOnAmendmentClearedCore;
			public bool ShouldKeepDeletedLinesOnAmendmentClearedCore;
		}

		class EntryHeaderForEntryLineDeletionTest : CusEntryHeader
		{
			public EntryHeaderForEntryLineDeletionTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsStatusChangingFromAmendmentPendingToCleared(ZString originalStatus, ZString newStatus)
			{
				return originalStatus == MessageStatusList.Codes.AwaitingChange && newStatus == MessageStatusList.Codes.ClearChange;
			}
		}
	}
}
