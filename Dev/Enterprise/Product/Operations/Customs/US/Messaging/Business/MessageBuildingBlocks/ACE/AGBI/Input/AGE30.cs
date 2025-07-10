namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE30")]
	public partial class AGE30 : MessageBlock
	{
		public AGE30()
			: base("GE30")
		{
		}

		/// <summary>
		/// Line 1 address corresponding to the entity provided.
		/// </summary>
		[MessageBlockString(76, 5, "M")]
		public ZString EntityAddressLine1;
	}
}
