namespace Enterprise.Customs.TR.Manifest.Business
{
	public static class ValidationConstants
	{
		public static string MustBeLoggedInUnderTRToSendTRMessages
		{
			get { return ResString.GetMultilingualString("2ED025E8-3764-4823-AE10-727BB4F58F50", "To create a message for Turkey you must be logged-in under a Turkey company."); }
		}

		public static string PleaseSupplyAValueTRToSendTRMessages
		{
			get { return ResString.GetMultilingualString("98920FC3-71E3-408B-BFF2-C5A8C0257F91", "Please supply a value"); }
		}
	}
}
