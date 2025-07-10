using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Test.HelperClasses
{
	public class MathExtensionHelperTest : TestCaseWithFactory
	{
		public void TestRoundUp()
		{
			var result = MathExtensionHelper.RoundUp(3.2222m, 2);
			AssertEquals(result, 3.23m);
		}
	}
}
