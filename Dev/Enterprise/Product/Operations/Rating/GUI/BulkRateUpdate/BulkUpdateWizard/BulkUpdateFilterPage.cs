using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateFilterPage : WizardPageStep
	{
		public BulkUpdateFilterPage()
		{
			InitializeComponent();
		}

		public override void NotifyActivated(WizardForm wizard)
		{
			base.NotifyActivated(wizard);

			wizard.PageHeaderTitle = Res.GetString("10a59e37-990a-46d2-b0d8-d09fc2345aea", "Filter");
			wizard.PageHeaderDescription = Res.GetString("91ecdd16-a973-4609-b0c6-a8caf3688408", "Enter the criteria for the rates that you wish to update");
		}

		public override void NotifyLeaving(WizardSteppingEventArgs args)
		{
			base.NotifyLeaving(args);

			if (args.MovementDirection != WizardSteppingEventArgs.Direction.Forward)
			{
				return;
			}

			if (!ValidatePage())
			{
				args.Cancel = true;
				Globals.Message.ShowError(Res.GetString("a9982d29-aa0c-4ee4-91fb-b7ab33a76410", "You have provided incorrect filter criteria"),
					UnableToProceedMessage);
			}
		}

		bool ValidatePage()
		{
			Updater.Validation.ValidateAll();
			return !Updater.HasErrors;
		}
	}
}

