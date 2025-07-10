using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class DrawbackSummaryMessageManager
	{
		public DrawbackSummaryMessageManager(IDrawbackSummary drawbackSummary)
		{
			this.drawbackSummary = drawbackSummary;
		}
		readonly IDrawbackSummary drawbackSummary;

		public void PopulateMessage(UpdateActionCode actionCode)
		{
			DrawbackSummaryMessageBuilder builder = new DrawbackSummaryMessageBuilder(drawbackSummary);
			MQEDIMessage message = builder.CreateMessage();
			drawbackSummary.AddMessage(message);
			CalculateStatus(actionCode, message);
		}

		void CalculateStatus(UpdateActionCode actionCode, MQEDIMessage message)
		{
			if (actionCode == UpdateActionCode.Add)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal;
			}
			else if (actionCode == UpdateActionCode.Delete)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryDelete;
			}
			message.UpdateErrorFlag();
			drawbackSummary.MessageStatus = new DrawbackSummaryMessageStatusCalculator().Calculate(message, ABIResponseStatus.Undefined);
		}

		public bool CanSendThisMessage(UpdateActionCode actionCode, out string messageText)
		{
			bool result;
			messageText = "";
			if (drawbackSummary.HasChanges)
			{
				result = false;
				messageText = "Drawback not yet saved, Please save before sending.";
			}
			else if (!drawbackSummary.IsABIFiled)
			{
				result = false;
				messageText = NotFiledAsABI;
			}
			else if (actionCode == UpdateActionCode.Add)
			{
				result = drawbackSummary.EntryStatus != DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
				if (!result)
				{
					messageText = "the Drawback Summary has already been added.";
				}
			}
			else
			{
				result = drawbackSummary.EntryStatus == DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
				if (!result)
				{
					messageText = "the Drawback Summary has not been added yet.";
				}
			}
			return result;
		}
		internal const string NotFiledAsABI = "Drawback is not filed as ABI (Auto).";

		public bool IsWaitingForResponse
		{
			get
			{
				return drawbackSummary.MessageStatus == DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryDelete ||
				drawbackSummary.MessageStatus == DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal;
			}
		}
	}
}
