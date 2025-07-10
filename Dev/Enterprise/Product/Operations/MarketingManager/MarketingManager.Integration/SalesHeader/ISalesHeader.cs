using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Integration
{
	public interface ISalesHeader : IBusiness
	{
		IOrgSalesProduct SalesProduct { get; }
		ZString SalesProductCode { get; }
		ZString SalesProductName { get; }
		IRefCurrency TotalCurrency { get; }
		ZString TotalCurrencyCode { get; }
	}
}
