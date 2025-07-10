using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public class UnknownMessageBlock : MessageBlock
	{
		public UnknownMessageBlock()
			: base("")
		{
		}

		[MessageBlockString(80, 1, "C")]
		public ZString Data;
	}
}