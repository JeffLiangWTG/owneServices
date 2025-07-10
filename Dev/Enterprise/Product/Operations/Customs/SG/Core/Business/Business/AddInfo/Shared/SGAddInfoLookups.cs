//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSGAddInfoLookups
//
//    This class should be used for overriding collections in AutoSGAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGAddInfoLookups : AutoSGAddInfoLookups
	{
		public SGAddInfoLookups(AutoSGAddInfo parent)
			: base(parent)
		{
		}

		public new AutoSGAddInfo Parent
		{
			get { return (AutoSGAddInfo)base.Parent; }
		}

		public TransportModeCodeList TransportTypeList
		{
			get { return transportTypeList ?? (transportTypeList = new TransportModeCodeList()); }
		}
		TransportModeCodeList transportTypeList;

		public UnitOfQuantityCodeList UnitOfQuantityList
		{
			get { return unitOfQuantityList ?? (unitOfQuantityList = new UnitOfQuantityCodeList()); }
		}
		UnitOfQuantityCodeList unitOfQuantityList;

		#region SGCPlaces

		public ZZRefCusCodeListCombinedCollection SGCPlacesList
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Now);
			}
		}

		#endregion

		#region EngineCapacityList

		public EngineCapacityCodeList EngineCapacityList
		{
			get { return engineCapacityList ?? (engineCapacityList = new EngineCapacityCodeList()); }
		}
		EngineCapacityCodeList engineCapacityList;

		#endregion

		#region ESNDP

		public MarkingCodeList ESNDPs
		{
			get { return esndp ?? (esndp = new MarkingCodeList()); }
		}
		MarkingCodeList esndp;

		#endregion

		#region CommodityTypeList

		public CommodityTypeList CommodityTypes
		{
			get { return commodityTypeList ?? (commodityTypeList = new CommodityTypeList()); }
		}
		CommodityTypeList commodityTypeList;

		#endregion
	}
}
