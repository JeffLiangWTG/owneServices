using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterData.GUI.Test
{
	internal class PersonFilterActivesListTest : TestCaseWithFactory
	{
		public void TestPersonFilterActivesListDescriptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("All", PersonFilterActivesList.Descriptions.All);
				AssertEquals("Active", PersonFilterActivesList.Descriptions.Active);
				AssertEquals("Inactive", PersonFilterActivesList.Descriptions.Inactive);
			});
		}

		public void TestPersonFilterActivesListPairs()
		{
			var personFilterActivesList = new PersonFilterActivesList();
			CombineAssertions(() =>
			{
				AssertEquals(personFilterActivesList.Count, 3);
				AssertEquals(personFilterActivesList[0].Description, "All");
				AssertEquals(personFilterActivesList[1].Description, "Active");
				AssertEquals(personFilterActivesList[2].Description, "Inactive");
			});
		}
	}
}
