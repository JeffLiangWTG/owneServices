using Enterprise.EConversation.ServiceTasks;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class NeoWhsOrderEmailProcessor : NeoBusinessObjectEmailProcessor<WhsOrder>
	{
		public override string EmailTypeName => nameof(WhsOrder);
	}
}
