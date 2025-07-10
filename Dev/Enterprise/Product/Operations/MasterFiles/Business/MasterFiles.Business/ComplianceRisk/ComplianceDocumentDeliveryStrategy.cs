using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceDocumentDeliveryStrategy
	{
		public DocumentDeliveryResultForComplianceWorkflow DocumentDeliveryResponse { get; set; } = DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery;
	}
}
