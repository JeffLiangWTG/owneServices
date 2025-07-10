using Enterprise.Freight.Integration.AWB;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentFHLMessageDetailsProvider
	{
		IFHLMessageDetailsProvider GetFHLMessageDetailsProvider();
		void SetLastUsageCodeForAllItems(string usageCode);
	}
}
