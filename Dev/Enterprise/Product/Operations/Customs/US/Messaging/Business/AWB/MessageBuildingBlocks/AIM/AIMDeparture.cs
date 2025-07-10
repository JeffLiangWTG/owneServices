namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using CargoWise.Types;
	using Enterprise.Messaging.Business.AWB;

	public partial class AIMDeparture : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "DEP");
			AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(4, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(6, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(9, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(12, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}

		/// <summary>
		/// Air carrier code and number assigned by the importing carrier. Format must be Importing Carrier(2-3 alphanumeric) + Flight Number(NNN, NNNA, NNNN or NNNNA).
		/// </summary>
		[AWBMessageBlockString(3, 5, 8, StatusType.Mandatory, CharType.AlphaNumeric)]
		public ZString FlightNumber;

		/// <summary>
		/// Scheduled date of arrival at the first US airport in NNAAA format.
		/// </summary>
		[AWBMessageBlockDate(5, StatusType.Mandatory, CharType.AlphaNumeric, "ddMMM")]
		public ZDate DateOfScheduledArrival;

		/// <summary>
		/// Actual departure date in NNAAA format at last foreign airport.
		/// </summary>
		[AWBMessageBlockDate(7, StatusType.Mandatory, CharType.AlphaNumeric, "ddMMM")]
		public ZDate LiftoffDate;

		/// <summary>
		/// Actual departure time (GMT) in HHMM 
		/// (hour, minute) format.
		/// </summary>
		[AWBMessageBlockString(8, 4, 4, StatusType.Mandatory, CharType.Numeric)]
		public ZString LiftoffTime;

		/// <summary>
		/// The carrier code of the actual airline that is carrying the freight.
		/// </summary>
		[AWBMessageBlockString(10, 2, 3, StatusType.Conditional, CharType.AlphaNumeric)]
		public ZString ActualImportingCarrier;

		/// <summary>
		/// Flight number for actual flight that is carrying the freight. Valid flight number formats are: three numeric (NNN), three numeric followed by an alpha character (NNNA), four numeric (NNNN), or four numeric following by an alpha character (NNNNA).
		/// </summary>
		[AWBMessageBlockString(11, 3, 5, StatusType.Conditional, CharType.AlphaNumeric)]
		public ZString ActualFlightNumber;
	}
}
