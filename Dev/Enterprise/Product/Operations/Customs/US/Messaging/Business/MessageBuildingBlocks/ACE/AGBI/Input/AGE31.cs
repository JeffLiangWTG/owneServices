namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE31")]
	public partial class AGE31 : MessageBlock
	{
		public AGE31()
			: base("GE31")
		{
		}

		/// <summary>
		/// Line 2 address corresponding to the entity provided.
		/// </summary>
		[MessageBlockString(76, 5, "M")]
		public ZString EntityAddressLine2;
	}
}
