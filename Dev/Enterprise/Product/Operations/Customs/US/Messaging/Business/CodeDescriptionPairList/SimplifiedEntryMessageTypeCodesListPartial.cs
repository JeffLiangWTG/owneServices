using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business
{
	partial class SimplifiedEntryMessageTypeCodesList
	{
		public static bool IsCargoReleaseAccepted(ZString code)
		{
			return code == Codes.MessageAccepted ||
				code == Codes.MessageAcceptedWithWarning ||
				code == Codes.CancellationRequestPending ||
				code == Codes.RecordAcceptedWithWarning ||
				code == "2GC" ||
				code == "2A4";
		}
	}
}
