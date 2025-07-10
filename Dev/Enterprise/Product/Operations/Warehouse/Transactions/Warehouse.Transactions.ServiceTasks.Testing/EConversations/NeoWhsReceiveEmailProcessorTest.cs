using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.ServiceTasks.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	public class NeoWhsReceiveEmailProcessorTest : NeoBusinessObjectEmailProcessorTest<WhsReceive>
	{
		protected override NeoBusinessObjectEmailProcessor<WhsReceive> GetProcessor() => new NeoWhsReceiveEmailProcessor();

		protected override WhsReceive GetExistingBusinessObject() => existingReceive;

		protected override string ExpectedEmailTypeName => nameof(WhsReceive);

		protected override void SetUp()
		{
			existingReceive = Factory.NewWithValidTestData<WhsReceive>();
			Factory.Save();

			base.SetUp();
		}
		WhsReceive existingReceive;
	}
}
