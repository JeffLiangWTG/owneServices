//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsVASOrderLineLookups
//
//    This class should be used for overriding collections in AutoWhsVASOrderLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderLineLookups : AutoWhsVASOrderLineLookups
	{
		public WhsVASOrderLineLookups(AutoWhsVASOrderLine parent)
			: base(parent)
		{
		}

		#region Products

		public override OrgSupplierPartCollection Products
		{
			get
			{
				var client = Parent.VASOrder != null ? Parent.VASOrder.Client : null;
				return client == null
					? Factory.GetCachedValue("WhsVASOrderLine|Products", () => new WhsOrgSupplierPartCollection(Factory))
					: Factory.GetCachedValue(string.Format(Culture.Invariant, (NoResString)"WhsVASOrderLine|Products|{0}", client.PK.ToString()), // Key used in Factory Cache
						() =>
						{
							var collection = new WhsOrgSupplierPartCollection(Factory, null, client, false);
							collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", client.PK));
							return collection;
						});
			}
		}

		#endregion

		#region Parent

		protected new WhsVASOrderLine Parent
		{
			get { return (WhsVASOrderLine)base.Parent; }
		}

		#endregion
	}
}
