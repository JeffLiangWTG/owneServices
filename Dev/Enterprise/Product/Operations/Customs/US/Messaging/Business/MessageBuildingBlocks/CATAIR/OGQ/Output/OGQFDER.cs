namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("FDER")]
	public partial class OGQFDER : MessageBlock
	{
		public OGQFDER()
			: base("FDER")
		{
		}

		/// <summary>
		/// A code identifying the error message.
		/// </summary>
		[MessageBlockString(3, 18, "M")]
		public ZString ErrorMessageIdentifier;

		/// <summary>
		/// A narrative description of the error.
		/// </summary>
		[MessageBlockString(40, 21, "M")]
		public ZString NarrativeMessage;
	}
}