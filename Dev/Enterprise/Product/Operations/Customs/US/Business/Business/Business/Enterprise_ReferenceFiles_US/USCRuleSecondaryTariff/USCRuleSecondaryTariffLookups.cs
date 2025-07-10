//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCRuleSecondaryTariffLookups
//
//    This class should be used for overriding collections in AutoUSCRuleSecondaryTariffLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCRuleSecondaryTariffLookups : AutoUSCRuleSecondaryTariffLookups
	{
		public USCRuleSecondaryTariffLookups(AutoUSCRuleSecondaryTariff parent) : base(parent)
		{
		}

		public USCTariffCollection Tariffs => new USCTariffCollection(Factory);
	}
}
