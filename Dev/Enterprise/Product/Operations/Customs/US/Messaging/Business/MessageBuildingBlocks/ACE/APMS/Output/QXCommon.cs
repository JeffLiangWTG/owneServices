namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("QX")]
	public partial class QXCommon : MessageBlock
	{
		public QXCommon()
			: base("QX")
		{
		}

		[MessageBlockString(78, 3, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}
}
