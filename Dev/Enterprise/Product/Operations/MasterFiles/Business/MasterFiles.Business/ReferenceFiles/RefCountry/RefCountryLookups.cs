using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryLookups : AutoRefCountryLookups
	{
		public RefCountryLookups(AutoRefCountry parent)
			: base(parent)
		{
		}

		public EconomicGroupList ListOfEconomicGroups
		{
			get { return new EconomicGroupList(); }
		}

		public CodeDescriptionPairList PostCodeValidationRules => CountryAddressValidationRuleList.GetPostcodeValidationRuleList((Parent as RefCountry).PostcodeFormattingRule?.Format);

		public CodeDescriptionPairList StateAndProvinceValidationRules => CountryAddressValidationRuleList.GetCountryAddressValidationRuleList();

		public CountryAddressFormattingRuleList CountryAddressFormattingRules
		{
			get
			{
				var result = new CountryAddressFormattingRuleList();
				result.Sort();
				return result;
			}
		}

		public ExternalAddressValidationRulesList ExternalAddressValidationRules
		{
			get { return new ExternalAddressValidationRulesList(); }
		}
	}
}
