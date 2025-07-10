namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("12")]
	public partial class FTZNF12 : MessageBlock
	{
		public FTZNF12()
			: base("12")
		{
		}

		[MessageBlockString(78, 3, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}
}
