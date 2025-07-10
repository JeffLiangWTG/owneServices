using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class GBISubmissionStatusList
	{
		public static ZBool IsWaitingForResponse(ZString code)
		{
			return code == Codes.AwaitingGBIAdd
				|| code == Codes.AwaitingGBIUpdate
				|| code == Codes.AwaitingGBIDelete;
		}
	}
}
