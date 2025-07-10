using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.ZArchitecture.Business.Event;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ
{
	sealed class ExportPreAdviceNotificationMessageEventProcessor : IMessageEventsProcessor
	{
		public ExportPreAdviceNotificationMessageEventProcessor(ForwardingConsol consol, IDocument document)
		{
			this.consol = consol;
			this.document = document;
		}

		readonly ForwardingConsol consol;
		readonly IDocument document;

		public void OnMessageSent()
		{
			CascadeEventToContainers(Events.MessageSent);
		}

		public void OnMessageWithdrawalSent()
		{
			CascadeEventToContainers(Events.MessageWithdrawCancelRequest);
		}

		public void OnResetToOriginal()
		{
		}

		void CascadeEventToContainers(Event @event)
		{
			if (document != null && document.Data is IDynamicData data && data.Value is ExportPreAdviceNotification exportPreAdviceNotification)
			{
				var containers = exportPreAdviceNotification.Containers.ToArray();

				var sendContainerPKs = containers
					.Select(container => container.Identifier)
					.OfType<ZGuid>()
					.Where(pk => pk.IsValid)
					.ToArray();

				var log = consol.Logs.MostRecentLogByEventTime(@event, l =>
				{
					return StmALog.GetParametersFromReference(l.SL_Reference, StmALog.ParseReferenceError.None).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out string value)
								 && value == Core.Constants.EventReferenceMessageTypes.ExportPreAdviceNotification;
				});

				if (log != null)
				{
					var factory = new BusinessObjectFactory();

					foreach (var containerPk in sendContainerPKs)
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
			}
		}
	}
}
