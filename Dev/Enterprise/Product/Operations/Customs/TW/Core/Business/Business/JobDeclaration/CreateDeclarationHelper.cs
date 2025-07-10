using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.TW.ICreateDeclarationHelper
	{
		public override Customs.Business.ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment) => new ImportJobDeclaration(shipment.Factory);
	}
}
