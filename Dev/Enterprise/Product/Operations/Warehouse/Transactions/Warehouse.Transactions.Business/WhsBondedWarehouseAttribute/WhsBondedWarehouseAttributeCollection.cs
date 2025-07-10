using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeCollection : BusinessObjectCollection<WhsBondedWarehouseAttribute>
	{
		public WhsBondedWarehouseAttributeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsBondedWarehouseAttributeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsBondedWarehouseAttributeCollection(BusinessObjectFactory factory, ZString entryKey, ZShort entryLineNo)
			: this(factory, GetFilterForEntryNoAndEntryLineNo(entryKey, entryLineNo))
		{
		}

		static ZQuery GetFilterForEntryNoAndEntryLineNo(ZString entryKey, ZShort entryLineNo)
		{
			ZQuery filter = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_EntryKey, entryKey);
			filter.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_EntryLineNo, entryLineNo);
			return filter;
		}
	}
}
