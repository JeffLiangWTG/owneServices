using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsMeasureTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNetWeightMeasure()
		{
			NUnit.Framework.Assert.That(goodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTariffQuantity()
		{
			NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnitCode()
		{
			NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("XX").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			goodsMeasure = new GoodsMeasure(2m, "XX", 1m);
			base.SetUp();
		}

		GoodsMeasure goodsMeasure;
	}
}
