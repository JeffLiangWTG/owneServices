using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class MawbApplicationCodeList : CodeDescriptionPairList
	{
		public MawbApplicationCodeList()
		{
			AddPair(Codes.ECI, Descriptions.ECI);
			AddPair(Codes.TSW, Descriptions.TSW);
		}

		static class Codes
		{
			public const string ECI = Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			public const string TSW = Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
		}

		static class Descriptions
		{
			public static MultilingualString ECI { get { return ResString.GetMultilingualString("MawbApplicationCodeList|ECI", "Legacy (ECI)"); } }
			public static MultilingualString TSW { get { return ResString.GetMultilingualString("MawbApplicationCodeList|TSW", "Trade Single Window"); } }
		}
	}
}
