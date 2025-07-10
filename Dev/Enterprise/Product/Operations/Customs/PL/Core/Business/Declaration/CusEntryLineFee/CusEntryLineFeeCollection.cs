using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLineFeeCollection : EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
{
	public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory) : base(entryLine, factory)
	{
		Sort(nameof(CusEntryLineFee.CF_ChargeType));
	}

	protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction) =>
		property.Name == nameof(CusEntryLineFee.CF_ChargeType)
			? new CusEntryLineFeeChargeTypeComparer(direction)
			: base.GetComparerForSort(property, direction);
}
