namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE41")]
	public partial class AGE41 : MessageBlock
	{
		public AGE41()
			: base("GE41")
		{
		}

		/// <summary>
		/// Website corresponding to the entity provided.
		/// </summary>
		[MessageBlockString(76, 5, "M")]
		public ZString Website;
	}
}
