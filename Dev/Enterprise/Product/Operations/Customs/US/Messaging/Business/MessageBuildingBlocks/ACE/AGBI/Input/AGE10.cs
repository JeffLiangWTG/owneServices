namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE10")]
	public partial class AGE10 : MessageBlock
	{
		public AGE10()
			: base("GE10")
		{
		}

		/// <summary>
		/// A code representing the type of transaction.
		/// </summary>
		[MessageBlockString(1, 5, "M")]
		public ZString ActionCode;

		/// <summary>
		/// The name of the entity represented by the identifier submitted.
		/// </summary>
		[MessageBlockString(75, 6, "M")]
		public ZString EntityName;
	}
}
