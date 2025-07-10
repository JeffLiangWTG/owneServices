using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CommodityRelatedPackagingWrapper))]
	sealed class CommodityRelatedPackagingWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodityRelatedPackaging()
		{
			ICommodityRelatedPackaging commodityRelatedPackaging = new CommodityRelatedPackagingWrapper("PackingMethodDescription", "MaterialCode", "Specification");
			NUnit.Framework.Assert.That(commodityRelatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo("PackingMethodDescription").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo("MaterialCode").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo("Specification").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetCommodityRelatedPackaging()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_InnerPackType = "1";
			invoiceLine.JI_InnerPackingMaterial = "456";
			invoiceLine.JI_InnerPackDescription = new ZString('A', 200);
			CombineAssertions(() =>
			{
				var commodityRelatedPackaging = CommodityRelatedPackagingWrapper.GetCommodityRelatedPackaging(invoiceLine);
				NUnit.Framework.Assert.That(commodityRelatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "Commodity.CommodityRelatedPackaging.PackingMethodDescription should be");
				NUnit.Framework.Assert.That(commodityRelatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison), "Commodity.CommodityRelatedPackaging.MaterialCode should be");
				NUnit.Framework.Assert.That(commodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo(new ZString('A', 200)), "Commodity.CommodityRelatedPackaging.Specification should be");
			});
		}
	}
}
