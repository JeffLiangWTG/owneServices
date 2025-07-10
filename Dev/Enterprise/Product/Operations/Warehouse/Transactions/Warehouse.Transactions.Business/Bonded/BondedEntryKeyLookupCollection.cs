using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class BondedEntryKeyLookupCollection : WhsInventoryViewCollection
	{
		public BondedEntryKeyLookupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new BondedEntryKeyFindBoxListProvider(this); }
		}

		class BondedEntryKeyFindBoxListProvider : FindBoxListProvider
		{
			public BondedEntryKeyFindBoxListProvider(WhsInventoryViewCollection collection)
				: base(collection)
			{
			}

			protected override string GetCodePropertyName(ZGuid pK)
			{
				return WhsInventoryViewSchema.WI_BondedEntryKey.Name;
			}

			protected override string GetCodePropertyName(string code)
			{
				return WhsInventoryViewSchema.WI_BondedEntryKey.Name;
			}
		}

		#endregion
	}
}
