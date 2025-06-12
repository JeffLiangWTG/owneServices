using Microsoft.BizTalk.Message.Interop;
using System;
using System.Text;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[SupportedServiceProvider(ServiceProvider.ARINC)]
    class ARINCEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new ARINCPromotedValue();
			InternalMessage = RemoveSpecialCharacters(promotedValue.Find(message, "InternalMessage"));
            var messagePriority = "";
            var priority_SenderAddress = RemoveSpecialCharacters(promotedValue.Find(message, "Priority-SenderAddress"));
            if (priority_SenderAddress.Contains(" "))
            {
                var priority_SenderArray = priority_SenderAddress.Split(' ');
                messagePriority = priority_SenderArray[0];
                SenderPIMA = priority_SenderArray[1];
            }
            else
            {
                SenderPIMA = priority_SenderAddress;
            }
            SenderID = partyResolver.ResolveParty(SenderPIMA);
			RecipientPIMA = promotedValue.Find(message, "RecipientPIMA");
            ClientAWB = ConextHelper.BuildAWBFromMessage(InternalMessage);
            RecipientID = partyResolver.ResolveClientAWB(ClientAWB);
			RecipientID = !String.IsNullOrEmpty(RecipientID) ? RecipientID : partyResolver.ResolveClientPIMA(RecipientPIMA);
			ExtractMessageTypeAndVersion();
            
            var signatureDate = promotedValue.Find(message, "Reference");
            var recipientAddress = promotedValue.Find(message, "RecipientAddress");

            if (MessageType == "FFA" || OriginalMessageType == "FFR")
            {
                MessageType = "FFAFMAFNA";
                InternalMessage = RebuildOriginalMessage(messagePriority, recipientAddress, signatureDate);
            }
       
            if (MessageType == "FSA" || OriginalMessageType == "FSR")
            {
                MessageType = "FSA";
                InternalMessage = RebuildOriginalMessage(messagePriority, recipientAddress, signatureDate);
            }
            
            Reference = "";
            if(!String.IsNullOrEmpty(signatureDate))
            {
                var parts = signatureDate.Split('/');
                if(parts.Length > 1) Reference = parts[0];
            }

            InternalMessage = RemoveExtraHeaderFromInternalMessage(InternalMessage);
        }

        string RebuildOriginalMessage(string messagePriority, string recipientAddress, string signatureDate)
        {
            var outputMessage = new StringBuilder();
            if (string.IsNullOrEmpty(messagePriority))
            {
                outputMessage.AppendLine(String.Format("{0}", SenderPIMA));
            }
            else
            {
                outputMessage.AppendLine(String.Format("{0} {1}", messagePriority, SenderPIMA));
            }
            outputMessage.AppendLine(String.Format(".{0} {1} {2}", recipientAddress, signatureDate, RecipientPIMA));
            outputMessage.AppendLine(InternalMessage);
            return outputMessage.ToString();

        }

        string RemoveSpecialCharacters(string message)
        {
            return !String.IsNullOrEmpty(message) ? message.Replace("\x01", "").Replace("\x02", "").Replace("\x03", "") : message;
        }
	}
}
