namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("PG28")]
	public partial class AEPAPG28 : MessageBlock
	{
		public AEPAPG28()
			: base("PG28")
		{
		}

		/// <summary>
		/// The first dimension of the can. If the container is rectangle, the dimension is in width, height, and length order. If the can is cylindrical, the dimensions are in diameter and height order. Can dimension information is restricted to use with acidified and low acid canned foods. The first two spaces are inches. The second two positions are in 16ths.
		/// </summary>
		[MessageBlockString(4, 5, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString CanDimensions1; // field type changed because of special format in ACE FDA

		/// <summary>
		/// The second dimension of the container. If the container is rectangle, the dimension is in width, height, and length order. If the can is cylindrical, the dimensions are in diameter and height order. The first two spaces are inches. The second two positions are in 16ths.
		/// </summary>
		[MessageBlockString(4, 9, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString CanDimensions2; // field type changed because of special format in ACE FDA

		/// <summary>
		/// The third dimension. If the container is rectangle, the dimension is in width, height, and length order. The first two spaces are inches. The second two positions are in 16ths.
		/// </summary>
		[MessageBlockString(4, 13, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString CanDimension3; // field type changed because of special format in ACE FDA

		/// <summary>
		/// Package Tracking Number Code and number fields are combined. The length of Package Tracking Number code has been changed from 3 to 4
		/// </summary>
		[MessageBlockString(54, 17, "C", ShouldTrimBegining = false)]
		public ZString PackageTrackingNumberDetails;
	}
}
