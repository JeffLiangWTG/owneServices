using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class MRJEventProcessor
	{
		public MRJEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}

		#region Process

		public void Process(WhsItemReceiveConsignment rcn, DocumentVisualizer.Business.VisualizerDocumentData documentData)
		{
			ProcessLogCore(rcn, documentData);
		}

		public void Process(WhsItemDispatchConsignment dcn, DocumentVisualizer.Business.VisualizerDocumentData documentData)
		{
			ProcessLogCore(dcn, documentData);
		}

		#endregion

		#region AddOrUpdatePortReference

		void ProcessLogCore(BusinessObject parent, DocumentVisualizer.Business.VisualizerDocumentData documentData)
		{
			if (eventDataObject.EventParameters.MessageType.HasValue && eventDataObject.EventParameters.MessageType.Value == CIN750NotificationConstants.CIN750MessageType && ZGuid.TryParse(eventDataObject.EventParameters.RequestNumber, out var messageID))
			{
				var historyLogs = GetCINHistoryLogsByMessageID(parent, messageID, documentData);

				if (historyLogs.Any() && parent is IStmALogParent logParent)
				{
					foreach (var historyLog in historyLogs)
					{
						historyLog.Cancel();
						var logInParent = logParent.Logs.Find(l => l.SL_Reference.EndsWith(historyLog.SL_Reference) && l.SL_EventTime == historyLog.SL_EventTime).SingleOrDefault();
						logInParent?.Cancel();
					}
				}
			}
		}

		List<StmALog> GetCINHistoryLogsByMessageID(BusinessObject parent, ZGuid messageID, DocumentVisualizer.Business.VisualizerDocumentData documentData)
		{
			var typeStrs = new List<string>();
			if (parent is WhsItemReceiveConsignment rcn)
			{
				typeStrs.Add(CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.In);
				typeStrs.Add(CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Cor);
			}
			else if (parent is WhsItemDispatchConsignment)
			{
				typeStrs.Add(CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Decons);
				typeStrs.Add(CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Cons);
				typeStrs.Add(CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Out);
			}

			var matchedAddOnValues = parent.GetAddOnValues(a => a.XV_Data.ToLower().StartsWith(messageID.ToString().ToLower()) && typeStrs.Any(s => a.XV_Name.StartsWith(s))).ToList();
			if (matchedAddOnValues.Any())
			{
				var historyIDs = matchedAddOnValues.Select(a => ZGuid.ParseSafe(a.XV_Data.Split("|")[1])).ToList();
				return documentData.Logs.Find(l => historyIDs.Contains(l.PK)).Where(l => !l.IsCancelled).ToList();
			}
			return new List<StmALog>();
		}

		#endregion

		protected readonly UniversalEvent eventDataObject;
		protected readonly IXmlImportLogger logger;
		protected readonly BusinessObjectFactory factory;
	}
}
