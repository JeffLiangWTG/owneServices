//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsClientPickPackParamsByWhsLookups
//
//    This class should be used for overriding collections in AutoWhsClientPickPackParamsByWhsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientPickPackParamsByWhsLookups : AutoWhsClientPickPackParamsByWhsLookups
	{
		public WhsClientPickPackParamsByWhsLookups(AutoWhsClientPickPackParamsByWhs parent)
			: base(parent)
		{
		}

		#region SalesChannels

		public WhsSalesChannelCollection SalesChannels => Factory.GetCachedValue("WhsClientPickPackParamsByWhsLookups|SalesChannels", () => new WhsSalesChannelCollection(Factory));

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("WhsClientPickPackParamsByWhsLookups|Warehouses", () => new WhsWarehouseCollectionWithSecurityCheck(Factory)); }
		}

		#endregion
	}
}
