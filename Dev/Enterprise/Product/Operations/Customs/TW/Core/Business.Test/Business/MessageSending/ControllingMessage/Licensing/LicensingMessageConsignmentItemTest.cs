using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageConsignmentItem))]
	sealed class LicensingMessageConsignmentItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodity()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_Description = "entry line desc";
			var line = Factory.New<JobComInvoiceLine>();
			line.JI_CL = entryLine.PK;
			line.JI_Group = "line group1";
			line.JI_GoodsType = "111";
			line.JI_BrandName = "b name";
			line.JI_InnerPackType = "2";
			line.JI_InnerPackingMaterial = "X";
			var licensingMessageConsignmentItem = new LicensingMessageConsignmentItem(line);
			CombineAssertions(() =>
			{
				var commodity = ((IConsignmentItem)licensingMessageConsignmentItem).Commodity;
				NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo(entryLine.CL_Calc_GoodsDescription), "Description");
				NUnit.Framework.Assert.That(commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison), "GoodsGroupNameCode");
				NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("b name").Using(CustomComparers.TypeComparison), "Name");

				var relatedPackaging = commodity.CommodityRelatedPackaging;
				NUnit.Framework.Assert.That(relatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "CommodityRelatedPackaging.PackingMethodDescription");
				NUnit.Framework.Assert.That(relatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo("X").Using(CustomComparers.TypeComparison), "CommodityRelatedPackaging.MaterialCode");
			});
		}

		[ExpectNoExceptions]
		public void TestOrigin()
		{
			var line = Factory.New<JobComInvoiceLine>();
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Taiwan;
			var licensingMessageConsignmentItem = new LicensingMessageConsignmentItem(line);
			NUnit.Framework.Assert.That(((IConsignmentItem)licensingMessageConsignmentItem).Origin.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison), "Origin.CountryCode");
		}
	}
}
