using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.ServiceTasks.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	public class NeoWhsOrderEmailProcessorTest : NeoBusinessObjectEmailProcessorTest<WhsOrder>
	{
		protected override NeoBusinessObjectEmailProcessor<WhsOrder> GetProcessor() => new NeoWhsOrderEmailProcessor();

		protected override WhsOrder GetExistingBusinessObject() => existingOrder;

		protected override string ExpectedEmailTypeName => nameof(WhsOrder);

		protected override void SetUp()
		{
			existingOrder = Factory.NewWithValidTestData<WhsOrder>();
			Factory.Save();

			base.SetUp();
		}
		WhsOrder existingOrder;
	}
}
