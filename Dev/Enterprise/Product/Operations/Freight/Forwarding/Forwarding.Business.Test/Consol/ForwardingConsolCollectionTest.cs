using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolCollection))]
	sealed class ForwardingConsolCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingConsolCollection(Factory);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(ForwardingConsolCollection), GetCollectionToTest().GetType());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingConsol>();
		}
	}
}
