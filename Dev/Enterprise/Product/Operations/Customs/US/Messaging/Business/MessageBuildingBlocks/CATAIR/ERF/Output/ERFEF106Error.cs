namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("EF106")]
	public partial class ERFEF106 : MessageBlock
	{
		public ERFEF106()
			: base("EF106")
		{
		}

		/// <summary>
		/// The carrier name spelled out.
		/// </summary>
		[MessageBlockString(35, 6, "C")]
		public ZString CarrierName;

		/// <summary>
		/// A 2, 3, or 4 position code identifying the carrier.
		/// </summary>
		[MessageBlockString(4, 41, "C")]
		public ZString CarrierCode;
	}
}
