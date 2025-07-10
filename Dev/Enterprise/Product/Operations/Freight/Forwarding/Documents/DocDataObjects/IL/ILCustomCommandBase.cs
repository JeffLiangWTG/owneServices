using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	abstract class ILCustomCommandBase : CustomCommand
	{
		protected ILCustomCommandBase(IILElectronicMessageProvider messageProvider)
		{
			this.messageProvider = Argument.NotNull(messageProvider, nameof(messageProvider));
		}

		protected void ShowGenericErrorMessage() => ShowMessage(Res.GetString("8CF4395B-1E04-45DC-B3CF-6E8D890DCD33", "An error has occurred while processing the request."));

		protected void ShowMessage(string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				var notificationService = documentInfo?.Services?.Resolve<IUserNotificationService>();
				var caption = documentInfo?.Document?.Name;
				notificationService?.ShowMessage(message, caption);
			}
		}

		protected void Notify<T>(T obj) where T : class
		{
			if (obj != null)
			{
				var broker = documentInfo?.Services?.Resolve<IEventBroker>();
				broker?.Publish(obj);
			}
		}

		protected bool CheckHasNoErrors(out string errorMessage)
		{
			if (documentInfo?.Document?.HasErrors() ?? true)
			{
				errorMessage = Res.GetString("2AD4413C-14CC-4501-A279-F2789F80D6B5", "This document contains errors. Please fix all errors before sending.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckAllowSendMessage()
		{
			var security = documentInfo?.Services?.Resolve<IDocumentSecurityService>();

			if (!security?.CanSendMessage ?? false)
			{
				security.ShowSendMessageError();
				return false;
			}

			return true;
		}

		protected bool ShowConfirmation(string message, string confirmation)
		{
			if (!string.IsNullOrWhiteSpace(confirmation))
			{
				var caption = Res.GetString("e8bf1c1b-8301-47a0-8bed-d0a97b0d2aa0", "Warning");
				var prompt = Res.GetString("e57269a6-82d3-4add-96f5-7d334afc56f4", "If you have done so, please type the following to confirm:");

				var notificationService = documentInfo?.Services?.Resolve<IUserNotificationService>();

				var confirmed = notificationService?.ShowConfirmation(message, caption, prompt, confirmation) ?? false;

				return confirmed;
			}

			return true;
		}

		protected abstract string DocumentName { get; }

		protected abstract ZString MessageReference { get; }

		protected readonly IILElectronicMessageProvider messageProvider;
	}
}
