using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommodityNumberWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodityNumber()
		{
			ICommodityNumber commodityNumber = new CommodityNumberWrapper("AA", "BB");
			NUnit.Framework.Assert.That(commodityNumber.ID, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "CommodityNumber.ID should be");
			NUnit.Framework.Assert.That(commodityNumber.IdentifierTypeCode, NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison), "CommodityNumber.IdentifierTypeCode should be");
		}
	}
}
