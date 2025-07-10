using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Business
{
	public class CusEntryNumCollection : BusinessObjectCollection<CusEntryNumber>
	{
		public CusEntryNumCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusEntryNumCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		public CusEntryNumber GetFirstNonEmpty()
		{
			foreach (CusEntryNumber cusEntryNum in this)
			{
				if (!cusEntryNum.CE_EntryType.IsEmpty || !cusEntryNum.CE_EntryNum.IsEmpty)
				{
					return cusEntryNum;
				}
			}
			return null;
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CusEntryNumber);
		}
	}
}
