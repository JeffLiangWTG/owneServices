namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("Y")]
	public partial class AABIInputY : MessageBlock
	{
		public AABIInputY()
			: base("Y")
		{
		}

		/// <summary>
		/// The code for the U.S. port where the enclosed transaction(s) are to be 'processed'.
		/// </summary>
		[MessageBlockString(4, 4, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ProcessingDistrictPortCode;

		/// <summary>
		/// Filer's identification code (as assigned by CBP).
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString FilerCode;

		/// <summary>
		/// A code that identifies the type of transaction data within the block.
		/// </summary>
		[MessageBlockString(2, 11, "M")]
		public ZString ApplicationIdentifierCode;

		/// <summary>
		/// Space fill. 
		/// 
		/// 
		/// 
		/// Number of input images (i.e., records) submitted in the block. The count does not include the B-Record or the Y-Record.
		/// 
		/// Space fill.
		/// </summary>
		[MessageBlockInt(5, 13, "O", FillType.ZeroFillUnlessEmpty)]
		public ZInt InputTransactionImageCountForeMAN; // field name changed

		/// <summary>
		/// A code agreed upon by the Filer and CBP representing a specific Filer 'office' (or sub-location).
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(2, 45, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ProcessingFilerOfficeCode;
	}
}
