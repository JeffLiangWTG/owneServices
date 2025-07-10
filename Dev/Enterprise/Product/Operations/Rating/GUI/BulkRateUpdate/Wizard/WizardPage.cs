namespace Enterprise.Rating.GUI
{
	using Enterprise.Rating.Business;
	using Enterprise.ZArchitecture.GUI;

	public abstract class WizardPage : ZUserControl
	{
		protected WizardForm Wizard
		{
			get { return TopLevelControl as WizardForm; }
		}

		protected BulkRateUpdater Updater
		{
			get { return (BulkRateUpdater)DataSource; }
		}

		public abstract void NotifyActivated(WizardForm wizard);

		public abstract void NotifyLeaving(WizardSteppingEventArgs args);
	}
}

