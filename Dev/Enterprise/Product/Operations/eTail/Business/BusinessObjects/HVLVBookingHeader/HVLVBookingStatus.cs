using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public static class HVLVBookingStatus
	{
		public static CodeDescriptionPairList GetAll()
		{
			var result = new CodeDescriptionPairList();
			result.Add(new CodeDescriptionPair(BookedCode, BookedDescription));
			result.Add(new CodeDescriptionPair(ConfirmedCode, ConfirmedDescription));
			return result;
		}

		public const string BookedCode = "BKD";
		public const string ConfirmedCode = "CNF";

		public static string BookedDescription => Res.GetString("d621ef82-7c38-4f6b-9d6e-c157a49cc6a6", "Booked");
		public static string ConfirmedDescription => Res.GetString("f178a06f-dd79-4a01-949a-cdeb97d8ec9a", "Confirmed");
	}
}
