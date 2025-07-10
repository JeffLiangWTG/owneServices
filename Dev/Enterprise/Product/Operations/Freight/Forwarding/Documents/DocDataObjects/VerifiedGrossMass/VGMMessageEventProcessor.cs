using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.ZArchitecture.Business.Event;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class VGMMessageEventProcessor : IMessageEventsProcessor
	{
		public VGMMessageEventProcessor(ForwardingConsol consol, IDocument document)
		{
			this.consol = consol;
			this.document = document;
		}

		readonly ForwardingConsol consol;
		readonly IDocument document;

		public void OnMessageSent()
		{
			CascadeVGMEventToContainers(Events.MessageSent, Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent);

			AddMSNEvent();
		}

		void AddMSNEvent()
		{
			if (document != null
				&& document.Data is IDynamicData data
				&& data.Value is VerifiedGrossMass vgm)
			{
				var carrier = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
				var vgmCarrier = consol.IsCoLoad ? vgm.FreightForwarder : vgm.Carrier;
				if ((!carrier?.ShippingLine?.RSL_VerifiedGrossContainerWeightAvailable ?? false) &&
					!vgmCarrier.Email.IsEmpty && !vgmCarrier.Contact.IsEmpty)
				{
					MSNEventProcessHelper.AddMSNEvent(consol, ConsolDocumentNames.VerifiedGrossContainerWeight);
				}
			}
		}

		public void OnMessageWithdrawalSent() => CascadeVGMEventToContainers(
			Events.MessageWithdrawCancelRequest,
			Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent);

		public void OnResetToOriginal() => CascadeVGMEventToContainers(
			Events.StatusUpdated,
			Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);

		void CascadeVGMEventToContainers(Event @event, ZString containerVerifiedStatusCode)
		{
			if (document != null
				&& document.Data is IDynamicData data
				&& data.Value is VerifiedGrossMass vgm)
			{
				var containers = vgm
					.Containers
					.ToArray();

				var sendContainerPKs = containers
					.Select(container => container.Identifier)
					.OfType<ZGuid>()
					.Where(pk => pk.IsValid)
					.ToArray();

				CascadeVGMEventToContainers(sendContainerPKs.ToArray(), @event);

				foreach (var container in containers)
				{
					container.VerifiedStatus.Code = containerVerifiedStatusCode;
				}

				if (data.GetDynamicProperty(nameof(VerifiedGrossMass.Containers)) is IDynamicDataCollection dynamicDataContainers)
				{
					AcceptContainerChanges(dynamicDataContainers);
				}
			}
		}

		void CascadeVGMEventToContainers(ZGuid[] containerPks, Event @event)
		{
			var log = consol.Logs.MostRecentLogByEventTime(@event, l =>
			{
				return StmALog.GetParametersFromReference(l.SL_Reference, StmALog.ParseReferenceError.None).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out string value)
				&& value == Core.Constants.EventReferenceMessageTypes.VerifiedGrossContainerWeight;
			});

			if (log == null)
			{
				return;
			}

			var factory = new BusinessObjectFactory();

			foreach (var containerPk in containerPks)
			{
				var containerToLogBizObj = factory.Load<ForwardingContainer>(containerPk);

				containerToLogBizObj?
					.Logs
					.CreateOrRecreateEventLog(
					@event,
					log.SL_IsEstimate
						? EstimateActual.Estimate
						: EstimateActual.Actual,
					log.SL_EventTimeOffset,
					log.SL_Reference);
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
		}

		void AcceptContainerChanges(IDynamicDataCollection containers)
		{
			foreach (var container in containers)
			{
				var statusCodePath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nameof(VGMMessagingContainer.VerifiedStatus), nameof(CodeDescription.Code));
				var statusCode = container.GetDynamicProperty(statusCodePath);
				statusCode.AcceptChanges();

				var statusDescriptionPath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nameof(VGMMessagingContainer.VerifiedStatus), nameof(CodeDescription.Description));
				var statusDescription = container.GetDynamicProperty(statusDescriptionPath);
				statusDescription.AcceptChanges();
			}
		}
	}
}
