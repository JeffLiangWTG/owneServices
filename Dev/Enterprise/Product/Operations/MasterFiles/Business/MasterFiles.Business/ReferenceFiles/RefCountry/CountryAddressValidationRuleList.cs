using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CountryAddressValidationRuleList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string NoValidationRule = "NVR";
			public const string MustBeEntered = "MBE";
			public const string MustNotBeEntered = "MNB";
			public const string MustBeFormatted = "MBF";
		}

		public static class Descriptions
		{
			public static MultilingualString NoValidationRule { get { return ResString.GetMultilingualString("CountryAddressValidationRuleList|NoValidationRule", "No Validation Rule"); } }
			public static MultilingualString MustBeEntered { get { return ResString.GetMultilingualString("CountryAddressValidationRuleList|MustBeEntered", "Must Be Entered"); } }
			public static MultilingualString MustNotBeEntered { get { return ResString.GetMultilingualString("CountryAddressValidationRuleList|MustNotBeEntered", "Must Not Be Entered"); } }
			public static MultilingualString MustBeFormatted { get { return ResString.GetMultilingualString("CountryAddressValidationRuleList|MustBeFormatted", "Must Be Formatted"); } }
		}

		public static CodeDescriptionPairList GetCountryAddressValidationRuleList()
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(Codes.NoValidationRule, Descriptions.NoValidationRule),
				new CodeDescriptionPair(Codes.MustBeEntered, Descriptions.MustBeEntered),
				new CodeDescriptionPair(Codes.MustNotBeEntered, Descriptions.MustNotBeEntered),
			};
		}

		public static CodeDescriptionPairList GetPostcodeValidationRuleList(string rule)
		{
			var result = new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(Codes.NoValidationRule, Descriptions.NoValidationRule),
				new CodeDescriptionPair(Codes.MustBeEntered, Descriptions.MustBeEntered),
			};

			if (!string.IsNullOrEmpty(rule))
			{
				result.AddPair(Codes.MustBeFormatted, Descriptions.MustBeFormatted + ": " + rule);
			}

			return result;
		}
	}
}
