using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	abstract class AirBookingCommand : CustomCommand
	{
		public override bool Invoke()
		{
			var result = OnInvokeCommand();
			Notify(new DocumentHardRefreshEvent(Document));
			return result;
		}

		protected abstract bool OnInvokeCommand();

		protected IDocument Document => documentInfo?.Document;

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

		protected bool CheckHasNoChanges(out string errorMessage)
		{
			if (documentInfo?.Document?.Data?.HasChanges == null)
			{
				errorMessage = Res.GetString("d29b488d-7f70-43ad-a05e-f4bcfdb36e18", "Unable to process your request");
				return false;
			}
			else if (documentInfo.Document.Data.HasChanges)
			{
				errorMessage = Res.GetString("90b3e284-b767-5c92-4b35-57b8d43e6c8b", "Please save changes before sending message.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckHasNoErrors(out string errorMessage)
		{
			if (documentInfo?.Document?.HasErrors() ?? true)
			{
				errorMessage = Res.GetString("b16a38aa-1687-466c-a4fe-6b05cc09f926", "This document contains errors. Please fix all errors before sending.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckHasNoMessageErrors(out string errorMessage)
		{
			if (documentInfo.Document.HasMessageErrors())
			{
				errorMessage = Res.GetString("1165352d-cefe-4128-8abc-3d933b9790b0", "This document contains message errors. Please fix all message errors before sending.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckCarrierIsSupported(string mawb, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (string.IsNullOrWhiteSpace(mawb))
			{
				errorMessage = Res.GetString("55c1948f-dc26-466c-a897-5d0e92e3028c", "The MAWB number is not valid.");
				return false;
			}

			var airlinePrefix = string.Concat(mawb.Take(3));

			if (!AirBookingCarrierConfigurationManager.IsSupported(airlinePrefix, out errorMessage)
				&& string.IsNullOrWhiteSpace(errorMessage))
			{
				var supportedAirlines = AirBookingCarrierConfigurationManager
					.SupportedAirlines
					.Values
					.Select(info => $"{info.Prefix} - {info.Name}") // programmatic constant
					.ToArray();

				if (supportedAirlines.Length == 0)
				{
					errorMessage = Res.GetString("f5948e01-e6c3-4f42-be99-44f82053c7e1", "The airline is not supported.");
				}
				else
				{
					errorMessage = Res.GetString("cd594908-d930-4340-b795-1ae1d28a3d1e", "The airline is not supported. Currently supported airlines are:\r\n{0}.", string.Join(", ", supportedAirlines));
				}

				return false;
			}

			return true;
		}

		protected BusinessObjectFactory GetFactory()
		{
			if (documentInfo.DocumentData is IBusiness bizObj)
			{
				return bizObj.Factory;
			}

			return new BusinessObjectFactory();
		}

		protected AirBookingRequest GetAirBookingRequest()
		{
			if (documentInfo?.Document?.Data?.Value is AirBookingRequest airBookingRequest)
			{
				return airBookingRequest;
			}

			return null;
		}

		protected void ShowGenericErrorMessage() => ShowMessage(Res.GetString("1319de2a-0e35-4833-8599-0776d39c1ce8", "An error has occurred while processing the request."));
		protected void ShowDetailedErrorMessage(string errorMessage) => ShowMessage(Res.GetString("ce4d544b-668e-4fd5-9acc-fcfb2a66b803", "The following error has occurred while processing the request:\r\n{0}", errorMessage));

		protected void ShowMessage(string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				var notificationService = documentInfo?.Services?.Resolve<IUserNotificationService>();
				var caption = documentInfo?.Document?.Name;
				notificationService?.ShowMessage(message, caption);
			}
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

		protected string QueryUserResponse(string message, string caption)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				var notificationService = documentInfo?.Services?.Resolve<IUserNotificationService>();

				return notificationService?.QueryUserResponse(
					message,
					caption ?? Document?.Name,
					2,
					100);
			}

			return string.Empty;
		}

		protected void Notify<T>(T obj) where T : class
		{
			if (obj != null)
			{
				var broker = documentInfo?.Services?.Resolve<IEventBroker>();
				broker?.Publish(obj);
			}
		}

		public static ResponseProcessResult ProcessRequestAndResponse(AirBookingRequestProcessor requestProcessor, BusinessObjectFactory factory)
		{
			Argument.NotNull(requestProcessor, nameof(requestProcessor));
			Argument.NotNull(factory, nameof(factory));

			var content = requestProcessor.Process();
			return new AirBookingResponseProcessor(factory).Process(content, requestProcessor);
		}

		protected void SaveDocumentToEDocs(AirBookingRequest airBookingRequest, string documentName)
		{
			var descriptor = documentInfo.Descriptor;

			if (airBookingRequest == null
				|| descriptor == null)
			{
				return;
			}

			if (airBookingRequest.HasTermsAndConditions()
				|| (descriptor.EDocsInstructions?.SaveCopyToEDocs ?? false))
			{
				var document = documentInfo.Document;
				var eDocsDeliveryParameters = new EDocsDeliveryParameters
				{
					BusinessObject = descriptor.EDocsInstructions?.Parent as IBusiness,
					DocumentName = documentName,
					DocumentTitle = documentName,
					DocumentType = descriptor.DocumentType,
					AttachedFileName = documentName
				};
				document.AddCopyToEDocs(eDocsDeliveryParameters);
			}
		}
	}
}
