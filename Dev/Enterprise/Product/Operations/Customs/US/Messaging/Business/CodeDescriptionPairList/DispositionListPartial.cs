using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business
{
	partial class DispositionList
	{
		public static bool IsNotableFTZDispositionCode(ZString code)
		{
			return code != Codes.B1 &&
				code != Codes.B2 &&
				code != Codes.B3;
		}
	}
}
