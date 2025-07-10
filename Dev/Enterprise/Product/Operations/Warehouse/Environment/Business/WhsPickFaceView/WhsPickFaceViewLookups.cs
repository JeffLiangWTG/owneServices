//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickFaceViewLookups
//
//    This class should be used for overriding collections in AutoWhsPickFaceViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceViewLookups : AutoWhsPickFaceViewLookups
	{
		public WhsPickFaceViewLookups(AutoWhsPickFaceView parent)
			: base(parent)
		{
		}

		public OrgHeaderCollection Clients => Factory.GetCachedValue("WhsPickFaceViewLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory));

		public OrgSupplierPartCollection Parts => Factory.GetCachedValue("WhsPickFaceViewLookups|Parts", () => new OrgSupplierPartCollection(Factory));

		public WhsWarehouseCollectionWithSecurityCheck Warehouses =>
			Factory.GetCachedValue("WhsPickFaceViewLookups|Warehouses", () =>
			{
				var result = new WhsWarehouseCollectionWithSecurityCheck(Factory);
				result.Load();
				return result;
			});

		public WhsLocationCollection Locations => Factory.GetCachedValue("WhsPickFaceViewLookups|Locations", () => new WhsLocationCollection(Factory));
	}
}
