//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFWSLicenseAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSFWSLicenseAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USFWSLicenseAddInfoLookups : AutoUSFWSLicenseAddInfoLookups
	{
		public USFWSLicenseAddInfoLookups(AutoUSFWSLicenseAddInfo parent)
			: base(parent)
		{
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public FWSLicenseTypeList LicenseTypes
		{
			get { return Factory.GetCachedValue<FWSLicenseTypeList>(); }
		}

		protected new USFWSLicenseAddInfo Parent
		{
			get { return (USFWSLicenseAddInfo)base.Parent; }
		}

		protected FWSLicense License
		{
			get { return Parent.Parent; }
		}
	}
}
