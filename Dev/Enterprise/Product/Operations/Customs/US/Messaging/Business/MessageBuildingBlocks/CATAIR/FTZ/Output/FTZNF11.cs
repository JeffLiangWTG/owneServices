namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("11")]
	public partial class FTZNF11 : MessageBlock
	{
		public FTZNF11()
			: base("11")
		{
		}

		[MessageBlockString(78, 3, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}
}
