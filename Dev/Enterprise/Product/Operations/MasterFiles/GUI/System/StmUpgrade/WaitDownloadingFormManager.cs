namespace Enterprise.MasterFiles.GUI
{
	using CargoWise.Data;
	using Enterprise.ZArchitecture.GUI;

	public class WaitDownloadingFormManager : ZProcessStatusFormManager<WaitDownloadingForm>
	{
		public WaitDownloadingFormManager()
			: base()
		{
		}

		protected override WaitDownloadingForm CreateFormCore()
		{
			disposableActionForDbConnection = Db.DisposableActionForDbConnection();
			return base.CreateFormCore();
		}
	}
}
