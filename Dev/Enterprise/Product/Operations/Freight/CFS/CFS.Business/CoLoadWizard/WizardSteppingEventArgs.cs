namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// Summary description for WizardSteppingEventArgs.
	/// </summary>
	public class WizardSteppingEventArgs : System.ComponentModel.CancelEventArgs
	{
		public WizardSteppingEventArgs(CoLoadWizardSteps currentStep, CoLoadWizardSteps nextStep)
		{
			this.CurrentStep = currentStep;
			this.NextStep = nextStep;
		}
		public readonly CoLoadWizardSteps CurrentStep;
		public CoLoadWizardSteps NextStep;
	}
}
