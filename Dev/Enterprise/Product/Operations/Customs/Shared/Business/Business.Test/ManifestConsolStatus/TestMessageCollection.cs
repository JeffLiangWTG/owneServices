using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestMessageCollection : EDIMessageCollection
	{
		public TestMessageCollection(TestConsol consol, BusinessObjectFactory factory) : base(consol, factory)
		{
		}

		public new TestMessage this[int index]
		{
			get { return (TestMessage)base[index]; }
		}
	}
}
