using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class PTTMessageManager
	{
		public PTTMessageManager(IFZEventHeader header, PTTSendingOption sendingOption)
		{
			this.header = header;
			this.sendingOption = sendingOption;
		}
		readonly IFZEventHeader header;
		readonly PTTSendingOption sendingOption;

		public bool CanSendThisMessage()
		{
			var result = true;
			if (!header.DirectDeliveryIndicator)
			{
				result = header.HasBeenLodgedAtCustoms;
			}
			return result;
		}

		public void PopulateMessage()
		{
			var builder = new PTTMessageBuilder(header, sendingOption);
			var message = builder.PopulateMessage();
			header.AddMessage(message);

			var messageSubType = GetPTTMessageSubType(sendingOption);
			message.EM_MessageSubType = messageSubType;
			header.SetMessageStatus(messageSubType, new FTZMessageStatusCalculator().Calculate(message, false, false));
		}

		public ZString GetPTTMessageSubType(PTTSendingOption option)
		{
			var result = ZString.Empty;
			switch (option)
			{
				case PTTSendingOption.SendPTTMessage:
					result = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
					break;
				case PTTSendingOption.CancellPTTMessage:
					result = EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer;
					break;
				case PTTSendingOption.SendPTTArrival:
					result = EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival;
					break;
				case PTTSendingOption.SendPTTUnArrival:
					result = EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival;
					break;
				default:
					break;
			}
			return result;
		}
	}
}
