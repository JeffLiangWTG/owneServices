using Enterprise.EConversation.ServiceTasks;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class NeoWhsReceiveEmailProcessor : NeoBusinessObjectEmailProcessor<WhsReceive>
	{
		public override string EmailTypeName => nameof(WhsReceive);
	}
}
