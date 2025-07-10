using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterData.GUI.Tests
{
	class PotentialDuplicationModelTest : TestCaseWithFactory
	{
		public void TestActive()
		{
			var model = new PotentialDuplicationModel();
			model.IsActive = false;
			Assert(!model.IsActive);
			AssertEquals("No", model.Active);

			model.IsActive = true;
			Assert(model.IsActive);
			AssertEquals("Yes", model.Active);
		}
	}
}

