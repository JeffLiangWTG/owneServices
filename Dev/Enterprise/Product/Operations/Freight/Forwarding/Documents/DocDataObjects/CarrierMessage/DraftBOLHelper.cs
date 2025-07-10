using System;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	static class DraftBOLHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public static UniversalShipment GetDraftBillOfLadingUniversalXml(this ForwardingConsol consol)
		{
			var dataImportLogs = consol
				?.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DataLinkedCode)
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.ToArray();

			foreach (var dataImportLog in dataImportLogs ?? Array.Empty<StmALog>())
			{
				if (dataImportLog.RelatedEDIMessage?.Message is IEDIMessage linkedMessage
					&& linkedMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
				{
					var usxml = dataImportLog.RelatedEDIMessage.Message.GetEM_MessageTextReader().Parse<UniversalShipment>();

					var documentName = usxml
						?.DataContext
						?.DocumentaryOverride
						?.DocumentName;

					const string draftBillOfLadingDocumentName = "Draft Bill Of Lading";

					if (draftBillOfLadingDocumentName.Equals(documentName, StringComparison.InvariantCultureIgnoreCase))
					{
						return usxml;
					}
				}
			}

			return null;
		}
	}
}
