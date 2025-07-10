namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FD02")]
	public abstract partial class OGAFD02 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFD02()
			: base("FD02")
		{
		}

		/// <summary>
		/// The first or base quantity associated with the FDA line item number. Two decimal places are implied. If the value is a whole number, the two low-order positions contain zeros. If the product is subject to BTA prior notice, this data element is mandatory.
		/// </summary>
		[MessageBlockDecimal(10, 5, "C", 2)]
		public ZDecimal Unit1Quantity;

		/// <summary>
		/// The unit of measure associated with the first quantity. If the product is subject to BTA prior notice, this data element is mandatory.
		/// </summary>
		[MessageBlockString(4, 15, "C")]
		public ZString Unit1Measure;

		/// <summary>
		/// The second or base quantity if it exists associated with the FDA line item number. Two decimal places are implied. If the value is a whole number, the two low-order positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 19, "C", 2)]
		public ZDecimal Unit2Quantity;

		/// <summary>
		/// The unit of measure associated with the second quantity if it exists.
		/// </summary>
		[MessageBlockString(4, 29, "C")]
		public ZString Unit2Measure;

		/// <summary>
		/// The third or base quantity if it exists associated with the FDA line item number. Two decimal places are implied. If the value is a whole number, the two low-order positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 33, "O", 2)]
		public ZDecimal Unit3Quantity;

		/// <summary>
		/// The unit of measure associated with the third quantity if it exists.
		/// </summary>
		[MessageBlockString(4, 43, "C")]
		public ZString Unit3Measure;

		/// <summary>
		/// The fourth or base quantity if it exists associated with the FDA line item number. Two decimal places are implied. If the value is a whole number, the two low-order positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 47, "C", 2)]
		public ZDecimal Unit4Quantity;

		/// <summary>
		/// The unit of measure associated with the fourth quantity if it exists.
		/// </summary>
		[MessageBlockString(4, 57, "C")]
		public ZString Unit4Measure;

		/// <summary>
		/// The fifth or base quantity if it exists associated with the FDA line number. Two decimal places are implied. If the value is a whole number, the two low-order positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 61, "C", 2)]
		public ZDecimal Unit5Quantity;

		/// <summary>
		/// The unit associated with the fifth quantity if it exists.
		/// </summary>
		[MessageBlockString(4, 71, "C")]
		public ZString Unit5Measure;
	}
}
