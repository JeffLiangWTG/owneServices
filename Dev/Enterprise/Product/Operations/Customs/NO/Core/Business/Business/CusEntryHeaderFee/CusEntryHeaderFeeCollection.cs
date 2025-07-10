using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public class CusEntryHeaderFeeCollection : NonPersistentBusinessObjectCollection<CusEntryHeaderFee>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CusEntryHeaderFee();
		}

		protected override bool AllowNewCore => false;

		public ZDecimal GetAmount(Func<CusEntryHeaderFee, bool> predicate) => Elements.Cast<CusEntryHeaderFee>().Where(predicate).Sum(x => x.Amount);
	}
}
