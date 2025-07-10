namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("FD30")]
	public partial class OGQFDPT30 : MessageBlock
	{
		public OGQFDPT30() : base("FD30")
		{
		}

		/// <summary>
		/// Bodgy hack to get around badly specified message segment
		/// Apply local knowledge of the response to pull apart
		/// </summary>
		[MessageBlockString(76, 5, "M")]
		public ZString Message;
	}
}
