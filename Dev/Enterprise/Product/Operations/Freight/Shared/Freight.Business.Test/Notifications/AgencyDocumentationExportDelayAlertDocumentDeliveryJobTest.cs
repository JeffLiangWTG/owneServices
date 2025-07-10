using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.Business.Testing
{
	sealed class AgencyDocumentationExportDelayAlertDocumentDeliveryJobTest : DelayAlertDocumentDeliveryJobTest
	{
		#region implementation

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			shipment.ConsignorPK = RecipientOrganisation.PK;
			return shipment;
		}

		protected override BusinessContext Context
		{
			get { return BusinessContext.AgencyDocumentation; }
		}

		protected override bool IsExportDA
		{
			get { return true; }
		}

		#endregion
	}
}
