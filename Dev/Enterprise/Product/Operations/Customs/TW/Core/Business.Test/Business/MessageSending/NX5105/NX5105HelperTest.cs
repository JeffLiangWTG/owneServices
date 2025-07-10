using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105HelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConvertBoolToString()
		{
			NUnit.Framework.Assert.That(ZBool.True.ConvertBoolToString("P", "N"), NUnit.Framework.Is.EqualTo("P").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(ZBool.False.ConvertBoolToString("P", "N"), NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}
	}
}
