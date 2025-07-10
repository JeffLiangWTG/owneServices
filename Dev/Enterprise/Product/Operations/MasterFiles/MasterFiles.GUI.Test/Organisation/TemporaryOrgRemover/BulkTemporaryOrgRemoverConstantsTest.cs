using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class BulkTemporaryOrgRemoverConstantsTest : TestCase
	{
		public void TestBulkRemoveWarningTitle()
		{
			AssertEquals("CRITICAL WARNING", BulkTemporaryOrgRemoverConstants.BulkRemoveWarningTitle);
		}

		public void TestBulkRemoveConfirmationMessage()
		{
			AssertEquals("THIS PROCESS IS IRREVERSIBLE", BulkTemporaryOrgRemoverConstants.BulkRemoveConfirmationMessage);
		}

		public void TestGetBulkRemoveWarning()
		{
			AssertEquals("You are about to delete 123456 unused temporary organizations. This operation is time consuming and is irreversible. It can be stopped, but organizations already deleted cannot be restored. If you decide to proceed, CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?", BulkTemporaryOrgRemoverConstants.GetBulkRemoveWarning(123456));
		}
	}
}
