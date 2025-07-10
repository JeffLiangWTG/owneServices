using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class DemandeDeTracingMessageProcessor : IProcessor
	{
		public DemandeDeTracingMessageProcessor(ForwardingContainer container, DemandeDeTracingDirection direction)
		{
			this.container = container;
			this.direction = direction;
		}

		readonly ForwardingContainer container;
		readonly DemandeDeTracingDirection direction;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var consol = container?.Consol;

			if (consol == null)
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, FormattableString.Invariant($"Tracing Request (TRC) message for {container?.HumanReadableName} cannot be send because it is not attached to a consolidation.")); // workflow processor message
				return;
			}

			if (consol.JK_UniqueConsignRef.IsEmpty)
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, FormattableString.Invariant($"Tracing Request (TRC) message for {container?.HumanReadableName} cannot be send without a valid consolidation number.")); // workflow processor message
				return;
			}

			if (!DemandeDeTracingHelper.IsTracingRequestApplicable(consol, direction))
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, FormattableString.Invariant($"{direction} Tracing Request (TRC) message for {container.HumanReadableName} ({consol.HumanReadableName}) has not been sent because it is not applicable for consol load/discharge port.")); // workflow processor message
				return;
			}

			if (DemandeDeTracingMessageSender.SendMessage(new ForwardingConsolTRCDetailsProvider(consol), container, direction, out var senderNotifications))
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, FormattableString.Invariant($"{direction} Tracing Request (TRC) message for {container.HumanReadableName} ({consol.HumanReadableName}) has been sent.")); // workflow processor message
			}
			else
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, FormattableString.Invariant($"{direction} Tracing Request (TRC) message for {container.HumanReadableName} ({consol.HumanReadableName}) has NOT been sent.")); // workflow processor message
			}

			if (senderNotifications != null)
			{
				foreach (var notification in senderNotifications)
				{
					notifications.Add(notification);
				}
			}
		}
	}
}
