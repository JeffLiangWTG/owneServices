namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("30")]
	public partial class AIRQX30 : MessageBlock
	{
		public AIRQX30()
			: base("30")
		{
		}

		/// <summary>
		/// This data element is currently used by the QP message set and is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(1, 3, "O")]
		public ZString ActionCode;

		/// <summary>
		/// Must be Y (Yes)
		/// </summary>
		[MessageBlockString(1, 4, "M")]
		public ZString BillIndicator;

		/// <summary>
		/// This data element is currently used by the QP message set and is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(4, 9, "O")]
		public ZString IssuerCodeOfMasterBillNumber;

		/// <summary>
		/// The master bill number as listed on the manifest. Do not include spaces, hyphens, slashes or special characters.
		/// </summary>
		[MessageBlockString(11, 13, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString MasterBillNumber;

		/// <summary>
		/// This data element is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(4, 25, "O")]
		public ZString IssuerCodeOfHouseBill;

		/// <summary>
		/// The house bill number as listed on the manifest. Left justified bill number and space fill following number.
		/// </summary>
		[MessageBlockString(12, 29, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString HouseBillNumber;

		/// <summary>
		/// This data element is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(4, 41, "C")]
		public ZString IssuerOfSubhouseBillNumber;

		/// <summary>
		/// This data element is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(12, 45, "C")]
		public ZString SubhouseBillNumber;

		/// <summary>
		/// The number identifying the previous in-bond movement. If the in-bond number is less than 11 positions, left justify. Do not include spaces, hyphens, slashes, or other special characters.
		/// </summary>
		[MessageBlockString(11, 57, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString PreviousInbondNumber;

		/// <summary>
		/// The data element is reserved for future use. Space fill. The full bill quantity will be applied to the in-bond.
		/// </summary>
		[MessageBlockDecimal(10, 69, "C", 0)]
		public ZDecimal InbondQuantity;
	}
}
