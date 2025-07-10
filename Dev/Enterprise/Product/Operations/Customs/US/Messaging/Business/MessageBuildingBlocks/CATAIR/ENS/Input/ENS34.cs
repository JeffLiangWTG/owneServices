namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("34")]
	public abstract partial class ENS34 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS34()
			: base("34")
		{
		}

		/// <summary>
		/// The CBP accounting class code.
		/// </summary>
		[MessageBlockString(3, 3, "M")]//If ZInt, the leading zeros will be lost
		public ZString ClassCode;

		/// <summary>
		/// A value representing the amount due. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 6, "M", 2)]
		public ZDecimal Amount;

		/// <summary>
		/// The CBP accounting class code.
		/// </summary>
		[MessageBlockString(3, 14, "C")]
		public ZString ClassCode1;

		/// <summary>
		/// A value representing the amount due. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros. If the preceding Class Code data field contains data, this Amount data field must be completed.
		/// </summary>
		[MessageBlockDecimal(8, 17, "C", 2)]
		public ZDecimal Amount1;

		/// <summary>
		/// The CBP accounting class code.
		/// </summary>
		[MessageBlockString(3, 25, "C")]
		public ZString ClassCode2;

		/// <summary>
		/// A value representing the amount due. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros. If the preceding Class Code data field contains data, this Amount data field must be completed.
		/// </summary>
		[MessageBlockDecimal(8, 28, "C", 2)]
		public ZDecimal Amount2;

		/// <summary>
		/// The CBP accounting class code.
		/// </summary>
		[MessageBlockString(3, 36, "C")]
		public ZString ClassCode3;

		/// <summary>
		/// A value representing the amount due. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros. If the preceding Class Code data field contains data, this Amount data field must be completed.
		/// </summary>
		[MessageBlockDecimal(8, 39, "C", 2)]
		public ZDecimal Amount3;

		/// <summary>
		/// The CBP accounting class code.
		/// </summary>
		[MessageBlockString(3, 47, "C")]
		public ZString ClassCode4;

		/// <summary>
		/// A value representing the amount due. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros. If the preceding Class Code data field contains data, this Amount data field must be completed.
		/// </summary>
		[MessageBlockDecimal(8, 50, "C", 2)]
		public ZDecimal Amount4;

		/// <summary>
		/// The CBP accounting class code.
		/// </summary>
		[MessageBlockString(3, 58, "C")]
		public ZString ClassCode5;

		/// <summary>
		/// A value representing the amount due. Two decimal places are implied. If the amount is a whole number, the two low-order (cents) positions contain zeros. If the preceding Class Code data field contains data, this Amount data field must be completed.
		/// </summary>
		[MessageBlockDecimal(8, 61, "C", 2)]
		public ZDecimal Amount5;
	}
}
