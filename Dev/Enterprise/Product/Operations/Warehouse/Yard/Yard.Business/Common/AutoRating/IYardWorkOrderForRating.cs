using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public interface IYardWorkOrderForRating
	{
		JobInvoicingConsumerType ConsumerType { get; }

		WhsWarehouse Yard { get; }

		OrgHeader Client { get; }
	}
}
