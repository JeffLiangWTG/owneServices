namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	// This message block has been removed from the latest spec but we still keep it here for old messages
	[InputBlock("SF31")]
	public partial class FTZSF31 : MessageBlock
	{
		public FTZSF31()
			: base("SF31")
		{
		}

		/// <summary>
		/// Code identifying the type of secondary name.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString EntityCode;

		/// <summary>
		/// The secondary name of the entity reported in the SF30 record.
		/// </summary>
		[MessageBlockString(35, 8, "C")]
		public ZString EntityName;
	}
}
