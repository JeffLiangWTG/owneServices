using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class MathHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCalculateDefaultQuantity()
		{
			NUnit.Framework.Assert.That(MathHelper.CalculateDefaultQuantity(20, 10), NUnit.Framework.Is.EqualTo(10M).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(MathHelper.CalculateDefaultQuantity(20, 20), NUnit.Framework.Is.EqualTo(0M).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(MathHelper.CalculateDefaultQuantity(20, 30), NUnit.Framework.Is.EqualTo(0M).Using(CustomComparers.TypeComparison));
		}
	}
}
