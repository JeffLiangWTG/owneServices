namespace Enterprise.Rating.GUI
{
	public class BulkUpdateFinishPage : WizardPageFinish
	{
		public BulkUpdateFinishPage()
		{
			Image = BulkUpdateWizardImages.WizardFinishImage;
			Title = Res.GetString("f92d7f91-d147-40aa-b596-b61fc262f44e", "Update completed");
		}

		public override void NotifyActivated(WizardForm wizard)
		{
			base.NotifyActivated(wizard);
			Description = Res.GetString("e3aa9302-87c4-4139-accf-7c1e357eabe4", "You have successfully updated {0} rate(s).", Updater.AllEntriesToUpdateCount);
		}
	}
}

