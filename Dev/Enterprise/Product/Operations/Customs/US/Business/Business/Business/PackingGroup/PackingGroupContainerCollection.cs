using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PackingGroupContainerCollection : Customs.Business.BasePackingGroupContainerCollection
	{
		public PackingGroupContainerCollection(CusContainer container)
			: base(container)
		{
		}

		public new PackingGroup this[int index]
		{
			get { return (PackingGroup)base[index]; }
		}

		public new PackingGroup AddNew()
		{
			return (PackingGroup)base.AddNew();
		}

		/// <summary>
		/// for each house bill linked to elements of this collection, return the first HouseBill if there is only one master bill number otherwise return null
		/// </summary>
		public Bill UniqueBillByMasterBill
		{
			get
			{
				if (uniqueBillCached == null)
				{
					uniqueBillCached = new CachedProperty<Bill>(Factory, delegate
					{
						List<ZString> masterBillNumbers = new List<ZString>();

						foreach (PackingGroup packGroup in this)
						{
							if (packGroup.Bill != null)
							{
								ZString masterBill = packGroup.Bill.CU_MasterBill;

								if (!masterBillNumbers.Contains(masterBill))
								{
									masterBillNumbers.Add(masterBill);
								}
							}
						}

						return masterBillNumbers.Count == 1 ? (Bill)Container.Declaration.Bills.FindByBillNumberAndType(masterBillNumbers[0], Customs.Business.BillTypeList.Codes.MasterBill) : null;
					}
					);
				}
				return uniqueBillCached.Value;
			}
		}
		CachedProperty<Bill> uniqueBillCached;
	}
}
