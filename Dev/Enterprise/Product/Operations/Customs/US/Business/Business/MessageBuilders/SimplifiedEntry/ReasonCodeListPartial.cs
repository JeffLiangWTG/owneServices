
namespace Enterprise.Customs.US.Business
{
	partial class ReasonCodeList
	{
		public static bool IsReferenceNoRequired(string code)
		{
			return code == Codes.EntryReplacedBy7512 ||
				code == Codes.EntryReplacedByFTZ ||
				code == Codes.MerchandiseClearedByAnother;
		}
	}
}
