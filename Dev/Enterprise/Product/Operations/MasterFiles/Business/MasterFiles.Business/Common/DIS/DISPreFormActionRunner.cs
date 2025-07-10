namespace Enterprise.MasterFiles.Business.DIS
{
	public class DISPreFormActionRunner : IDISPreFormActionRunner
	{
		public DISPreFormActionRunner(IDISHost disHost)
		{
			DisHost = disHost;
		}

		readonly IDISHost DisHost;

		#region IDISPreFormActionRunner Members

		public bool Execute()
		{
			return !DisHost.NeedToDoPreFormAction() || DisHost.DoPreFormAction();
		}

		#endregion
	}
}
