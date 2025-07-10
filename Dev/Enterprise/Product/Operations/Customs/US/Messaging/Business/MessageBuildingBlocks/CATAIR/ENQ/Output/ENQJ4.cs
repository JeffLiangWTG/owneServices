namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J4")]
	public abstract partial class ENQJ4 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ4()
			: base("J4")
		{
		}

		/// <summary>
		/// A code representing the district/port of entry summary. This code is required for entry numbers issued prior to October 1, 1986, and it is optional for entry numbers issued after that date. If no district/port of entry summary is provided, space fill. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntrySummary;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 7-9 of the J4 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the importer of record number.
		/// </summary>
		[MessageBlockString(12, 19, "M")]
		public ZString ImporterOfRecord;

		/// <summary>
		/// A code representing the ultimate consignee.
		/// </summary>
		[MessageBlockString(12, 31, "C")]
		public ZString UltimateConsignee;

		/// <summary>
		/// The reference number contained on CBP Form (CBP F) 4811
		/// </summary>
		[MessageBlockString(12, 43, "C")]
		public ZString CF4811ReferenceNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the liquidation/ cancellation date.
		/// </summary>
		[MessageBlockDate(55, "M", "MMddyy")]
		public ZDate LiquidationCancellationDate;

		/// <summary>
		/// A code representing the liquidation type. Valid Liquidation Type Codes are:
		/// 
		/// 1 = No change
		/// 2 = Bill
		/// 3 = Refund
		/// 4 = Reliquidation Bill
		/// 5 = Reliquidation Refund
		/// 6 = Auto Liquidation
		/// 7 = Deemed Liquidation
		/// 8 = Cancelled
		/// </summary>
		[MessageBlockInt(1, 61, "M")]
		public ZInt LiquidationType;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the archive date. The archive date is the date the entry data is taken from the Entry Master File and moved to the Abbreviated Liquidation File.
		/// </summary>
		[MessageBlockDate(62, "M", "MMddyy")]
		public ZDate ArchiveDate;

		/// <summary>
		/// A code representing the reason for the bill. Refer to Record Identifier J1, Note 2 of this chapter for valid codes.
		/// </summary>
		[MessageBlockInt(2, 68, "C")]
		public ZInt BillReasonCode;
	}
}
