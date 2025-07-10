using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ImportManifestMessagingExtensions : BaseMessagingExtensions
	{
		public ImportManifestMessagingExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			importManifest = document?.Data.Value as ImportManifest;
			Argument.NotNull(importManifest, nameof(importManifest));

			Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(logParent, messageInstructions.DocumentName, ConsolDocumentDataStoreNames.ProvisionalUnpackingListLPD);

			this.logParent = Argument.NotNull(logParent, nameof(logParent));
		}

		readonly ImportManifest importManifest;
		readonly FrenchMessageContext messageContext;
		readonly IStmALogParent logParent;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (!IsContainerExistRSLEvent(notifications))
			{
				return false;
			}

			return null;
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			if (!IsContainerExistRSLEvent(notifications))
			{
				return false;
			}

			return messageContext.ContinueWithSendingMessageAmendment(notifications);
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (!IsContainerExistRSLEvent(notifications))
			{
				return false;
			}

			return messageContext.ContinueWithSendingMessageWithdrawal(notifications);
		}

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => messageContext.ContinueWithResetToOriginal(notifications);

		#region Implementation

		bool IsContainerExistRSLEvent(IUserNotifications notifications)
		{
			if (logParent is ForwardingConsol consolBo)
			{
				var containers = importManifest.Containers;
				foreach (var container in containers)
				{
					var containerBo = consolBo.Containers.FirstOrDefault(x => x.PK == (ZGuid)container.Identifier);

					var logs = (containerBo as ForwardingContainer)?.Logs.GetAllLogs().OfType<StmALog>();

					if (logs == null || !logs.Any(x => !x.IsCancelled && x.SL_SE_NKEvent == Events.ReleasedCode && x.Parameters.Contains(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, (NoResString)"Carrier")))) // It is a part of a EventReference which is not localizable
					{
						var message = string.Format((NoResString)"The container({0}) must have received both RLS(Released / BAD - Bon à Délivrer) and FUL(Estimated Unload/ APD - Annonce Prévisionnelle de Déchargement) notifications from the Shipping Line / port system in order to send this message.\n\r\n\r" +
(NoResString)"Both notifications were not received from the Shipping Line / port system.Do you want to continue sending this message?", container.Number);  // Message
						return notifications?.ShowConfirmation(message, Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("26519505-9f04-4d57-a430-774e3018e75e", "Confirmation")) ?? false;
					}
				}
			}

			return true;
		}

		#endregion
	}
}
