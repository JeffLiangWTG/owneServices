namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J7")]
	public abstract partial class ENQJ7 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ7()
			: base("J7")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 3-5 of the J7 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code assigned to the collection transaction.
		/// </summary>
		[MessageBlockInt(4, 15, "M")]
		public ZInt CollectionTransactionNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the collection processing date. The collection date should be input as the date the entry summary with duties is filed with CBP. The collection process date may be the same as the collection date or may be a later date.
		/// </summary>
		[MessageBlockDate(19, "M", "MMddyy")]
		public ZDate CollectionProcessingDate;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the collection date. The collection date should be input as the date the entry summary with duties is filed with CBP. The collection process date may be the same as the collection date or may be a later date.
		/// </summary>
		[MessageBlockDate(25, "M", "MMddyy")]
		public ZDate CollectionDate;

		/// <summary>
		/// A value representing the total amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 31, "M", 2)]
		public ZDecimal TotalAmount;

		/// <summary>
		/// A code representing the collection class. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockInt(3, 42, "M")]
		public ZInt ClassCode;

		/// <summary>
		/// A value representing the class code amount. Two decimal places are implied. If the number is a whole number, the two low-order positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 45, "M", 2)]
		public ZDecimal ClassCodeAmount;

		/// <summary>
		/// A code representing the collection class. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockInt(3, 56, "C")]
		public ZInt ClassCode1;

		/// <summary>
		/// A value representing the class code amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 59, "C", 2)]
		public ZDecimal ClassCodeAmount1;

		/// <summary>
		/// A code of 1 if the error is included on the CBP Error Report; otherwise, space fill.
		/// </summary>
		[MessageBlockInt(1, 70, "C")]
		public ZInt ErrorStatus;
	}
}
