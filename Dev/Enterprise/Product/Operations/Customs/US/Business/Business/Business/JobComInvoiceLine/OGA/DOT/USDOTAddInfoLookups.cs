//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDOTAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSDOTAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USDOTAddInfoLookups : AutoUSDOTAddInfoLookups
	{
		public USDOTAddInfoLookups(AutoUSDOTAddInfo parent)
			: base(parent)
		{
		}

		public USCCountryCollection USCountries
		{
			get { return new USCCountryCollection(Factory); }
		}

		public DepartmentOfTransportBoxNumberList BoxNumbers
		{
			get { return new DepartmentOfTransportBoxNumberList(); }
		}

		public ClarificationCodeList ClarificationCodes
		{
			get { return new ClarificationCodeList(); }
		}
	}
}
