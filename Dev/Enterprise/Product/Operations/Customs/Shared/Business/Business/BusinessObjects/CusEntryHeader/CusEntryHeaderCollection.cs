using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryHeaderCollection<out TCusEntryHeader> : IBusinessObjectCollection<TCusEntryHeader>
		where TCusEntryHeader : CusEntryHeader
	{
		BaseJobDeclaration Declaration { get; }
		new TCusEntryHeader this[int index] { get; }
		new TCusEntryHeader[] ToArray();
		new TCusEntryHeader[] Find(ZQuery filter);
		new TCusEntryHeader FindByPK(ZGuid pk);
		ZDateTime LastEntryToClearDate { get; }
		bool HasAnEntryWithEntryNumber { get; }
		bool EntriesExistAndAllHaveEntryNumbers { get; }
		bool HasAnyEntryGotTransactionsWithCustoms { get; }
		bool HasAnyEntryWhichMessagesCannotBeChanged { get; }
		bool HasAnyEntryWhichMessagesCannotBeChangedCore { get; }
		bool AreAnyHeadersWaitingForAResponse { get; }
		bool HasEntryWithPostLodgeStatus { get; }
		bool AreWithdrawalLodgementQuestionsGeneratedAndAnswered { get; }
		bool AreAmendmentLodgementQuestionsGeneratedAndAnswered { get; }
		bool IsAnyLodgementQuestion7AnsweredNo { get; }
		bool DoAllEntriesHaveEntryNumber { get; }
		bool AreAllCPQuestionsAnswered { get; }
		bool StatusNeedsRecalculation { get; }
		bool HasAnEntryWithEntryStatus { get; }
		ZDecimal TotalAmountPayableForThisSession { get; }

		void ClearPackages();
		void RemoveAndDeleteUnNecessaryCusEntryHeaders();

		TCusEntryHeader AddNew(Type type);
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
		List<ZGuid> GetPKs();
		void AddRange(params BusinessObject[] businessObject);
	}

	public class CusEntryHeaderCollection<TCusEntryHeader> :
		DependentBusinessObjectCollection<TCusEntryHeader, BaseJobDeclaration>,
		ICusEntryHeaderCollection<TCusEntryHeader>
		where TCusEntryHeader : CusEntryHeader
	{
		public CusEntryHeaderCollection(BaseJobDeclaration parentBO, BusinessObjectFactory factory) : base(parentBO,
			factory)
		{
		}

		protected override bool AllowNewCore => false;

		bool IBindingList.AllowRemove => false;

		public void RemoveRelationshipsToInvoices()
		{
			foreach (var entryHeader in this)
			{
				entryHeader.CH_JE = ZGuid.Empty;
			}
		}

		public bool AreAllStatusesEqualTo(params ZString[] status)
		{
			return this.All(header => status.Contains(header.CH_Status));
		}

		public bool AreAnyStatusesEqualTo(params ZString[] status)
		{
			return this.Any(header => status.Contains(header.CH_Status));
		}

		public bool AreAnyHeadersWaitingForAResponse => Factory.GetValue(ref cachedAreAnyHeadersWaitingForAResponse,
			() => AreAnyHeadersWaitingForAResponseCore);
		CachedProperty<bool> cachedAreAnyHeadersWaitingForAResponse;

		protected virtual bool AreAnyHeadersWaitingForAResponseCore =>
			this.Any(header => header.Messages.IsWaitingForAResponse);

		public ZDecimal GSTAmount => Factory.GetValue(ref gstAmountCached, () =>
			this.Aggregate<TCusEntryHeader, ZDecimal>(0, (current, entryHeader) => current + entryHeader.GSTAmount));
		CachedProperty<ZDecimal> gstAmountCached;

		public bool EntriesExistAndAllHaveEntryNumbers => Factory.GetValue(ref cachedEntriesExistAndAllHaveEntryNumbers, () =>
			Count > 0 && this.All(header => !header.EntryNumber.IsEmpty));
		CachedProperty<bool> cachedEntriesExistAndAllHaveEntryNumbers;

		public bool HasAnEntryWithEntryNumber => Factory.GetValue(ref cachedHasAnEntryWithEntryNumber,
			() => this.Any(entry => !entry.EntryNumber.IsEmpty));
		CachedProperty<bool> cachedHasAnEntryWithEntryNumber;

		public ZDateTime LastEntryToClearDate => this.MaxOrDefault(h => h.ClearanceDate);

		/// <summary>
		/// Sort by CusEntryHeader.Comparer. If you need to sort differently, override GetCustomsEntryHeaderComparer()
		/// </summary>
		public void CustomSort()
		{
			Sort(GetCustomsEntryHeaderComparer());
		}

		#region Implementation

		protected virtual IComparer GetCustomsEntryHeaderComparer()
		{
			return new CusEntryHeader.BaseCusEntryComparer();
		}

		public BaseJobDeclaration Declaration => Master;

		#endregion

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() =>
			new FetchStrategies.CusEntryHeaderCollectionFetchStrategy(this);

		public IEnumerator<TCusEntryHeader> GetEnumerator() => Elements.Cast<TCusEntryHeader>().GetEnumerator();

		public virtual bool HasAnyEntryGotTransactionsWithCustoms =>
			this.Cast<CusEntryHeader>().Any(ceh => ceh.HasTransactionsWithCustoms);

		public virtual bool HasAnyEntryWhichMessagesCannotBeChanged => HasAnyEntryWhichMessagesCannotBeChangedCore;
		public virtual bool HasAnyEntryWhichMessagesCannotBeChangedCore => false;
		public virtual bool HasEntryWithPostLodgeStatus => false;
		public virtual bool AreWithdrawalLodgementQuestionsGeneratedAndAnswered => false;
		public virtual bool AreAmendmentLodgementQuestionsGeneratedAndAnswered => false;
		public virtual bool IsAnyLodgementQuestion7AnsweredNo => false;
		public virtual bool DoAllEntriesHaveEntryNumber => false;
		public virtual bool AreAllCPQuestionsAnswered => false;
		public virtual bool StatusNeedsRecalculation => false;

		public virtual bool HasAnEntryWithEntryStatus =>
			this.Cast<CusEntryHeader>().Any(ceh => !ceh.CH_EntryStatus.IsEmpty);

		public virtual ZDecimal TotalAmountPayableForThisSession => 0;

		public virtual void ClearPackages()
		{
		}

		public virtual void RemoveAndDeleteUnNecessaryCusEntryHeaders()
		{
		}

		public new TCusEntryHeader[] ToArray() => (TCusEntryHeader[])ToArray(typeof(TCusEntryHeader));
		public new TCusEntryHeader[] Find(ZQuery filter) => base.Find(filter).Cast<TCusEntryHeader>().ToArray();
		public new TCusEntryHeader FindByPK(ZGuid pk) => (TCusEntryHeader)base.FindByPK(pk);
	}
}
