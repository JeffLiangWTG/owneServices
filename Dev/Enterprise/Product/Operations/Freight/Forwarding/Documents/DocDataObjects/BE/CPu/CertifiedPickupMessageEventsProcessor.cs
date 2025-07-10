using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class CertifiedPickupMessageEventsProcessor : IMessageEventsProcessor
	{
		public CertifiedPickupMessageEventsProcessor(ForwardingConsol consol, IDocument document)
		{
			this.consol = consol;
			this.document = document;
		}

		readonly ForwardingConsol consol;
		readonly IDocument document;

		#region IMessageEventsProcessor

		public void OnMessageSent()
		{
			if (document != null
				&& document.Data is IDynamicData data
				&& data.Value is CertifiedPickup certifiedPickup
				&& certifiedPickup.IsTransferMode)
			{
				var @event = Events.MessageSent;
				var log = consol.Logs.MostRecentLogByEventTime(@event, l =>
				{
					return StmALog.GetParametersFromReference(l.SL_Reference, StmALog.ParseReferenceError.None).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out string value)
					&& value == CertifiedPickupConstants.ParameterMessageTypes.Transfer;
				});

				if (log != null)
				{
					var containerPks = certifiedPickup.SelectedContainers
						.Where(c => c.CurrentStatus != CertifiedPickupConstants.Status.TransferSentAwaitingResponse && (c.Action.IsTransferToForwarder || c.Action.IsTransferToTransporter))
						.Select(c => c.Identifier).OfType<ZGuid>().Where(pk => pk.IsValid).ToArray();

					var factory = new BusinessObjectFactory();

					containerPks.ForEach(pk =>
					{
						var container = factory.Load<ForwardingContainer>(pk);

						container?.Logs.CreateOrRecreateEventLog(
							eventType: @event,
							estimateActual: log.SL_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual,
							eventTime: log.SL_EventTimeOffset,
							logReference: log.SL_Reference,
							new System.Collections.Generic.KeyValuePair<string, string>(CertifiedPickupConstants.ContainerEventParameter.EventCode, CertifiedPickupConstants.ContainerEventParameter.Values.TransferSentAwaitingResponse),
							new System.Collections.Generic.KeyValuePair<string, string>(EventReferenceParameters.Codes.EquipmentReferenceNumber, container.JC_ContainerNum));
					});

					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
				}
			}
		}

		public void OnMessageWithdrawalSent()
		{ }

		public void OnResetToOriginal()
		{ }

		#endregion
	}
}
