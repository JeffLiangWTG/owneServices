//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusConfigurationLookups
//
//    This class should be used for overriding collections in AutoZZRefCusConfigurationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusConfigurationLookups : AutoZZRefCusConfigurationLookups
	{
		public ZZRefCusConfigurationLookups(AutoZZRefCusConfiguration parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ValuationDateDefaultTypeList => Factory.GetCachedValue<ValuationDateDefaultTypeList>();

		public CodeDescriptionPairList ValuationDateExportDefaultTypeList => Factory.GetCachedValue<ValuationDateExportDefaultTypeList>();

		public CodeDescriptionPairList VATValueCodeList => Factory.GetCachedValue<VATValueCodeList>();

		public CodeDescriptionPairList CustomsValueCodeList => Factory.GetCachedValue<CustomsValueCodeList>();

		public CodeDescriptionPairList IsReciprocalExchangeRateList => Factory.GetCachedValue<IsReciprocalExchangeRateList>();
	}
}
