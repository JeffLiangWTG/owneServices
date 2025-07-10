using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ShipmentDossierEventProcessor : IMessageEventsProcessor
	{
		public ShipmentDossierEventProcessor(ForwardingShipment shipment, IDocument document)
		{
			this.shipment = shipment;
			this.document = document;
		}

		readonly ForwardingShipment shipment;
		readonly IDocument document;

		public void OnMessageSent()
		{
			if (document != null &&
					document.Data is IDynamicData data &&
					data.Value is Dossier dossier)
			{
				var factory = new BusinessObjectFactory();
				var shipmentInAnotherFactory = factory.Load<ForwardingShipment>(shipment.PK);

				var cfsCountryCode = shipmentInAnotherFactory.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Core.Constants.CountryCodes.France;
				var agentReferenceList = dossier.AgentReference.Split(", ");

				foreach (var entryNum in shipmentInAnotherFactory?.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(e => e.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC && agentReferenceList.Contains(e.CE_EntryNum) && e.CE_EntryStatus.IsEmpty && e.CE_RN_NKCountryCode == cfsCountryCode)))
				{
					entryNum.CE_EntryStatus = Events.MessageSentCode;
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
			}
		}

		public void OnMessageWithdrawalSent() { }

		public void OnResetToOriginal() { }
	}
}
