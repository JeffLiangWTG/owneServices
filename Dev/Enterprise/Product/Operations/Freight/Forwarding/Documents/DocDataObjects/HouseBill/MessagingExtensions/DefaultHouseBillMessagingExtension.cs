using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class DefaultHouseBillMessagingExtension : BaseMessagingExtensions
	{
		public DefaultHouseBillMessagingExtension(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public override string GetXmlNamespace() => shipment.IsEditingElectronicBOL ? DocDataConstants.XmlNamespaces.EHBL : null;

		public override KeyValuePair<string, string>[] GetAdditionalParametersForEvent()
		{
			var result = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Core.Constants.BillStatusUpdatedTypes.OriginalBillSentForPublication),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Action, shipment.JS_ElectronicBillOfLadingStatus != Freight.Business.FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress ? (NoResString)"Original" : (NoResString)"Amendment")
			};

			return result.ToArray();
		}

		public override bool? IsSendingAmendment()
		{
			return shipment.JS_ElectronicBillOfLadingStatus == Freight.Business.FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
		}

		public override bool? ShowEvents()
		{
			return shipment.IsEditingElectronicBOL;
		}

		public override bool? ShowLastEventDetails()
		{
			return false;
		}
	}
}
