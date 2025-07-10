using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class EntryLinesMessagesTabControlTest : TestCaseWithFactory
	{
		public void TestEntryLinesTabPage()
		{
			AssertNotNull(tabControl.FindSingleOrDefault<ZTabPage>("EntryLinesTabPage", maxLevelsDeep: 1));
		}

		public void TestEntryLinesDutiesUserControl()
		{
			AssertNotNull(tabControl.FindSingleOrDefault<ZTabControl>("EntryLineInfoTabControl", maxLevelsDeep: 2));
		}

		public void TestDutiesTabPage()
		{
			AssertNotNull(tabControl.FindSingleOrDefault<ZTabPage>("DutiesTabPage", maxLevelsDeep: 1));
		}

		public void TestDutiesGridUserControl()
		{
			var tab = tabControl.FindSingleOrDefault<ZTabPage>("DutiesTabPage", maxLevelsDeep: 1);
			AssertNotNull(tab.FindSingleOrDefault<EntryDutiesUserControl>("EntryDutiesUserControl"));
		}

		MessageUserControl messageUserControl;
		ZTabControl tabControl;

		protected override void SetUp()
		{
			messageUserControl = new MessageUserControl();
			tabControl = messageUserControl.FindSingleOrDefault<ZTabControl>("EntryLinesMessagesTabControl");
			base.SetUp();
		}

		protected override void TearDown()
		{
			messageUserControl?.Dispose();
			base.TearDown();
		}
	}
}
