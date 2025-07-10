using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public interface IPRAContainerMessaging : IBusiness
	{
		ZString CurrentPRAStatus { get; }
		ZPropertyInfo CurrentPRAStatusInfo { get; }
		PRAMessageCollection PRAMessages { get; }
	}

	public static class IPRAMessagingExtender
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a some code abbreviature.")]
		public static ZString GetCurrentPRAStatus(this IPRAContainerMessaging container)
		{
			EDIMessage lastPRAMessageSent = GetLastPRAMessageSent(container);

			string result;

			if (lastPRAMessageSent == null)
			{
				result = Res.GetString("454bc544-e807-41a9-ae60-fb639b96f25f", "No PRA Messages Have Been Sent.");
			}
			else
			{
				if (lastPRAMessageSent.EM_MessageDescriptionFromSubType == "Sent")
				{
					result = Res.GetString("d73fab55-8864-4e45-9f49-dcfe2ec8cdd8", "PRA Submit Message Sent") + " ";
				}
				else
				{
					result = Res.GetString("36e8d698-ef16-4fc6-b7ce-e2625dd3d84b", "PRA Cancellation Message Sent") + " ";
				}

				EDIMessage lastPRAMessage = GetLastPRAMessage(container);

				if (lastPRAMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
				{
					result += Res.GetString("0be04ecd-9fd3-45b4-9889-c5f6c1e62713", "but not responded to yet.");
				}
				else if (lastPRAMessage.EM_MessageDescriptionFromSubType == "Rejected")
				{
					result += Res.GetString("ff220bf1-a76c-45a0-8db5-b73ae8b8edc3", "and was Rejected.");
				}
				else
				{
					result += Res.GetString("d5e43c49-a972-4250-869d-28d3c464c69f", "and was Accepted.");
				}
			}

			return result;
		}

		public static bool GetIsWaitingForPRAResponse(this IPRAContainerMessaging container)
		{
			PRAMessage lastPRAMessage = GetLastPRAMessage(container);
			return lastPRAMessage != null && lastPRAMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a some code abbreviature.")]
		public static bool GetLastPRAMessageSentWasCancellation(this IPRAContainerMessaging container)
		{
			PRAMessage msg = GetLastPRAMessageSent(container);
			return msg != null && msg.EM_MessageDescriptionFromSubType != "Sent";
		}

		public static PRAMessage GetLastPRAMessage(this IPRAContainerMessaging container)
		{
			return container.GetPRAMessages().GetLastMessage(EDIMessage.ApplicationCodes.OneStop, PRAMessage.MessageType);
		}

		public static PRAMessage GetLastPRAMessageSent(this IPRAContainerMessaging container)
		{
			return container.GetPRAMessages().GetLastMessage(EDIMessage.ApplicationCodes.OneStop, PRAMessage.MessageType, EDIMessage.Direction.Transmit);
		}

		public static PRAMessageCollection GetPRAMessages(this IPRAContainerMessaging container)
		{
			PRAMessageCollection collection = new PRAMessageCollection((BusinessObject)container);
			collection.Load();
			collection.Sort(EDIMessageSchema.EM_SystemCreateTimeUtc.Name, ListSortDirection.Ascending);
			return collection;
		}
	}
}
