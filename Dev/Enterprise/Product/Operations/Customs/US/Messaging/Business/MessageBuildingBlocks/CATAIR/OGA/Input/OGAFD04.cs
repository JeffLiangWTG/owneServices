namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FD04")]
	public abstract partial class OGAFD04 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFD04()
			: base("FD04")
		{
		}

		/// <summary>
		/// The sixth or base quantity if it exists associated with the FDA line item number. Two decimal places are implied. If the value is a whole number, the two low-order positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 5, "C", 2)]
		public ZDecimal Unit6Quantity;

		/// <summary>
		/// The unit of measure associated with the sixth quantity if it exists.
		/// </summary>
		[MessageBlockString(4, 15, "C")]
		public ZString Unit6Measure;

		/// <summary>
		/// The name of the individual that FDA can contact. If the product is subject to BTA prior notice requirements, this data element is mandatory. May be different from submitter.
		/// </summary>
		[MessageBlockString(10, 19, "C")]
		public ZString ContactName;

		/// <summary>
		/// The telephone number of the contact person. Omit dashes. If the product is subject to BTA prior notice requirements, this data element is mandatory. May be different from submitter.
		/// </summary>
		[MessageBlockString(10, 29, "C")]
		public ZString ContactTelephoneNumber;

		/// <summary>
		/// Do not program this data element unless requested by CBP ACS.
		/// </summary>
		[MessageBlockDecimal(12, 39, "C", 0)]
		public ZDecimal ValuePerBaseUnit;
	}
}
