using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.Business.Testing
{
	sealed class AgencyDocumentationImportDelayAlertDocumentDeliveryJobTest : DelayAlertDocumentDeliveryJobTest
	{
		#region Implementation

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			shipment.ConsigneePK = RecipientOrganisation.PK;
			return shipment;
		}

		protected override BusinessContext Context
		{
			get { return BusinessContext.AgencyDocumentation; }
		}

		protected override bool IsExportDA
		{
			get { return false; }
		}

		#endregion
	}
}
