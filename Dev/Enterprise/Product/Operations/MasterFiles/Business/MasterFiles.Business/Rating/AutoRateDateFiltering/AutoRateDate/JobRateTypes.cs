using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class JobRateTypes
	{
		public static class Codes
		{
			public const string All = "";
			public const string Revenue = "REV";
			public const string Cost = "CST";
		}

		public static class Descriptions
		{
			public static MultilingualString All => ResString.GetMultilingualString("MasterFiles|JobRateTypes|Any", "Any Rate Type");
			public static MultilingualString Revenue => ResString.GetMultilingualString("MasterFiles|JobRateTypes|Revenue", "Revenue");
			public static MultilingualString Cost => ResString.GetMultilingualString("MasterFiles|JobRateTypes|Cost", "Cost");
		}

		public static CodeDescriptionPairList JobRateTypeList
		{
			get
			{
				var rateTypeList = new CodeDescriptionPairList();

				rateTypeList.AddPair(Codes.All, Descriptions.All);
				rateTypeList.AddPair(Codes.Revenue, Descriptions.Revenue);
				rateTypeList.AddPair(Codes.Cost, Descriptions.Cost);

				return rateTypeList;
			}
		}
	}
}
