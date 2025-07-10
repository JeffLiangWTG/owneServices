using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageSendingQueryCollectionTest : TestCaseWithFactory
	{
		public void TestEnumeration()
		{
			MessageSendingQuery query1 = new MessageSendingQuery();
			MessageSendingQuery query2 = new MessageSendingQuery();
			MessageSendingQueryCollection collection = new MessageSendingQueryCollection();
			collection.Add(query1);
			collection.Add(query2);
			int count = 0;
			foreach (MessageSendingQuery query in collection)
			{
				AssertEquals("", query.Caption);
				count++;
			}
			AssertEquals(2, count);
		}

		public void TestMessageSendingQueryAndDelegate()
		{
			MessageSendingQuery query = new MessageSendingQuery();
			query.Question = "Are Cuckoo Squeakers awesome?";
			query.Caption = "Cuckoo Squeaker Caption!!";
			MessageSendingQueryDelegate queryDelegate = new MessageSendingQueryDelegate(DelegateTrigger);
			query.Delegate = queryDelegate;
			AssertEquals("Are Cuckoo Squeakers awesome?", query.Question);
			AssertEquals("Cuckoo Squeaker Caption!!", query.Caption);
			AssertEquals(queryDelegate, query.Delegate);
		}

		void DelegateTrigger(bool cuckooSqueaker)
		{
		}
	}
}
