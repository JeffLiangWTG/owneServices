using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IAirBookingResponseConsolCostingDataObjectReader
	{
		void Import(IXmlImportLogger logger, IDataObject consolCostsDataObject, Enterprise.Integration.Forwarding.IForwardingConsol consol);
	}
}
