using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public abstract class DocDataObjectMessageSender : IDocDataObjectMessageSender
	{
		public bool SendMessage(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!IsValidBeforeSending(bizObj, menuItem, notifications) || !IsValidForSendingMessage(bizObj, menuItem, notifications))
			{
				return false;
			}

			var builder = new DocumentInfoBuilder(bizObj, menuItem);
			var documentInfo = builder.CreateDocumentInfos().FirstOrDefault();
			var messageInstructions = documentInfo?.Descriptor?.MessageInstructions;
			var documentData = documentInfo?.DocumentData;

			if (documentInfo?.Document?.Data?.Value is DocDataObject docDataObject)
			{
				var documentOverrideMerger = new DocumentOverrideMerger(documentInfo.Services, documentInfo.DocumentData);
				var applyResult = documentOverrideMerger.ApplyOverride(documentInfo?.Document);

				if (applyResult.IsLeft)
				{
					notifications.AddMessageError(applyResult.Left);
					return false;
				}

				docDataObject.ValidateAllIncludingChildren();
				notifications.AddRange(docDataObject.NotificationsIncludingChildren);

				if (docDataObject.HasMessageErrors || docDataObject.HasErrors)
				{
					return false;
				}

				if (!AllowSendMessageAmendment && IsSendingAmendment(bizObj, documentInfo, documentData, messageInstructions))
				{
					notifications.AddMessageError((NoResString)"A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.");  // ErrorMessage
					return false;
				}

				return NoUIMessageSender.SendUXml(messageInstructions, documentInfo, notifications);
			}

			notifications.AddMessageError((NoResString)"There has been a problem creating document data.");  // ErrorMessage
			return false;
		}

		protected virtual bool IsValidForSendingMessage(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!bizObj.IsApplicable(menuItem.SU_FilterList) || menuItem.Documents?.Count == 0)
			{
				notifications.AddMessageError(MenuFilterDoesNotMatchMessageError);
				return false;
			}
			else
			{
				return true;
			}
		}

		protected abstract ZString MenuFilterDoesNotMatchMessageError { get; }

		protected abstract ZString MenuItemMessageError { get; }

		protected virtual bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (menuItem == null)
			{
				notifications.AddMessageError(MenuItemMessageError);
				return false;
			}
			else
			{
				return true;
			}
		}

		protected virtual bool AllowSendMessageAmendment => false;

		bool IsSendingAmendment(BusinessObject bizObj, IDocumentInfo documentinfo, IVisualizerDocumentData documentData, IMessageInstructions messageInstructions)
		{
			var mesageHasBeenSent = documentData?.CalculateDataVersion(messageInstructions?.DocumentName, false) > 1;

			var messageExtensions = bizObj
				?.GetSupporter()
				?.GetMessagingExtensions(documentinfo?.Document, messageInstructions);

			return messageExtensions?.IsSendingAmendment() ?? mesageHasBeenSent;
		}
	}
}
