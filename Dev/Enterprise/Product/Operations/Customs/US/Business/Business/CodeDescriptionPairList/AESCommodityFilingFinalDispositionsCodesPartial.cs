using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public partial class AESCommodityFilingFinalDispositionsCodes
	{
		public static string GetEntryStatus(string code)
		{
			string result;
			switch (code)
			{
				case Codes._97H:
					result = AESDirectCustomsEntryStatus.Codes.Hold;
					break;
				case Codes._97R:
					result = AESDirectCustomsEntryStatus.Codes.Released;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}
	}
}
