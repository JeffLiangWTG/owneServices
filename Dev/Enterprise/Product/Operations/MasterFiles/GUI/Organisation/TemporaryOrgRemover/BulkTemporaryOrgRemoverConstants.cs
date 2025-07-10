
namespace Enterprise.MasterFiles.GUI
{
	public static class BulkTemporaryOrgRemoverConstants
	{
		public static string BulkRemoveWarningTitle
		{
			get { return Res.GetString("92312a0f-a746-4aec-b6fe-de81f7b97859", "CRITICAL WARNING"); }
		}
		public static string BulkRemoveConfirmationMessage
		{
			get { return Res.GetString("2080d3a0-3cfd-4c91-897d-e2d9cf3a246f", "THIS PROCESS IS IRREVERSIBLE"); }
		}
		public static string GetBulkRemoveWarning(int number)
		{
			return Res.GetString("680709b3-0003-471b-9a43-c43d6929205d", "You are about to delete {0} unused temporary organizations. This operation is time consuming and is irreversible. It can be stopped, but organizations already deleted cannot be restored. If you decide to proceed, CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?", number.ToString());
		}
	}
}
