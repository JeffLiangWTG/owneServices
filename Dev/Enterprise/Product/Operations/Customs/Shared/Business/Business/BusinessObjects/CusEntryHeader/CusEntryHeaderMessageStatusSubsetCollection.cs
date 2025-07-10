using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public enum EntryMessageStatusFilterType { All, CanSendOriginal, CanSendAmendment, CanSendWithdraw }

	/// <summary>
	/// This collection contains entry headers whose message status match EntryHeaderSubsetCollectionMessageStatusSupported.
	/// After contructed, it contains all entry headers.
	/// </summary>
	public abstract class CusEntryHeaderMessageStatusSubsetCollection : SubsetBusinessObjectCollection<CusEntryHeader>
	{
		protected CusEntryHeaderMessageStatusSubsetCollection(ActiveCusEntryHeaderCollection allEntryHeaders) : base(allEntryHeaders)
		{
			fMessageStatusFilter = EntryMessageStatusFilterType.All;
		}

		protected CusEntryHeaderMessageStatusSubsetCollection(CusEntryHeader[] entries, BusinessObjectFactory factory) : base(new EntryCollectionWithPassedEntries(entries, factory))
		{
			fMessageStatusFilter = EntryMessageStatusFilterType.All;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			CusEntryHeader entryHeader = element as CusEntryHeader;
			bool result = true;
			if (MessageStatusFilter == EntryMessageStatusFilterType.CanSendOriginal)
			{
				result = CanSendOriginalForThisEntry(entryHeader);
			}
			else if (MessageStatusFilter == EntryMessageStatusFilterType.CanSendAmendment)
			{
				result = CanSendAmendmentForThisEntry(entryHeader);
			}
			else if (MessageStatusFilter == EntryMessageStatusFilterType.CanSendWithdraw)
			{
				result = CanSendWithdrawForThisEntry(entryHeader);
			}
			return result;
		}

		public EntryMessageStatusFilterType MessageStatusFilter
		{
			get { return fMessageStatusFilter; }
			set
			{
				if (fMessageStatusFilter != value)
				{
					fMessageStatusFilter = value;
					Rebuild();
				}
			}
		}
		EntryMessageStatusFilterType fMessageStatusFilter;

		protected abstract bool CanSendOriginalForThisEntry(CusEntryHeader entryHeader);
		protected abstract bool CanSendAmendmentForThisEntry(CusEntryHeader entryHeader);
		protected abstract bool CanSendWithdrawForThisEntry(CusEntryHeader entryHeader);
	}
}
