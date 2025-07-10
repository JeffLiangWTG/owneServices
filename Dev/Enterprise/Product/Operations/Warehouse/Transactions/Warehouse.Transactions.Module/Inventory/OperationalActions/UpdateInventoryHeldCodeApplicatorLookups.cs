using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public sealed class UpdateInventoryHeldCodesApplicatorLookups : ZLookups
	{
		public UpdateInventoryHeldCodesApplicatorLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region InventoryHeldCodeCollection

		public CodeDescriptionPairList InventoryHeldCodeCollection
		{
			get
			{
				return Factory.GetCachedValue("UpdateInventoryHeldCodesApplicatorLookups|InventoryHeldCodeCollection",
					() =>
					{
						var collection = new CodeDescriptionPairList();
						collection.Add(new CodeDescriptionPair("", Res.GetString("UpdateInventoryHeldCodesApplicatorLookups|None", "None")));
						collection.AddRange(new WhsInventoryHeldCodeCollection(Factory));
						return collection;
					});
			}
		}

		#endregion
	}
}
