using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module
{
	class TradeGroupsFilterLookups
	{
		public TradeGroupsFilterLookups(TradeGroupsFilterStripBusinessObject filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		public RefCountryCollection CountryCodeLookup => new RefCountryCollection(Factory);

		readonly TradeGroupsFilterStripBusinessObject filterBusinessObject;
		BusinessObjectFactory Factory => filterBusinessObject.Factory;
	}
}
