using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class TSCAIndicatorList
	{
		public static string GetMessagingCodeFor(ZString code)
		{
			if (code == Codes.TSCAPositive)
			{
				return "TSCA+";
			}
			else if (code == Codes.TSCANegative)
			{
				return "TSCA-";
			}
			else
			{
				return "";
			}
		}
	}
}
