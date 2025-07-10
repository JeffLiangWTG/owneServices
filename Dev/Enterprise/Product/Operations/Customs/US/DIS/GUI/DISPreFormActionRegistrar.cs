using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.GUI
{
	public class DISPreFormActionRegistrar : IDISPreFormActionRegistrar
	{
		#region IDISPreFormActionRegistrar Members

		public IDISPreFormActionRunner GetDISPreFormActionRunner(IDISHost disHost)
		{
			IDISPreFormActionRunner runner = null;

			if (disHost is Integration.Customs.US.IJobDeclaration)
			{
				runner = new JobDeclarationDISPreFormActionRunner(disHost);
			}

			return runner;
		}

		#endregion
	}
}
