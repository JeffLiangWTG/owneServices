//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseLookups
//
//    This class should be used for overriding collections in AutoUSCACCaseLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseLookups : AutoUSCACCaseLookups
	{
		public USCACCaseLookups(AutoUSCACCase parent) : base(parent)
		{
		}

		public ACCaseStatusList CaseStatusList => Factory.GetCachedValue<ACCaseStatusList>();

		public USCCountryCollection CountryCodeList => new USCCountryCollection(Factory);
	}
}
