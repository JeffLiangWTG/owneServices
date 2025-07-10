namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J5")]
	public abstract partial class ENQJ5 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ5()
			: base("J5")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 3-5 of the J5 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// The date part of the code indicating the CBP document filing location. This code consists of a 6-position numeric date in MMDDYY (month, day, year) format; a 4-position terminal identification code; a 3-position numeric batch code (001-999); and a 3-position numeric sequence number. For TIB entries (entry type 23), if this data element is shown, the first six digits (the numeric date) indicate the closure date.
		/// </summary>
		[MessageBlockString(6, 15, "C")]
		public ZString CBPDocumentFilingLocationDate;

		/// <summary>
		/// The terminal identification part of the code indicating the CBP document filing location. This code consists of a 6-position numeric date in MMDDYY (month, day, year) format; a 4-position terminal identification code; a 3-position numeric batch code (001-999); and a 3-position numeric sequence number. For TIB entries (entry type 23), if this data element is shown, the first six digits (the numeric date) indicate the closure date.
		/// </summary>
		[MessageBlockString(4, 21, "C")]
		public ZString CBPDocumentFilingLocationTerminalId;

		/// <summary>
		/// The batch code part of the code indicating the CBP document filing location. This code consists of a 6-position numeric date in MMDDYY (month, day, year) format; a 4-position terminal identification code; a 3-position numeric batch code (001-999); and a 3-position numeric sequence number. For TIB entries (entry type 23), if this data element is shown, the first six digits (the numeric date) indicate the closure date.
		/// </summary>
		[MessageBlockString(3, 25, "C")]
		public ZString CBPDocumentFilingLocationBatchCode;

		/// <summary>
		/// The sequence number part of the code indicating the CBP document filing location. This code consists of a 6-position numeric date in MMDDYY (month, day, year) format; a 4-position terminal identification code; a 3-position numeric batch code (001-999); and a 3-position numeric sequence number. For TIB entries (entry type 23), if this data element is shown, the first six digits (the numeric date) indicate the closure date.
		/// </summary>
		[MessageBlockString(3, 28, "C")]
		public ZString CBPDocumentFilingLocationSequenceNumber;

		/// <summary>
		/// A code representing the first protest number.
		/// </summary>
		[MessageBlockDecimal(12, 33, "C", 0)]
		public ZDecimal ProtestNumber1;

		/// <summary>
		/// A code representing the protest type. Valid codes are:
		/// 
		/// 1	514 Protest Section
		/// 2	520(c) Petition 
		/// 3	520(d) Petition
		/// 4	181-115 Intervention
		/// </summary>
		[MessageBlockInt(1, 45, "C")]
		public ZInt ProtestType1;

		/// <summary>
		/// A code representing the status of the first protest. Valid codes are:
		/// 
		/// OP	Open
		/// AP	Approved
		/// DN	Denied
		/// NP	Not Protestable
		/// SP	Suspended
		/// PD	Partly Denied
		/// WD	Withdrawn
		/// UT	Untimely
		/// </summary>
		[MessageBlockString(2, 46, "C")]
		public ZString ProtestStatus1;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the protest decision date. This date is returned only if the status of the protest is other than open.
		/// </summary>
		[MessageBlockDate(48, "C", "MMddyy")]
		public ZDate ProtestDecisionDate1;

		/// <summary>
		/// A code representing the summons status. Valid Summons Indicator Codes are:
		/// 
		/// 1 = Summons
		/// 0 = No Summons
		/// </summary>
		[MessageBlockInt(1, 54, "C")]
		public ZInt SummonsIndicator1;

		/// <summary>
		/// A code representing the second protest number.
		/// </summary>
		[MessageBlockDecimal(12, 55, "C", 0)]
		public ZDecimal ProtestNumber2;

		/// <summary>
		/// A code representing the protest type.
		/// </summary>
		[MessageBlockInt(1, 67, "C")]
		public ZInt ProtestType2;

		/// <summary>
		/// A code representing the status of the second protest.
		/// </summary>
		[MessageBlockString(2, 68, "C")]
		public ZString ProtestStatus2;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the protest decision date. This date is returned only if the status of the protest is other than open.
		/// </summary>
		[MessageBlockDate(70, "C", "MMddyy")]
		public ZDate ProtestDecisionDate2;

		/// <summary>
		/// A code representing the summons status. Valid summons indicator codes are:
		/// 
		/// 1 = Summons
		/// 0 = No Summons
		/// </summary>
		[MessageBlockInt(1, 76, "C")]
		public ZInt SummonsIndicator2;
	}
}
