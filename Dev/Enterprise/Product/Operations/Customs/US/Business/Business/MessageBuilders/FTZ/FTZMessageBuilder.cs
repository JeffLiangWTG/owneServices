using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FTZMessageBuilder
	{
		public FTZMessageBuilder(IFTZHeader ftzHeader, UpdateActionCode actionCode)
		{
			this.ftzHeader = ftzHeader;
			this.actionCode = actionCode;
		}
		readonly IFTZHeader ftzHeader;
		readonly UpdateActionCode actionCode;

		public MQEDIMessage PopulateMessage()
		{
			var block = new ACEInputBlockControlGenerator(ftzHeader);
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			block.AddMessageBlocks(new FTZMessageBlockBuilder(ftzHeader).Build(actionCode));

			return block.CreateMessage<MQEDIMessage>(((IMessageAttachee)ftzHeader).Factory);
		}
	}
}
