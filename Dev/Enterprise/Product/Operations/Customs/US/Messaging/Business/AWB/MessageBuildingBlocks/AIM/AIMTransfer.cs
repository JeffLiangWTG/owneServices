namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using CargoWise.Types;
	using Enterprise.Messaging.Business.AWB;

	public partial class AIMTransfer : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "TRN");
			AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(4, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
			AddSpecialField(6, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(8, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(10, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}

		/// <summary>
		/// The 3-character IATA U.S. airport code of destination or "000" (numeric zeroes) to cancel previously authorized transfer information.
		/// </summary>
		[AWBMessageBlockString(3, 3, 3, StatusType.Mandatory, CharType.AlphaNumeric)]
		public ZString DestinationAirport;

		/// <summary>
		/// Enter "I" (International) when the shipment is to be exported from the CBP territory at the 
		/// U.S. destination airport. Enter "D" (Domestic) when the shipment is to remain at the U.S. destination airport pending further disposition. Enter "R" when this is a Foreign 
		/// Cargo Remaining On Board (FROB) condition. Omit when canceling a previously accepted transfer via TRN/000 (numeric code "000").
		/// </summary>
		[AWBMessageBlockString(5, 1, 1, StatusType.Conditional, CharType.Alpha)]
		public ZString DomesticInternationalIdentifier;

		/// <summary>
		/// Formats accepted: 
		/// NN-NNNNNNNAA or 
		/// NN-NNNNNNNNN (importer/IRS#); NNN-NN-NNNN (SSN); 
		/// NNNNNN-NNNNN (CBP assigned). 
		/// Hyphens required.
		/// 
		/// The air carrier code of the bonded onward carrier.
		/// </summary>
		[AWBMessageBlockString(7, 2, 12, StatusType.Conditional, CharType.Text)]
		public ZString BondedCarrierIDOrOnwardCarrier;

		/// <summary>
		/// When transferring freight to the terminal facility of another airline, the air carrier code may be used. When transferring freight to a bonded deconsolidator, a FIRMS code must be used.
		/// </summary>
		[AWBMessageBlockString(9, 2, 9, StatusType.Conditional, CharType.AlphaNumeric)]
		public ZString BondedPremisesIdentifierOrInbondControlNumber;
	}
}
