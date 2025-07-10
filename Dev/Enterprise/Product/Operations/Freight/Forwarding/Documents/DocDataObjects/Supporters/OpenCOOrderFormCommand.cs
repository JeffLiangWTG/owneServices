using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters
{
	class OpenCOOrderFormCommand : CustomCommand
	{
		public override string Id => CommandIds.SendMessage;
		public override string Caption => Res.GetString("3b1b2753-db2a-44a3-871e-57b1fa50ee97", "Open CO Order Form");
		public override object Image => Properties.Resources.open_co_order_form;
		public override bool IsEnabled => true;
		public override bool IsVisible => true;

		public override bool Invoke()
		{
			if (documentInfo?.Document?.Data?.Value is DocDataObject docDataObject)
			{
				docDataObject.ValidateAllIncludingChildren();
				if (docDataObject.HasMessageErrors)
				{
					ShowMessage($@"{Res.GetString("1165352d-cefe-4128-8abc-3d933b9790b0", "This document contains message errors. Please fix all message errors before sending.")}

{GetErrorString(docDataObject.GetMessageErrors().GetUniqueNotifications())}");
					return false;
				}

				if (docDataObject.HasErrors)
				{
					ShowMessage($@"{Res.GetString("b16a38aa-1687-466c-a4fe-6b05cc09f926", "This document contains errors. Please fix all errors before sending.")}

{GetErrorString(docDataObject.GetErrors().GetUniqueNotifications())}");
					return false;
				}

				SaveUnSavedChanges(documentInfo.Document, documentInfo.DocumentData);

				if (documentInfo.DocumentData is IStmALogParent logParent &&
					docDataObject is ISupportingDocDataObject supportingDocDataObject)
				{
					var extensions = new CertificateOfOriginMessagingExtensions(logParent, documentInfo.Document.Name);
					var notifications = documentInfo.Services.Resolve<IUserNotificationService>();
					if (extensions.ContinueWithSendingMessage(notifications) == true && ShowSupportingDocPopupForm(supportingDocDataObject))
					{
						return SendMessageCommand();
					}
				}
			}

			return false;
		}

		string GetErrorString(IEnumerable<INotification> notifications)
		{
			var stringBuilder = new StringBuilder();

			foreach (var notification in notifications)
			{
				var fistIndexOfSeparator = notification.Message.IndexOf(':');
				if (fistIndexOfSeparator != -1)
				{
					stringBuilder.AppendLine(notification.Message.Substring(fistIndexOfSeparator + 1));
				}
			}

			return stringBuilder.ToString().TrimEnd('\r', '\n');
		}

		protected void ShowMessage(string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				var notificationService = documentInfo?.Services?.Resolve<IUserNotificationService>();
				var caption = documentInfo?.Document?.Name;
				notificationService?.ShowMessage(message, caption);
			}
		}

		public virtual bool ShowSupportingDocPopupForm(ISupportingDocDataObject supportingDocDataObject)
		{
			using (var form = ObjectFactory.Get<ISupportingDocForm>(nameof(ISupportingDocForm), supportingDocDataObject))
			{
				var result = form.ShowDialogAndGetResult();
				if (result == ZDialogResult.OK)
				{
					return true;
				}
			}

			return false;
		}

		public virtual bool SendMessageCommand()
		{
			var sendMessageCommand = new SendMessageCommand();
			return sendMessageCommand.Invoke(null, documentInfo);
		}

		protected void SaveUnSavedChanges(DocumentVisualizer.Core.IDocument document, IVisualizerDocumentData documentData)
		{
			if ((document?.Data?.HasChanges ?? false)
				|| (documentData?.HasChanges ?? false))
			{
				DocumentDataSave(documentInfo?.Services, documentInfo?.Document, documentInfo?.DocumentData);
			}
		}

		protected virtual void DocumentDataSave(IServiceContainer services, DocumentVisualizer.Core.IDocument document, IVisualizerDocumentData documentData)
		{
			new DocumentDataSaver(services, document, documentData).Save();
		}
	}
}
