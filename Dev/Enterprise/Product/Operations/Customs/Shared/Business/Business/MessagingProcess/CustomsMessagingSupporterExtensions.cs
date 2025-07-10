using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public static class CustomsMessagingSupporterExtensions
	{
		public static IReadOnlyCollection<MessageSendingNotification> RunPreSendValidation(this ICustomsMessagingSupporter supporter, ActionResult previousResult)
		{
			var notifications = new List<MessageSendingNotification>();
			var provider = supporter.Provider;

			if (!(supporter.Messengers?.Any() ?? false))
			{
				notifications.Add(new MessageSendingError(Res.GetString("MessagingProcess|Supporter|NoMessengersError", "There are no messengers to send messages with")));
			}

			var objectsToValidate = new List<BusinessObject> { supporter.TopLevelBusinessObject };
			objectsToValidate.AddRange(provider.GetAdditionalValidationBusinessObjects());

			notifications.AddRange(GetBusinessObjectValidationMessageErrors(objectsToValidate));
			notifications.AddRange(provider.RunPreSendValidation(previousResult));

			return notifications;
		}

		static IReadOnlyCollection<MessageSendingNotification> GetBusinessObjectValidationMessageErrors(IReadOnlyCollection<BusinessObject> toplevelBusinessObjects)
		{
			var result = new List<MessageSendingNotification>();

			var notifications = new List<CargoWise.ComponentModel.INotification>();

			foreach (IBusiness bo in toplevelBusinessObjects)
			{
				bo.MarkAsNeedingValidationIncludingChildren();
				bo.RunPreSaveValidationFetch(true);
				bo.RunPreSaveValidation();
				notifications.AddRange(new CustomsNotificationCollector(bo, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());
			}

			if (notifications.Any())
			{
				var message = $"{MessageSendingValidation.MessageErrorsExistHeaderText}\n{string.Join("\n", notifications.Select(x => x.Message).Distinct())}";
				result.Add(new MessageSendingWarning(message));
			}

			return result;
		}

		public static ISendMessagesBusinessActionProvider GetSendMessagesBusinessActionProvider(this ICustomsMessagingSupporter supporter) => new SendMessagesBusinessActionProvider(supporter);
		public static ActionResult SendMessages(this ICustomsMessagingSupporter supporter, ISendMessagesGuiActionProvider guiActionProvider = null) => SendMessagesProcess.SendMessages(GetSendMessagesBusinessActionProvider(supporter), guiActionProvider);
	}
}
