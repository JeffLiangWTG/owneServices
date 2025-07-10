using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class OneInterchangeToOneMessageInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestBuildInterchangeBatchesCore()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddNew();
			messages.AddNew();

			OneInterchangeToOneMessageInterchangeProviderTestClass provider = new OneInterchangeToOneMessageInterchangeProviderTestClass(messages);
			AssertEquals(2, provider.Interchanges.Length);
		}

		class OneInterchangeToOneMessageInterchangeProviderTestClass : OneInterchangeToOneMessageInterchangeProvider
		{
			public OneInterchangeToOneMessageInterchangeProviderTestClass(NonDependentEDIMessageCollection messages) : base(messages)
			{
			}

			protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
			{
			}

			protected override string InstructionHowToSetInterchangeSenderID
			{
				get { return ""; }
			}
		}
	}
}
