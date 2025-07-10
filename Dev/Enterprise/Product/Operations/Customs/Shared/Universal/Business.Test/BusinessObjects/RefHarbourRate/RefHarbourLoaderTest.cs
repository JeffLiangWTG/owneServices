using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefHarbourRate.Loader))]
	sealed class RefHarbourLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refHarbourRate1 = helper.CreateHarbourRate("IMP", "FRPAR", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([T] > 1, MAX(2, 0.5 * [T]), 0)", "FR");
			var refHarbourRate2 = helper.CreateHarbourRate("EXP", "FRPAR", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([T] > 1, MAX(2, 0.5 * [T]), 0)", "FR");
			var refHarbourRate3 = helper.CreateHarbourRate("IMP", "FRLEH", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40FT] * 10 + [20FT] * 5 + [45FT] * 2", "FR");
			var refHarbourRate4 = helper.CreateHarbourRate("IMP", "FRPAR", "FCL", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40FT] * 10 + [20FT] * 5 + [45FT] * 2", "FR");
			var refHarbourRate5 = helper.CreateHarbourRate("IMP", "FRLEH", "LCL", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40FT] * 10 + [20FT] * 5 + [45FT] * 2", "FR");
			var refHarbourRate6 = helper.CreateHarbourRate("IMP", "FRPAR", "CON", "", ZDate.Today.AddDays(1), ZDate.Today.AddDays(2), "[40FT] * 10 + [20FT] * 5 + [45FT] * 2", "FR");
			var loader = new RefHarbourRate.Loader(Factory);
			var harbourRates = loader.Load("IMP", "FR", "CNT", ZDateTime.Today, "FRPAR");
			AssertEquals("harbourRates.Length", 1, harbourRates.Length);
			AssertCollectionContains(refHarbourRate1, harbourRates);
			AssertCollectionNotContains(refHarbourRate4, harbourRates);
			harbourRates = loader.Load("IMP", "FR", "CNT", ZDateTime.Today, "FRLEH");
			AssertEquals("harbourRates.Length", 1, harbourRates.Length);
			AssertCollectionContains(refHarbourRate3, harbourRates);
			AssertCollectionNotContains(refHarbourRate5, harbourRates);
			harbourRates = loader.Load("IMP", "FR", "LCL", ZDateTime.Today, "FRLEH");
			AssertEquals("harbourRates.Length", 2, harbourRates.Length);
			AssertCollectionContains(refHarbourRate3, harbourRates);
			AssertCollectionContains(refHarbourRate5, harbourRates);
			harbourRates = loader.Load("IMP", "FR", "CNT", ZDateTime.Today, "FRMRS");
			AssertEquals("harbourRates.Length", 0, harbourRates.Length);
			harbourRates = loader.Load("EXP", "FR", "CNT", ZDateTime.Today, "FRPAR");
			AssertEquals("harbourRates.Length", 1, harbourRates.Length);
			AssertCollectionContains(refHarbourRate2, harbourRates);

			var refHarbourRate7 = helper.CreateHarbourRate("IMP", "FRPAR", "CNT", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40FT] * 10 + [20FT] * 5 + [45FT] * 2", "FR");
			harbourRates = loader.Load("IMP", "FR", "CNT", ZDateTime.Today, "FRPAR", true);
			AssertEquals("harbourRates.Length", 2, harbourRates.Length);
			AssertContainsExactElementsInAnyOrder(new RefHarbourRate[] { refHarbourRate1, refHarbourRate7 }, harbourRates);
			harbourRates = loader.Load("IMP", "FR", "CNT", ZDateTime.Today, "FRPAR", false);
			AssertEquals("harbourRates.Length", 1, harbourRates.Length);
			AssertContainsExactElementsInAnyOrder(new RefHarbourRate[] { refHarbourRate7 }, harbourRates);

			var refHarbourRate8 = helper.CreateHarbourRate("TAX", "ITVCE", "ALL", "A1", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0.1226 * [TNE]", "IT", "9AA");
			harbourRates = loader.Load("TAX", "IT", "ALL", ZDateTime.Today, "ITVCE", false, "A1");
			AssertEquals("harbourRates.Length", 1, harbourRates.Length);
			AssertContainsExactElementsInAnyOrder(new RefHarbourRate[] { refHarbourRate8 }, harbourRates);

			harbourRates = loader.Load("TAX", "IT", "ALL", ZDateTime.Empty, "ITVCE", false, "A1");
			AssertEquals("When filtering date is not valid, harbourRates.Length", 0, harbourRates.Length);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefHarbourRate.Loader(Factory);
		}
	}
}
