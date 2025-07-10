using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.ZArchitecture.Business.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class LegacyVGMMessageEventProcessor : IMessageEventsProcessor
	{
		public LegacyVGMMessageEventProcessor(ForwardingConsol consol, IDocument document)
		{
			this.consol = consol;
			this.document = document;
		}

		readonly ForwardingConsol consol;
		readonly IDocument document;

		public void OnMessageSent() => CascadeVGMEventToContainers(Events.MessageSent);

		public void OnMessageWithdrawalSent() => CascadeVGMEventToContainers(Events.MessageWithdrawCancelRequest);

		public void OnResetToOriginal() => CascadeVGMEventToContainers(Events.StatusUpdated);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		void CascadeVGMEventToContainers(Event @event)
		{
			if (document != null
				&& string.Compare(document.Name, "Verified Gross Container Weight", System.StringComparison.OrdinalIgnoreCase) == 0
				&& document.Data is IDynamicData data
				&& data.GetDynamicProperty(nameof(UniversalShipment.ContainerCollection)) is IDynamicDataCollection containerCollection)
			{
				var containerPKs = containerCollection
					.Select(uxmlContainer => uxmlContainer.GetMetaData<ZGuid>(MetaDataType.Identifier))
					.Where(pk => pk.IsValid)
					.ToArray();

				CascadeVGMEventToContainers(containerPKs.ToArray(), @event);
			}
		}

		void CascadeVGMEventToContainers(ZGuid[] containerPks, Event @event)
		{
			var log = consol.Logs.MostRecentLogByEventTime(@event);

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
	}
}
