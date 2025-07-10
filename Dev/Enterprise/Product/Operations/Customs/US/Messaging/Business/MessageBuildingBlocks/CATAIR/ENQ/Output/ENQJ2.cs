namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J2")]
	public abstract partial class ENQJ2 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ2()
			: base("J2")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 3-5 of the J2 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the collection date.
		/// </summary>
		[MessageBlockDate(15, "C", "MMddyy")]
		public ZDate CollectionDate;

		/// <summary>
		/// A value representing the estimated duties paid. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 21, "C", 2)]
		public ZDecimal EstimatedDutiesPaid;

		/// <summary>
		/// A value representing the estimated taxes paid. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 32, "C", 2)]
		public ZDecimal EstimatedTaxesPaid;

		/// <summary>
		/// A numeric date in the MMDDYY (month, day, year) format representing the liquidation date. TIB entries (entry type 52) may be closed, but will not show liquidation date. See record identifier J5, CBP Document Filing Location data element.
		/// </summary>
		[MessageBlockDate(43, "C", "MMddyy")]
		public ZDate LiquidationDate;

		/// <summary>
		/// A value representing the liquidated duty. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 49, "C", 2)]
		public ZDecimal LiquidatedDuty;

		/// <summary>
		/// A value representing the liquidated tax. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 60, "C", 2)]
		public ZDecimal LiquidatedTax;

		/// <summary>
		/// A number indicating the number of withdrawals.
		/// </summary>
		[MessageBlockInt(3, 71, "C")]
		public ZInt NumberOfWithdrawals;

		/// <summary>
		/// The import specialist team number.
		/// </summary>
		[MessageBlockString(3, 74, "C")]
		public ZString ImportSpecialistTeam;

		/// <summary>
		/// The number of line items included in this record.
		/// </summary>
		[MessageBlockInt(3, 77, "C")]
		public ZInt NumberOfLineItems;

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
		[MessageBlockString(1, 80, "C")]
		public ZString PaperlessStatusIndicator;
	}
}
