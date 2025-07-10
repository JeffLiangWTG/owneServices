using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentExportDelayAlertDocumentDeliveryJobTest : DelayAlertDocumentDeliveryJobTest
	{
		#region Implementation

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.ConsignorPK = RecipientOrganisation.PK;
			shipment.JS_IsForwardRegistered = false; // to allow delete in the unit test
			return shipment;
		}

		protected override BusinessContext Context
		{
			get { return BusinessContext.Shipment; }
		}

		protected override bool IsExportDA
		{
			get { return true; }
		}

		#endregion
	}
}
