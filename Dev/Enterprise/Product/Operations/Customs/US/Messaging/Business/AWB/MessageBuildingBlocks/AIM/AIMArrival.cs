namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using CargoWise.Types;
	using Enterprise.Messaging.Business.AWB;

	public partial class AIMArrival : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "ARR");
			AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(4, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(6, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(8, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(11, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(14, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}

		/// <summary>
		/// Air carrier code and number assigned by the importing carrier. Format must be Importing Carrier(2-3 alphanumeric) + Flight Number(NNN, NNNA, NNNN or NNNNA).
		/// </summary>
		[AWBMessageBlockString(3, 5, 8, StatusType.Mandatory, CharType.AlphaNumeric)]
		public ZString FlightNumber;

		/// <summary>
		/// Scheduled arrival date in NNAAA format, where NN is the two character numerical day of the month and AAA is the first three alpha characters of the month. For example December 10 is 10DEC.
		/// </summary>
		[AWBMessageBlockDate(5, StatusType.Mandatory, CharType.AlphaNumeric, "ddMMM")]
		public ZDate ScheduledArrivalDate;

		/// <summary>
		/// Alpha code assigned to one flight when the cargo covered by a single air waybill arrives on more than one aircraft and actual boarded piece count is less than total waybill piece count. Also known as a "split" indicator.
		/// </summary>
		[AWBMessageBlockString(7, 1, 1, StatusType.Conditional, CharType.Alpha)]
		public ZString PartArrivalReference;

		/// <summary>
		/// A code of "B" to signify that the following count is the actual boarded quantity.
		/// </summary>
		[AWBMessageBlockString(9, 1, 1, StatusType.Conditional, CharType.Alpha)]
		public ZString BoardedQuantityIdentifier;

		/// <summary>
		/// Actual number of pieces boarded on this flight. This value must be greater than zero and less than the total piece count of the air waybill.
		/// </summary>
		[AWBMessageBlockDecimal(10, 1, 5, StatusType.Conditional, CharType.Numeric)]
		public ZDecimal BoardedPieceCount;

		/// <summary>
		/// K (Kilos) or L (Pounds)
		/// </summary>
		[AWBMessageBlockString(12, 1, 1, StatusType.Conditional, CharType.Alpha)]
		public ZString WeightCode;

		/// <summary>
		/// Weight of the boarded pieces.
		/// </summary>
		[AWBMessageBlockDecimal(13, 1, 7, StatusType.Conditional, CharType.NumericWithDecimal)]
		public ZDecimal Weight;
	}
}
