namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("35")]
	public abstract partial class ENS35 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS35()
			: base("35")
		{
		}

		/// <summary>
		/// A value representing the bonded ADD duty. Two decimal places are implied. If the duty is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 3, "M", 2)]
		public ZDecimal BondedADDDuty;

		/// <summary>
		/// A code representing the bonded ADD indicator. Valid Bonded ADD Indicator Codes are:
		/// 
		/// 0 = No
		/// 1 = Yes
		/// 
		/// If this code = 1, the ADD Surety Code must be included in positions 49-51 of this record. If this code = 1 and the ADD/CVD Surety Code is space filled, an error message is system generated and the transaction is rejected.
		/// </summary>
		[MessageBlockString(1, 14, "C")]
		public ZString BondedADDIndicator;

		/// <summary>
		/// A value representing the ADD payment duty. Two decimal places are implied. If the duty is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 15, "M", 2)]
		public ZDecimal PayableADDDuty;

		/// <summary>
		/// A value representing the bonded CVD duty. Two decimal places are implied. If the duty is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 26, "M", 2)]
		public ZDecimal BondedCVDDuty;

		/// <summary>
		/// A code representing the bonded CVD indicator. Valid Bonded CVD Indicator Codes are:
		/// 
		/// 0 = No
		/// 1 = Yes
		/// 
		/// If this code = 1, the CVD surety code must be included in positions 49-51 of this record. If this code = 1 and the ADD/CVD Surety Code is space filled, an error message is system generated and the transaction is rejected.
		/// </summary>
		[MessageBlockString(1, 37, "C")]
		public ZString BondedCVDIndicator;

		/// <summary>
		/// A value representing the payable CVD duty. Two decimal places are implied. If the duty is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 38, "M", 2)]
		public ZDecimal PayableCVDDuty;

		/// <summary>
		/// A code representing the ADD/CVD surety. If the bonded indicator in position 14 or 37 = 1, there must be a Surety Code in this data field; otherwise, an error message is system generated and the transaction is rejected. The Surety Code must be valid for the ADD and CVD computation date and must be in an active status for that date. If the surety is not active, an error message is system generated and the transaction is rejected. This code may or may not be the same as the Surety Code in Record Identifier 10. If bonded indicators (positions 14 and/or 37) and not = 1, space fill this field.
		/// </summary>
		[MessageBlockString(3, 49, "C")]
		public ZString ADDCVDSuretyCode;
	}
}
