
namespace Enterprise.Rating.GUI
{
	public partial class WizardPageStep : WizardPage
	{
		#region Ctor

		public WizardPageStep()
			: base()
		{
			InitializeComponent();
		}

		#endregion

		#region Overrides

		public override void NotifyActivated(WizardForm wizard)
		{
			wizard.PageHeaderVisible = true;
		}

		public override void NotifyLeaving(WizardSteppingEventArgs args)
		{
		}

		#endregion

		static protected string UnableToProceedMessage
		{
			get { return Res.GetString("00405607-2343-47f5-aef9-b653682248c0", "Unable to proceed"); }
		}
	}
}

