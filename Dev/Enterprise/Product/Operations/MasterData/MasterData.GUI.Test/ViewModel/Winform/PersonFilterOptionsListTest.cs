using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterData.GUI.Test
{
	internal class PersonFilterOptionsListTest : TestCaseWithFactory
	{
		public void TestPersonFilterOptionsDescriptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Contains", PersonFilterOptionsList.Descriptions.Contains);
				AssertEquals("Exact Match", PersonFilterOptionsList.Descriptions.ExactMatch);
			});
		}

		public void TestPersonFilterOptionsListPairs()
		{
			var personFilterOptionsList = new PersonFilterOptionsList();
			CombineAssertions(() =>
			{
				AssertEquals(personFilterOptionsList.Count, 2);

				AssertEquals(personFilterOptionsList[0].Description, "Contains");
				AssertEquals(personFilterOptionsList[1].Description, "Exact Match");
			});
		}
	}
}
