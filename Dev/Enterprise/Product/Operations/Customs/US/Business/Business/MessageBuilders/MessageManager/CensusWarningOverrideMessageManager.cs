using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class CensusWarningOverrideMessageManager
	{
		public MessageSendingNotificationCollection GetNotificationsForSendingCWO(CusEntryHeader entry)
		{
			var result = GetNotificationsForSavingCWO(entry);

			if (!entry.HasBeenLodgedAtCustoms)
			{
				result.AddError(NotAcceptedYet);
			}

			return result;
		}

		internal const string NotAcceptedYet = "The Census Warning Override message cannot be sent until a 7501 has been accepted by customs.";

		public MessageSendingNotificationCollection GetNotificationsForSavingCWO(CusEntryHeader entry)
		{
			var result = new MessageSendingNotificationCollection();

			if (entry.IsWaitingForResponse)
			{
				result.AddError(WaitingForResponse);
			}

			return result;
		}

		internal const string WaitingForResponse = "Changes should not be made to the Census Warning Overrides while a response from customs is pending.";

		public void SendMessage(CusEntryHeader entry)
		{
			var builder = new CensusWarningOverrrideMessageBuilder(entry);
			builder.PopulateMessage();

			entry.US_CWOStatus = ImportMessageStatusList.Codes.AwaitingCensusWarningOverride;
		}
	}
}
