using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageGenerationResultCollectionTest : TestCaseWithFactory
	{
		public void TestHasMessagesGenerated()
		{
			MessageGenerationResultCollection coll = new MessageGenerationResultCollection();
			AssertEquals(false, coll.HasMessagesGenerated);

			coll.Add("original", 1, new EDIMessage[] { Factory.New<EDIMessage>() });
			AssertEquals(true, coll.HasMessagesGenerated);
		}

		public void TestDeleteAllMessagesGenerated()
		{
			MessageGenerationResultCollection coll = new MessageGenerationResultCollection();
			EDIMessage message = Factory.New<EDIMessage>();

			coll.Add("original", 1, new EDIMessage[] { message });
			coll.DeleteAllMessagesGenerated();
			AssertEquals("message is deleted", true, message.IsDeleted);
			AssertEquals(false, coll.HasMessagesGenerated);
		}

		public void TestFullDescriptionsOfMessagesGenerated()
		{
			MessageGenerationResultCollection coll = new MessageGenerationResultCollection();
			coll.Add("original", 1, new EDIMessage[] { Factory.New<EDIMessage>() });
			coll.Add("amendment", 2, new EDIMessage[] { Factory.New<EDIMessage>(), Factory.New<EDIMessage>() });
			AssertEquals("1 original(s) 2 amendment(s)", coll.FullDescriptionsOfMessagesGenerated(" "));
		}
	}
}
