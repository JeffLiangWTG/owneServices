using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods
{
	public static class RadioactiveConstants
	{
		public const string RadioactiveClass = "7";
	}

	public class RadioactiveLabelCategoryList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string WhiteI = "WH1";
			public const string YellowII = "YL2";
			public const string YellowIII = "YL3";
		}

		public static class Descriptions
		{
			public const string WhiteI = "White-I";
			public const string YellowII = "Yellow-II";
			public const string YellowIII = "Yellow-III";
		}

		public RadioactiveLabelCategoryList()
		{
			AddPair(Codes.WhiteI, Descriptions.WhiteI);
			AddPair(Codes.YellowII, Descriptions.YellowII);
			AddPair(Codes.YellowIII, Descriptions.YellowIII);
		}
	}

	public class RadioactiveMaximumActivityUnitList : CodeDescriptionPairList
	{
		public RadioactiveMaximumActivityUnitList()
		{
			AddPair(RadioactiveUnits.Terabecquerel, RadioactiveUnits.GetDescription(RadioactiveUnits.Terabecquerel));
			AddPair(RadioactiveUnits.Gigabecquerel, RadioactiveUnits.GetDescription(RadioactiveUnits.Gigabecquerel));
			AddPair(RadioactiveUnits.Megabecquerel, RadioactiveUnits.GetDescription(RadioactiveUnits.Megabecquerel));
			AddPair(RadioactiveUnits.Curie, RadioactiveUnits.GetDescription(RadioactiveUnits.Curie));
			AddPair(RadioactiveUnits.Millicurie, RadioactiveUnits.GetDescription(RadioactiveUnits.Millicurie));
			AddPair(RadioactiveUnits.Microcurie, RadioactiveUnits.GetDescription(RadioactiveUnits.Microcurie));
		}
	}
}
