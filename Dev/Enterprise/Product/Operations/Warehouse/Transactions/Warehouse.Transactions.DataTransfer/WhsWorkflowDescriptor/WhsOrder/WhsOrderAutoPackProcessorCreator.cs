using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsOrderAutoPackProcessorCreator : IWhsOrderAutoPackProcessorCreator
	{
		public IProcessor CreateOrderAutoPackProcessor(IWorkflowProvider provider)
		{
			return new WhsOrderAutoPackProcessor((IBusiness)provider);
		}
	}
}
