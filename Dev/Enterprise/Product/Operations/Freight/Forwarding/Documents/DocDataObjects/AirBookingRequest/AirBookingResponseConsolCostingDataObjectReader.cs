using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirBookingResponseConsolCostingDataObjectReader : IAirBookingResponseConsolCostingDataObjectReader
	{
		public void Import(IXmlImportLogger logger, IDataObject consolCostsDataObject, IForwardingConsol consol)
		{
			if (logger != null
				&& consol is ForwardingConsol consolBizObj
				&& consolCostsDataObject is ConsolCosts consolCostsData)
			{
				var importer = new AirBookingResponseConsolCostingImporter(logger, consolBizObj.Factory);
				importer.ImportConsolCosting(consolCostsData, consolBizObj);
			}
		}
	}
}
