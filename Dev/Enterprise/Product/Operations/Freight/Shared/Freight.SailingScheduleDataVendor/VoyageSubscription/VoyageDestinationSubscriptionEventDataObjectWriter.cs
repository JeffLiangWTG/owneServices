using System.Collections.Generic;

using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.SailingScheduleDataVendor
{
	class VoyageDestinationSubscriptionEventDataObjectWriter : EventDataObjectWriter
	{
		public VoyageDestinationSubscriptionEventDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override void PopulateDataObject(BaseStmALog logBO, UniversalEvent logData)
		{
			base.PopulateDataObject(logBO, logData);
			var parentBO = logBO.Master;
			var voyageDestination = (VoyageDestination)parentBO;
			var voyageOriginContext = new List<Context>();

			if (!string.IsNullOrWhiteSpace(voyageDestination.JB_RL_NKPortOfDischarge))
			{
				var portContext = new Context
				{
					Type = new ContextType() { Type = nameof(UniversalEvent.ContextTypes.LegDestinationUNLOCO) },
					Value = voyageDestination.JB_RL_NKPortOfDischarge
				};

				voyageOriginContext.Add(portContext);
			}

			if (!voyageDestination.JB_E_ARV.IsEmpty)
			{
				var estimatedTimeContext = new Context
				{
					Type = new ContextType() { Type = nameof(UniversalEvent.ContextTypes.EstimatedTimeOfArrival) },
					Value = voyageDestination.JB_E_ARV.ToISO8601ShortDateString()
				};

				voyageOriginContext.Add(estimatedTimeContext);
			}

			logData.ContextCollection.AddRange(voyageOriginContext);
		}
	}
}
