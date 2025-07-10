using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class VoyageRecyclingPeriodList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Default = "DEF";
			public const string None = "NON";
			public const string ThreeMonths = "3";
			public const string SixMonths = "6";
			public const string NineMonths = "9";
			public const string TwelveMonths = "12";
			public const string TwentyFourMonths = "24";
			public const string ThirtySixMonths = "36";
			public const string FortyEightMonths = "48";
			public const string SixtyMonths = "60";
			public const string OneHundredAndTwentyMonths = "120";
		}

		public static class Descriptions
		{
			public static MultilingualString Default { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|DEF", "Default to System Registry"); } }
			public static MultilingualString None { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|NON", "None"); } }
			public static MultilingualString ThreeMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|3", "3 Months"); } }
			public static MultilingualString SixMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|6", "6 Months"); } }
			public static MultilingualString NineMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|9", "9 Months"); } }
			public static MultilingualString TwelveMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|12", "12 Months"); } }
			public static MultilingualString TwentyFourMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|24", "24 Months"); } }
			public static MultilingualString ThirtySixMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|36", "36 Months"); } }
			public static MultilingualString FortyEightMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|48", "48 Months"); } }
			public static MultilingualString SixtyMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|60", "60 Months"); } }
			public static MultilingualString OneHundredAndTwentyMonths { get { return ResString.GetMultilingualString("VoyageRecyclingPeriodList|120", "120 Months"); } }
		}

		public VoyageRecyclingPeriodList(bool withDefault = false)
		{
			if (withDefault)
			{
				AddPair(Codes.Default, Descriptions.Default);
			}

			AddPair(Codes.None, Descriptions.None);
			AddPair(Codes.ThreeMonths, Descriptions.ThreeMonths);
			AddPair(Codes.SixMonths, Descriptions.SixMonths);
			AddPair(Codes.NineMonths, Descriptions.NineMonths);
			AddPair(Codes.TwelveMonths, Descriptions.TwelveMonths);
			AddPair(Codes.TwentyFourMonths, Descriptions.TwentyFourMonths);
			AddPair(Codes.ThirtySixMonths, Descriptions.ThirtySixMonths);
			AddPair(Codes.FortyEightMonths, Descriptions.FortyEightMonths);
			AddPair(Codes.SixtyMonths, Descriptions.SixtyMonths);
			AddPair(Codes.OneHundredAndTwentyMonths, Descriptions.OneHundredAndTwentyMonths);
		}

		public const short Default = -1;
		public const short Invalid = -2;

		public static ZShort GetAmountFromCode(ZString code)
		{
			switch (code)
			{
				case Codes.Default:
					return Default;
				case Codes.None:
					return 0;
				case Codes.ThreeMonths:
					return 3;
				case Codes.SixMonths:
					return 6;
				case Codes.NineMonths:
					return 9;
				case Codes.TwelveMonths:
					return 12;
				case Codes.TwentyFourMonths:
					return 24;
				case Codes.ThirtySixMonths:
					return 36;
				case Codes.FortyEightMonths:
					return 48;
				case Codes.SixtyMonths:
					return 60;
				case Codes.OneHundredAndTwentyMonths:
					return 120;
				default:
					return Invalid;
			}
		}

		public static ZString GetCodeFromAmount(ZShort amount)
		{
			switch ((short)amount)
			{
				case Default:
					return Codes.Default;
				case 0:
					return Codes.None;
				case 3:
					return Codes.ThreeMonths;
				case 6:
					return Codes.SixMonths;
				case 9:
					return Codes.NineMonths;
				case 12:
					return Codes.TwelveMonths;
				case 24:
					return Codes.TwentyFourMonths;
				case 36:
					return Codes.ThirtySixMonths;
				case 48:
					return Codes.FortyEightMonths;
				case 60:
					return Codes.SixtyMonths;
				case 120:
					return Codes.OneHundredAndTwentyMonths;
				default:
					return ZString.Empty;
			}
		}
	}
}
