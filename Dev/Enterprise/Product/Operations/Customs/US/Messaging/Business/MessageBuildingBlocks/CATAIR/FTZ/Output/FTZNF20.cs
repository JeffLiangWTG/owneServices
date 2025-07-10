namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("20")]
	public partial class FTZNF20 : MessageBlock
	{
		public FTZNF20()
			: base("20")
		{
		}

		[MessageBlockString(78, 3, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}
}
