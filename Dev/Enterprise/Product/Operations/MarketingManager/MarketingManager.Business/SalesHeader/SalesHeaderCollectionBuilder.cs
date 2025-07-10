using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class SalesHeaderCollectionBuilder : ISalesHeaderCollectionBuilder
	{
		public ISalesHeaderCollection New(ISalesValueAssociatedEntity entity, bool includeTraded)
		{
			return new SalesHeaderCollection(entity, includeTraded);
		}
	}
}
