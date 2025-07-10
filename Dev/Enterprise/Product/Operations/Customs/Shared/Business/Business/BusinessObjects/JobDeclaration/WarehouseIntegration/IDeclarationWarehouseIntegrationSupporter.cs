namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public interface IDeclarationWarehouseIntegrationSupporter : IWarehouseIntegrationSupporter
	{
		bool HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails { get; }
	}
}
