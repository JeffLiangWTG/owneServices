namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J1")]
	public abstract partial class ENQJ1 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ1()
			: base("J1")
		{
		}

		/// <summary>
		/// A code representing the district/port of entry summary. This code is required for entry numbers issued prior to October 1, 1986, and it is optional for entry numbers issued after that date. If no district/port of entry summary is provided, space fill. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntrySummary;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 7-9 of the J1 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the importer of record.
		/// </summary>
		[MessageBlockString(12, 19, "C")]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// A code representing the entry type. Valid entry type codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 31, "C")]
		public ZString EntryType;

		/// <summary>
		/// A value representing the approximate payment due on an imported product. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 33, "C", 2)] // 2 decimals
		public ZDecimal EstimatedDuty;

		/// <summary>
		/// A value representing the approximate tax due on an imported product. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 44, "C", 2)]
		public ZDecimal EstimatedTax;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of entry into the United States.
		/// </summary>
		[MessageBlockDate(55, "C", "MMddyy")]
		public ZDate DateOfEntry;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the summary filing date.
		/// </summary>
		[MessageBlockDate(61, "C", "MMddyy")]
		public ZDate SummaryFilingDate;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the latest reject date.
		/// </summary>
		[MessageBlockDate(67, "C", "MMddyy")]
		public ZDate RejectDate;

		/// <summary>
		/// A code representing the collection status. Valid Collection Status Codes are:
		/// 
		/// 0 = Unpaid
		/// 1 = Not fully paid
		/// 2 = Fully paid
		/// </summary>
		[MessageBlockString(1, 73, "M")]
		public ZString CollectionStatus;

		/// <summary>
		/// A code representing the release status. Valid Release Status Codes are:
		/// 
		/// 0 = Not released
		/// 1 = Released
		/// 2 = Info Not Permitted
		/// 
		/// Release Status Code 2 will be returned if the query date is less than five days from the CBP release/examination date, the entry is for a border port location, and the mode of transportation code is 12, 20, 21, 30, 31, 32, 33 or 34. For a description of the mode of transportation codes, refer to Appendix B of this publication.
		/// </summary>
		[MessageBlockString(1, 74, "M")]
		public ZString ReleaseStatus;

		/// <summary>
		/// A code representing the entry summary status. Valid Entry Summary Status Codes are:
		/// 
		/// ABI Status
		/// 1 = Error free
		/// 2 = With Census Warnings
		/// 
		/// CBP Status
		/// 3 = Rejected
		/// 4 = Cancelled
		/// 5 = Accepted/Not Liquidated
		/// 6 = Liquidated
		/// 7 = Reliquidated
		/// </summary>
		[MessageBlockInt(1, 75, "M")]
		public ZInt EntrySummaryStatus;

		/// <summary>
		/// A code representing the reason for the bill.
		/// </summary>
		[MessageBlockString(2, 76, "C")]
		public ZString BillReasonCode;

		/// <summary>
		/// 1 = Accelerated drawback approved and paid
		/// 2 = Accelerated drawback approved but not yet paid
		/// 3 = Accelerated drawback not claimed or not approved
		/// </summary>
		[MessageBlockInt(1, 78, "M")]
		public ZInt AcceleratedDrawbackIndicator;

		/// <summary>
		/// A code indicating the paperless status. Valid codes are:
		/// 
		/// Space fill = Paper
		/// E = Paperless Summary (Electronic Invoice)
		/// U = Paperless Summary (Rulings)
		/// B = Paperless Summary (Bypass)
		/// R = Paper (Documents Required)
		/// I = Paperless Summary (Informal)
		/// P = Paperless Summary (ACE)
		/// </summary>
		[MessageBlockString(1, 79, "C")]
		public ZString PaperlessStatusIndicator;

		/// <summary>
		/// A code indicating electronic invoice capability. Valid codes are:
		/// 
		/// Space fill = No electronic invoice capability for summary data
		/// E = Filer has declared ability to transmit electronically complete summary data
		/// </summary>
		[MessageBlockString(1, 80, "C")]
		public ZString ElectronicInvoiceIndicator;
	}
}
