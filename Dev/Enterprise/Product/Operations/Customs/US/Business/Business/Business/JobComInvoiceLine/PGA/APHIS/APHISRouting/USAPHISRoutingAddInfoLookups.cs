//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISRoutingAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAPHISRoutingAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISRoutingAddInfoLookups : AutoUSAPHISRoutingAddInfoLookups
	{
		public USAPHISRoutingAddInfoLookups(AutoUSAPHISRoutingAddInfo parent)
			: base(parent)
		{
		}

		public RoutingTypeList RoutingTypeList
		{
			get { return Factory.GetCachedValue<RoutingTypeList>(); }
		}

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		protected new USAPHISRoutingAddInfo Parent
		{
			get { return (USAPHISRoutingAddInfo)base.Parent; }
		}

		protected APHISRouting Document
		{
			get { return Parent.Parent; }
		}
	}
}
