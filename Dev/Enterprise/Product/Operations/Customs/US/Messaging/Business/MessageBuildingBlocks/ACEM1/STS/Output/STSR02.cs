namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE + "1")]
	[OutputBlock("R02")]
	public partial class STSR02 : MessageBlock
	{
		public STSR02()
			: base("R02")
		{
		}

		/// <summary>
		/// The bill of lading sequence number. The sequence number must be unique for the issuer (not repeated within three years). This data element does not contain the issuer code; the issuer is included in the preceding J01 record.
		/// </summary>
		[MessageBlockString(12, 4, "M")]
		public ZString BillOfLadingSequenceNumber;

		/// <summary>
		/// A code advising the carrier, NVOCC, port authority, service bureau, exporter, or agent of the posting action taken on a bill of lading, or export transaction. The first character identifies the posting agency and the second identifies the transaction. Refer to the CAMIR Appendix D for valid disposition codes.
		/// </summary>
		[MessageBlockString(2, 16, "M")]
		public ZString DispositionCode;

		/// <summary>
		/// The quantity associated with the Disposition Code. Quantity can be partial amount for release.
		/// </summary>
		[MessageBlockDecimal(10, 18, "C", 0)]
		public ZDecimal Quantity;

		/// <summary>
		/// A code representing the category of entry. See CAMIR Appendix B for valid entry type codes.
		/// </summary>
		[MessageBlockString(2, 28, "C")]
		public ZString EntryType;

		/// <summary>
		/// The CBP entry number, form number (e.g., CBP Form 3299), a regulatory provision, or an in-bond number.
		/// </summary>
		[MessageBlockString(15, 30, "C")]
		public ZString EntryNumber;

		/// <summary>
		/// A date in YYMMDD (year, month, day) format representing the date on which the action was authorized by CBP or another Federal Agency.
		/// </summary>
		[MessageBlockDate(45, "M", "yyMMdd")]
		public ZDate ActionDate;

		/// <summary>
		/// A time in HHMM (hour, minute) 24-hour clock format representing the time that the release (or other posting action) was authorized. Eastern Standard/Daylight time will be returned.
		/// </summary>
		[MessageBlockString(4, 51, "M")]
		public ZString ActionTime;

		/// <summary>
		/// A code N is used when a negative quantity is associated with a disposition code of 1A, 1B or 1C. If not, space fill.
		/// </summary>
		[MessageBlockString(1, 67, "C")]
		public ZString NegativeIndicator;

		/// <summary>
		/// A code 1 indicates there is a second R02 record. Space filled if there is no continuation record.
		/// </summary>
		[MessageBlockString(1, 68, "C")]
		public ZString LineDelimiter;

		/// <summary>
		/// A code Y is used when a HOLD disposition code in R02.Disposition Code is re-sent as a result of a change in Port of Discharge or Vessel Name. Space filled if disposition is not being re-sent.
		/// </summary>
		[MessageBlockString(1, 69, "C")]
		public ZString ResendIndicator;
	}
}
