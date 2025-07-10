using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeduplicationResultsListDataSourceTest : TestCaseWithFactory
	{
		public void TestDefaultValue()
		{
			var model = new DeduplicationResultsListDataSource();
			AssertEquals(false, model.EmailPanelVisible);
			AssertEquals(false, model.PhonePanelVisible);
			AssertEquals(false, model.AssociationsPanelVisible);
		}

		public void TestAssociationsPanelVisible()
		{
			var model = new DeduplicationResultsListDataSource();
			model.Associations = "Associations";
			AssertEquals(true, model.AssociationsPanelVisible);
		}
	}
}
