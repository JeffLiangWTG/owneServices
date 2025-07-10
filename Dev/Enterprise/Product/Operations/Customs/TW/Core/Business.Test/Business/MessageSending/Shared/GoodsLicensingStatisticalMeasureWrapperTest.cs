using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsLicensingStatisticalMeasureWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGoodsLicensingStatisticalMeasureWrapper()
		{
			IGoodsLicensingStatisticalMeasure commodityRelatedPackaging = new GoodsLicensingStatisticalMeasureWrapper(999.900, "StatisticalUnitCode");
			var decimalValue = 9m;

			NUnit.Framework.Assert.That(decimalValue, NUnit.Framework.Is.EqualTo((decimal)999).Within((decimal)commodityRelatedPackaging.LicensingQuantity));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("StatisticalUnitCode").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}
	}
}
