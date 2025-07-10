namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J8")]
	public abstract partial class ENQJ8 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ8()
			: base("J8")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 3-5 of the J8 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the collection transaction number.
		/// </summary>
		[MessageBlockInt(4, 15, "M")]
		public ZInt CollectionTransactionNumber;

		/// <summary>
		/// A code representing the collection class. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockInt(3, 19, "M")]
		public ZInt CollectionClassCode;

		/// <summary>
		/// A value representing the collection class code amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 22, "C", 2)]
		public ZDecimal CollectionClassCodeAmount;

		/// <summary>
		/// A code representing the collection class. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockInt(3, 33, "C")]
		public ZInt CollectionClassCode1;

		/// <summary>
		/// A value representing the collection class code amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 36, "C", 2)]
		public ZDecimal CollectionClassCodeAmount1;

		/// <summary>
		/// A code representing the collection class. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockInt(3, 47, "C")]
		public ZInt CollectionClassCode2;

		/// <summary>
		/// A value representing the collection class code amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 50, "C", 2)]
		public ZDecimal CollectionClassCodeAmount2;

		/// <summary>
		/// A code representing the collection class. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockInt(3, 61, "C")]
		public ZInt CollectionClassCode3;

		/// <summary>
		/// A value representing the collection class code amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 64, "C", 2)]
		public ZDecimal CollectionClassCodeAmount3;
	}
}
