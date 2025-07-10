using System.Collections.Generic;

using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.SailingScheduleDataVendor
{
	class VoyageOriginSubscriptionEventDataObjectWriter : EventDataObjectWriter
	{
		public VoyageOriginSubscriptionEventDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override void PopulateDataObject(BaseStmALog logBO, UniversalEvent logData)
		{
			base.PopulateDataObject(logBO, logData);
			var parentBO = logBO.Master;
			var voyageOrigin = (VoyageOrigin)parentBO;
			var voyageOriginContext = new List<Context>();

			if (!string.IsNullOrWhiteSpace(voyageOrigin.JA_RL_NKPortOfLoading))
			{
				var portContext = new Context
				{
					Type = new ContextType() { Type = nameof(UniversalEvent.ContextTypes.LegOriginUNLOCO) },
					Value = voyageOrigin.JA_RL_NKPortOfLoading
				};

				voyageOriginContext.Add(portContext);
			}

			if (!voyageOrigin.JA_E_DEP.IsEmpty)
			{
				var estimatedTimeContext = new Context
				{
					Type = new ContextType() { Type = nameof(UniversalEvent.ContextTypes.EstimatedTimeOfDeparture) },
					Value = voyageOrigin.JA_E_DEP.ToISO8601ShortDateString()
				};

				voyageOriginContext.Add(estimatedTimeContext);
			}

			logData.ContextCollection.AddRange(voyageOriginContext);
		}
	}
}
