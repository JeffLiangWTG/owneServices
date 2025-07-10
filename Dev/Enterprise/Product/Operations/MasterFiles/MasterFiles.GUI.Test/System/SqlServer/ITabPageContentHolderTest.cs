namespace Enterprise.MasterFiles.GUI.Test
{
	interface ITabPageContentHolderTest
	{
		void TestFormIsClosed();
		void TestClosingFormCanLeadToSaveIfUserAgrees();
		void TestClosingFormDoNotLeadToSaveIfUserDoesNotAgree();
		void TestClosingFormCanBeCanceled();
		void TestSwitchTab();
		void TestSwitchingTabCanLeadToSaveIfUserAgrees();
		void TestSwitchingTabDoNotSaveIfUserDoesNotAgree();
	}
}
