using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;

namespace Enterprise.Customs.Business
{
	public class CusEntryPayInfoCollection<TCusEntryPayInfo> : DependentBusinessObjectCollection<TCusEntryPayInfo, CusEntryHeader>, ICusEntryPayInfoCollection<TCusEntryPayInfo>
		where TCusEntryPayInfo : CusEntryPayInfo
	{
		public CusEntryPayInfoCollection(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public TCusEntryPayInfo GetItemByMessageNum(ZString payResponseNum)
		{
			foreach (TCusEntryPayInfo payInfo in this)
			{
				if (payInfo.C9_IncomingPayResponseNo == payResponseNum)
				{
					return payInfo;
				}
			}
			return null;
		}

		public TCusEntryPayInfo[] GetPendingItems()
		{
			ArrayList result = new ArrayList();
			foreach (TCusEntryPayInfo payInfo in this)
			{
				if (payInfo.IsPending)
				{
					result.Add(payInfo);
				}
			}
			return (TCusEntryPayInfo[])result.ToArray(typeof(TCusEntryPayInfo));
		}

		public TCusEntryPayInfo InsertOrUpdateEntryPayInfo(Func<TCusEntryPayInfo, bool> predicate, ZDecimal totalAmount, ZString transactionType, ZDateTime expirationDate, ZString incomingPayResponseNo, ZString paymentParty)
		{
			var entryPayInfo = ElementsAsEnumerable.SingleOrDefault(predicate);
			if (entryPayInfo == null)
			{
				entryPayInfo = AddNew();
				entryPayInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Pending;
			}
			entryPayInfo.C9_IncomingPayResponseNo = incomingPayResponseNo;
			entryPayInfo.C9_PaymentParty = paymentParty;
			entryPayInfo.C9_PaymentAmount = totalAmount;
			entryPayInfo.C9_TransactionType = transactionType;
			entryPayInfo.C9_PaymentDate = expirationDate;
			return entryPayInfo;
		}

		public IEnumerable<TCusEntryPayInfo> ElementsAsEnumerable => Elements.Cast<TCusEntryPayInfo>();

		public IEnumerator<TCusEntryPayInfo> GetEnumerator() => ElementsAsEnumerable.GetEnumerator();
	}
}
