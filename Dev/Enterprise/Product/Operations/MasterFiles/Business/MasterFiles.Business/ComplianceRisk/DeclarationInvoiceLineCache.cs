using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class DeclarationInvoiceLineCache
	{
		public ComplianceCommodity[] Commodities { get; }
		public ZString[] CountriesOfOrigin { get; }

		public DeclarationInvoiceLineCache(ComplianceCommodity[] commodities, ZString[] countriesOfOrigin)
		{
			Commodities = commodities;
			CountriesOfOrigin = countriesOfOrigin;
		}
	}
}
