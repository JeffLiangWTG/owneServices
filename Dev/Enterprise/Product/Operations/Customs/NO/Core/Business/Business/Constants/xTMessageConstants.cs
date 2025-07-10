namespace Enterprise.Customs.NO.Business;

static class xTMessageConstants
{
	public static class MessageTypes
	{
		public static class Codes
		{
			public const string Acknowledgement = "ACK";
			public const string Error = "ERR";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "These are constants being used in registry.")]
		public static class Descriptions
		{
			public const string Acknowledgement = "Acknowledgement";
			public const string Error = "Rejected";
		}
	}
}
