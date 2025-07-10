//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNMFSVesselsAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNMFSVesselsAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USNMFSVesselsAddInfoLookups : AutoUSNMFSVesselsAddInfoLookups
	{
		public USNMFSVesselsAddInfoLookups(AutoUSNMFSVesselsAddInfo parent) : base(parent)
		{
		}

		public RefCountryCollection RefCountries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public RefVesselCollection RefVessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		public ABIUnitOfMeasureList UnitOfMeasureList
		{
			get { return Factory.GetCachedValue<ABIUnitOfMeasureList>(); }
		}
	}
}
