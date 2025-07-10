namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J3")]
	public abstract partial class ENQJ3 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ3()
			: base("J3")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 3-5 of the J3 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry number. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format that represents the cancellation date.
		/// </summary>
		[MessageBlockDate(15, "C", "MMddyy")]
		public ZDate CancellationDate;

		/// <summary>
		/// A code representing the most recent extension/ suspension. Valid Extension/Suspension Codes are:
		/// 
		/// Code	Description
		/// 
		/// 01	Extension, CBP
		/// 02	Extension, Importer
		/// 03	TIB Extension
		/// 04	Suspension, Countervailing
		/// 05	Suspension, Antidumping
		/// 06	Suspension, Court Order
		/// 07	Suspension, Actual Use
		/// 08	Suspension, Other
		/// </summary>
		[MessageBlockInt(2, 21, "C")]
		public ZInt ExtensionSuspensionCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the input date by CBP of the most recent extension/ suspension.
		/// </summary>
		[MessageBlockDate(23, "C", "MMddyy")]
		public ZDate ExtensionSuspensionDate;

		/// <summary>
		/// A number from 00 to 03 representing the number of times the entry summary has been extended. This field may not represent the current extension/suspension code. The entry may have been extended at one time, but may not be currently.
		/// </summary>
		[MessageBlockInt(2, 29, "C")]
		public ZInt NumberOfTimesExtended;

		/// <summary>
		/// A code representing the reference number contained on CBP Form (CBPF) 4811.
		/// </summary>
		[MessageBlockString(12, 31, "C")]
		public ZString CF4811ReferenceNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date selected for this entry summary to appear on the Preliminary Statement.
		/// </summary>
		[MessageBlockDate(43, "C", "MMddyy")]
		public ZDate PreliminaryStatementPrintDate;

		/// <summary>
		/// A code representing the surety.
		/// </summary>
		[MessageBlockString(3, 49, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// A code representing the bond number.
		/// </summary>
		[MessageBlockString(9, 52, "C")]
		public ZString BondNumber;

		/// <summary>
		/// A value representing the antidumping/ countervailing duties paid. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 61, "C", 2)]
		public ZDecimal AntidumpingCountervailingDutyAmountsPaid;

		/// <summary>
		/// An optional code provided by the participant. This field is not edited during ACS processing. It is for internal user system control in entry summary processing.
		/// </summary>
		[MessageBlockString(9, 72, "C")]
		public ZString BrokerReferenceNumber;
	}
}
