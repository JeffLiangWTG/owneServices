//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsVASOrderLookups
//
//    This class should be used for overriding collections in AutoWhsVASOrderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderLookups : AutoWhsVASOrderLookups
	{
		public WhsVASOrderLookups(AutoWhsVASOrder parent)
			: base(parent)
		{
		}

		#region Areas

		public WhsAreaCollection Areas
		{
			get
			{
				if (Parent.WarehousePK.IsValid)
				{
					return Factory.GetCachedValue(string.Format(Culture.Invariant, (NoResString)"WhsVASOrderLookups|Areas|{0}", Parent.WarehousePK), () => new WhsAreaCollection(Factory, Parent.Warehouse)); // Key used in Factory Cache
				}
				else
				{
					return Factory.GetCachedValue("WhsVASOrderLookups|Areas", () => new WhsAreaCollection(Factory)); // Key used in Factory Cache
				}
			}
		}

		#endregion

		#region Clients

		public override OrgHeaderCollection Clients
		{
			get { return Factory.GetCachedValue("WhsVASOrderLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory)); }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("WhsVASOrderLookups|Warehouses", () => new WhsWarehouseCollectionWithSecurityCheck(Factory)); }
		}

		#endregion

		#region Parent

		protected new WhsVASOrder Parent
		{
			get { return (WhsVASOrder)base.Parent; }
		}

		#endregion
	}
}
