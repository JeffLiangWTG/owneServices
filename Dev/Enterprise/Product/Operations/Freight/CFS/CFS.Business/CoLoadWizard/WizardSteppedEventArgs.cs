using System;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// Summary description for WizardSteppedEventArgs.
	/// </summary>
	public class WizardSteppedEventArgs : EventArgs
	{
		public WizardSteppedEventArgs(CoLoadWizardSteps currentStep)
		{
			this.CurrentStep = currentStep;
		}

		public readonly CoLoadWizardSteps CurrentStep;
	}
}
