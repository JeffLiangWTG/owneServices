namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("89")]
	public abstract partial class ENS89 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS89()
			: base("89")
		{
		}

		/// <summary>
		/// The CBP accounting code. Valid class codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString ClassCode;

		/// <summary>
		/// A value representing the total estimated amount for the class code in the preceding field. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 6, "M", 2)]
		public ZDecimal TotalAmount;

		/// <summary>
		/// The CBP accounting code. Valid class codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 17, "C")]
		public ZString ClassCode1;

		/// <summary>
		/// A value representing the total estimated amount for the class code in the preceding field. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 20, "C", 2)]
		public ZDecimal TotalAmount1;

		/// <summary>
		/// The CBP accounting code. Valid class codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 31, "C")]
		public ZString ClassCode2;

		/// <summary>
		/// A value representing the total estimated amount for the class code in the preceding field. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 34, "C", 2)]
		public ZDecimal TotalAmount2;

		/// <summary>
		/// The CBP accounting code. Valid class codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 45, "C")]
		public ZString ClassCode3;

		/// <summary>
		/// A value representing the total estimated amount for the class code in the preceding field. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 48, "C", 2)]
		public ZDecimal TotalAmount3;

		/// <summary>
		/// The CBP accounting code. Valid class codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 59, "C")]
		public ZString ClassCode4;

		/// <summary>
		/// A value representing the total estimated amount for the class code in the preceding field. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 62, "C", 2)]
		public ZDecimal TotalAmount4;
	}
}
