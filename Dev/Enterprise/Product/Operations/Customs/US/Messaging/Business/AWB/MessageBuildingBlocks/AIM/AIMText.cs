namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using CargoWise.Types;
	using Enterprise.Messaging.Business.AWB;

	public partial class AIMText : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "TXT");
			AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(4, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}

		/// <summary>
		/// Information relative to FSC code.
		/// </summary>
		[AWBMessageBlockString(3, 60, 60, StatusType.Mandatory, CharType.Text)]
		public ZString Information;
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using CargoWise.Types;
	using Enterprise.Messaging.Business.AWB;

	public partial class AIMText_FSC_RoutingInformation : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "TXT");
			AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(3, StatusType.Mandatory, CharType.Alpha, ValueType.ColumnIdentifier, "ARR");
			AddSpecialField(4, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(6, StatusType.Conditional, CharType.Special, ValueType.Special, " ");
			AddSpecialField(7, StatusType.Optional, CharType.Alpha, ValueType.ColumnIdentifier, "PTP");
			AddSpecialField(8, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(10, StatusType.Conditional, CharType.Special, ValueType.Special, " ");
			AddSpecialField(11, StatusType.Optional, CharType.Alpha, ValueType.ColumnIdentifier, "INB");
			AddSpecialField(12, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(15, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(17, StatusType.Conditional, CharType.Special, ValueType.Special, " ");
			AddSpecialField(18, StatusType.Optional, CharType.Alpha, ValueType.ColumnIdentifier, "T");
			AddSpecialField(20, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}

		/// <summary>
		/// ARR – (airport of first arrival in U.S.)
		/// The IATA code of the first airport of arrival in the United States. Valid U.S. airport codes are located in Appendix A.
		/// </summary>
		[AWBMessageBlockString(5, 3, 3, StatusType.Mandatory, CharType.Alpha)]
		public ZString AirportOfArrival;

		/// <summary>
		/// PTP – (Permit-to-Proceed destination, if applicable)
		/// The U.S. airport code of destination when an air waybill is transported by the air carrier under the provisions of a permit to proceed.
		/// </summary>
		[AWBMessageBlockString(9, 3, 3, StatusType.Conditional, CharType.Alpha)]
		public ZString PermitToProceedDestinationAirport;

		/// <summary>
		/// INB – (Transferred in-bond between two U.S. ports)
		/// The U.S. airport code of the in-bond origin port.
		/// </summary>
		[AWBMessageBlockString(13, 3, 3, StatusType.Conditional, CharType.Alpha)]
		public ZString InBondOriginAirport;

		/// <summary>
		/// INB – (Transferred in-bond between two U.S. ports)
		/// The U.S. airport code of the in-bond destination port.
		/// </summary>
		[AWBMessageBlockString(14, 3, 3, StatusType.Conditional, CharType.Alpha)]
		public ZString InBondDestinationAirport;

		/// <summary>
		/// A - the segment has been arrived at the destination
		/// E - it has been both arrived and exported
		/// </summary>
		[AWBMessageBlockString(16, 1, 1, StatusType.Conditional, CharType.Alpha)]
		public ZString InBondStatus;

		/// <summary>
		/// Total number of pieces.
		/// </summary>
		[AWBMessageBlockDecimal(19, 1, 5, StatusType.Conditional, CharType.Numeric)]
		public ZDecimal NumberOfPieces;
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using CargoWise.Types;
	using Enterprise.Messaging.Business.AWB;

	public partial class AIMText_Continuation_FSC_SplitBillArrival : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(3, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(5, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(7, StatusType.Conditional, CharType.Special, ValueType.Special, " ");
			AddSpecialField(10, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}

		/// <summary>
		/// Alpha code assigned to one flight when the cargo covered by a single air waybill arrives on more than one aircraft and actual boarded piece count is less than total waybill piece count. Also known as a "split" indicator.
		/// </summary>
		[AWBMessageBlockString(2, 1, 1, StatusType.Mandatory, CharType.Alpha)]
		public ZString PartArrivalReference;

		/// <summary>
		/// Air carrier code and number assigned by the importing carrier. Format must be Importing Carrier(2-3 alphanumeric) + Flight Number(NNN, NNNA, NNNN or NNNNA).
		/// </summary>
		[AWBMessageBlockString(4, 5, 8, StatusType.Mandatory, CharType.AlphaNumeric)]
		public ZString FlightNumber;

		/// <summary>
		/// Scheduled arrival date in NNAAA format, where NN is the two character numerical day of the month and AAA is the first three alpha characters of the month. For example December 10 is 10DEC.
		/// </summary>
		[AWBMessageBlockDate(6, StatusType.Mandatory, CharType.AlphaNumeric, "ddMMM")]
		public ZDate ScheduledArrivalDate;

		/// <summary>
		/// A code of "B" to signify that the following count is the actual boarded quantity.
		/// </summary>
		[AWBMessageBlockString(8, 1, 1, StatusType.Conditional, CharType.Alpha)]
		public ZString BoardedQuantityIdentifier;

		/// <summary>
		/// Actual number of pieces boarded on this flight. This value must be greater than zero and less than the total piece count of the air waybill.
		/// </summary>
		[AWBMessageBlockDecimal(9, 1, 5, StatusType.Conditional, CharType.Numeric)]
		public ZDecimal BoardedPieceCount;
	}
}
