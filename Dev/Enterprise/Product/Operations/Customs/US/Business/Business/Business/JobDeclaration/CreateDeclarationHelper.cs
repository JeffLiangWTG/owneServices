using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.US.ICreateDeclarationHelper
	{
		public override Customs.Business.ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment)
		{
			return new ImportJobDeclaration(shipment.Factory);
		}
	}
}
