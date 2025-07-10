using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	public class CountrySanctionTest : TestCaseWithFactory
	{
		public void TestLibyaSanctionProgram()
		{
			var sanctionProvider = new CountrySanction().ConditionState("US", "LY", ZDateTime.UtcToday);
			AssertEquals("ConditionDescription", "OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/pages/libya.aspx", sanctionProvider.ConditionDescription);
			AssertEquals("ConditionType", "ALL", sanctionProvider.ConditionType);
			AssertEquals("IsConditionActive", true, sanctionProvider.IsConditionActive);
		}

		public void TestNorthKoreaSanctionProgram()
		{
			var sanctionProvider = new CountrySanction().ConditionState("US", "KP", ZDateTime.UtcToday);
			AssertEquals("ConditionDescription", "OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/pages/nkorea.aspx", sanctionProvider.ConditionDescription);
			AssertEquals("ConditionType", "ALL", sanctionProvider.ConditionType);
			AssertEquals("IsConditionActive", true, sanctionProvider.IsConditionActive);
		}

		public void TestIranSanctionProgram()
		{
			var sanctionProvider = new CountrySanction().ConditionState("US", "IR", ZDateTime.UtcToday);
			AssertEquals("ConditionDescription", "OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/Pages/iran.aspx", sanctionProvider.ConditionDescription);
			AssertEquals("ConditionType", "ALL", sanctionProvider.ConditionType);
			AssertEquals("IsConditionActive", true, sanctionProvider.IsConditionActive);
		}

		public void TestBelarusSanctionProgram()
		{
			var sanctionProvider = new CountrySanction().ConditionState("US", "BY", ZDateTime.UtcToday);
			AssertEquals("ConditionDescription", "OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/pages/belarus.aspx", sanctionProvider.ConditionDescription);
			AssertEquals("ConditionType", "ALL", sanctionProvider.ConditionType);
			AssertEquals("IsConditionActive", true, sanctionProvider.IsConditionActive);
		}

		public void TestNonSanctionedCountry()
		{
			var sanctionProvider = new CountrySanction().ConditionState("US", "AU", ZDateTime.UtcToday);
			AssertEquals("ConditionDescription", "", sanctionProvider.ConditionDescription);
			AssertEquals("ConditionType", "", sanctionProvider.ConditionType);
			AssertEquals("IsConditionActive", false, sanctionProvider.IsConditionActive);
		}
	}
}
