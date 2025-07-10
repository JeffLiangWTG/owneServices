namespace Enterprise.Freight.Business
{
	public static class CMRConsignmentNoteConstants
	{
		public static class Length
		{
			public const int MaxLinesForAddress = 4;
			public const int MaxAddressLineLength = 30;
			public const int MaxLengthInternationalConsignementNote = 27;
			public const int MaxLengthPlaceOfDelivery = 40;
			public const int MaxDetailsLines = 12;
			public const int MaxLineDetailLength = 80;
			public const int MaxLinesForSendersInstructions = 5;
			public const int MaxLengthForSendersInstructions = 50;
			public const int MaxLinesForSpecialAgreements = 4;
			public const int MaxLengthForSpecialAgreements = 50;
		}

		public static class Formats
		{
			public const string CMRDateFormat = "dd/MM/yyyy";
			public const int NumberOfSignificativeDecimalsToUse = 6;
			public const int TrailingZerosWhenNumberHasNoSignificativeDecimals = 2;
		}

		public static class Delimiters
		{
			public const string CommaAndSpace = ", ";
			public const string SimpleSpace = " ";
			public const string SemicolonAndSpace = "; ";
			public const string DashBetweenSpaces = " - ";
		}
	}
}
