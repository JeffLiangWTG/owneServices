using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IGlobalCommercialInvoiceComplianceProcessor
	{
		IGlobalCommercialInvoiceComplianceProvider GetGlobalCommercialInvoiceBusinessObject(IBusiness hostBusinessEntity);

		public interface IGlobalCommercialInvoiceComplianceProvider
		{
			IEnumerable<IScreeningParty> Parties { get; }
			IEnumerable<IComplianceLocation> Locations { get; }
			IEnumerable<IComplianceCommodity> Commodities { get; }
		}

		public interface IGlobalCommercialInvoiceProvider
		{
			IGlobalCommercialInvoiceComplianceProvider DataProvider { get; }
		}
	}
}
