namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("10")]
	public partial class FTZNF10 : MessageBlock
	{
		public FTZNF10()
			: base("10")
		{
		}

		[MessageBlockString(78, 3, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}

	[OutputBlock("10", "01")]
	public partial class FTZNF10_01 : MessageBlock
	{
		public FTZNF10_01()
			: base("10")
		{
		}

		[MessageBlockString(78, 3, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}
}
