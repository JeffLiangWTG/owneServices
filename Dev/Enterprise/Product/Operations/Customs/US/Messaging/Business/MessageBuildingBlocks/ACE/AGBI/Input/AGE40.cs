namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE40")]
	public partial class AGE40 : MessageBlock
	{
		public AGE40()
			: base("GE40")
		{
		}

		/// <summary>
		/// Phone number corresponding to the entity provided.
		/// </summary>
		[MessageBlockString(22, 5, "M")]
		public ZString PhoneNumber;
	}
}
