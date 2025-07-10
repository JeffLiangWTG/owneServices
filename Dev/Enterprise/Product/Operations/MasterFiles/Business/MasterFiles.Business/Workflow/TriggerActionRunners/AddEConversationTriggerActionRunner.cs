using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	sealed class AddEConversationTriggerActionRunner : IProcessor
	{
		internal AddEConversationTriggerActionRunner(ProcessTaskNotification action, BusinessObject parentJob, Lazy<IStmALog> eventProvider, bool isMessageInternalOnly)
		{
			this.parentJob = parentJob;
			this.action = action;
			this.isMessageInternalOnly = isMessageInternalOnly;
			this.eventProvider = eventProvider;
		}

		readonly ProcessTaskNotification action;
		readonly BusinessObject parentJob;
		readonly bool isMessageInternalOnly;
		readonly Lazy<IStmALog> eventProvider;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logs are not translated.")]
		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (parentJob == null)
			{
				notifications.AddError((NoResString)"The parent entity to which this trigger belongs cannot be found - it may have been deleted");
				return;
			}

			var conversationProvider = parentJob as IConversationProvider;

			if (conversationProvider == null)
			{
				notifications.AddError((NoResString)"The parent entity to which this trigger belongs does not support eConversation");
				return;
			}

			var message = TriggerActionCommunicationModeSubstitutor.Substitute(action, parentJob, eventProvider.Value, MessageDelivery.CommunicationModeSubstitutorProperty.EmailText, action.PQ_EmailTextFallbackToTemplate);

			if (string.IsNullOrWhiteSpace(message))
			{
				notifications.AddWarning("The eConversation message to be sent was empty. This is not supported.");
			}
			else
			{
				conversationProvider.eConversation.AddMessageFromCurrentUser(message, isInternal: isMessageInternalOnly, isSystem: true);
			}
		}
	}
}
