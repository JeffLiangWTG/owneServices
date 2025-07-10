namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common
{
	using CargoWise.Types;

	public partial class APLY : MessageBlock
	{
		public APLY()
			: base("Y")
		{
		}

		/// <summary>
		/// A code representing the processing district/ port. Valid district/port codes can be queried through the Extract Reference File chapter of this publication. This code must be the same as that code in Record Identifier B.
		/// </summary>
		[MessageBlockString(4, 4, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ProcessingDistrictPortCode;

		/// <summary>
		/// A unique code assigned by the CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// A code identifying the application. This code must be the same as that code in Record Identifier B.
		/// </summary>
		[MessageBlockString(2, 11, "M")]
		public ZString ApplicationIdentifier;

		/// <summary>
		/// The number of data detail records in the block. This does not include the header and trailer control record count.
		/// </summary>
		[MessageBlockInt(5, 13, "M")]
		public ZInt NumberOfTransactionDetailRecordsInTheBlock;

		/// <summary>
		/// This applies only to entry summary transactions. The value of the total estimated duty for entry summaries contained in this particular block (from Record Identifier 90, positions 3-13). Two decimal places are implied. If the duty is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(12, 18, "C", 2)]
		public ZDecimal TotalEstimatedDuty;

		/// <summary>
		/// This applies only to entry summary transactions. The value of the total estimated tax (from Record Identifier 90, positions 14-24). Two decimal places are implied. If the tax is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(12, 30, "C", 2)]
		public ZDecimal TotalEstimatedTax;
	}
}
