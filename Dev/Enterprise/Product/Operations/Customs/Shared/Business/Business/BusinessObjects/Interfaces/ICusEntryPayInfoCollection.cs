using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.BusinessObjects.Interfaces
{
	public interface ICusEntryPayInfoCollection<out TCusEntryPayInfo> : IBusinessObjectCollection<TCusEntryPayInfo>
		where TCusEntryPayInfo : CusEntryPayInfo
	{
		TCusEntryPayInfo GetItemByMessageNum(ZString payResponseNum);

		TCusEntryPayInfo[] GetPendingItems();

		TCusEntryPayInfo InsertOrUpdateEntryPayInfo(Func<TCusEntryPayInfo, bool> predicate, ZDecimal totalAmount, ZString transactionType, ZDateTime expirationDate, ZString incomingPayResponseNo, ZString paymentParty);

		new TCusEntryPayInfo this[int index] { get; }

		new TCusEntryPayInfo AddNew();
	}
}
