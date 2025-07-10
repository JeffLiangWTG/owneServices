using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZDeliveryMessageBuilder : FZMessageBuilder
	{
		public FZDeliveryMessageBuilder(FZEventAction action)
			: base(action.EventHeader)
		{
			this.action = action;
		}
		readonly FZEventAction action;

		protected override IEnumerable<MessageBlock> Build()
		{
			yield return GenerateFZ10WithMandatoryFields(1, Header.FTZAdmissionNumber, action.US_ActionCode);
		}
	}
}
