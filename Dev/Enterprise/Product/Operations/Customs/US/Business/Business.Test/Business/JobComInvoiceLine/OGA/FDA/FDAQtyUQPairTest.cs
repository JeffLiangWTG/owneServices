using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FDAQtyUQPairTest : TestCase
	{
		public void TestFDAQtyUQPair()
		{
			FDAQtyUQPair fdaQtyUQPair = new FDAQtyUQPair(10, "KG");
			AssertEquals(10m, fdaQtyUQPair.Qty);
			AssertEquals("KG", fdaQtyUQPair.UQ);
		}
	}
}
