using System.Reflection;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class CompanyTariffCodeAttributeTest : TestCase
	{
		public void TestHasNoServiceDirection()
		{
			var companyTariffCodes = new CompanyTariffCodes();
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.AIR));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.CST));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.DST));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.FCL));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.LCL));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.ORG));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.PAC));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.SCO));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.SDE));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.SED));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.SID));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.SNC));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.SOR));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.TRN));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.TBC));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.UNP));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.WHS));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.TRW));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.TWU));
			Assert(companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.CYD));

			// Customs Rate
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.CAI));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.CFC));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.CLC));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.COR));
			Assert(!companyTariffCodes.HasNoServiceDirection(RatingConstants.RateCategory.CDS));
		}

		public void TestGetRateCategories()
		{
			var companyTariffCodes = new CompanyTariffCodes();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), companyTariffCodes.GetRateCategories(""));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), companyTariffCodes.GetRateCategories("###"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.FCL, RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.CAI, RatingConstants.RateCategory.CFC, RatingConstants.RateCategory.CLC }, companyTariffCodes.GetRateCategories("FRT"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.COR }, companyTariffCodes.GetRateCategories("ORG"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.DST, RatingConstants.RateCategory.CDS }, companyTariffCodes.GetRateCategories("DST"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.SOR }, companyTariffCodes.GetRateCategories("SOR"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.SDE }, companyTariffCodes.GetRateCategories("SDE"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.SCO, RatingConstants.RateCategory.SNC }, companyTariffCodes.GetRateCategories("SFR"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.SED, RatingConstants.RateCategory.SID }, companyTariffCodes.GetRateCategories("SCD"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.PAC, RatingConstants.RateCategory.UNP, RatingConstants.RateCategory.CST }, companyTariffCodes.GetRateCategories("CFS"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.WHS }, companyTariffCodes.GetRateCategories("WHS"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.TRW }, companyTariffCodes.GetRateCategories("TRW"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.TWU }, companyTariffCodes.GetRateCategories("TWU"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.CYD }, companyTariffCodes.GetRateCategories("CYD"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.TRN }, companyTariffCodes.GetRateCategories("TRN"));
			AssertContainsExactElementsInAnyOrder(new[] { RatingConstants.RateCategory.TBC }, companyTariffCodes.GetRateCategories("TBC"));
		}

		public void TestRateCategoriesExistForEveryRateCategoryField()
		{
			var companyTariffCodes = new CompanyTariffCodes();

			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
			{
				var fieldInfo = typeof(RatingConstants.RateCategory).GetField(rateCategory, BindingFlags.Public | BindingFlags.Static);
				Assert("companyTariffCodes should contain item for " + rateCategory, companyTariffCodes.ContainsCode(rateCategory));
			}
		}

		public void TestAllDescriptionsAreTranslatable()
		{
			using (var mockRes = Res.UseMockData())
			{
				const string hao = "好";
				mockRes.SetResourceGetter(new ResourceStringGetter(delegate(string key)
				{ return new ResourceStringData(key, hao); }));
				foreach (CompanyTariffCode companyTariffCode in new CompanyTariffCodes())
				{
					AssertEquals(hao, companyTariffCode.Description);
					AssertEquals(hao, companyTariffCode.CategoryDescription.Description);
				}
			}
		}
	}
}
