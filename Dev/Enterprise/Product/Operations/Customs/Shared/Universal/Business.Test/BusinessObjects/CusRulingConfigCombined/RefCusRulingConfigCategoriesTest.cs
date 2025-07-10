using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class RefCusRulingConfigCategoriesTest : TestCaseWithFactory
	{
		public void TestRefCusRulingConfigCategoriesCodes()
		{
			var refCusRulingConfigCategories = new RefCusRulingConfigCategories();
			var codes = refCusRulingConfigCategories.GetAllCodes();

			AssertEquals(6, codes.Length);
			AssertContainsExactElementsInAnyOrder(new string[] { "DAT", "DTY", "EXC", "EXD", "GST", "SIM" }, codes);
			AssertEquals("Effective Date Rule", refCusRulingConfigCategories.GetDescriptionFromCode("DAT"));
			AssertEquals("Duty(All)", refCusRulingConfigCategories.GetDescriptionFromCode("DTY"));
			AssertEquals("Excise Tax", refCusRulingConfigCategories.GetDescriptionFromCode("EXC"));
			AssertEquals("Excise Duty", refCusRulingConfigCategories.GetDescriptionFromCode("EXD"));
			AssertEquals("GST", refCusRulingConfigCategories.GetDescriptionFromCode("GST"));
			AssertEquals("SIMA", refCusRulingConfigCategories.GetDescriptionFromCode("SIM"));
		}
	}
}
