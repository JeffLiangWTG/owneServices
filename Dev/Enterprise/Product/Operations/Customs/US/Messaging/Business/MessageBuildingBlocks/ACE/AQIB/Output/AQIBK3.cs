namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("K3")]
	public partial class AQIBK3 : MessageBlock
	{
		public AQIBK3()
			: base("K3")
		{
		}

		/// <summary>
		/// Street address or post office box number of the importer/consignee.
		/// </summary>
		[MessageBlockString(32, 3, "C")]
		public ZString AddressLineOne;

		/// <summary>
		/// Second line of the mailing address.
		/// </summary>
		[MessageBlockString(32, 35, "C")]
		public ZString AddressLineTwo;
	}
}