using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class CusEntryLineFeeCollection : CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
	{
		public CusEntryLineFeeCollection(CusEntryLine master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override bool AllowNewCore => false;

		public ZDecimal GetAmount(Func<CusEntryLineFee, bool> predicate) => Elements.Cast<CusEntryLineFee>().Where(predicate).Sum(x => x.CF_ChargeAmount);
	}
}
