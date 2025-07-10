using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Integration
{
	public interface ISalesHeaderCollectionBuilder
	{
		ISalesHeaderCollection New(ISalesValueAssociatedEntity relatedBusinessObject, bool includeTraded);
	}
}
